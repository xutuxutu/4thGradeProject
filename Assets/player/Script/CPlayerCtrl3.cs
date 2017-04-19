using MyDebug;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;
using RootMotion.FinalIK;

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

    //IK관련 변수
    public LimbIK[] limbIK; // reference to the LimbIK component
    public Transform[] foot;
    public Transform[] knee;
    public float ground;

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

    [System.Serializable]
    public struct PhysicalStatus
    {
        public int maxHp;
        public float maxStamina;

        public int hp;
        public float stamina;
    }
    [Space(10)]
    public PhysicalStatus physicalStatus;

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


    [System.NonSerialized] public Transform wallTr;
    [System.NonSerialized] public CharacterController controller;
    [System.NonSerialized] public Animator animator;

    public PLAYER_VIEW_DIR viewDir;
    public IInteractiveObject InteractObjectTrigger;

    #region 초기화
    void fPhysicalStatusInit()
    {
        physicalStatus.hp = physicalStatus.maxHp;
        physicalStatus.stamina = physicalStatus.maxStamina;
    }

    void fAcrionStateInit()
    {
        oldAcrionState = idelState;
        curActionState = idelState;
        curActionState.fEnterState(this);
    }

    void fComponentInit()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerColider = GetComponentInChildren<CPlayerColider>();
        playerColider.fAwake();
    }

    void fOtherVariable()
    {
        canHanging = false;
        isZmove = false;
        viewDir = PLAYER_VIEW_DIR.RIGHT;
    }
    #endregion

    public void fAwake()
    {
        fComponentInit();
        fAcrionStateInit();
        fOtherVariable();
        fPhysicalStatusInit();
    }

    
    public void fUpdate ()
    {
        CDebug.Log(DebugType.PLAYER_ACTION_STATE, curActionState);
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

        fActionStateUpdate();
        Move(this);
    }

    public void fLateUpdate()
    {
        fSetFootPosition_IK();
    }


    #region 상태 머신
    void fActionStateUpdate()
    {
        CPlayerActionState returnActionState = curActionState.fUpdateState(this);
        if (returnActionState != null)
        {
            curActionState.fExitState(this);
            oldAcrionState = curActionState;
            curActionState = returnActionState;
            curActionState.fEnterState(this);
        }
    }

    #endregion

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
    // 위로 올라가는 애니메이션 후 강제 이동
    public void fEndEvnet_HangUp()
    {
        transform.position += new Vector3((int)viewDir * 0.81f, 1.46f);
    }
    #endregion

    #region IK
    public void fSetFootPosition_IK()
    {
        RaycastHit hit;
        LayerMask mask = (1 << 8) | (1 << 10) | (1 << 11);
        mask = ~mask;

        //경사도에 따른 충돌체 크기 조절
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, -Vector3.up, out hit, 1.0f, mask))
        {
            float angle = Vector3.Dot(Vector3.right, hit.normal);
            angle = (Mathf.Acos(angle) * Mathf.Rad2Deg - 90);

            if (angle < 30 && angle > 0)
            {
                float height = 1.6f - (angle / 100);
                controller.height = (controller.height + height) / 2;
            }
        }

        //왼발 위치 및 각도 설정
        if (limbIK[0] != null)
        {

            Vector3 hitNormal = Vector3.up;
            Vector3 hitPoint = foot[0].position;
            //Debug.DrawRay(foot[0].position + Vector3.up * 0.5f, -Vector3.up, Color.red, 0.6f);
            if (Physics.Raycast(foot[0].position + Vector3.up * 0.5f, -Vector3.up, out hit, 0.6f, mask))
            {
                hitNormal = hit.normal;
                hitPoint = hit.point + Vector3.up * ground;
            }

            float angle = Vector3.Dot(Vector3.right, hitNormal);
            angle = (Mathf.Acos(angle) * Mathf.Rad2Deg - 90) * (int)viewDir;

            Vector3 a = foot[0].eulerAngles - new Vector3(-2, 82, 92);
            a = new Vector3(a.z - angle, a.y, a.x);
            limbIK[0].solver.SetIKRotation(Quaternion.Euler(a));

            limbIK[0].solver.SetIKPosition(hitPoint);

            limbIK[0].solver.bendNormal = knee[0].forward;
        }
        //오른발 위치 및 각도 설정
        if (limbIK[1] != null)
        {
            Vector3 hitNormal = Vector3.up;
            Vector3 hitPoint = foot[1].position;
            //Debug.DrawRay(foot[1].position + Vector3.up * 0.5f, -Vector3.up, Color.red, 0.6f);
            if (Physics.Raycast(foot[1].position + Vector3.up * 0.5f, -Vector3.up, out hit, 0.6f, mask))
            {
                hitNormal = hit.normal;
                hitPoint = hit.point + Vector3.up * ground;
            }

            float angle = Vector3.Dot(Vector3.right, hitNormal);
            angle = (Mathf.Acos(angle) * Mathf.Rad2Deg - 90) * (int)viewDir;

            Vector3 a = foot[1].eulerAngles - new Vector3(-2, 82, 92);
            a = new Vector3(a.z, a.y, a.x);
            limbIK[1].solver.SetIKRotation(Quaternion.Euler(a));

            limbIK[1].solver.SetIKPosition(hitPoint);
            limbIK[1].solver.bendNormal = knee[1].forward;
        }
    }
    #endregion
}
