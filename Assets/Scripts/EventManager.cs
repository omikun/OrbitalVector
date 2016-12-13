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
	public List<GameObject> eventObjects = new List<GameObject>();
	public GameObject eventText;
	bool EventFired = false;
	// Use this for initialization
	void Start () {
		Debug.Log("EventManager start up");
		Events.instance.AddListener<ManeuverEvent>(OnManeuverEvent);
        eventText = GameObject.Find("EventTitle");
		if (eventText == null)
		{
			Debug.Log("================eventText not found?!?!");
		}
	}
	
	void OnManeuverEvent(ManeuverEvent e)
	{
		Debug.Log("EventManager got an event");
		EventFired = true;
		var newObj = (GameObject)Instantiate(eventText);
		newObj.transform.parent = eventText.transform.parent;
		newObj.transform.localPosition = new Vector3(0,-0.06f*(1+eventObjects.Count),0);
		newObj.transform.localRotation = Quaternion.identity;
		eventObjects.Add(newObj);
	}
	// Update is called once per frame
	void Update () {
	
		if (EventFired)
		{
			EventFired = false;
			Debug.Log("EventManger got an event fired!");
		}
	}
}
