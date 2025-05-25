using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMove : CustomMonoBehaviour
{
    public Segment CurrentSegment;
    private Vector3 direction;
    private Vector3 lastDirection;
    public Rigidbody rb;
    [SerializeField] private Transform targetPoint;
    private Quaternion targetRotation;
    private float rotationSpeed = 5f;
    [SerializeField] private float detectionDistance = 1f;
    [SerializeField] private LayerMask carLayer; // Layer chứa các xe
    private bool isWaiting = false;
    void Start()
    {
        detectionDistance = 1f;
        targetPoint = CurrentSegment.wayPoint.GetStartPoint();
        InitSegmentMove();
    }

    void Update()
    {
        if (targetPoint != null)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public override void LoadComponent()
    {
        // Không cần dùng
    }

    void FixedUpdate()
    {
        if (targetPoint == null) return;

        // Nếu có xe phía trước thì không di chuyển
        if (IsCarInFront())
        {
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * detectionDistance, Color.red);
            return;
        }
        else
        {
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * detectionDistance, Color.green);
        }

        Vector3 velocity = direction * 3f;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

        float distanceToTarget = Vector3.Distance(transform.position, targetPoint.position);
        if (distanceToTarget < 1f)
        {
            if (targetPoint == CurrentSegment.wayPoint.GetEndPoint())
            {
                SwitchToNextSegment();
            }
        }
    }
    private bool IsCarInFront()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 1f);
        Quaternion orientation = transform.rotation;
        Vector3 castDirection = transform.forward;
        if (Physics.BoxCast(origin, halfExtents, castDirection, out RaycastHit hit, orientation, detectionDistance))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.CompareTag("TrafficLight"))
            {
                TrafficLight light = hitObj.GetComponent<TrafficLight>();
                if (light != null && light.currentState == LightState.Red)
                {
                    lastDirection = transform.forward;
                    return true;
                }
            }
            else if (((1 << hit.collider.gameObject.layer) & carLayer) != 0)
            {
                lastDirection = transform.forward;
                return true;
            }
        }
        return false;
    }


    private void InitSegmentMove()
    {
        if (CurrentSegment == null)
        {
            Debug.LogWarning("CurrentSegment is null.");
            return;
        }

        Transform pointA = CurrentSegment.wayPoint.GetStartPoint();
        Transform pointB = CurrentSegment.wayPoint.GetEndPoint();

        transform.position = pointA.position;

        direction = (pointB.position - pointA.position).normalized;
        targetPoint = pointB;
        lastDirection = direction;
        targetRotation = Quaternion.LookRotation(direction);
    }

    void SwitchToNextSegment()
    {
        if (CurrentSegment == null)
        {
            Debug.LogWarning("CurrentSegment is null in SwitchToNextSegment.");
            return;
        }

        List<Segment> possibleSegments = new List<Segment>();
        if (CurrentSegment.forwardSegment != null) possibleSegments.Add(CurrentSegment.forwardSegment);
        if (CurrentSegment.leftSegment != null) possibleSegments.Add(CurrentSegment.leftSegment);
        if (CurrentSegment.rightSegment != null) possibleSegments.Add(CurrentSegment.rightSegment);

        if (possibleSegments.Count == 0)
        {
            Debug.Log("No next segment found. Car will stop.");
            direction = Vector3.zero;
            targetPoint = null;
            return;
        }

        Segment nextSegment = possibleSegments[Random.Range(0, possibleSegments.Count)];
        StartCoroutine(MoveToNextSegmentStart(nextSegment));
    }

    IEnumerator MoveToNextSegmentStart(Segment nextSegment)
    {
        // B1: Di chuyển tới điểm A của đoạn mới
        Transform nextPointA = nextSegment.wayPoint.GetStartPoint();
        direction = (nextPointA.position - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(direction);
        targetPoint = nextPointA;
        lastDirection = direction;

        // Đợi xe tới gần pointA
        yield return new WaitUntil(() => Vector3.Distance(transform.position, nextPointA.position) < 1f);

        // B2: Gán đoạn mới, di chuyển từ A tới B của đoạn đó
        CurrentSegment = nextSegment;

        Transform pointB = CurrentSegment.wayPoint.GetEndPoint();
        direction = (pointB.position - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(direction);
        targetPoint = pointB;
        lastDirection = direction;


    }


    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.collider.CompareTag("TrafficLight"))
    //    {
    //        var light = collision.collider.GetComponent<TrafficLight>();
    //        light.OnLightChanged += OnTrafficLightChanged;

    //        if (light.currentState == LightState.Red)
    //        {
    //            Debug.Log("Đèn đỏ - dừng");
    //            isWaiting = true;
    //            direction = Vector3.zero;
    //        }
    //        else
    //        {
    //            Debug.Log("Đèn xanh - tiếp tục đi");
    //            isWaiting = false;
    //            direction = lastDirection;
    //        }
    //    }
    //}

    //private void OnTrafficLightChanged(LightState newState)
    //{
    //    if (newState == LightState.Green && isWaiting)
    //    {
    //        Debug.Log("Đèn chuyển xanh - xe tiếp tục");
    //        direction = Vector3.forward;
    //        isWaiting = false;
    //    }
    //    else if (newState == LightState.Red)
    //    {
    //        Debug.Log("Đèn chuyển đỏ - xe dừng");
    //        direction = Vector3.zero;
    //        isWaiting = true;
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("TrafficLight"))
    //    {
    //        var light = other.GetComponent<TrafficLight>();
    //        light.OnLightChanged -= OnTrafficLightChanged;
    //        Debug.Log("Xe đã rời khỏi vùng đèn giao thông");
    //    }
    //}
    IEnumerator CoutinueMove(float time)
    {
        yield return new WaitForSeconds(time);
        direction = lastDirection;
    }

}
