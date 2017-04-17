using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MOVE_STATE_X
{
    STOP = 0,
    LEFT = -1,
    RIGHT = 1,
}

public enum MOVE_STATE_Y
{
    INPLACE_JUMP,
    RUN_JUMP,
    DEADRUN_JUMP,
}

public enum PLAYER_STATE
{ 
    IDEL,
    RUN,
    DEADRUN,
    JUMP,
}

// inputManager 변경 후, 캐릭터 컨트롤러 사용
public class CPlayerCtrl : MonoBehaviour
{
    private delegate void voidFunction();
    private voidFunction Move;

    private CharacterController controller;
    private Animator animator;
    private CInputMamager input;
    private CPlayerColider playerColider;
    public IInteractiveObject InteractObjectTrigger;

    [System.NonSerialized]
    public float curMaxSpeedX = 4;          // 현재 x축 최대 속도
    public float runMaxSpeedX = 4;          // 달리기중 x축 최대 속도
    public float deadrunMaxSpeedX = 7;      // 전력질주 X축 최대 속도
    public float boxMoveMaxSpeedX = 2;      // 박스이동중 x축 최대 속도
    public float maxJumpHeight = 5;         // 점프 높이
    public float frictionForce = 0.07f;     // 마찰력
    public float accelX = 0.15f;            // X축 이동 속도 (가속도)
    private float accelY;                   // Y축 이동 속도 (가속도)
    private float _speedX;                  // 이동 속도
    public float speedX
    {
        get { return _speedX; }
        set { _speedX = ((Mathf.Abs(_speedX) > curMaxSpeedX) ? (_speedX > 0 ? 1 : -1) * curMaxSpeedX : value); }
    }
    

    public bool isLookRight// 바라보는 방향 (오른쪽을 보고 있는지)
    {
        get { return _isLookRight;  }
        set { if (_isLookRight != value) canTurn = true;    _isLookRight = value; }
    }

    private float oldSpeedX;
    private bool _isLookRight;
    public bool isTurnning;                // 방향전환 중인지
    private bool canTurn;                   // 방향전환 가능한지
    private bool canIsJumpping;
    private bool isJumpping;                // 점프 중인지

    private PLAYER_STATE playerState;        // 플레이어 상태
    private MOVE_STATE_X moveState_X;       // X축 이동 상태
    private MOVE_STATE_X _oldMoveState_X;
    private MOVE_STATE_X oldMoveState_X
    {
        get { return _oldMoveState_X; }
        set { _oldMoveState_X = value != MOVE_STATE_X.STOP ? value : _oldMoveState_X; }
    }
    private MOVE_STATE_Y moveState_Y;       // y축 이동 상태



    public void fAwake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        input = CGameManager.instance.inputManager;
        playerColider = GetComponentInChildren<CPlayerColider>();

        Move = fMove_Run;

