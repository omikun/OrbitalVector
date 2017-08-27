using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIJoust : MonoBehaviour {
    public GameObject target;
    Rigidbody rb;
    public float acceleration = 20;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(Joust(target));
	}
    WaitForSeconds delay = new WaitForSeconds(.1f);
    WaitForFixedUpdate fixedDelay = new WaitForFixedUpdate();
    bool midway = false;
    bool runaway = false;
	IEnumerator Joust(GameObject target) {
        while(true)
        {
            // if target within 180 of forward, run at target
            // else maintain current heading
            var toTarget = target.transform.position - transform.position;
            float angle = Vector3.Angle(toTarget, transform.forward);
            //move away when too close
            if (runaway)
            {
                if (toTarget.magnitude > 100)
                {
                    runaway = false;
                    Debug.Log("turning back now");
                } else
                {
                    yield return fixedDelay;
                    continue;
                }
            }
            if (toTarget.magnitude < 20)
            {
                var newDir = Vector3.RotateTowards(transform.forward, transform.right, .5f * Time.fixedDeltaTime, 1).normalized;
                transform.forward = newDir;
                midway = true;
            } else if (toTarget.magnitude > 50 || Mathf.Abs(angle) > 25)
                //&& (Mathf.Abs(angle) < 20 ))
            {
                if (midway)
                {
                    Debug.Log("running away!");
                    runaway = true;
                    midway = false;
                }
                var newDir = Vector3.RotateTowards(transform.forward, toTarget, 2 * Time.fixedDeltaTime, 1).normalized;
                transform.forward = newDir;
            }
            var boostVel = transform.forward * acceleration * Time.deltaTime;
            rb.velocity += boostVel;
            if (rb.velocity.magnitude > 10)
            {
                rb.velocity = rb.velocity.normalized * 10;
            }
            Debug.Log("angle: " + angle + " newVel: " + rb.velocity.magnitude + " dist: " + toTarget.magnitude);
            yield return fixedDelay;
            //fire when in range and in fov
            //yield return null;
            //if out of range, turn back
            //yield return null;
        }
    }
	// Update is called once per frame
	void Update () {
        //Joust();
	}
}
