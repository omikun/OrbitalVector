using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPhysics : MonoBehaviour {
    Rigidbody rb;
    public bool nullSpin = false;
    public float torque = 10;

    public KeyCode kLeft = KeyCode.A;
    public KeyCode kRight = KeyCode.D;
    public KeyCode kUp = KeyCode.W;
    public KeyCode kDown = KeyCode.S;

    public float SlowDown = 1;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;
	}
    
    void GetKeyboard()
    {
        if (Input.GetKey(kLeft)) rb.AddTorque(transform.forward * torque);
        else if (Input.GetKey(kRight))  rb.AddTorque(-transform.forward * torque); 
        else if (Input.GetKey(kUp))  rb.AddTorque(transform.right * torque); 
        else if (Input.GetKey(kDown))  rb.AddTorque(-transform.right * torque);
        if (Input.GetKey(kLeft)) Debug.Log("left key");
        else if (Input.GetKey(kRight)) Debug.Log("right key");
        else if (Input.GetKey(kUp)) Debug.Log("up key");
        else if (Input.GetKey(kDown)) Debug.Log("down key");
    }
    void NullSpin()
    {
        if (nullSpin)
        {
            if (rb.angularVelocity.magnitude > SlowDown * Time.deltaTime)
            {
                rb.AddTorque(-rb.angularVelocity.normalized * SlowDown, ForceMode.Acceleration);
            } else
            {
                nullSpin = false;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
	// Update is called once per frame
	void Update () {
        GetKeyboard();
        NullSpin();
        /*
        Spin(ref SpinLeft, transform.up);
        Spin(ref SpinRight, -transform.up);
        Spin(ref SpinUp, transform.right);
        Spin(ref SpinDown, -transform.right);
        */
    }
}
