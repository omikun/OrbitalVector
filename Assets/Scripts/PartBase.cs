using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartBase : MonoBehaviour {
	protected float mass = 7;
	protected float cost = 7;
	protected float health = 7;

	public float GetMass() { return mass; }
	public float GetCost() { return cost; }
	public float GetHealth() { return health; }
}
