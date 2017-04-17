using UnityEngine;

public enum DOOR_STATE { OPEN, CLOSE, OPENING, CLOSING }

public abstract class CDoor : MonoBehaviour, IInteractiveObject
{
    public OBJECT_TYPE objectType
    {
        get;
        private set;
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

    public DOOR_STATE doorState
    {
        get;
        protected set;
    }

    public void fInit()
    {
        objectActive = true;
        objectType = OBJECT_TYPE.DOOR;
        doorState = DOOR_STATE.CLOSE;

        groupID = CObjectManager.instance.fAddObjectList(this);
    }

    public abstract bool fStartEvent();
    public abstract bool fEndEvent();

    public abstract void fOpenDoor();
    public abstract void fCloseDoor();
}
