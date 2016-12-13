using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameEvent
{
	GameEvent(GameObject src, GameObject tgt, float fireTime, string action)
	{
		source = src;
		target = tgt;
		eventFireTime = fireTime;
		actionString = action;
	}
	public float GetTime() { return eventFireTime; }
	GameObject source, target;
	float eventFireTime;
	string actionString = "Undefined";
}

//http://stackoverflow.com/questions/13981406/simple-priority-queue-in-c-sharp-what-will-be-better-than-list-with-custom-sor
/// <typeparam name="TElement">The type of the actual elements that are stored</typeparam>
/// <typeparam name="TKey">The type of the priority.  It probably makes sense to be an int or long, \
/// but any type that can be the key of a SortedDictionary will do.</typeparam>
public class PriorityQueue<T> : IEnumerable<KeyValuePair<float, T>>
{
    public SortedDictionary<float, T> dictionary = new SortedDictionary<float, T>();

    public PriorityQueue()
    {
    }

	public IEnumerator<KeyValuePair<float, T>> GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}
	IEnumerator<T> IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
	void UpdateGUIPositions()
	{
		int index = 1;
		foreach(var item in dictionary)
		{
			var gee = item.Value.Peek();
			if (gee.GetType() == typeof(GameObject))
			{
				GameObject g = gee;
                g.transform.localPosition = new Vector3(0,-0.06f*index,0);
			}
			index++;
		}
	}
    public void Enqueue(T item, float key)
    {
		/*
        Queue<T> queue;
        if (!dictionary.TryGetValue(key, out queue))
        {
            queue = new Queue<T>();
            dictionary.Add(key, queue);
        }

        queue.Enqueue(item);
		*/
		dictionary.Add(key, item);
    }

	public T GetEvent(float time)
	{
		return dictionary[time].Peek(); //TODO get other than top of queue
	}
	public float GetNextTime()
	{
		return dictionary.Keys.Min();
	}
    public T Dequeue()
    {
        if (dictionary.Count == 0)
            Debug.Log("No items to Dequeue:");
        var key = GetNextTime();
        //var key = dictionary.First().Key;
		return RemoveEvent(key);
	}
	public T RemoveEvent(float key)
	{
        var queue = dictionary[key];
        var output = queue.Dequeue();
        if (queue.Count == 0)
            dictionary.Remove(key);
        return output;
    }
}

public class Events : MonoBehaviour
{
	static Events instanceInternal = null;
	public static Events instance
	{
		get
		{
			if (instanceInternal == null)
			{
				instanceInternal = new Events();
			}
			
			return instanceInternal;
		}
	}
	
	public delegate void EventDelegate<T> (T e) where T : GameEvent;
	private delegate void EventDelegate (GameEvent e);
	
	PriorityQueue<GameEvent> eventQueue;

	private Dictionary<System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();
	private Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();
	
	public void AddListener<T> (EventDelegate<T> del) where T : GameEvent
	{	
		// Early-out if we've already registered this delegate
		if (delegateLookup.ContainsKey(del))
			return;
		
		// Create a new non-generic delegate which calls our generic one.
		// This is the delegate we actually invoke.
		EventDelegate internalDelegate = (e) => del((T)e);
		delegateLookup[del] = internalDelegate;
		
		EventDelegate tempDel;
		if (delegates.TryGetValue(typeof(T), out tempDel))
		{
			delegates[typeof(T)] = tempDel += internalDelegate; 
		}
		else
		{
			delegates[typeof(T)] = internalDelegate;
		}
	}
	
	public void RemoveListener<T> (EventDelegate<T> del) where T : GameEvent
	{
		EventDelegate internalDelegate;
		if (delegateLookup.TryGetValue(del, out internalDelegate))
		{
			EventDelegate tempDel;
			if (delegates.TryGetValue(typeof(T), out tempDel))
			{
				tempDel -= internalDelegate;
				if (tempDel == null)
				{
					delegates.Remove(typeof(T));
				}
				else
				{
					delegates[typeof(T)] = tempDel;
				}
			}
			
			delegateLookup.Remove(del);
		}
	}
	
	public void Raise (GameEvent e)
	{
		EventDelegate del;
		if (delegates.TryGetValue(e.GetType(), out del))
		{
			del.Invoke(e);
		}
	}

	PriorityQueue<GameEvent> m_eventQueue;
	public bool Queue(GameEvent evt) {
        if (!delegates.ContainsKey(evt.GetType())) {
            Debug.LogWarning("EventManager: QueueEvent failed due to no listeners for event: " + evt.GetType());
            return false;
        }

        m_eventQueue.Enqueue(evt, evt.GetTime());
		CreateNewEvent(evt);
        return true;
    }

	public GameObject eventGUI;
	PriorityQueue<GameObject> GUIEventQueue;
	float WarpFactor = 1.0f;
    float simTime;
	void CreateNewEvent(GameEvent e)
	{
		Debug.Log("EventManager got an event");
		var newObj = (GameObject)Instantiate(eventGUI);
		newObj.transform.parent = eventGUI.transform.parent;
		GUIEventQueue.Enqueue(newObj, e.GetTime());
		//update position
		UpdateGUIPosition();
	}
	void UpdateGUIPosition() 
	{
		newObj.transform.localPosition = new Vector3(0,-0.06f*(1+eventObjects.Count),0);
		newObj.transform.localRotation = Quaternion.identity;
		eventObjects.Add(newObj)
		foreach(var item in dictionary)
		{
			var gee = item.Value.Peek();
			if (gee.GetType() == typeof(GameObject))
			{
				GameObject g = gee;
                g.transform.localPosition = new Vector3(0,-0.06f*index,0);
			}
			index++;
		;
	}
	void Update()
	{
		//get simulation time
		simTime += WarpFactor * Time.deltaTime;
		//check if top of queue is time
		var nextTime = eventQueue.GetNextTime();
		if (nextTime <= simTime)
		{
			//raise event and get 
			var e = eventQueue.Dequeue();
			Raise(e);
		}
	}
}