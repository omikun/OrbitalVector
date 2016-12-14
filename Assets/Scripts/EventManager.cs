using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {
	static EventManager instanceInternal = null;
	EventManager() {}
	public static EventManager instance{
		get {
			if (instanceInternal == null)
			{
				instanceInternal = new EventManager();
			}
			return instanceInternal;
		}
	}
	bool EventFired = false;
	// Use this for initialization
	void Start () {
		Debug.Log("EventManager start up");
		Events.instance.AddListener<ManeuverEvent>(OnManeuverEvent);
        
	}
	
	void OnManeuverEvent(ManeuverEvent e)
	{
		Debug.Log("EventManager got an event");
		EventFired = true;
	}
    float WarpFactor = 1.0f;
    float simTime;
	// Update is called once per frame
	void Update () {
	
		if (EventFired)
		{
			EventFired = false;
			Debug.Log("EventManger got an event fired!");
		}
        //get simulation time
        simTime += WarpFactor * Time.deltaTime;
		//check if top of queue is time
        if (Events.instance == null)
        {
            Debug.Log("Events not set??");
            return;
        }else if (Events.instance.eventQueue == null)
        {
            Debug.Log("Events.instance.eventQueue not set??");
            return;
        }
        if (Events.instance.eventQueue.Count() > 0)
        {
            var nextTime = Events.instance.eventQueue.GetNextTime();
            if (nextTime <= simTime)
            {
                //raise event and get 
                var e = Events.instance.eventQueue.Dequeue();
                Events.instance.Raise(e);
            }

            //update time in each event
            var eEvent = Events.instance.eventQueue.GetEnumerator();
            var eGui = Events.instance.GUIEventQueue.GetEnumerator();
            while (eEvent.MoveNext() && eGui.MoveNext())
            {
                var e = eEvent.Current.Value;
                var eventText = eGui.Current.Value;

                //update tooltip position
                var eventObjects = Events.instance.eventQueue;
                eventText.transform.localPosition = new Vector3(0, -0.06f * (1 + eventObjects.Count()), 0);
                eventText.transform.localRotation = Quaternion.identity;

                //update tooltip text
                var tooltip = eventText.GetComponent<Tooltip>();
                tooltip.displayText = e.GetSource().name + " " + e.GetAction() + " " + e.GetTarget().name + " @ " + eGui.Current.Key;
                tooltip.Reset();
                //trigger when next to update text
            }
        }
    }
}
