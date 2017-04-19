using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMoveState : CInteractionState
{
    protected float curAccelSpeed;
    protected float curMaxSpeed;
    protected bool isLeftKeyLastInput;

    public override void fEnterState(CPlayerCtrl3 player)
    {
        player.Move = fMove_Normal;
        curMaxSpeed = player.moveValueStruct.maxRunSpeed;
        isLeftKeyLastInput = player.fIsInputKey(INPUTSTATE.LEFT);
    }

    public override CPlayerActionState fUpdateState(CPlayerCtrl3 player)
    {
        //if (player.fIsInputKey(INPUTSTATE.LEFT) || (player.fIsInputKey(INPUTSTATE.RIGHT)))
        if(CInputMamager.instance.fGetKey(INPUTSTATE.LEFT) || CInputMamager.instance.fGetKey(INPUTSTATE.RIGHT))
        {
            fMoveInputUpdate(player);
            fAccelSpeed(player);
        }

        fMoveAnimPlay(player);

        return fStopState(player);
    }

    public override void fExitState(CPlayerCtrl3 player) { }


    private void fMoveInputUpdate(CPlayerCtrl3 player)
    {
        PLAYER_VIEW_DIR oldDir = player.viewDir;

        if (CInputMamager.instance.fGetKey(INPUTSTATE.LEFT) && CInputMamager.instance.fGetKey(INPUTSTATE.RIGHT))
        {
            if (isLeftKeyLastInput)
                player.viewDir = PLAYER_VIEW_DIR.RIGHT;
            else
                player.viewDir = PLAYER_VIEW_DIR.LEFT;
        }
        else
        {
            if (CInputMamager.instance.fGetKey(INPUTSTATE.LEFT))
            {
                player.viewDir = PLAYER_VIEW_DIR.LEFT;
                isLeftKeyLastInput = true;


            }

            if (CInputMamager.instance.fGetKey(INPUTSTATE.RIGHT))
            {
                isLeftKeyLastInput = false;
                player.viewDir = PLAYER_VIEW_DIR.RIGHT;


            }
        }

        fTurnAnimPlay(player, oldDir);
    }

    private void fAccelSpeed(CPlayerCtrl3 player)
    {
        switch (player.viewDir)
        {
            case PLAYER_VIEW_DIR.LEFT: player.fMoveToTargetReturnReviseSpeed(-curMaxSpeed, curAccelSpeed); break;
            case PLAYER_VIEW_DIR.RIGHT: player.fMoveToTargetReturnReviseSpeed(curMaxSpeed, curAccelSpeed); break;
        }
    }

    protected virtual void fMoveAnimPlay(CPlayerCtrl3 player) { }

    protected virtual void fTurnAnimPlay(CPlayerCtrl3 player, PLAYER_VIEW_DIR oldDir) { }

    protected virtual CIdelState fStopState(CPlayerCtrl3 player) { return null; }

    public virtual void fMove_Normal(CPlayerCtrl3 player)
    {
        //        player.controller.Move(new Vector3(player.speedX, Physics.gravity.y, 0) * Time.deltaTime);
        player.ControllerMove(new Vector3(player.speedX, Physics.gravity.y, 0));
    }

}
