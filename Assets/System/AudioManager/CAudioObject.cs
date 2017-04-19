using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAudioObject : CPoolableObject
{
    public AudioSource audioSource;
    private Transform orignParentTr; 
    public override void Create(CObjectPool<CPoolableObject> pool)
    {
        base.Create(pool);
    }

    public override void fDispose()
    {
        transform.parent = orignParentTr;
        base.fDispose();
    }

    public override void _OnEnableContents()
    {
        base._OnEnableContents();
        
    }

    public override void _OnDisableContents()
    {
        base._OnDisableContents();
    }

    public void fPlay(Transform tr, AudioClip clip, float spatialBlend, float maxDistance, float volume, bool isLoopping)
    {
        orignParentTr = transform.parent;
        transform.position = tr.transform.position;
        transform.parent = tr.transform;

        audioSource.clip = clip;
        audioSource.spatialBlend = spatialBlend;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = volume;
        audioSource.loop = isLoopping;

        audioSource.Play();
        StartCoroutine(fPlaySound(clip));
    }

    public void fPlay(Transform tr, AudioClip clip)
    {
        fPlay(tr, clip, 1f, 5f, 1f, false);
    }

    public void fPlay(Transform tr, AudioClip clip, bool isLoopping)
    {
        fPlay(tr, clip, 1f, 5f, 1f, isLoopping);
    }


    IEnumerator fPlaySound(AudioClip clip)
    {
        while (true)
        {
            if (audioSource.isVirtual)
            {
                fDispose();
                break;
            }
            yield return null;
        } 
    }
}
