using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CInteractionState : CPlayerActionState
{
    OBJECT_TYPE objType;
    private static CMoveState boxMoveState = new CBoxMoveState();
    private CPlayerActionState curInteractState;

    public override void fEnterState(CPlayerCtrl3 player)
    {
        player.InteractObjectTrigger.fStartEvent();
        
        switch (objType)
        {
            case OBJECT_TYPE.BOX: curInteractState = boxMoveState;  break;
        }

        curInteractState.fEnterState(player);

    }

    public override CPlayerActionState fUpdateState(CPlayerCtrl3 player)
    {
        return curInteractState.fUpdateState(player);
    }

    public override void fExitState(CPlayerCtrl3 player)
    {
        Debug.Log(DebugType.PLAYER_ACTION_STATE, "인터렉트 끝");
        curInteractState.fExitState(player);
        player.InteractObjectTrigger.fEndEvent();
    }

    public override void fCheckOnStateExit(ref AnimatorStateInfo stateInfo)
    {

    }

    public override void fCheckOnStateEnter(ref AnimatorStateInfo stateInfo)
    {
        
    }

    public bool fInteractObject(OBJECT_TYPE objectType, bool interact)
    {
        if (interact)
        {
            switch (objectType)
            {
                case OBJECT_TYPE.BOX:
                    objType = objectType;
                    return true;
                case OBJECT_TYPE.LADDER:
                    return true;
            }
        }
        else
        {
            return true;
        }

        return false;
    }

    public void fMove_PushBox(CPlayerCtrl3 player)
    {

    }
}
