using UnityEngine;


public class CRunState : CMoveState
{

    public override void fEnterState(CPlayerCtrl3 player)
    {
        base.fEnterState(player);
        curAccelSpeed = player.moveValueStruct.runSpeed;
        curMaxSpeed = player.moveValueStruct.maxRunSpeed;
        player.animator.SetBool("Run", true);
    }

    public override CPlayerActionState fUpdateState(CPlayerCtrl3 player)
    {
        
        if (CInputMamager.instance.fGetKeyDown(INPUTSTATE.JUMP))
        {
            return CPlayerCtrl3.jumpState;
        }
        else if (CInputMamager.instance.fGetKeyDown(INPUTSTATE.INTERACT) && player.InteractObjectTrigger != null)
        {
            return CPlayerCtrl3.interactionState;
        }

        return base.fUpdateState(player);
    }
    public override void fExitState(CPlayerCtrl3 player)
    {
        base.fExitState(player);
        player.speedX = 0;
        player.animator.SetBool("Run", false);
        player.animator.SetFloat("RunSpeed", 0);
    }

    protected override CIdelState fStopState(CPlayerCtrl3 player)
    {
        if (!(player.fIsInputKey(INPUTSTATE.LEFT) || (player.fIsInputKey(INPUTSTATE.RIGHT))) && player.isPlayerStop)
        {
            return CPlayerCtrl3.idelState;
        }
        return null;
    }

    protected override void fMoveAnimPlay(CPlayerCtrl3 player)
    {
        player.animator.SetFloat("RunSpeed", Mathf.Abs(player.speedX) / player.moveValueStruct.maxRunSpeed);
    }

    protected override void fTurnAnimPlay(CPlayerCtrl3 player, PLAYER_VIEW_DIR oldDir)
    {
        if (oldDir != player.viewDir)
            player.animator.SetTrigger("Turn");
    }
}

/*
public class CRunState : CInteractionState
{
    public float curMaxSpeed;
    private bool isLeftKeyLastInput;

    public override void fEnterState(CPlayerCtrl3 player)
    {
        player.Move = fMove_Normal;
        curMaxSpeed = player.moveValueStruct.maxRunSpeed;
        isLeftKeyLastInput = player.fIsInputKey(INPUTSTATE.LEFT);
        player.animator.SetBool("Run", true);
    }

    public override CPlayerActionState fUpdateState(CPlayerCtrl3 player)
    {

        if (player.fIsInputKey(INPUTSTATE.LEFT) || (player.fIsInputKey(INPUTSTATE.RIGHT)))
        {
            if (player.fIsInputKey(INPUTSTATE.DEADRUN))
            {
                curMaxSpeed = player.moveValueStruct.maxDeadrunSpeed;
            }
            else
            {
                curMaxSpeed = player.moveValueStruct.maxRunSpeed;
            }

            fRunInputUpdate(player);
            fAccelSpeed(player);

        }
        else
        {
            if (player.isPlayerStop)
                return CPlayerCtrl3.idelState;
        }
        

        if (CInputMamager.instance.fGetKeyDown(INPUTSTATE.JUMP))
        {
            return CPlayerCtrl3.jumpState;
        }


        fRunAnimPlay(player);
        return null;
    }
    public override void fExitState(CPlayerCtrl3 player)
    {
        player.speedX = 0;
        player.animator.SetBool("Run", false);
    }


    private void fRunInputUpdate(CPlayerCtrl3 player)
    {
        PLAYER_VIEW_DIR oldDir = player.viewDir;

        if (player.fIsInputKey(INPUTSTATE.LEFT) && (player.fIsInputKey(INPUTSTATE.RIGHT)))
        {
            if(isLeftKeyLastInput)
                player.viewDir = PLAYER_VIEW_DIR.RIGHT;
            else
                player.viewDir = PLAYER_VIEW_DIR.LEFT;
        }
        else
        {
            if (player.fIsInputKey(INPUTSTATE.LEFT))
            {
                player.viewDir = PLAYER_VIEW_DIR.LEFT;
                isLeftKeyLastInput = true;

                
            }

            if (player.fIsInputKey(INPUTSTATE.RIGHT))
            {
                isLeftKeyLastInput = false;
                player.viewDir = PLAYER_VIEW_DIR.RIGHT;


            }
        }

        if (oldDir != player.viewDir)
            player.animator.SetTrigger("Turn");
    }

    private void fAccelSpeed(CPlayerCtrl3 player)
    {
        switch (player.viewDir)
        {
            case PLAYER_VIEW_DIR.LEFT: player.fMoveToTargetReturnReviseSpeed(-curMaxSpeed, player.moveValueStruct.runSpeed); break;
            case PLAYER_VIEW_DIR.RIGHT: player.fMoveToTargetReturnReviseSpeed(curMaxSpeed, player.moveValueStruct.runSpeed); break;
        }
    }

    private void fRunAnimPlay(CPlayerCtrl3 player)
    {
        //나중에 전력질주 애니메이션이 추가 되면 아래 주석된 코드로 바꾸자
        //player.animator.SetFloat("RunSpeed", Mathf.Abs(player.speedX) / player.speedStruct.maxDeadrunSpeed);

        player.animator.SetFloat("RunSpeed", Mathf.Abs(player.speedX) / player.moveValueStruct.maxRunSpeed);
    }
    public void fMove_Normal(CPlayerCtrl3 player)
    {
        player.controller.Move(new Vector3(player.speedX, 0, 0) * Time.deltaTime);
    }
}
*/
