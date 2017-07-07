using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPhysics : MonoBehaviour {
    Rigidbody rb;
    public float torque = 10;
    public bool SpinLeft = false;
    public bool SpinRight = false;
    public bool SpinUp = false;
    public bool SpinDown = false;
    public KeyCode kLeft = KeyCode.A;
    public KeyCode kRight = KeyCode.D;
    public KeyCode kUp = KeyCode.W;
    public KeyCode kDown = KeyCode.S;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
    void Spin(ref bool spin, Vector3 dir)
    {
        if (spin)
        {
            spin = false;
            rb.AddTorque(dir * torque);
        }
    }
    void GetKeyboard()
    {
        if (Input.GetKey(kLeft))  rb.AddTorque(transform.forward * torque); 
        else if (Input.GetKey(kRight))  rb.AddTorque(-transform.forward * torque); 
        else if (Input.GetKey(kUp))  rb.AddTorque(transform.right * torque); 
        else if (Input.GetKey(kDown))  rb.AddTorque(-transform.right * torque); 
    }
	// Update is called once per frame
	void Update () {
        GetKeyboard();
        /*
        Spin(ref SpinLeft, transform.up);
        Spin(ref SpinRight, -transform.up);
        Spin(ref SpinUp, transform.right);
        Spin(ref SpinDown, -transform.right);
        */
    }
}
