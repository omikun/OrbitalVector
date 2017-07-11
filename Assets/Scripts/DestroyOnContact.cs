using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnContact : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Contact! die!!");
        Destroy(gameObject);
        Destroy(collision.gameObject);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
