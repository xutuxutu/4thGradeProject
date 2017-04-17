using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPlayerColider : MonoBehaviour
{
    private List<Transform> interactObjects;
    private Transform curObjectTr;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Object") && other.GetComponent<IInteractiveObject>() != null)
        {
            interactObjects.Add(other.transform);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Object"))
        {
            interactObjects.Remove(other.transform);
        }
    }

    public void fAwake()
    {
        interactObjects = new List<Transform>();
    }

    public void fUpdate()
    {
        fObject();
    }

    private void fObject()
    {
        Transform tempObjectTr;
        OBJECT_TYPE tempObjectType, curObjectType;

        foreach (Transform interactObject in interactObjects)
        {
            if (curObjectTr == null)
            {
                curObjectTr = interactObject;
                curObjectType = interactObject.GetComponent<IInteractiveObject>().objectType;

                if (Vector3.Distance(transform.position, curObjectTr.position)
                    > CObjectManager.instance.fObjectDistance(curObjectType))
                {
                    curObjectTr = null;
                    continue;
                }
            }

            tempObjectTr = interactObject;
            tempObjectType = interactObject.GetComponent<IInteractiveObject>().objectType;
            curObjectType = curObjectTr.GetComponent<IInteractiveObject>().objectType;

            // 새로 꺼낸 오브젝트가 사용 가능 거리에 있는가
            if (Vector3.Distance(transform.position, tempObjectTr.position)
                < CObjectManager.instance.fObjectDistance(tempObjectType))
            {
                // 새로 꺼낸 오브젝트가 현재 선택된 오브젝트 보다 우선 순위가 높다면
                if (CObjectManager.instance.fChackPriority(tempObjectType, curObjectType) == tempObjectType)
                {
                    curObjectTr = tempObjectTr;
                    curObjectType = tempObjectType;
                }
            }
        }

        if (curObjectTr != null)
        {
            CGameManager.instance.playerCtrl.InteractObjectTrigger = curObjectTr.GetComponent<IInteractiveObject>();
            curObjectTr = null;
        }
    }


}
