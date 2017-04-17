using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CHangState : CInteractionState
{
    bool isEndedHangState;
    bool isActing;
    float playerHeight = 1.8f;
    float targetY;
    Vector3 targetPos;
    public override void fEnterState(CPlayerCtrl3 player)
    {
        isActing = false;
        isEndedHangState = false;
        Physics.gravity = Vector3.zero;         // 캐릭터 컨트롤러를 박으면 기본적으로 중력 영향을 받기 때문에
    
    }
    

    public override CPlayerActionState fUpdateState(CPlayerCtrl3 player)
    {
        if(CGameManager.instance.inputManager.fGetKeyDown(INPUTSTATE.DOWN) && !isActing)
        {
            isActing = true;
            player.animator.SetTrigger("HangDown");
        }
        else if(CGameManager.instance.inputManager.fGetKeyDown(INPUTSTATE.UP) && !isActing)
        {
            isActing = true;
            player.animator.SetTrigger("HangUp");
        }

        if (player.isGrounded)
        {
            player.animator.SetBool("IsGround", true);
        }
        else
        {
            player.animator.SetBool("IsGround", false);
        }

        if(isEndedHangState)
        {
            return CPlayerCtrl3.idelState;
        }

        return null;
    }

    public override void fExitState(CPlayerCtrl3 player)
    {
        Physics.gravity = new Vector3(0, -9.81f, 0);
    }


    public override void fCheckOnStateEnter(ref AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName("BoxDown_Middle"))
        {
            CGameManager.instance.playerCtrl.Move = fMove_HangDown;

            Physics.gravity = new Vector3(0, -9.81f, 0);
        }
        else if (stateInfo.IsName("BoxDown_End"))
        {
            isEndedHangState = true;
        }

    }

    public override void fCheckOnStateExit(ref AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName("HangUp"))
        {
            isEndedHangState = true;
        }
    }

    public void fMove_HangDown(CPlayerCtrl3 player){   }
    
}