        playerColider.fAwake();
    }

    public void fStart()
    {
        curMaxSpeedX = runMaxSpeedX;
        oldMoveState_X = MOVE_STATE_X.RIGHT;
        oldSpeedX = 0;
        canIsJumpping = false;
        isJumpping = false;
        canTurn = false;
        isTurnning = false;
    }

    public void fUpdate()
    {
        playerColider.fUpdate();

        fMoveState_X_Update();
        fMoveSpeedUpdate();
        
        if (controller.isGrounded)
        {
            if(isJumpping)
            {
                isJumpping = false;
            }
            
            Move = fMove_Run;
            input.fEndInputException(INPUT_EXCEPTION_STATE.JUMP);
        }

        fInputUpdate();

        Move();
        
        fAnimatorUpdate();
    }



    #region 상태에 따른 Move 함수들

    // 달리기 이동
    void fMove_Run()
    {
        accelY = Physics.gravity.y * Time.deltaTime;
        controller.Move(new Vector3(speedX, accelY, 0) * Time.deltaTime);
    }

    // 점프, 점프중 이동
    void fMove_Jump()
    {
        switch (moveState_Y)
        {
            case MOVE_STATE_Y.INPLACE_JUMP:
                accelY += Physics.gravity.y * Time.deltaTime;
                controller.Move(new Vector3(0, accelY, 0) * Time.deltaTime);
                break;
            case MOVE_STATE_Y.RUN_JUMP:
                accelY += Physics.gravity.y * 0.05f * Time.deltaTime;
                controller.Move(new Vector3(runMaxSpeedX * (int)moveState_X, accelY, 0) * Time.deltaTime);
                break;
            case MOVE_STATE_Y.DEADRUN_JUMP:
                accelY += Physics.gravity.y * 0.2f * Time.deltaTime;
                controller.Move(new Vector3(deadrunMaxSpeedX * (int)moveState_X, accelY, 0) * Time.deltaTime);
                break;
        }
    
    }

    // 박스상호 작용 중 이동
    void fMove_PushBox()
    {
        fMove_Run();

        //(InteractObjectTrigger as CMovableBox).move(speedX);
    }
    #endregion



    #region 플레이어 상태 체크

    //바라보는 방향 체크
    public void fLookDirUpdate()
    {
        // 오른쪽을 바라 보면
        if(transform.TransformDirection(Vector3.right).z < 1)
            isLookRight = true;
        else
            isLookRight = false;

       
    }

    public bool fIsLookRight()
    {
        return transform.TransformDirection(Vector3.right).z < 1 ? true : false ;
    }

    // 플레이어 X축 이동 상태 확인
    private void fMoveState_X_Update()
    {
        if (speedX > 0.03f)
            moveState_X = MOVE_STATE_X.RIGHT;
        else if (speedX < -0.03f)
            moveState_X = MOVE_STATE_X.LEFT;
        else
        {
            moveState_X = MOVE_STATE_X.STOP;
            speedX = 0;
        }
    }

    // 플레이어 이동 상태에 따른 이동 업데이트
    private void fMoveSpeedUpdate()
    {
        switch (moveState_X)
        {
            case MOVE_STATE_X.LEFT:
                speedX += frictionForce;
                break;
            case MOVE_STATE_X.RIGHT:
                speedX -= frictionForce;
                break;
            case MOVE_STATE_X.STOP:
                break;
        }
    }

    // 플리이어가 점프를 시작할 때 상태 체크
    void fStartJump()
    {
        switch (playerState)
        {
            case PLAYER_STATE.IDEL:    moveState_Y = MOVE_STATE_Y.INPLACE_JUMP; break;
            case PLAYER_STATE.RUN:     moveState_Y = MOVE_STATE_Y.RUN_JUMP; break;
            case PLAYER_STATE.DEADRUN: moveState_Y = MOVE_STATE_Y.DEADRUN_JUMP; break;
        }
        Move = fMove_Jump;
    }
    #endregion



    // 키 입력 처리
    void fInputUpdate()
    {
        // 좌측
        if (input.fGetKey(INPUTSTATE.LEFT))
        {
            // 회전 처리
            if (oldMoveState_X == MOVE_STATE_X.RIGHT && !canTurn)
            {
                canTurn = true;
                CGameManager.instance.inputManager.fStartInputException(INPUT_EXCEPTION_STATE.TURN);
            }

            // 반대키 입력
            if(input.fGetKey(INPUTSTATE.RIGHT))
            {
                speedX = 0;
            }

            //현재 방향 설정
            oldMoveState_X = MOVE_STATE_X.LEFT;
            moveState_X = MOVE_STATE_X.LEFT;

            //가속
            speedX -= accelX;

            // 전력질주 상태가 아니면 달리기 상태
            if(playerState != PLAYER_STATE.DEADRUN)
                playerState = PLAYER_STATE.RUN;

        }

        // 우측
        if (input.fGetKey(INPUTSTATE.RIGHT))
        {
            // 회전 처리
            if (oldMoveState_X == MOVE_STATE_X.LEFT && !canTurn)
            {
                canTurn = true;
                CGameManager.instance.inputManager.fStartInputException(INPUT_EXCEPTION_STATE.TURN);
            }

            // 반대키 입력
            if (input.fGetKey(INPUTSTATE.LEFT))
            {
                speedX = 0;
            }

            //현재 방향 설정
            oldMoveState_X = MOVE_STATE_X.RIGHT;
            moveState_X = MOVE_STATE_X.RIGHT;

            //가속
            speedX += accelX;

            // 전력질주 상태가 아니면 달리기 상태
            if (playerState != PLAYER_STATE.DEADRUN)
                playerState = PLAYER_STATE.RUN;
        }

        // 전력 질주
        if (input.fGetKey(INPUTSTATE.DEADRUN))
        {
            playerState = PLAYER_STATE.DEADRUN;
            curMaxSpeedX = deadrunMaxSpeedX;
        }
        else if (input.fGetKeyUp(INPUTSTATE.DEADRUN))
        {
            playerState = moveState_X != MOVE_STATE_X.STOP ? PLAYER_STATE.RUN : PLAYER_STATE.IDEL;
            curMaxSpeedX = runMaxSpeedX;
        }

        // 점프
        if (input.fGetKeyDown(INPUTSTATE.JUMP))
        {
            if (!isJumpping)
            {
                isJumpping = true;
                canIsJumpping = true;
                fStartJump();
                playerState = PLAYER_STATE.JUMP;
                accelY = maxJumpHeight;
                speedX = 10 * (int)moveState_X;
                
            }
        }

        // 상호작용
        if (InteractObjectTrigger != null)
        {
            if (input.fGetKeyDown(INPUTSTATE.INTERACT))
            {
                InteractObjectTrigger.fStartEvent();
            }
            if (input.fGetKeyUp(INPUTSTATE.INTERACT))
            {
                InteractObjectTrigger.fEndEvent();
            }

        }
    }

    public void aa()
    {
        input.fEndInputException(INPUT_EXCEPTION_STATE.JUMP);
    }

    void fAnimatorUpdate()
    {

        if (canTurn && !isTurnning)
        {
            animator.SetTrigger("Turn");
            canTurn = false;
            isTurnning = true;
        }

        if(canIsJumpping && isJumpping)
        {
            animator.SetTrigger("Jump");
            canIsJumpping = false;
        }

        switch (moveState_X)
        {
            case MOVE_STATE_X.LEFT:
            case MOVE_STATE_X.RIGHT:
                animator.SetBool("Run", true);
                animator.SetFloat("RunSpeed", Mathf.Abs(speedX) / curMaxSpeedX);
                break;
            case MOVE_STATE_X.STOP:
                animator.SetBool("Run", false);
                break;
        }

    }

    public bool fInteractObject(OBJECT_TYPE objectType, bool interact)
    {
        if (interact)
        {
            switch (objectType)
            {
                case OBJECT_TYPE.BOX:
                    curMaxSpeedX = boxMoveMaxSpeedX;
                    Move = fMove_PushBox;
                    return true;
                case OBJECT_TYPE.LADDER:
                    return true;
            }
        }
        else
        {
            curMaxSpeedX = runMaxSpeedX;
            Move = fMove_Run;
            return true;
        }

        return false;
    }

}

