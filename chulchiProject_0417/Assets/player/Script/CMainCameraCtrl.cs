using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CMainCameraCtrl : MonoBehaviour
{
    private float viewTargetDamper;

    public GameObject viewTarget;
    public float camDist;
    public float camHeight;
    public float moveDist;

    private bool isFollow;
    public float traceSpeed;

    private float camYpos;
    private bool changeYpos = false;

    private bool isRotate = false;
    public float rotAngle;
    public float rotTime;

    private delegate void VOID_FUNCTION();
    private VOID_FUNCTION del_CameraMove;

    public bool cameraEvent
    {
        get;
        private set;
    }

    public void fAwake()
    {
        transform.position = viewTarget.transform.position + -Vector3.forward * camDist + Vector3.up * camHeight;
        transform.LookAt(viewTarget.transform);

        camYpos = transform.position.y;

        del_CameraMove = fCameraMoveAxis_X;
    }

    public void fLateUpdate()
    {
        del_CameraMove();
    }

    private void fCameraMoveAxis_X()
    {
        if (changeYpos)
            camYpos = viewTarget.transform.position.y + camHeight;

        Vector3 movePosition = viewTarget.transform.position + -Vector3.forward * camDist;
        movePosition.y = camYpos;

        float dist = Mathf.Abs(viewTarget.transform.position.x - transform.position.x);

        if (isFollow == false && dist > moveDist)
            isFollow = true;

        else if (isFollow == true && dist < 0.005f)
        {
            isFollow = false;
            transform.position = movePosition;
        }

        if (isFollow)
            transform.position = Vector3.Lerp(transform.position, movePosition, Time.deltaTime * traceSpeed);
    }

    private void fCameraMoveAxis_Z()
    {
        if (changeYpos)
            camYpos = viewTarget.transform.position.y + camHeight;

        Vector3 movePosition = viewTarget.transform.position + -Vector3.right * camDist;
        movePosition.y = camYpos;

        float dist = Mathf.Abs(viewTarget.transform.position.z - transform.position.z);

        if (isFollow == false && dist > moveDist)
            isFollow = true;

        else if (isFollow == true && dist < 0.005f)
        {
            isFollow = false;
            transform.position = movePosition;
        }

        if (isFollow)
            transform.position = Vector3.Lerp(transform.position, movePosition, Time.deltaTime * traceSpeed);
    }

    private void fLookAtViewTarget()
    {
        transform.LookAt(viewTarget.transform);
    }

    public void fRotateCamera(bool isZMove)
    {
        if (cameraEvent || isZMove == isRotate)
            return;

        cameraEvent = true;
        isRotate = isZMove;
        del_CameraMove = fLookAtViewTarget;

        StartCoroutine("fRotateCameraEvent");
    }

    private IEnumerator fRotateCameraEvent()
    {
        float deltaTime = 0;
        float totalAngle = 0;
        float rotPS = rotAngle / rotTime;

        if (!isRotate)
            rotPS = -rotPS;

        while(Mathf.Abs(totalAngle) < Mathf.Abs(rotAngle))
        {
            float rotPD = rotPS * Time.deltaTime;
            deltaTime += Time.deltaTime;
            totalAngle += rotPD;

            if(totalAngle >= 90)
            {
                float temp = totalAngle - 90;
                rotPD -= temp;
            }

            transform.RotateAround(viewTarget.transform.position, Vector3.up, rotPD);
            yield return null;
        }

        if (isRotate)
        {
            del_CameraMove = fCameraMoveAxis_Z;
            //transform.localRotation = Quaternion.Euler(transform.localRotation.x, rotAngle, transform.localRotation.z);
        }
        else
        {
            del_CameraMove = fCameraMoveAxis_X;
            //transform.localRotation = Quaternion.Euler(transform.localRotation.x, 0, transform.localRotation.z);
        }
        //transform.LookAt(viewTarget.transform);

        cameraEvent = false;
    }
}
