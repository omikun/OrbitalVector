using UnityEngine;
using System.Collections;

public class RotateMe : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    //transform.RotateAround(Vector3.zero, Vector3.up, 200 * Time.deltaTime);

        var rot = transform.rotation*Quaternion.Euler(0, 60 * Time.deltaTime, 0);
        transform.rotation = rot;
	}
}
