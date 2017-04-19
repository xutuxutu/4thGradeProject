using MyDebug;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


 public enum DebugType
 {
     ETC = 1 << 0,
     PLAYER_ACTION_STATE = 1 << 1,
     OBJECT_STATE = 1 << 2,
 }

public class CGameManager : MonoBehaviour
{

    private static CGameManager singleton;
    public static CGameManager instance { get { return singleton; } private set { singleton = value; } }

    public CAudioManager audioManager { get; private set; }
    public CInputMamager inputManager { get; private set; }
    public CEventManager eventManager { get; private set; }
    public CMainCameraCtrl cameraCtrl { get; private set;}
    public CPlayerCtrl3 playerCtrl;
    private CDebug debug;
    private CTimer timer;
    private void Awake()
    {
        singleton = this;

        audioManager = gameObject.AddComponent<CAudioManager>();
        inputManager = gameObject.AddComponent<CInputMamager>();
        eventManager = gameObject.AddComponent<CEventManager>();
        cameraCtrl = Camera.main.GetComponent<CMainCameraCtrl>();

        debug = new CDebug(DebugType.PLAYER_ACTION_STATE, DebugType.OBJECT_STATE);
        timer = new CTimer();

        audioManager.fAwake();
        inputManager.fAwake();
        eventManager.fAwake();

        playerCtrl.fAwake();
        cameraCtrl.fAwake();
    }

    private void Start()
    {
        audioManager.fStart();
        inputManager.fStart();
        timer.fStart();
    }

    private void Update()
    {
        inputManager.fUpdate();
        playerCtrl.fUpdate();
        timer.fUpdate();
    }

    private void LateUpdate()
    {
        cameraCtrl.fLateUpdate();
    }
}
