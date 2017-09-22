using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreatePrefab : MonoBehaviour {
	public GameObject go;
	// Use this for initialization
	void Start () {
		
	}
	
	void OnMouseDown()
	{
		Debug.Log("Creating prefab! " + go.name);
		PrefabUtility.CreatePrefab("Assets/RuntimePrefabs/" + go.name + ".prefab", go);
	}
	// Update is called once per frame
	void Update () {
		
	}
}
