using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PLAYER_STATE2
{
    NULL,
    IDEL,
    RUN,
    TURN,
    DEADRUN,
    JUMP,
}

public enum PLAYER_JUMP_STATE
{
    INPLACE,
    RUN,
    DEADRUN,
}

public enum PLAYER_VIEW_DIR
{
    LEFT = -1,
    RIGHT = 1,
}

public enum PLAYER_MOVE_DIR
{
    NONEMOVE = 0,
    LEFT = -1,
    RIGHT = 1,
}

public struct PlayerState
{
    public PLAYER_STATE2 actionState;
    public PLAYER_JUMP_STATE jumpState;
    public PLAYER_VIEW_DIR viewDir;
    public PLAYER_MOVE_DIR moveDir;
    public INPUTSTATE inputState;
}

public class CPlayerCtrl2 : MonoBehaviour {


    private delegate void voidFunction();
    private voidFunction Move;
    private CharacterController controller;
    private CInputMamager input;
    private PlayerState playerState;
    private Animator animator;
    private Dictionary<PLAYER_STATE2, PLAYER_STATE2> oldActionState;

    private float speedY;
    private float speedX;
    private float accelX = 0.15f;
    private float frictionForce = 0.09f;
    private float runMaxSpeed = 3;
    private float deadrunMaxSpeed = 5;
    private float jumpPower = 5;
    private float groundCheckDistance = 0.15f;
    private float nearGroundCheckDistance = 0.6f;


    private bool isGrounded;
    private bool isNearGround;
    private Vector3 groundNormal;

