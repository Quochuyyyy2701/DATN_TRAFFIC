using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Segment : CustomMonoBehaviour
{
    public WayPoint wayPoint;

    [Header("NextSegment")]
    public Segment leftSegment;
    public Segment rightSegment;
    public Segment forwardSegment;


    public override void LoadComponent()
    {
        this.LoadWayPoint();
    }

    public void LoadWayPoint()
    {
        if (wayPoint != null) return;
        wayPoint = transform.Find("WayPoint").GetComponent<WayPoint>();
    }    


}
