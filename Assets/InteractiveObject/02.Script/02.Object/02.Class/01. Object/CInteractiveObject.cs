using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CInteractiveObject : MonoBehaviour, IInteractiveObject
{
    public OBJECT_TYPE objectType
    {
        get;
        protected set;
    }

    public int groupID
    {
        get;
        private set;
    }

    public bool objectActive
    {
        get;
        set;
    }

    public bool objectInteract
    {
        get;

        protected set;
    }

    public void fInit()
    {
        objectActive = true;
        objectInteract = false;
        groupID = CObjectManager.instance.fAddObjectList(this);
    }

    public abstract bool fStartEvent();
    public abstract bool fEndEvent();
}
