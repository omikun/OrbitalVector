using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//GUI Event manager
public class EventManager : MonoBehaviour {
	static EventManager instanceInternal = null;
	public GameObject eventText;
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
	
    //create an entry to represent event on GUI event list
	public void CreateNewEvent(GameEvent e)
	{
		if (eventText == null)
		{
            eventText = GameObject.Find("EventTitle");
            if (eventText == null)
                Debug.Log("================eventText not found?!?!");
        }
        Debug.Log("EventManager got an event to enqueue");
		var newObj = Instantiate(eventText);
		newObj.transform.parent = eventText.transform.parent;
		Events.instance.GUIEventQueue.Enqueue(newObj, e.GetTime());
		//update position
		//TODO signal eventmanager update? UpdateGUIPosition();
	}
	void OnManeuverEvent(ManeuverEvent e)
	{
		Debug.Log("EventManager got an event");
		EventFired = true;
	}
    float WarpFactor = 1.0f;
    float simTime;

    string FormatTime(float time)
    {
        string timeStr;
        string suffix = "S";
        float minute = 60;
        float hour = 60 * minute;
        float day = 24 * hour;
        float week = 7 * day;
        float year = 52 * week;
        if (time < minute)    { }
        else if (time < hour) { time /= minute; suffix = "M"; }
        else if (time < day)  { time /= hour;   suffix = "H"; }
        else if (time < week) { time /= day;    suffix = "D"; }
        else if (time < year) { time /= week;   suffix = "W"; }

        int integer = (int)time;
        int tenth = (int)((time - integer) * 10);
        timeStr = integer.ToString() + "." + tenth + suffix;
        return timeStr;
    }
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
            int index = 1;
            while (eEvent.MoveNext() && eGui.MoveNext())
            {
                var e = eEvent.Current.Value;
                var eventText = eGui.Current.Value;

                //update tooltip position
                var eventObjects = Events.instance.eventQueue;
                eventText.transform.localPosition = new Vector3(0, -0.06f * index, 0);
                eventText.transform.localRotation = Quaternion.identity;

                //update tooltip text
                float time = eGui.Current.Key;
                var timeStr = FormatTime(time);
                var tooltip = eventText.GetComponent<Tooltip>();
                tooltip.displayText = e.GetSource().name + " " + e.GetAction() + " " + e.GetTarget().name + " @ " + timeStr;
                tooltip.Reset();
                //trigger when next to update text
                index++;
            }
        }
    }
}
