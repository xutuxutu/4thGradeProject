public enum OBJECT_TYPE { DOOR, BOX, LADDER }

public interface IInteractiveObject
{
    OBJECT_TYPE objectType
    {
        get;
    }

    bool objectActive
    {
        get;
        set;
    }

    void fInit();
    bool fStartEvent();
    bool fEndEvent();
}
