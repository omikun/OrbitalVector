using UnityEngine;
using System.Collections;
using VRTK;
using UnityEngine.Events;

public class UIDraggable : VRTK_InteractableObject
{
    public UnityEvent OnDragEvent;
    public bool startTime = true;
    public override void StartUsing(GameObject currentUsingObject)
    {
        Ray ray = new Ray(currentUsingObject.transform.position, currentUsingObject.transform.forward);
        RaycastHit hitInfo;
        if (GetComponent<Collider>().Raycast(ray, out hitInfo, 100))
        {
            if (startTime)
                transform.parent.GetComponent<PorkChopPlot>()._DragStartTimeIndicator(hitInfo);
            else
                transform.parent.GetComponent<PorkChopPlot>()._DragTravelTimeIndicator(hitInfo);
        }
    }
    public override void StopUsing(GameObject currentUsingObject)
    {
        Debug.Log("StopUsing button");
    }
     public void Drag()
    {
        if (OnDragEvent == null)
            return;
        OnDragEvent.Invoke();
    }
    void Start()
    {
        base.Start();
    }

    void Update()
    {
    }
}
