using System;
using UnityEngine;

/*
 IDEL,       
 MOVE,       
 RELODE,     
 SNEAK,      
 HIDE,       
 SHOOT,      
 INTERACTION,
 RESTORE,    
     */

public class CIdelState : CPlayerActionState
{

    public override void fEnterState(CPlayerCtrl3 player)
    {
        player.Move = fMove_NonMove;
        player.animator.SetBool("IsGround", true);
    }

    public override CPlayerActionState fUpdateState(CPlayerCtrl3 player)
    {
        if (player.fIsInputKey(INPUTSTATE.LEFT) || player.fIsInputKey(INPUTSTATE.RIGHT))
            return CPlayerCtrl3.runState;
        else if (CInputMamager.instance.fGetKeyDown(INPUTSTATE.JUMP))
                return CPlayerCtrl3.jumpState;
        else if (CInputMamager.instance.fGetKeyDown(INPUTSTATE.INTERACT) && player.InteractObjectTrigger != null)
            return CPlayerCtrl3.interactionState;
        else
        {
            Debug.Log(DebugType.PLAYER_ACTION_STATE, "대기 중");
            return null;
        }
    }

    public override void fExitState(CPlayerCtrl3 player)
    {

    }


    public override void fCheckOnStateExit(ref AnimatorStateInfo stateInfo)
    {

    }

    public override void fCheckOnStateEnter(ref AnimatorStateInfo stateInfo)
    {
        
    }

    public void fMove_NonMove(CPlayerCtrl3 player)
    {
        player.controller.Move(new Vector3(player.speedX, 0, 0) * Time.deltaTime);
    }

}