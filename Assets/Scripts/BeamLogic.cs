using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamLogic : MonoBehaviour {
    public float DieAfterTime = 0;
    public float BornTime = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (DieAfterTime < Time.time - BornTime)
        {
            Destroy(gameObject);
        }
	}
}
