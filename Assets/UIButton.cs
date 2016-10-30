using UnityEngine;
using System.Collections;
using VRTK;

public class UIButton : VRTK_InteractableObject {
    public override void StartUsing(GameObject currentUsingObject)
    {
        Debug.Log("using moveIntercept");
    }
    public override void StopUsing(GameObject currentUsingObject)
    {
        Debug.Log("NOT using moveIntercept");
    }
	// Use this for initialization
	void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
	}
}
