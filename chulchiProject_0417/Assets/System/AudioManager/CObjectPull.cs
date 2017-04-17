using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CObjectPool<T> where T : CPoolableObject
{

    public List<T> list;              // 활성화 된 오브젝트 목록
    private Stack<T> stack;           // 비활성화 된 오브젝트

    public delegate T Initializer();
    private Initializer initializer;  // 오브젝트의 초기화 함수

    private int poolSize;
    private int curPoolSize;
    public CObjectPool() { }
    public CObjectPool(int size, Initializer init )
    {
        initializer = init;
        poolSize = size;
        curPoolSize = 0;
        list = new List<T>();
        stack = new Stack<T>();
    }

    public void PushObject(T obj)
    {
        obj.gameObject.SetActive(false);

        list.Remove(obj);
        stack.Push(obj);

    }

    public void Allocate()
    {
        curPoolSize += poolSize;
        for (int i = 0; i < poolSize; ++i)
        {
            stack.Push(initializer());
        }
    }

    public T PopObject()
    {
        if (stack.Count <= 0)
        {
            Allocate();
            Debug.Log( DebugType.ETC, "Pool 부족으로 추가 오브젝트 생성, 현재 " + curPoolSize + "개");
        }
        
        T obj = stack.Pop();
        list.Add(obj);

        obj.gameObject.SetActive(true);

        return obj;
    }

    public void Dispose()
    {
        if (stack == null || list == null)
            return;

        list.ForEach(obj => stack.Push(obj));

        while (stack.Count > 0)
        {
            GameObject.Destroy(stack.Pop());
        }

        list.Clear();
        stack.Clear();
    }

}

public class CPoolableObject : MonoBehaviour
{
    protected CObjectPool<CPoolableObject> objectPull;

    public virtual void Create(CObjectPool<CPoolableObject> objectPull)
    {
        this.objectPull = objectPull;
        gameObject.SetActive(false);
    }

    public virtual void fDispose()
    {
        objectPull.PushObject(this);
    }

    public virtual void _OnEnableContents() { }
    public virtual void _OnDisableContents() { }
}