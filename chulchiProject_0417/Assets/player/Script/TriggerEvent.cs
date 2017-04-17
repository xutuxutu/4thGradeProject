using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ITriggerEvent
{
    void fTriggerEnter(TriggerEvent triggerEvent);
    void fTriggerStay(TriggerEvent triggerEvent);
    void fTriggerExit(TriggerEvent triggerEvent);
}

public class TriggerEvent : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            transform.parent.GetComponent<ITriggerEvent>().fTriggerEnter(this);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            transform.parent.GetComponent<ITriggerEvent>().fTriggerStay(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            transform.parent.GetComponent<ITriggerEvent>().fTriggerExit(this);
    }
}
