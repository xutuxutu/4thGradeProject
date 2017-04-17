using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class CZMover : MonoBehaviour, ITriggerEvent
{

    public Transform startPos;
    public Transform endPos;

    public float quat;
    public Vector3 normalMoveVector;


    private bool isMoveToZ;
    private Vector3 tempRot;
    private bool isDestinationRight;
    private float DirectionCorrection;


    struct TriggerState
    {
        public bool t1;
        public bool t2;
    }
    private TriggerState triggerState;

    void Awake ()
    {
        
        isMoveToZ = false;
        isDestinationRight = endPos.position.x > startPos.position.x;
        if(isDestinationRight)
        {
            quat = Vector3.Angle(startPos.position - endPos.position, -endPos.right);
            normalMoveVector = Vector3.Normalize(endPos.position - startPos.position);
        }
        else
        {
            quat = Vector3.Angle(startPos.position - endPos.position, -endPos.right) - 180;
            normalMoveVector = Vector3.Normalize(startPos.position - endPos.position);
        }

    }

    void fStartPosSetInfo()
    {
        CGameManager.instance.playerCtrl.zMoveInfo.hasOhterVector = true;
        normalMoveVector.y = 1;
        CGameManager.instance.playerCtrl.zMoveInfo.moveOtherVector = normalMoveVector;

        tempRot = CGameManager.instance.playerCtrl.transform.rotation.eulerAngles;
        tempRot.y += Mathf.Round(quat);
        StartCoroutine(CGameManager.instance.playerCtrl.TrunToZ(Quaternion.Euler(tempRot)));

        triggerState.t1 = true;
        triggerState.t2 = false;
    }
    
    void fEndPosSetInfo(Vector3 targetPos)
    {
        CGameManager.instance.playerCtrl.zMoveInfo.hasOhterVector = false;
        CGameManager.instance.playerCtrl.zMoveInfo.moveOtherVector = Vector3.zero;

        Vector3 temp = CGameManager.instance.playerCtrl.transform.position;
        temp.z = targetPos.z;
        CGameManager.instance.playerCtrl.transform.position = temp;

        tempRot = CGameManager.instance.playerCtrl.transform.rotation.eulerAngles;
        tempRot.y -= Mathf.Round(quat);
        StartCoroutine(CGameManager.instance.playerCtrl.TrunToZ(Quaternion.Euler(tempRot)));

        triggerState.t1 = false;
        triggerState.t2 = true;
    }

    public void fTriggerEnter(TriggerEvent triggerEvent)
    {
        switch(triggerEvent.name)
        {
            case "EventArea":
                if (CGameManager.instance.inputManager.fGetKey(INPUTSTATE.DOWN))
                {
                    if(isDestinationRight && CGameManager.instance.playerCtrl.viewDir == PLAYER_VIEW_DIR.RIGHT ||
                        !isDestinationRight && CGameManager.instance.playerCtrl.viewDir == PLAYER_VIEW_DIR.LEFT)
                    isMoveToZ = true;
                }
                break;
            case "Start":
               
                break;
            case "End":
                isMoveToZ = true;
                break;
        }
    }

    public void fTriggerStay(TriggerEvent triggerEvent)
    {
        
        switch (triggerEvent.name)
        {
            case "EventArea":
                break;
            case "Start":
                if (isMoveToZ)
                {
                    if (isDestinationRight)
                    {
                        if (CGameManager.instance.playerCtrl.transform.position.x > startPos.position.x && !triggerState.t1)
                            fStartPosSetInfo();  
                                  
                        else if (CGameManager.instance.playerCtrl.transform.position.x < startPos.position.x && triggerState.t1 && !triggerState.t2)
                        {
                            fEndPosSetInfo(startPos.position);     
                            isMoveToZ = false;
                        }
                    }
                    else
                    {
                        if (CGameManager.instance.playerCtrl.transform.position.x < startPos.position.x && !triggerState.t1)
                            fStartPosSetInfo();

                        else if (CGameManager.instance.playerCtrl.transform.position.x > startPos.position.x && triggerState.t1 && !triggerState.t2)
                        {
                            fEndPosSetInfo(startPos.position);
                            isMoveToZ = false;
                        }
                    }
                }
                break;
            case "End":

                if (isMoveToZ)
                {
                    if (isDestinationRight)
                    {
                        if (CGameManager.instance.playerCtrl.transform.position.x < endPos.position.x && !triggerState.t1)
                            fStartPosSetInfo();

                        else if (CGameManager.instance.playerCtrl.transform.position.x > endPos.position.x && triggerState.t1 && !triggerState.t2)
                            fEndPosSetInfo(endPos.position);
                    }
                    else
                    {
                        if (CGameManager.instance.playerCtrl.transform.position.x > endPos.position.x && !triggerState.t1)
                            fStartPosSetInfo();

                        else if (CGameManager.instance.playerCtrl.transform.position.x < endPos.position.x && triggerState.t1 && !triggerState.t2)
                            fEndPosSetInfo(endPos.position);
                    }
                }

                break;
        }
        
    }

    public void fTriggerExit(TriggerEvent triggerEvent){ }

}

