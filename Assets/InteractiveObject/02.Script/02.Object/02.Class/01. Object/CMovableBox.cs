using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyDebug;


public class CMovableBox : CInteractiveObject
{
    public void Awake()
    {
        fInit();
        
    }

    public new void fInit()
    {
        base.fInit();
        objectType = OBJECT_TYPE.BOX;
    }

    public override bool fStartEvent()
    {
        if(objectActive && !objectInteract)
        {
            //if (CGameManager.instance.playerCtrl.fInteractObject(objectType, true))
            if (CPlayerCtrl3.interactionState.fInteractObject(objectType, true))
            {
                objectInteract = true;
                return true;
            }
        }
        return false;
    }
    public override bool fEndEvent()
    {
        if (objectActive && objectInteract)
        {
            //if (CGameManager.instance.playerCtrl.fInteractObject(objectType, false))
            if (CPlayerCtrl3.interactionState.fInteractObject(objectType, true))
            {
                objectInteract = false;
                gameObject.layer = 10;
                return true;
            }
        }
        return false;
    }

    public void move(Vector3 speed, PLAYER_VIEW_DIR state)
    {
        RaycastHit hit;
        LayerMask mask = ~(1 << 8);
        
        if (Physics.Raycast(transform.position, transform.right * (int)state, out hit, 2.0f, mask))
        {
            if(!hit.collider.CompareTag("UpHill"))
            {
                float d = (transform.position - hit.point).magnitude;
                if (d <= 0.50f)
                {
                    Debug.Log( DebugType.OBJECT_STATE, hit.collider);
                    gameObject.layer = 10; 
                    return;
                }
                else
                    gameObject.layer = 11;
            }
        }
        else
            gameObject.layer = 11;

        if (Mathf.Abs(speed.magnitude) < 0.1f)
            return;

        float[] dist = new float[2];
        Vector3[] hitPoint = new Vector3[2];

        transform.position += transform.up * 0.5f;
        transform.position += speed * Time.deltaTime;
        
        
        if (Physics.Raycast(transform.position + transform.right * 0.5f + -transform.up * 0.5f, -transform.up, out hit, 5.0f, mask))
        {
            hitPoint[0] = hit.point;
            dist[0] = (hit.point - (transform.position + transform.right * 0.5f + -transform.up * 0.5f)).magnitude;
        }

        if (Physics.Raycast(transform.position + -transform.right * 0.5f + -transform.up * 0.5f, -transform.up, out hit, 5.0f, mask))
        {
            hitPoint[1] = hit.point;
            dist[1] = (hit.point - (transform.position + -transform.right * 0.5f + -transform.up * 0.5f)).magnitude;
        }

        float height = dist[1] - dist[0];
        float angle = Mathf.Atan2(height, 1) * Mathf.Rad2Deg - transform.rotation.z;
        transform.Rotate(new Vector3(0, 0, angle));

        transform.position = (hitPoint[0] + hitPoint[1]) / 2 + transform.up * 0.5f;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 5.0f, mask))
        {
            float distance = (hit.point - transform.position).magnitude;
            if (distance < 0.5f)
                transform.position = hit.point + transform.up * 0.5f;
        }
    }
}

