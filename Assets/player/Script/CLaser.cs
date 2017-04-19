using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLaser : MonoBehaviour {

    private LineRenderer lineRenderer;
    public Transform tr;
    public Transform l;
    public Transform r;
    public Transform point;
    public float curLength;
    public float maxLength = 100;
    public Vector3 collisionPoint;
    Vector3 dirVector;

    void Start()
    {

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startColor = Color.red;
        lineRenderer.startColor = Color.yellow;
        lineRenderer.startWidth = 0.01f;
        curLength = maxLength;
    }
	

	void Update () {
        dirVector = (r.position - l.position).normalized;
        CheckLaserCollisionObject();
        lineRenderer.SetPosition(0, tr.localPosition);
        lineRenderer.SetPosition(1, tr.localPosition + (dirVector * curLength));
        point.position = collisionPoint;

    }

    
    void CheckLaserCollisionObject()
    {
        RaycastHit hitInfo;
        Debug.DrawLine(tr.position, tr.position + (dirVector * maxLength));

        if (Physics.Raycast(tr.position, dirVector, out hitInfo, maxLength))
        {
            collisionPoint = hitInfo.point;
            curLength = Vector3.Distance(tr.position, collisionPoint);
        }
        else
        {
            collisionPoint = Vector3.zero;
        }

    }
    
}
