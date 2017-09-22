using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowShipSpeed : MonoBehaviour {

	Tooltip indicator;
	// Use this for initialization
	void Start () {
		indicator = GetComponent<Tooltip>();
		if (indicator == null)
			Debug.Log("ERROR: not finding tooltip for speed indicator");
	}
	
	// Update is called once per frame
	void Update () {
		var go = UXStateManager.GetSource();
		if (go == null)
			return;
		var speed = go.GetComponent<OrbitData>().getVel().magnitude;
		var strSpeed = OVTools.FormatDistance(speed) + "/s";
		indicator.displayText = go.name + ": " + strSpeed;
		indicator.Reset();
	}
}
