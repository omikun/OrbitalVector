using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartEngine : PartBase {
	public float isp = 400;
	public float thrust = 250;
	public float heatOutput = 3000;
	PartEngine() {
		mass = 20;
        cost = 50;
        health = 20;
    }
	//returns tons/second
	public float GetFlowRate()
	{
		return thrust * isp / 1000.0f;
	}
	public float GetThrust()
	{
		return thrust;
	}
	public float GetIsp()
	{
		return isp;
	}
	// Use this for initialization
	void Awake () {
		
	}

}
