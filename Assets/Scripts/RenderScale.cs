using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderScale : MonoBehaviour {
    public GameObject mainCamera;
    [Range(1.0f, 1000.0f)]
    public float scale = 1;
    [Range(1.0f, 10.0f)]
    public float actualScale = 1;
    Vector3 origDistance;
    Camera myCamera;
	// Use this for initialization
	void Start () {
        myCamera = mainCamera.GetComponent<Camera>();
        origDistance = transform.position - mainCamera.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.localScale = Vector3.one * scale * actualScale;
        transform.position = mainCamera.transform.position + origDistance * scale; 

	}
}
