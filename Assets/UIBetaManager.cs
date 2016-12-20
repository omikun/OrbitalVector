using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBetaManager : MonoBehaviour {
    public GameObject moveMenu, buildMenu;
	// Use this for initialization
	void Start () {
        moveMenu.SetActive(false);
        buildMenu.SetActive(false);
	}
	
    public void ActivateMoveMenu()
    {
        moveMenu.SetActive(true);
        buildMenu.SetActive(false);
    }

    public void ActivateBuildMenu()
    {
        moveMenu.SetActive(false);
        buildMenu.SetActive(true);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