#region 이전 캐릭터 스크립트
// inputManager 변경 전
/*
public class CPlayerCtrl : Actor {

    private CharacterController controller;
    private Animator animator;

    public float maxSpeedX = 4;             // x축 최대 속도
    public float maxJumpHeight = 5;         // 점프 높이
    public float frictionForce = 0.07f;     // 마찰력
    public float frictionForce_Object = 0f; // 오브젝트 마찰력
    public float accelX = 0.15f;            // 이동 속도 (가속도)

    private bool lookRight;                 // 바라보는 방향 (오른쪽을 보고 있는지)
    private bool canTurn;                   // 방향전환 가능한지
    public bool turnning { get; set; }

    private bool isJumpping;
    private MOVE_STATE_X moveState_X;
    private float _speedX;
    public float speedX
    {
        get { return _speedX; }
        set { _speedX = ((Mathf.Abs(_speedX) > maxSpeedX) ? (_speedX > 0 ? 1 : -1) * maxSpeedX : value); }
    }
    private float accelY{ get; set;}


    public void fAwake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    public void fStart()
    {
        GameManager.instance.inputManager.actor1 = this;
        isJumpping = false;
        canTurn = false;
        lookRight = true;
    }

    public void fUpdate()
    {
        fMoveLeftRightDirCheck();
        fApplyFrictionForce();
        if (controller.isGrounded)
            isJumpping = false;

        fUpdateAnimator();
    }

    public void fFixedUpdate()
    {
        accelY += Physics.gravity.y * Time.deltaTime;
        controller.Move(new Vector3(speedX, accelY, 0) * Time.deltaTime);
    }


    public void fMoveLeftRightDirCheck()
    {
        if (speedX < -0.05f)
        {
            moveState_X = MOVE_STATE_X.LEFT;
            if (lookRight == true)
                canTurn = true;
            lookRight = false;
        }
        else if (speedX > 0.05f)
        {
            moveState_X = MOVE_STATE_X.RIGHT;
            if (lookRight == false)
                canTurn = true;
            lookRight = true;
        }
        else
            moveState_X = MOVE_STATE_X.STOP;

    }

    private void fApplyFrictionForce()
    {
        switch (moveState_X)
        {
            case MOVE_STATE_X.LEFT:
                speedX += frictionForce + frictionForce_Object;
                break;
            case MOVE_STATE_X.RIGHT:
                speedX -= frictionForce - frictionForce_Object;
                break;
            case MOVE_STATE_X.STOP:
                break;
        }

    }

    #region 키 입력 처리
    public override void MoveLeft()
    {
        //if(!turnning)
            speedX -= accelX;
    }
    public override void MoveRight()
    {
        //if(!turnning)
            speedX += accelX;        
    }
    public override void Jump()
    {
        if (!isJumpping)
        {
            isJumpping = true;
            accelY = maxJumpHeight;
        }
    }
    public override void Interaction()
    {
        
    }
    #endregion

    void fUpdateAnimator()
    {
        
        if(canTurn)
        {
            animator.SetTrigger("Turn");
            turnning = true;
            canTurn = false;
        }

        switch (moveState_X)
        {
            case MOVE_STATE_X.LEFT:
            case MOVE_STATE_X.RIGHT:
                animator.SetBool("Run", true);
                animator.SetFloat("RunSpeed", Mathf.Abs(speedX) / maxSpeedX);
                break;
            case MOVE_STATE_X.STOP:
                animator.SetBool("Run", false);
                break;
        }
        
    }
}
*/
#endregion