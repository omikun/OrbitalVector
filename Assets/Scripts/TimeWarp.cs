using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeWarp : MonoBehaviour {
	public void IncreaseWarp() {
		if (HoloManager.SimTimeScale == 0)
            HoloManager.SimTimeScale = 1;
        if (HoloManager.SimTimeScale < 1024)
            HoloManager.SimTimeScale *= 4;
    }
    public void DecreaseWarp() {
        if (HoloManager.SimTimeScale > 1)
            HoloManager.SimTimeScale /= 2;

        if (HoloManager.SimTimeScale < 1)
            HoloManager.SimTimeScale = 1;
	}
	public void StopWarp() {
		HoloManager.SimTimeScale = 1;
	}
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
