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

    [SerializeField] private float detectionDistance = 2f;
    [SerializeField] private LayerMask carLayer; // Layer chứa các xe

    [SerializeField] private int priority;
    public float speed = 5;

    private float waitTimer = 0f;
    private float maxWaitTime = 2f;

    void Start()
    {
        targetPoint = CurrentSegment.wayPoint.GetStartPoint();
        priority = gameObject.GetInstanceID();
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

        if (IsObstacleAhead())
        {
            waitTimer += Time.fixedDeltaTime;

            if (waitTimer > maxWaitTime)
            {
                Debug.Log($"{name} waited too long, bypassing obstacle.");
                // Cho phép đi tiếp dù có chướng ngại (bỏ qua return)
                waitTimer = 0f;
            }
            else
            {
                Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * detectionDistance, Color.red);
                return;
            }
        }
        else
        {
            waitTimer = 0f;
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * detectionDistance, Color.green);
        }

        Vector3 velocity = direction * speed;
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

    private float GetDynamicDetectionDistance()
    {
        return Mathf.Max(detectionDistance, speed * Time.fixedDeltaTime * 5f);
    }

    private bool IsObstacleAhead()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 1f);
        Quaternion orientation = transform.rotation;
        Vector3 castDirection = transform.forward;

        float dynamicDistance = GetDynamicDetectionDistance();

        if (Physics.BoxCast(origin, halfExtents, castDirection, out RaycastHit hit, orientation, dynamicDistance))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.CompareTag("TrafficLight"))
            {
                TrafficLight light = hitObj.GetComponent<TrafficLight>();
                if (light != null && (light.currentState == LightState.Red || light.currentState == LightState.Yellow))
                {
                    return true;
                }
            }
            else if (hit.collider.CompareTag("Car"))
            {
                CarMove otherCar = hit.collider.GetComponent<CarMove>();

                if (otherCar != null)
                {
                    Vector3 toOther = otherCar.transform.position - transform.position;
                    float forwardDot = Vector3.Dot(transform.forward, toOther.normalized);
                    bool isInFront = forwardDot > 0.3f;

                    if (!isInFront)
                        return false;

                    float distanceToOther = Vector3.Distance(transform.position, otherCar.transform.position);

                    if (this.priority > otherCar.priority && distanceToOther < 2f && !otherCar.IsMoving())
                    {
                        return false;
                    }

                    if (otherCar.IsStopped() || this.priority <= otherCar.priority)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        return false;
    }

    public bool IsStopped()
    {
        return targetPoint == null || rb.velocity.magnitude < 0.1f;
    }

    public bool IsMoving()
    {
        return rb.velocity.magnitude > 0.1f;
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

        speed = 5;

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
        Transform nextPointA = nextSegment.wayPoint.GetStartPoint();
        direction = (nextPointA.position - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(direction);
        targetPoint = nextPointA;
        lastDirection = direction;

        yield return new WaitUntil(() => Vector3.Distance(transform.position, nextPointA.position) < 1f);

        CurrentSegment = nextSegment;

        Transform pointB = CurrentSegment.wayPoint.GetEndPoint();
        direction = (pointB.position - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(direction);
        targetPoint = pointB;
        lastDirection = direction;
    }
}
