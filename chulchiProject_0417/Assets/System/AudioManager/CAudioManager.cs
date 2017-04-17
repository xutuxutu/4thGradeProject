using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 오디오 상태 (종류)
public enum AudioState
{
    MP3_Background_1,
    MP3_Background_2,
    MP3_Tick_1
}

public class CAudioManager : MonoBehaviour
{
    public CObjectPool<CPoolableObject> audioPool;            // 오디오 풀
    public Dictionary<AudioState,AudioClip> soundList;      // 사운드 목록

    public void fAwake()
    {
        audioPool = new CObjectPool<CPoolableObject>();
        soundList = new Dictionary<AudioState, AudioClip>();
        fLoadSound();   // 사운드 로드
    }

    public void fStart()
    {
        // 오디오 풀 초기화
        GameObject tempGameObj;
        CAudioObject tempAudioObj;
        audioPool = new CObjectPool<CPoolableObject>(5, () =>
        {

            tempGameObj = new GameObject("SoundObject");
            tempGameObj.transform.parent = transform;
            tempGameObj.gameObject.AddComponent<CAudioObject>();
            tempGameObj.gameObject.AddComponent<AudioSource>();

            tempAudioObj = tempGameObj.GetComponent<CAudioObject>();
            tempAudioObj.audioSource = tempAudioObj.GetComponent<AudioSource>();
            tempAudioObj.Create(audioPool);
            
            return tempAudioObj;
        });

        // 오디오 할당
        audioPool.Allocate();
    }

    // 소멸자
    public void fOnDestroy()
    {
        audioPool.Dispose();
    }

    // 오디오 로딩
    private void fLoadSound()
    {
        soundList.Add(AudioState.MP3_Background_1, Resources.Load("Sound/MP3_Background_1") as AudioClip);
        soundList.Add(AudioState.MP3_Background_2, Resources.Load("Sound/MP3_Background_2") as AudioClip);
        soundList.Add(AudioState.MP3_Tick_1,       Resources.Load("Sound/MP3_Tick_1") as AudioClip);

    }

    // 사운드 재생
    public void fPlaySound(Transform tr, AudioState audioState)
    {
        audioPool.PopObject().GetComponent<CAudioObject>().fPlay(tr, soundList[audioState]);
    }

}
