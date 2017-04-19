using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 이벤트 모음
public enum CUSTOM_EVENT
{
    capsule,
    cube,
    objects,
}

// 이벤트 실행
public interface IEventable
{
    void Active(CUSTOM_EVENT key);
}

// 이벤트 메니져
public class CEventManager : MonoBehaviour
{
    Dictionary<CUSTOM_EVENT, List<IEventable>> eventDictionary;

    public void fAwake()
    {
        eventDictionary = new Dictionary<CUSTOM_EVENT, List<IEventable>>();
    }

    // 이벤트 대상을 리스트에 등록
    public void fEventRegist(CUSTOM_EVENT cusEvent, IEventable eventable)
    {
        
        if (!eventDictionary.ContainsKey(cusEvent))  // 처음 등록되는 이벤트의 경우에만 리스트 초기화 설정 해준다.
            eventDictionary.Add(cusEvent, new List<IEventable>());

        eventDictionary[cusEvent].Add(eventable);
    }

    // 이벤트 대상을 리스트에서 삭제
    public void fEventRemove(CUSTOM_EVENT cusEvent, IEventable eventable)
    {
        eventDictionary[cusEvent].Remove(eventable);
    }

    // 이벤트 대상들에게 실행 명령
    public void fEventSend(CUSTOM_EVENT cusEvent)
    {
        foreach (var events in eventDictionary[cusEvent])
        {
            events.Active(cusEvent);
        }
    }
   
}