    public void fAwake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        input = CGameManager.instance.inputManager;
        oldActionState = new Dictionary<PLAYER_STATE2, PLAYER_STATE2>();
    }
	public void fStart()
    {
        playerState.actionState = PLAYER_STATE2.IDEL;
        playerState.viewDir = PLAYER_VIEW_DIR.RIGHT;
        playerState.moveDir = PLAYER_MOVE_DIR.NONEMOVE;
        playerState.inputState = INPUTSTATE.NONE;
        foreach(PLAYER_STATE2 oldState in Enum.GetValues(typeof(PLAYER_STATE2)) as PLAYER_STATE2[])
        {
            oldActionState.Add(oldState, PLAYER_STATE2.NULL);
        }

        Move = fMove_Run;
        speedX = 0;
    }
    public void fUpdate()
    {
        fInGroundUpdate();
        fInputUpdate();
        CheckGroundStatus(); 
        fSpeedX_Update();
        Move();
        fAnimatorUpdate();
        //Debug.Log(playerState.actionState);
        //Debug.Log(playerState.actionState + " : "+ speedX);
        //Debug.Log((playerState.inputState & INPUTSTATE.LEFT) == INPUTSTATE.LEFT);
        //Debug.Log(playerState.viewDir);
    }


    private void fMove_Run()
    {
        speedY = Physics.gravity.y * Time.deltaTime;
        controller.Move(new Vector3(speedX,speedY,0) * Time.deltaTime);
    }

    private void fMove_Jump()
    {
        speedY += Physics.gravity.y * Time.deltaTime;
        switch (playerState.jumpState)
        {
            case PLAYER_JUMP_STATE.INPLACE:
                controller.Move(new Vector3(0, speedY, 0) * Time.deltaTime);
                break;
            case PLAYER_JUMP_STATE.RUN:
                controller.Move(new Vector3(5 * (int)playerState.moveDir, speedY, 0) * Time.deltaTime);
                break;
            case PLAYER_JUMP_STATE.DEADRUN:
                controller.Move(new Vector3(7 * (int)playerState.moveDir, speedY, 0) * Time.deltaTime);
                break;
        }
        
    }



    #region 입력 처리
    private void fInputUpdate()
    {
        fLeftKeyDown();
        fRightKeyDown();
        fDeadRunKeyDown();
        fJumpKeyDown();
    }

    private void fLeftKeyDown()
    {
        if (input.fGetKeyDown(INPUTSTATE.LEFT))
        {
            playerState.inputState |= INPUTSTATE.LEFT;

            // 전력 질주 키가 눌렸는가 체크
            if (((playerState.inputState & INPUTSTATE.DEADRUN) != INPUTSTATE.DEADRUN))
                playerState.actionState = PLAYER_STATE2.RUN;
            else
                playerState.actionState = PLAYER_STATE2.DEADRUN;

            // 방향 체크
            playerState.viewDir = PLAYER_VIEW_DIR.LEFT;
            playerState.moveDir = PLAYER_MOVE_DIR.RIGHT;
            if (input.fGetKeyDown(INPUTSTATE.RIGHT))
            {
                playerState.viewDir = PLAYER_VIEW_DIR.RIGHT;
                playerState.moveDir = PLAYER_MOVE_DIR.LEFT;
            }

        }

        else if(input.fGetKeyUp(INPUTSTATE.LEFT))
        {
            playerState.inputState ^= INPUTSTATE.LEFT;

            if (!input.fGetKey(INPUTSTATE.RIGHT))
                playerState.actionState = PLAYER_STATE2.IDEL;
            else
            {
                playerState.viewDir = PLAYER_VIEW_DIR.RIGHT;
                playerState.moveDir = PLAYER_MOVE_DIR.RIGHT;
            }

        }
    }

    private void fRightKeyDown()
    {
        if (input.fGetKeyDown(INPUTSTATE.RIGHT))
        {
            playerState.inputState |= INPUTSTATE.RIGHT;

            // 전력 질주 키가 눌렸는가 체크
            if (((playerState.inputState & INPUTSTATE.DEADRUN) != INPUTSTATE.DEADRUN))
                playerState.actionState = PLAYER_STATE2.RUN;
            else
                playerState.actionState = PLAYER_STATE2.DEADRUN;

            // 방향 체크
            playerState.viewDir = PLAYER_VIEW_DIR.RIGHT;
            if (input.fGetKeyDown(INPUTSTATE.LEFT))
                playerState.viewDir = PLAYER_VIEW_DIR.LEFT;

        }

        else if (input.fGetKeyUp(INPUTSTATE.RIGHT))
        {
            playerState.inputState ^= INPUTSTATE.RIGHT;

            if (!input.fGetKey(INPUTSTATE.LEFT))
                playerState.actionState = PLAYER_STATE2.IDEL;
            else
                playerState.viewDir = PLAYER_VIEW_DIR.LEFT;

        }
    }

    
    private void fDeadRunKeyDown()
    {
        if (input.fGetKeyDown(INPUTSTATE.DEADRUN))
        {
            playerState.inputState |= INPUTSTATE.DEADRUN;

            switch (playerState.actionState)
            {
                case PLAYER_STATE2.IDEL:
                    break;
                case PLAYER_STATE2.RUN:
                    playerState.actionState = PLAYER_STATE2.DEADRUN;
                    break;
                case PLAYER_STATE2.DEADRUN:
                    break;
                case PLAYER_STATE2.JUMP:
                    break;
            }
        }
        else if(input.fGetKeyUp(INPUTSTATE.DEADRUN))
        {
            playerState.inputState ^= INPUTSTATE.DEADRUN;
            if(((playerState.inputState & INPUTSTATE.LEFT) == INPUTSTATE.LEFT) ||((playerState.inputState & INPUTSTATE.RIGHT) == INPUTSTATE.RIGHT))
                playerState.actionState = PLAYER_STATE2.RUN;
        }

    }

    private void fJumpKeyDown()
    {
        if (input.fGetKeyDown(INPUTSTATE.JUMP))
        {
            if(playerState.actionState != PLAYER_STATE2.JUMP)
            {
                
                switch (playerState.actionState)
                {
                    case PLAYER_STATE2.IDEL:
                        playerState.jumpState = PLAYER_JUMP_STATE.INPLACE;
                        break;
                    case PLAYER_STATE2.RUN:
                        playerState.jumpState = PLAYER_JUMP_STATE.RUN;
                        break;
                    case PLAYER_STATE2.DEADRUN:
                        playerState.jumpState = PLAYER_JUMP_STATE.DEADRUN;
                        break;
                }
                //input.fStartInputException(INPUT_EXCEPTION_STATE.JUMP);
                Move = fMove_Jump;
                playerState.actionState = PLAYER_STATE2.JUMP;
                speedY = jumpPower;
                speedX = 0;
            }
        }
        else if(input.fGetKeyUp(INPUTSTATE.JUMP))
        {
            if (((playerState.inputState & INPUTSTATE.LEFT) == INPUTSTATE.LEFT) || ((playerState.inputState & INPUTSTATE.RIGHT) == INPUTSTATE.RIGHT))
                playerState.actionState = PLAYER_STATE2.RUN;
        }
    }
    #endregion



    private void fSpeedX_Update()
    {
        switch (playerState.actionState)
        {
            case PLAYER_STATE2.IDEL:
                break;
            case PLAYER_STATE2.RUN:
                if (playerState.moveDir == PLAYER_MOVE_DIR.RIGHT)
                    speedX = fMoveToTargetReturnReviseSpeed(speedX, runMaxSpeed, accelX);
                else
                    speedX = fMoveToTargetReturnReviseSpeed(speedX, -runMaxSpeed, accelX);

                break;
            case PLAYER_STATE2.DEADRUN:
                if (playerState.moveDir == PLAYER_MOVE_DIR.RIGHT)
                    speedX = fMoveToTargetReturnReviseSpeed(speedX, deadrunMaxSpeed, accelX);
                else
                    speedX = fMoveToTargetReturnReviseSpeed(speedX, -deadrunMaxSpeed, accelX);

                break;
            case PLAYER_STATE2.JUMP:
                break;
        }

        fSpeedOfSpeedToZero();
    }


    public float fAccelSpeed(float initSpeed, float maxSpeed, float accel)
    {
        if (initSpeed == maxSpeed)
            return initSpeed;
        else
        {
            initSpeed += accel;
            return (Mathf.Abs(maxSpeed) > initSpeed) ? initSpeed : maxSpeed;
        }
    }

    // 속도를 0으로 맞춤 ( 마찰력 )
    private void fSpeedOfSpeedToZero()
    {
        if (speedX > 0.05f || speedX < -0.05f)
        {
            speedX = fMoveToTargetReturnReviseSpeed(speedX, 0, frictionForce);
        }
        else
        {
            speedX = 0;
        }
    }

    private void fInGroundUpdate()
    {
        if(controller.isGrounded)
        {
            Move = fMove_Run;
            if (playerState.actionState == PLAYER_STATE2.JUMP)
            {
                playerState.actionState = PLAYER_STATE2.IDEL;
            }
        }

        if(isNearGround)
        {
            if (controller.velocity.y < 1f)
            {
                //input.fEndInputException(INPUT_EXCEPTION_STATE.JUMP);
            }
        }
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;

        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f) + (Vector3.right * 0.1f), transform.position + (Vector3.right * 0.1f) +  (Vector3.up * 0.1f) + (Vector3.down * nearGroundCheckDistance),Color.yellow);

        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance))
        {
            isNearGround = false;
            isGrounded = true;
            groundNormal = hitInfo.normal;
        }
        else if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, nearGroundCheckDistance))
        {
            isNearGround = true;
        }
        else
        {
            isNearGround = false;
            isGrounded = false;
            groundNormal = Vector3.up;
        }

    }

    private float fMoveToTargetReturnReviseSpeed(float initial, float target, float variation)
    {

        if(initial >= target)
        {
            initial -= variation;
            if (initial < target)
                initial = target;
        }
        else
        {
            initial += variation;
            if (initial > target)
                initial = target;
        }
        return initial;
        
    }

    void fAnimatorUpdate()
    {

        switch (playerState.actionState)
        {
            case PLAYER_STATE2.IDEL:
                break;
            case PLAYER_STATE2.RUN:

                switch (playerState.moveDir)
                {
                    case PLAYER_MOVE_DIR.LEFT:
                    case PLAYER_MOVE_DIR.RIGHT:
                        animator.SetBool("Run", true);
                        animator.SetFloat("RunSpeed", Mathf.Abs(speedX) / runMaxSpeed);
                        break;
                    case PLAYER_MOVE_DIR.NONEMOVE:
                        animator.SetBool("Run", false);
                        break;
                }

                break;
            case PLAYER_STATE2.TURN:
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("RunJump"))
                {
                    animator.SetTrigger("Turn");
                }
                break;
            case PLAYER_STATE2.DEADRUN:
                break;
            case PLAYER_STATE2.JUMP:
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
                {
                    animator.SetTrigger("Jump");
                }
                break;
        }
        

    }
}
