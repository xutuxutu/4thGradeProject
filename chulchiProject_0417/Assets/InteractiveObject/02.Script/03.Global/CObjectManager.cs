using System.Collections.Generic;

public class CObjectManager
{
    private int objectNum;
    private Dictionary<int, IInteractiveObject> objectList = new Dictionary<int, IInteractiveObject>();

    public readonly static CObjectManager instance = new CObjectManager();

    public int fAddObjectList(IInteractiveObject _object)
    {
        objectList.Add(objectNum, _object);
        ++objectNum;

        return objectNum - 1;
    }

    public float fObjectDistance(OBJECT_TYPE object_Type)
    {
        switch (object_Type)
        {
            case OBJECT_TYPE.DOOR:  return 2f;
            case OBJECT_TYPE.BOX:   return 2f;
            default: return -9999;
        }
    }

    public OBJECT_TYPE fChackPriority(OBJECT_TYPE object_Type, OBJECT_TYPE object_Type2)
    {
        return fGetPriority(object_Type) > fGetPriority(object_Type2) ? object_Type : object_Type2;
    }

    private int fGetPriority(OBJECT_TYPE object_Type)
    {
        // 우선순위 (숫자가 높을 수록 순위가 높음)
        switch (object_Type)
        {
            case OBJECT_TYPE.DOOR: return 1;
            case OBJECT_TYPE.BOX: return 2;
            default: return -9999;
        }
    }

    public void fSendObjectEventStart(int gID) { objectList[gID].fStartEvent(); }
    public void fSendObjectEventEnd(int gID) { objectList[gID].fEndEvent(); }

    public OBJECT_TYPE fFindObjectType(int gID) { return objectList[gID].objectType; }
    public IInteractiveObject fFindObject(int gID) { return objectList[gID]; }
}
