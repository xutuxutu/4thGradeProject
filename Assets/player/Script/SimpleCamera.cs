using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCamera : MonoBehaviour {

    public Transform playerTr;

    Transform tr;         // 현재 카메라 Transform
    Vector3 camAddPos;    // 플레이어와 카메라의 벡터값

    float camAddPosX;
    float camAddPosY;
    float camAddPosUpY;
    float camAddPosDownY;

    float curYspeed;
    float val = 20f;
    float orignYspeed = 5f;
    float upYspeed = 1f;
    float downYspeed = 20f;

    void Start()
    {
        tr = transform;
        camAddPos = tr.position - playerTr.position;
        camAddPosY = camAddPos.y;
        camAddPosUpY = camAddPos.y + 0.8f;
        camAddPosDownY = camAddPos.y - 1f;
    }

    Vector3 tempPos;
    void Update()
    {
        float tempSpeed = Vector3.Distance(tr.position, playerTr.position + camAddPos) * 2.5f;
        tempPos = Vector3.Lerp(tr.position, playerTr.position + camAddPos, tempSpeed * Time.deltaTime);
        tempPos.y = Mathf.Lerp(tr.position.y, playerTr.position.y + camAddPos.y, curYspeed * Time.deltaTime);
        tr.position = tempPos;
    }
}
