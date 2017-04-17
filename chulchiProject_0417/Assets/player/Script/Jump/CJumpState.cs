using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CJumpState : CInteractionState
{
    float speedX;
    float speedY;
    float playerHeight = 1.45f;

    bool isJumpping;
    bool isHangingJump;
    bool goHanging;
    Vector3 targetPosForWall;
    public override void fEnterState(CPlayerCtrl3 player)
    {
        speedY = 0;
        goHanging = false;
        isHangingJump = false;
        if (player.isPlayerStop)
        {
            //Time.timeScale = 0.5f;
            speedX = 0;
            speedY = player.jumpValueStruct.InplaceJumpSpeedY;
            isJumpping = true;

            if (player.canHanging)
            {
                isHangingJump = true;
                Physics.gravity = Vector3.zero;
                WallHangJumpHegith(player);
                player.animator.SetTrigger("InplaceJump_Hang");
            }  
            else
                player.animator.SetTrigger("InplaceJump");
            
        }
        else if( player.oldAcrionState == CPlayerCtrl3.runState)
        {
            speedX = player.jumpValueStruct.RunJumpPower;
            speedY = player.jumpValueStruct.RunJumpSpeedY;
            isJumpping = true;

            player.animator.SetTrigger("RunJump");
        }


    }

    public override CPlayerActionState fUpdateState(CPlayerCtrl3 player)
    {
        // 레이 : 캐릭터 착지 애니메이션 재생을 위함
        // 캐컨 : 캐릭터 점프시 지형에 따라 밀림 방지
        if(player.isGrounded || player.controller.isGrounded)
        {
            player.animator.SetBool("IsGround", true);
        }
        else
        {
            player.animator.SetBool("IsGround", false);
        }
        
        if(player.canHanging && !isHangingJump)
        {
            //isHangingJump = true;
            //Physics.gravity = Vector3.zero;
            //WallHangJumpHegith(player);
            //player.animator.SetTrigger("InplaceJump_Hang");
        }

        if(goHanging)
        {
            return CPlayerCtrl3.hangState;
        }
      

        
        if(!isJumpping)
        {
            return CPlayerCtrl3.idelState;
        }
         
        return null;
    }

    public override void fExitState(CPlayerCtrl3 player)
    {
       // Time.timeScale = 1f;
    }

    public void fMove_NonMove(CPlayerCtrl3 player){ }
    public void fMove_InplaceJump(CPlayerCtrl3 player)
    {
        speedY += player.jumpValueStruct.customGravity * Time.deltaTime;
        player.ControllerMove(new Vector3(speedX, speedY, 0));
    }
    public void fMove_InplaceHangJump(CPlayerCtrl3 player)
    {
        player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosForWall, Time.deltaTime * 4);
    }
    public void fMove_RunJump(CPlayerCtrl3 player)
    {
        Debug.Log(DebugType.PLAYER_ACTION_STATE, "달리기점프중");
        speedY += player.jumpValueStruct.customGravity * Time.deltaTime;
        player.ControllerMove(new Vector3(speedX * (int) player.viewDir, speedY, 0));

        
    }


    

    public override void fCheckOnStateEnter(ref AnimatorStateInfo stateInfo)
    {
        // 제자리 점프
        if (stateInfo.IsName("Inplace_Jump_Middle"))
        {
            CGameManager.instance.playerCtrl.Move = fMove_InplaceJump;
            
        }
        else if (stateInfo.IsName("Inplace_Jump_End"))
        {
            isJumpping = false;
        }

        // 제자리 매달리기 점프
        if (stateInfo.IsName("Inplace_HangJump_Middle"))
        {
            CGameManager.instance.playerCtrl.Move = fMove_InplaceHangJump;

        }
        else if (stateInfo.IsName("Inplace_HangJump_End"))
        {
            CGameManager.instance.playerCtrl.Move = fMove_NonMove;
            goHanging = true;
        }

        // 달리기 점프
        if (stateInfo.IsName("Run_Jump_Start"))
        {
            CGameManager.instance.playerCtrl.Move = fMove_RunJump;
        }
        else if (stateInfo.IsName("Run_Jump_End"))
        {
            CGameManager.instance.playerCtrl.animator.SetFloat("RunSpeed", 0);
            isJumpping = false;
        }
    }

    public override void fCheckOnStateExit(ref AnimatorStateInfo stateInfo)
    {

    }

    void WallHangJumpHegith(CPlayerCtrl3 player)
    {
        targetPosForWall = player.transform.position;
        targetPosForWall.y = (player.wallTr.position.y + (player.wallTr.lossyScale.y * 0.5f)) - playerHeight;
        if(player.viewDir == PLAYER_VIEW_DIR.LEFT)
        {
            targetPosForWall.x = (player.wallTr.position.x + (player.wallTr.lossyScale.x * 0.5f)) + 0.4f;
        }
        else
        {
            targetPosForWall.x = (player.wallTr.position.x - (player.wallTr.lossyScale.x * 0.5f)) - 0.4f;
        }
    }
}

