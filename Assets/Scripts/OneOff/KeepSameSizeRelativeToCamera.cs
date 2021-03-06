﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepSameSizeRelativeToCamera : MonoBehaviour {
    [Range(0.02f, 10)]
    public float ScaleFactor = .1f;
	GameObject camera;
	GameObject holoRoot;
	Vector3 actualScale;
	// Use this for initialization
	void Start () {
		if (InitMe.GetInstance().EnableVR) {
			camera = GameObject.Find ("Camera (eye)");
		} else {
			camera = GameObject.Find ("Camera");
		}
        if (camera == null)
        {
            Debug.Log("Can't find camera");
        }
		holoRoot = GameObject.Find("HoloRoot");
		actualScale = holoRoot.transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		float scale = ScaleFactor * (camera.transform.position - transform.position).magnitude;
		float diffScale = actualScale.magnitude / holoRoot.transform.localScale.magnitude;
		transform.localScale = scale * diffScale * Vector3.one; 
	}
}
