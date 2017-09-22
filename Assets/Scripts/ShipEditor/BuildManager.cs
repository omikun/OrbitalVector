using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour {
	
	float AvailableFunds = 400 * OVTools.billion;
	public GameObject AvailableFundsObj;
	// Use this for initialization
	void Start () {
		AvailableFundsObj = GameObject.Find("AvailableFunds");
		UpdateFundsText(AvailableFunds);
	}
	
	
	public void Buy(float amount)
	{
		ChangeFunds(-1 * amount);
	}
	public void ChangeFunds(float amount)
	{
		Debug.Log("Changing funds by " + amount);
		AvailableFunds += amount;
		UpdateFundsText(AvailableFunds);
	}
	void UpdateFundsText(float funds)
	{
		Debug.Log("Updating funds text");
		var tooltip = AvailableFundsObj.GetComponent<Tooltip>();
		tooltip.displayText = "Funds: " + OVTools.FormatMoney(funds);
		tooltip.Reset();
	}
	public void AddShipyard()
	{
		float cost = 220 * OVTools.billion;
		Buy(cost);
		//add build event to build event list
	}
	// Update is called once per frame
	void Update () {
	}
}
