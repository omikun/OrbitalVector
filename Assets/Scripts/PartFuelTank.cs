using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartFuelTank : PartBase {
	float fuel = 400; //tons
	float dryMass = 4;
	PartFuelTank()
    {
		mass = 400;
        cost = 25;
        health = 20;
	}
	public float GetDryMass()
	{
		return dryMass;
	}
	public float GetFuel()
	{
		return mass - dryMass;
	}
	// Use this for initialization
	public float consume(float consumed)
	{
		if (mass - dryMass >= consumed)
		{
			mass -= consumed;
			return consumed;
		} else if (mass > dryMass)
		{
			consumed = mass - dryMass;
			mass = dryMass;
			return consumed;
		} else {
			return 0;
		}
	}
}
