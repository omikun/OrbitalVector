using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameEvent
{
	public GameEvent(GameObject src, GameObject tgt, float fireTime, string action)
	{
		source = src;
		target = tgt;
		eventFireTime = fireTime;
		actionString = action;
	}
	public float GetTime() { return eventFireTime; }
    public GameObject GetSource() { return source; }
    public GameObject GetTarget() { return target; }
	//public void UpdateTime(float dt) { eventFireTime -= dt; }
    public string GetAction() { return actionString; }
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
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
	void UpdateGUIPositions()
	{
		int index = 1;
		foreach(var item in dictionary)
		{
			{
                //GameObject g;
                //g.transform.localPosition = new Vector3(0,-0.06f*index,0);
			}
			index++;
		}
	}
	public bool isNotEmpty()
	{
		return Count() > 0;
	}
	public bool isEmpty()
	{
		return Count() == 0;
	}
    public int Count()
    {
        return dictionary.Count;
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
		return dictionary[time]; //TODO get other than top of queue
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
        var output = dictionary[key];
        dictionary.Remove(key);
        return output;
    }
}

public class Events
{
	static Events instanceInternal = null;
    Events() { }
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
	
	public PriorityQueue<GameEvent> eventQueue = new PriorityQueue<GameEvent>();
	public PriorityQueue<GameObject> GUIEventQueue = new PriorityQueue<GameObject>();
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

	public bool Queue(GameEvent evt) {
        if (!delegates.ContainsKey(evt.GetType())) {
            Debug.LogWarning("EventManager: QueueEvent failed due to no listeners for event: " + evt.GetType());
            return false;
        }

        Debug.Log("Events queuing up new event");
        eventQueue.Enqueue(evt, evt.GetTime());
		EventManager.instance.CreateNewEvent(evt);
        return true;
    }

    public void FixedUpdate()
	{
		//iterate over all events and update time
		var eEvent = eventQueue.GetEnumerator();
		while (eEvent.MoveNext())
		{
			var e = eEvent.Current.Value;
			//e.UpdateTime(HoloManager.SimTimeScale * Time.fixedDeltaTime);
		}

    }
}