using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;


public class CPlayerCtrl3 : MonoBehaviour
{
    private CPlayerColider playerColider;
    public delegate void voidFunction(CPlayerCtrl3 player);
    public voidFunction Move;

    private INPUTSTATE curInputState;
    public bool isZmove;
    
    public CPlayerActionState curActionState { get; private set; }
    public CPlayerActionState oldAcrionState { get; private set; }
    public static CIdelState idelState = new CIdelState();
    public static CMoveState runState = new CRunState();
    public static CJumpState jumpState = new CJumpState();
    public static CHangState hangState = new CHangState();
    static public CInteractionState interactionState = new CInteractionState();


    [System.Serializable]
    public struct AppointMoveValueStruct
    {
        public float frictionForce;

        public float maxRunSpeed;
        public float runSpeed;

        public float maxDeadrunSpeed;
        public float deadrunSpeed;

        public float maxBoxMoveSpeed;
        public float boxMoveSpeed;
    }
    public AppointMoveValueStruct moveValueStruct;

    [System.Serializable]
    public struct AppointJumpValueStruct
    {
        public float customGravity;

        public float InplaceJumpSpeedY;
        public float RunJumpSpeedY;
        public float RunJumpPower;
        
    }
    [Space(10)]
    public AppointJumpValueStruct jumpValueStruct;

    public struct ZMoveInfo
    {
        public bool hasOhterVector;
        public Vector3 moveOtherVector;
        public Quaternion moveOtherRot;

    }
    public ZMoveInfo zMoveInfo;

    [Space(10)]
    public float groundCheckDistance = 0.6f;
    public float wallCheckDistance = 0.6f;
    

    [Space(10)]
    [Header("- Cur State -")]
    public float speedX;
    public bool isPlayerStop;
    public bool isGrounded;
    public bool canHanging;
    private Vector3 groundNormal;


    //public bool hasOhterVector;
    //public Vector3 moveOtherVector;
    //public Quaternion moveOtherRot;
    

    [System.NonSerialized] public Transform wallTr;
    [System.NonSerialized] public CharacterController controller;
    [System.NonSerialized] public Animator animator;

    public PLAYER_VIEW_DIR viewDir;
    public IInteractiveObject InteractObjectTrigger;


    public void fAwake()
    {
        playerColider = GetComponentInChildren<CPlayerColider>();
        playerColider.fAwake();
        viewDir = PLAYER_VIEW_DIR.RIGHT;  
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        oldAcrionState = idelState;
        curActionState = idelState;
        curActionState.fEnterState(this);
        canHanging = false;
        isZmove = false;
    }

	public void fUpdate ()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!CGameManager.instance.cameraCtrl.cameraEvent)
            {
                isZmove = !isZmove;
                CGameManager.instance.cameraCtrl.fRotateCamera(isZmove);
            }
        }

        playerColider.fUpdate();
        fCurInputsUpdate();
        fSpeedOfSpeedToZero();
        
        CheckGroundStatus();
        ChackWallState();

        CPlayerActionState returnActionState = curActionState.fUpdateState(this);
        if(returnActionState != null)
        {
            curActionState.fExitState(this);
            oldAcrionState = curActionState;
            curActionState = returnActionState;
            curActionState.fEnterState(this);
        }
        Move(this);
    }

    #region 키 입력 처리
    private void fCurInputsUpdate()
    {
        foreach(INPUTSTATE inputState in Enum.GetValues(typeof(INPUTSTATE)) as INPUTSTATE[])
        {
            if (CGameManager.instance.inputManager.fGetKeyDown(inputState))
                curInputState |= inputState;
            else if (CGameManager.instance.inputManager.fGetKeyUp(inputState))
                curInputState ^= inputState;

        }
    }

    public bool fIsInputKey(INPUTSTATE inputState)
    {
        return ((curInputState & inputState) == inputState) ? true : false;
    }
    #endregion

    #region 이동 관련
    private void fSpeedOfSpeedToZero()
    {
        if (speedX > 0.05f || speedX < -0.05f)
        {
            isPlayerStop = false;
            speedX = fMoveToTargetReturnReviseSpeed(0, moveValueStruct.frictionForce);
        }
        else
        {
            isPlayerStop = true;
            speedX = 0;
        }
    }

    public float fMoveToTargetReturnReviseSpeed(float target, float variation)
    {

        if (speedX >= target)
        {
            speedX -= variation;
            if (speedX < target)
                speedX = target;
        }
        else
        {
            speedX += variation;
            if (speedX > target)
                speedX = target;
        }
        return speedX;

    }

    public void ControllerMove(Vector3 moveVector)
    {
        if (isZmove)
        {
            float tempPos;
            tempPos = moveVector.x;
            moveVector.x = moveVector.z;
            moveVector.z = tempPos;
        }

        if (zMoveInfo.hasOhterVector)
        {
            controller.Move(new Vector3(moveVector.x * zMoveInfo.moveOtherVector.x, moveVector.y * zMoveInfo.moveOtherVector.y, moveVector.x * zMoveInfo.moveOtherVector.z) * Time.deltaTime);
        }
        else
        {
            controller.Move(moveVector * Time.deltaTime);
        }

    }

    public IEnumerator TrunToZ(Quaternion eulerAngle)
    {
        
        if (viewDir == PLAYER_VIEW_DIR.RIGHT)
            CGameManager.instance.inputManager.fStartInputException(INPUT_EXCEPTION_STATE.PREVENT_LEFT);
        else
            CGameManager.instance.inputManager.fStartInputException(INPUT_EXCEPTION_STATE.PREVENT_RIGHT);

        while (true)
        {
            transform.rotation = Quaternion.LerpUnclamped(transform.rotation, eulerAngle, Time.deltaTime * 10f);
            if (Mathf.Round(transform.rotation.eulerAngles.y) == Mathf.Round(eulerAngle.eulerAngles.y) )
            {
                transform.rotation = eulerAngle;
                if (viewDir == PLAYER_VIEW_DIR.RIGHT)
                    CGameManager.instance.inputManager.fEndInputException(INPUT_EXCEPTION_STATE
                        .PREVENT_LEFT);
                else
                    CGameManager.instance.inputManager.fEndInputException(INPUT_EXCEPTION_STATE.PREVENT_RIGHT);
                break;
            }
            yield return null;
        }
    }

    #endregion

    #region 레이 체크
    float curY, oldY = 0;
    void CheckGroundStatus()
    {
        RaycastHit hitInfo;

        curY = controller.velocity.y;
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));

        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance) && !hitInfo.collider.isTrigger)
        {
            if(curY < oldY)
            {
                isGrounded = true;
            }
            groundNormal = hitInfo.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
        oldY = curY;

    }

    void ChackWallState()
    {

        RaycastHit hitInfo;
        Debug.DrawLine(transform.position, transform.position + (Vector3.right * (int)viewDir * wallCheckDistance));

        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.right * (int)viewDir, out hitInfo, wallCheckDistance) && hitInfo.transform.CompareTag("Wall"))
        {
            canHanging = true;
            wallTr = hitInfo.transform;
        }
        else
        {
            canHanging = false;
            wallTr = null;
        }

    }
    #endregion

    #region 이벤트 함수
    public void fEndEvnet_HangUp()
    {
        transform.position += new Vector3((int)viewDir * 0.81f, 1.46f);
    }
    #endregion
}
