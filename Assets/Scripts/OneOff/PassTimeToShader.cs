using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PassTimeToShader : MonoBehaviour {
    Material material;
	// Use this for initialization
	void Start () {
        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            material = rend.sharedMaterial;
        }
	}
	
	// Update is called once per frame
	void Update () {
        var time = Application.isEditor ? (float)EditorApplication.timeSinceStartup : Time.time;
        material.SetFloat("_phase", time);
	}
}
