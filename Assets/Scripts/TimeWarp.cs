using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeWarp : MonoBehaviour {
	public void IncreaseWarp() {
		if (HoloManager.SimTimeScale == 0)
            HoloManager.SimTimeScale = 1;
        if (HoloManager.SimTimeScale < 1024 * 1024)
            HoloManager.SimTimeScale *= 8;
    }
    public void DecreaseWarp() {
        if (HoloManager.SimTimeScale > 1)
            HoloManager.SimTimeScale /= 2;
	}
	public void StopWarp() {
		HoloManager.SimTimeScale = 0;
	}
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
