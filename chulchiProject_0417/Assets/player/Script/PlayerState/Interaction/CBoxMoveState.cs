using MyDebug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBoxMoveState : CMoveState
{
    PLAYER_VIEW_DIR firstDir;
    public override void fEnterState(CPlayerCtrl3 player)
    {
        base.fEnterState(player);
        firstDir = player.viewDir;
        curAccelSpeed = player.moveValueStruct.boxMoveSpeed;
        curMaxSpeed = player.moveValueStruct.maxBoxMoveSpeed;
        player.animator.SetTrigger("BoxMove");
        player.animator.SetBool("IsBoxMove", true);

    }

    public override CPlayerActionState fUpdateState(CPlayerCtrl3 player)
    {
        if (CInputMamager.instance.fGetKeyDown(INPUTSTATE.JUMP))
        {
            return CPlayerCtrl3.jumpState;
        }

        return base.fUpdateState(player);
    }
    public override void fExitState(CPlayerCtrl3 player)
    {
        base.fExitState(player);

        player.speedX = 0;
        player.animator.SetBool("IsBoxMove", false);
        player.animator.SetFloat("BoxMoveSpeed", 0);
        if (player.viewDir != firstDir)
            player.viewDir = firstDir;
            //player.animator.SetTrigger("Turn");
            //player.transform.Rotate(0, 180, 0);


    }

    protected override void fMoveAnimPlay(CPlayerCtrl3 player)
    {
        player.animator.SetFloat("BoxMoveSpeed", player.speedX/ curMaxSpeed * (int)firstDir);
    }

    protected override void fTurnAnimPlay(CPlayerCtrl3 player, PLAYER_VIEW_DIR oldDir)
    {

    }

    protected override CIdelState fStopState(CPlayerCtrl3 player)
    {
        if (CInputMamager.instance.fGetKeyUp(INPUTSTATE.INTERACT))
        {
            return CPlayerCtrl3.idelState;
        }
        return null;
    }

    public override void fMove_Normal(CPlayerCtrl3 player)
    {
        base.fMove_Normal(player);
        (player.InteractObjectTrigger as CMovableBox).move(player.controller.velocity, player.viewDir);
    }
}