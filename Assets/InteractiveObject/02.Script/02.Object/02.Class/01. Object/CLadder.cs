using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLadder : CInteractiveObject
{

    public void Awake()
    {
        fInit();
    }

    public new void fInit()
    {
        base.fInit();
        objectType = OBJECT_TYPE.LADDER;
    }

    public override bool fStartEvent()
    {
        if (objectActive && !objectInteract)
        {
            if (CPlayerCtrl3.interactionState.fInteractObject(objectType, true))
            {
                objectInteract = true;
                return true;
            }
        }
        return false;
    }
    public override bool fEndEvent()
    {
        if (objectActive && objectInteract)
        {
            if (CPlayerCtrl3.interactionState.fInteractObject(objectType, true))
            {
                objectInteract = false;
                return true;
            }
        }
        return false;
    }
}
