using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGameManager : MonoBehaviour
{

    private static CGameManager singleton;
    public static CGameManager instance { get { return singleton; } private set { singleton = value; } }

    public CAudioManager audioManager { get; private set; }
    public CInputMamager inputManager { get; private set; }
    public CEventManager eventManager { get; private set; }
    public CMainCameraCtrl cameraCtrl { get; private set;}
    public CPlayerCtrl3 playerCtrl;
    private Debug debug;
    private CTimer timer;
    private void Awake()
    {
        singleton = this;

        audioManager = gameObject.AddComponent<CAudioManager>();
        inputManager = gameObject.AddComponent<CInputMamager>();
        eventManager = gameObject.AddComponent<CEventManager>();
        cameraCtrl = Camera.main.GetComponent<CMainCameraCtrl>();

        debug = new Debug();
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
