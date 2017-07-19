using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMe : MonoBehaviour {
    public Vector3 velocity = Vector3.right;
    Rigidbody rb;
	// Use this for initialization
	void Start () {
        //GetComponent<Rigidbody>().velocity = velocity;
        rb = GetComponent<Rigidbody>();
        lastPos = transform.position;
	}

    // Update is called once per frame
    Vector3 lastPos;
    Vector3 lastVel;
    [Range(1,10)]
    public float TimeScale = 3;
    [Range(1,10)]
    public float SpaceScale = 1;
	void Update () {
        var x =  SpaceScale * 60 * Mathf.Sin(Time.time/10);
        var y =  SpaceScale * 30 * Mathf.Cos(Time.time/10);
        var z =  SpaceScale * 20 * Mathf.Sin(Mathf.PI * Time.time/10);
        transform.position = new Vector3(x, y, z);
        var velocity = (transform.position - lastPos);
        var speed = velocity.magnitude / Time.deltaTime;
        lastPos = transform.position;
        Logdump.Log("target speed: " + speed);
        var accel = (velocity - lastVel).magnitude / Time.deltaTime;
        Logdump.Log("target accel: " + accel);
	}
}
