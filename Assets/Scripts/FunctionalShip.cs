using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionalShip : MonoBehaviour {
	SnapMe[] smArray;
	//traverse through ship to get data
	public float GetMass()
	{
		//find connectors
		PartBase[] parts = gameObject.GetComponentsInChildren<PartBase>();
		float weight = 0;
		foreach(var part in parts)
		{
			weight += part.GetMass();
		}
		return weight;
	}
	public float GetThrust()
	{
		PartEngine[] engines = GetComponentsInChildren<PartEngine>();
		float thrust = 0;
		foreach(var engine in engines)
		{
			thrust += engine.thrust;
		}
		return thrust;
	}
	public float Accelerate()
	{
		var thrust = GetThrust();
		var mass = GetMass();
		float accel = thrust/mass;
		//TODO: find how much fuel was consumed
		//find how much fuel could be consumed
		//get percentage of fuel and percentage of acceleration
		return accel;
	}
	float mass = 0;
	float cost = 0;
	float fuel = 0;
	float thrust = 0;
	float isp = 0;
	float dryMass = 0;
	float fuelTankMass = 0;
	float dv = 0;
	public void UpdateShip()
	{
		mass = 0;
		cost = 0;
		fuel = 0;
		dryMass = 0;
		fuelTankMass = 0;
		thrust = 0;
		//go through all children to find parts;
		{
            PartBase[] comps = GetComponentsInChildren<PartBase>();
            foreach (var comp in comps)
            {
                mass += comp.GetMass();
                cost += comp.GetCost();
            }
		}
		{
            PartFuelTank[] comps = GetComponentsInChildren<PartFuelTank>();
            foreach (var comp in comps)
            {
                fuel += comp.GetFuel();
				dryMass += comp.GetDryMass();
				fuelTankMass += comp.GetMass();
            }
        }
		{
			float denom = 0;
            PartEngine[] comps = GetComponentsInChildren<PartEngine>();
            foreach (var comp in comps)
            {
                thrust += comp.GetThrust();
				denom += (comp.GetThrust()/comp.GetIsp());
            }
			isp = thrust / denom;
        }

		//TODO find engine/fueltank dependencies
		dv = Mathf.Log(mass/(mass-fuelTankMass+dryMass)) * isp * 9.81f;
		Debug.Log("Mass: " + mass + " cost: " + cost + " thrust: " + thrust + " isp: " + isp + " dv: " + dv);
	}
	public float GetDV()
	{
		UpdateShip();
		return dv;
	}
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
