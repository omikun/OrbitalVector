using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//GUI Event manager
public class EventManager : MonoBehaviour
{
    public GameObject eventText;
    public GameObject simTimeObj;
    EventManager() { }

    double SimTime;
    public double GetSimTime()
    {
        return SimTime;
    }
    bool EventFired = false;
    // Use this for initialization
    void Start()
    {
        Debug.Log("EventManager start up");
        Events.instance.AddListener<ManeuverEvent>(OnManeuverEvent);
        SimTime = Time.time;
        simTimeObj = GameObject.Find("SimTime");
    }

    public bool Queue(GameEvent evt)
    {
        CreateNewEvent(evt);
        Debug.Log("Event time: " + evt.GetTime());
        return Events.instance.Queue(evt);
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

    void FixedUpdate()
    {
        //moved to Update
        //Events.instance.FixedUpdate();
    }
    // Update is called once per frame
    void Update()
    {
        SimTime += HoloManager.SimTimeScale * Time.fixedDeltaTime;

        if (EventFired)
        {
            EventFired = false;
            Debug.Log("EventManger got an event fired!");
        }
        //get simulation time
        //check if top of queue is time
        if (Events.instance == null)
        {
            Debug.Log("Events not set??");
            return;
        }
        else if (Events.instance.eventQueue == null)
        {
            Debug.Log("Events.instance.eventQueue not set??");
            return;
        }
        var tt = simTimeObj.GetComponent<Tooltip>();
        tt.displayText = "SimTime: " + OVTools.FormatTime((float)SimTime);
        tt.Reset();
        if (Events.instance.eventQueue.isNotEmpty())
        {

            while (Events.instance.eventQueue.isNotEmpty()
                && Events.instance.eventQueue.GetNextTime() <= SimTime)
            {
                //raise event and get 
                var e = Events.instance.eventQueue.Dequeue();
                Debug.Log("Raising event now: " + OVTools.FormatTime(e.GetTime()));
                Events.instance.Raise(e);
                var egui = Events.instance.GUIEventQueue.Dequeue();
                egui.SetActive(false);
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
                //float time = eGui.Current.Key - (float)SimTime;
                float time = e.GetTime() - (float)SimTime;
                var timeStr = OVTools.FormatTime(time);
                var tooltip = eventText.GetComponent<Tooltip>();
                tooltip.displayText = e.GetSource().name + " " + e.GetAction() + " " + e.GetTarget().name + " @ " + timeStr;
                tooltip.Reset();
                //trigger when next to update text
                index++;
            }
        }
    }
}
