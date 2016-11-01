using UnityEngine;
using System.Collections;
using VRTK;
using UnityEngine.Events;

public class UIButton : VRTK_InteractableObject {

    public UnityEvent OnEvent;
    int count = 0;

    public override void StartUsing(GameObject currentUsingObject)
    {
        Debug.Log("using moveIntercept: "+count++);
        OnEvent.Invoke();
    }
    public override void StopUsing(GameObject currentUsingObject)
   { 
        Debug.Log("NOT using moveIntercept");
    }
    public override void OnInteractableObjectTouched (InteractableObjectEventArgs e)
    {
        Debug.Log("touching!");
    }
	// Use this for initialization
	void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
	}
}
