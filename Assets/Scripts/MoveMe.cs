using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMe : MonoBehaviour {
    public Vector3 velocity = Vector3.right;
	// Use this for initialization
	void Start () {
        GetComponent<Rigidbody>().velocity = velocity;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
