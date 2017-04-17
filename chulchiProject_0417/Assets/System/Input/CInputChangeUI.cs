using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class CInputChangeUI : MonoBehaviour {

    public void ChangeKeyboardButton(GameObject obj)
    {
        string keyName = obj.GetComponentInChildren<Text>().text;
        obj.GetComponentInChildren<Text>().text = "";
        StartCoroutine(CGameManager.instance.inputManager.fReSettingKey(obj, true, keyName));
    }

    public void ChangeJoypadButton(GameObject obj)
    {
        string keyName = obj.GetComponentInChildren<Text>().text;
        obj.GetComponentInChildren<Text>().text = "";
        StartCoroutine(CGameManager.instance.inputManager.fReSettingKey(obj, false, keyName));
    }

    public void ChangeButtonText(GameObject obj, string keyName)
    {
        obj.GetComponentInChildren<Text>().text = keyName;
    }
}
