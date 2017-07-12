using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLogic : MonoBehaviour {
    public float BornTime = 0;
    public float DieAfterTime = 30;
    public float FireDelay = 1;
    public GameObject target;
    public GameObject rbObj;
    public float Acceleration = 10;
    Rigidbody rb;

    //PN algorithm
    public float N = 3;
    Vector3 oldRTM;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	Vector3 ProportionalNavigation()
    {
        Vector3 accel = Vector3.zero;
        //accel = N * Vc * LOS_rate
        //N = navigation gain/constant
        //Vc = closing velocity
        //line of sight rotation rate

        //to get LOS rate
        var newRTM = target.transform.position - transform.position;
        if (oldRTM == null)
        {
            oldRTM = newRTM;
            return Vector3.zero;
        }
        var LOSDelta = (newRTM - oldRTM);
        var LOSRate = LOSDelta.magnitude;
        oldRTM = newRTM;

        var Vc = LOSRate;
        accel = newRTM * N * Vc * LOSRate + LOSDelta * N;// t * (0.5 * N);
        accel = accel.normalized * Mathf.Min(300, accel.magnitude);

        return accel;
    }
	// Update is called once per frame
	void Update () {
		if (Time.time - BornTime > DieAfterTime)
        {
            Destroy(gameObject);
            return;
        }
		if (Time.time - BornTime > FireDelay)
        {
            if (target == null)
                return;
            //accelerate towards target!
            var dir = target.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(dir);
            //rb.AddForce(dir.normalized * Acceleration * Time.deltaTime);
            //rb.velocity += dir.normalized * Acceleration * Time.deltaTime;
            //rocket acceleration
            var rocketAccel = rb.velocity.normalized * 10 * Time.deltaTime;
            rb.velocity += ProportionalNavigation() * Time.deltaTime * 1f + rocketAccel;
        }
	}
}
