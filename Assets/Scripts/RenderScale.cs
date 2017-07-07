using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderScale : MonoBehaviour {
    public GameObject mainCamera;
    [Range(1000f, 100000000.0f)]
    public float scale = 1000;
    [Range(.01f, 1.0f)]
    public float scale2 = 1;
    [HideInInspector]
    public float realScale = 1;
    [Range(1.0f, 10.0f)]
    public float actualScale = 1;
    [Range(.01f, 1.0f)]
    public float actualScale2 = 1;
    [HideInInspector]
    public float realActualScale = 1;
    Vector3 origDistance;
    Camera myCamera;
	// Use this for initialization
	void Start () {
        myCamera = mainCamera.GetComponent<Camera>();
        origDistance = transform.position - mainCamera.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        realScale = scale * scale2;
        realActualScale = actualScale * actualScale2;
        transform.localScale = Vector3.one * realScale * realActualScale;
        if (mainCamera == null)
            return;
        transform.position = mainCamera.transform.position + origDistance * realScale; 

	}
}
