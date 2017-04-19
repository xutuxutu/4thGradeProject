using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform TargetPoint;

    bool Xmove = true;
    public float LeadTime;

    public float[] angle;
    Vector2[] point;
    Vector2 pp;

    Vector3[] bPoint;

	// Use this for initialization
	void Start ()
    {
        //StartCoroutine("fXmove");
        point = new Vector2[4];
        bPoint = new Vector3[3];

        point[0] = new Vector2(transform.position.x, transform.position.z);

        point[1].x = point[0].x + Mathf.Cos(angle[0] * Mathf.Deg2Rad);
        point[1].y = point[0].y + Mathf.Sin(angle[0] * Mathf.Deg2Rad);

        point[2] = new Vector2(TargetPoint.position.x, TargetPoint.position.z);

        point[3].x = point[2].x + Mathf.Cos(angle[1] * Mathf.Deg2Rad);
        point[3].y = point[2].y + Mathf.Sin(angle[1] * Mathf.Deg2Rad);

        pp = CrossPoint(point[0], point[1], point[2], point[3]);

        bPoint[0] = transform.position;
        bPoint[1] = TargetPoint.position;
        bPoint[2] = new Vector3(pp.x, 1, pp.y);

        StartCoroutine("fMove");
    }
	
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(pp.x, 1, pp.y), 0.5f);
    }

	// Update is called once per frame
	void Update ()
    {
	}

    public Vector2 CrossPoint(Vector2 aLine_p1, Vector2 aLine_p2, Vector2 bLine_p1, Vector2 bLine_p2)
    {
        Vector2 crossPoint = Vector2.zero;

        float a1 = aLine_p2.y - aLine_p1.y;
        float b1 = aLine_p2.x - aLine_p1.x;
        //기울기
        float gradient_A = a1 / b1;
        //y절편
        float intercept_A = -(gradient_A * aLine_p1.x) + aLine_p1.y;

        float a2 = bLine_p2.y - bLine_p1.y;
        float b2 = bLine_p2.x - bLine_p1.x;
        float gradient_B = a2 / b2;
        float intercept_B = -(gradient_B * bLine_p1.x) + bLine_p1.y;

        //y축에 수평인 그래프일 때.
        if (b1 == 0)
        {
            crossPoint.x = aLine_p1.x;
            crossPoint.y = gradient_B * aLine_p1.x + intercept_B;
        }
        else if (b2 == 0)
        {
            crossPoint.x = bLine_p1.x;
            crossPoint.y = gradient_A * bLine_p1.x + intercept_A;
        }
        else
        {
            crossPoint.x = (intercept_B - intercept_A) / (gradient_A - gradient_B);
            crossPoint.y = (intercept_A * gradient_B - intercept_B * gradient_A) / (gradient_B - gradient_A);
        }

        UnityEngine.Debug.DrawLine(new Vector3(aLine_p1.x, 1, aLine_p1.y), new Vector3(crossPoint.x, 1, crossPoint.y), Color.blue);
        UnityEngine.Debug.DrawLine(new Vector3(bLine_p1.x, 1, bLine_p1.y), new Vector3(crossPoint.x, 1, crossPoint.y), Color.red);

        return crossPoint;
    }

    public Vector3 Linear(Vector3 p1, Vector3 p2, float t) { return ((1 - t) * p1) + (t * p2); }

    public Vector3 BezierCurve(Vector3 p1, Vector3 p2, Vector3 relaxP, float t)
    {
        Vector3 pa = Linear(p1, relaxP, t);
        Vector3 pb = Linear(relaxP, p2, t);
        return Linear(pa, pb, t);
    }

    public IEnumerator fMove()
    {
        float t = 0;
        float tClock = (1 / LeadTime) * 0.01f;
        while(t < 1)
        {
            t += tClock;
            transform.position = BezierCurve(bPoint[0], bPoint[1], bPoint[2], t);
            yield return new WaitForSeconds(0.01f);
        }
    }
}
