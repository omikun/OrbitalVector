using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour {
	const float thousand = 1000;
	const float million = 1000 * thousand;
	const float billion = 1000 * million;
	const float trillion = 1000 * billion;
	float AvailableFunds = 400 * billion;
	public GameObject AvailableFundsObj;
	// Use this for initialization
	void Start () {
		AvailableFundsObj = GameObject.Find("AvailableFunds");
		UpdateFundsText(AvailableFunds);
	}
	
	string FormatMoney(float money)
	{
		string moneyStr;
		string prefix = "$";
		string suffix = "";
		if (money < thousand)  		{ }
		else if (money < million)	{ money /= thousand; suffix = "K"; }
		else if (money < billion)	{ money /= million; suffix = "M"; }
		else if (money < trillion)	{ money /= billion; suffix = "B"; }
		else 						{ money /= trillion; suffix = "T"; }
		int integer = (int)money;
		int tenth = (int)((money - integer) * 10);
		moneyStr = prefix + integer + "." + tenth + suffix;
		return moneyStr;
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
		tooltip.displayText = "Funds: " + FormatMoney(funds);
		tooltip.Reset();
	}
	public void AddShipyard()
	{
		float cost = 220 * billion;
		Buy(cost);
		//add build event to build event list
	}
	// Update is called once per frame
	void Update () {
	}
}