/*
 public class CZMover : MonoBehaviour, ITriggerEvent
{

    public Transform startPos;
    public Transform endPos;

    public float quat;
    public Vector3 normalMoveVector;


    private bool isMoveToZ;
    private Vector3 tempRot;
    private bool isDestinationRight;
    private float DirectionCorrection;

    enum Triggered
    {
        NONE,
        EVENT_AREA,
        START,
        END
    }
    private Triggered lastTriggeredState;
    enum MoveState
    {
        NONE,
        ENTER,
        STAY,
        EXIT,
    }
    private MoveState curMoveState; 

    void Awake ()
    {
        isMoveToZ = false;
        curMoveState = MoveState.NONE;
        isDestinationRight = endPos.position.x > startPos.position.x;
        if(isDestinationRight)
        {
            quat = Vector3.Angle(startPos.position - endPos.position, -endPos.right);
            normalMoveVector = Vector3.Normalize(endPos.position - startPos.position);
        }
        else
        {
            quat = Vector3.Angle(startPos.position - endPos.position, -endPos.right) - 180;
            normalMoveVector = Vector3.Normalize(startPos.position - endPos.position);
        }

    }

    IEnumerator fStartPosChack()
    {
        while(true)
        {
            if (lastTriggeredState == Triggered.START)
            {
                if (CGameManager.instance.playerCtrl.transform.position.x > startPos.position.x && Vector3.Distance(CGameManager.instance.playerCtrl.transform.position, startPos.position) < 0.5f)
                {
                    fStartPosSetInfo();
                    break;
                }
            }
            else if(lastTriggeredState == Triggered.END)
            {
                if (CGameManager.instance.playerCtrl.transform.position.x < endPos.position.x && Vector3.Distance(CGameManager.instance.playerCtrl.transform.position, endPos.position) < 0.5f)
                {
                    fStartPosSetInfo();
                    break;
                }
            }
            yield return null;
        }
    }

    IEnumerator fEndPosChack()
    {
        while (true)
        {
            if (lastTriggeredState == Triggered.START)
            {
                if (CGameManager.instance.playerCtrl.transform.position.x < startPos.position.x && Vector3.Distance(CGameManager.instance.playerCtrl.transform.position, startPos.position) < 0.5f)
                {
                    fEndPosSetInfo(startPos.position);
                    break;
                }
            }
            else if (lastTriggeredState == Triggered.END)
            {
                if (CGameManager.instance.playerCtrl.transform.position.x > endPos.position.x && Vector3.Distance(CGameManager.instance.playerCtrl.transform.position, endPos.position) < 0.5f)
                {
                    fEndPosSetInfo(endPos.position);

                    break;
                }
            }

            yield return null;
        }
    }


    void fStartPosSetInfo()
    {
        CGameManager.instance.playerCtrl.zMoveInfo.hasOhterVector = true;
        CGameManager.instance.playerCtrl.zMoveInfo.moveOtherVector = normalMoveVector;

        tempRot = CGameManager.instance.playerCtrl.transform.rotation.eulerAngles;
        tempRot.y += Mathf.Round(quat);
        StartCoroutine(CGameManager.instance.playerCtrl.TrunToZ(Quaternion.Euler(tempRot)));
    }
    
    void fEndPosSetInfo(Vector3 targetPos)
    {
        CGameManager.instance.playerCtrl.zMoveInfo.hasOhterVector = false;
        CGameManager.instance.playerCtrl.zMoveInfo.moveOtherVector = Vector3.zero;

        Vector3 temp = CGameManager.instance.playerCtrl.transform.position;
        temp.z = targetPos.z;
        CGameManager.instance.playerCtrl.transform.position = temp;

        tempRot = CGameManager.instance.playerCtrl.transform.rotation.eulerAngles;
        tempRot.y -= Mathf.Round(quat);
        StartCoroutine(CGameManager.instance.playerCtrl.TrunToZ(Quaternion.Euler(tempRot)));
    }

    public void fTriggerEnter(TriggerEvent triggerEvent)
    {
        switch(triggerEvent.name)
        {
            case "EventArea":
                lastTriggeredState = Triggered.EVENT_AREA;
                if (CGameManager.instance.inputManager.fGetKey(INPUTSTATE.DOWN))
                {
                    isMoveToZ = true;
                }

                break;
            case "Start":
                lastTriggeredState = Triggered.START;
                switch (curMoveState)
                {
                    case MoveState.NONE:
                        if (isMoveToZ)
                        {
                            StartCoroutine(fStartPosChack());
                            curMoveState = MoveState.ENTER;
                        }
                        break;

                    case MoveState.ENTER:
                        isMoveToZ = false;
                        StartCoroutine(fEndPosChack());
                        curMoveState = MoveState.NONE;
                        break;

                    case MoveState.EXIT:
                        break;
                }
               
                break;
            case "End":
                lastTriggeredState = Triggered.END;
                switch (curMoveState)
                {
                    case MoveState.NONE:
                        isMoveToZ = true;
                        curMoveState = MoveState.ENTER;
                        StartCoroutine(fStartPosChack());

                        break;

                    case MoveState.ENTER:
                        if (isMoveToZ)
                        {
                            isMoveToZ = false;
                            StartCoroutine(fEndPosChack());
                            curMoveState = MoveState.NONE;
                        }                     
                        break;

                    case MoveState.EXIT:
                        
                        break;
                }

                
                break;
        }
    }
    private void Update()
    {
        Debug.Log("lastTriggeredState : " + lastTriggeredState + "  curMoveState : " + curMoveState);
    }

    public void fTriggerStay(TriggerEvent triggerEvent)
    {
        
        switch (triggerEvent.name)
        {
            case "EventArea":
                break;
            case "Start":
                break;
            case "End":
                if (isDestinationRight)
                {
                    if (lastTriggeredState == Triggered.END &&
                        CGameManager.instance.playerCtrl.transform.position.x < endPos.position.x &&
                        Vector3.Distance(CGameManager.instance.playerCtrl.transform.position, endPos.position) < 0.5f)
                    {
                        Debug.Log("Stay");
                        if (curMoveState == MoveState.NONE && CGameManager.instance.playerCtrl.viewDir == PLAYER_VIEW_DIR.LEFT)
                        {
                            isMoveToZ = true;
                            StartCoroutine(fStartPosChack());
                            curMoveState = MoveState.ENTER;
                        }
                        else if (curMoveState == MoveState.ENTER && CGameManager.instance.playerCtrl.viewDir == PLAYER_VIEW_DIR.RIGHT)
                        {
                            isMoveToZ = true;
                            StartCoroutine(fEndPosChack());
                            curMoveState = MoveState.NONE;
                        }
                    }
                }
                else
                {
                    
                }
                break;
        }
        
    }

    public void fTriggerExit(TriggerEvent triggerEvent)
    {
        
        switch (triggerEvent.name)
        {
            case "EventArea":
                break;
            case "Start":
                break;
            case "End":
                
                break;
        }
        
    }

   

}
*/

