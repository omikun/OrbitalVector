using UnityEngine;
using System.Collections;

public class GetValueForSign : MonoBehaviour {//: VRTK.VRTK_ObjectTooltip {
    public GameObject camera;
    public string prependText;
    VRTK.VRTK_ObjectTooltip link;

	// Use this for initialization
	void Start () {
        link = GetComponent<VRTK.VRTK_ObjectTooltip>();
        //link.displayText = prependText + "4000m/s";// speed.magnitude.ToString();
	}
	
	// Update is called once per frame
	void Update () {
        var userSelection = UXStateManager.GetSource();
        if (userSelection == null) { return; }

        var speed = userSelection.GetComponent<OrbitData>().getVel();
        link.displayText = prependText + speed.magnitude.ToString();
        link.Reset();
        //transform.LookAt(camera.transform, Vector3.up);
        transform.LookAt(camera.transform);
	}
}
