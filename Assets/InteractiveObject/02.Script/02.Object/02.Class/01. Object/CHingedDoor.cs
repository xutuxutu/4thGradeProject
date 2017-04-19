using UnityEngine;

public enum OPEN_DIR { LEFT = 1, RIGHT }

public class CHingedDoor : CDoor
{
    public OPEN_DIR openDirection;
    private Animator animCtrl;

    public void Awake()
    {
        fInit();
        animCtrl = GetComponent<Animator>();
    }

    public override bool fStartEvent()
    {
        if(objectActive)
        {
            switch(doorState)
            {
                case DOOR_STATE.CLOSE :
                    fOpenDoor();
                    return true;
                default :
                    return false;
            }
        }
        return false;
    }
    public override bool fEndEvent()
    {
        /*
        if (objectActive)
        {
            switch (doorState)
            {
                case DOOR_STATE.OPENING :
                    doorState = DOOR_STATE.OPEN;
                    break;
                case DOOR_STATE.CLOSING :
                    doorState = DOOR_STATE.CLOSE;
                    break;
                default :
                    return false;
            }
            return true;
        }*/
        return false;
    }

    public override void fOpenDoor()
    {
        doorState = DOOR_STATE.OPENING;
        animCtrl.SetInteger("open", (int)openDirection);
    }
    public override void fCloseDoor()
    {
        doorState = DOOR_STATE.CLOSING;
        animCtrl.SetBool("isOpen", false);
    }
}
