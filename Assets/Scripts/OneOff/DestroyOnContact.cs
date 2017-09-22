using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnContact : MonoBehaviour {
    public GameObject Explosion;
	// Use this for initialization
	void Start () {
		
	}
	void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Contact! die!!");
        Explosion.transform.position = transform.position;
        Explosion.SetActive(false);
        Explosion.SetActive(true);
        Destroy(gameObject);
        Destroy(collision.gameObject);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
