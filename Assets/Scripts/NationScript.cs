using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NationScript : MonoBehaviour {

	public float happiness = 1f;
	public float population = 400 * OVTools.million;
	public float food = 400 * OVTools.million;
	public float employment = 350 * OVTools.million;
	public float productivity = .2f;
	public float production = 1f;
	public float taxRate = .3f;
	public string GetRevenueString () { return OVTools.FormatMoney(GetRevenue());}

	//spending
	public float interest = 0;
	public float interestRate = .025f;
	public float spaceFund = 100 * OVTools.billion;
	public float militaryFund = 1.4f * OVTools.trillion;
	public float domesticFund = 2.7f * OVTools.trillion;
	float domesticRequired = 1.2f * OVTools.trillion;
    float debt = 0;

	//weights
	public float gdpGain = 200;
	Dictionary<string, float> happinessEvents = new Dictionary<string, float>();
	public float GetSpending() {
		float interest = interestRate * debt;
		return interest + spaceFund + militaryFund + domesticFund;
	}
	public float GetGDP() { 
		float gdp = 0;
		gdp = gdpGain * employment * productivity;
		return gdp;
		//return 14 * OVTools.trillion; 
	}
	public float GetRevenue () { return taxRate * GetGDP(); }
	public float treasury = 2 * OVTools.trillion;

    // Use this for initialization
	EventManager eventManager;
    float nextTick;
	float tickInterval = OVTools.minute;
	void Start () {
        eventManager = GameObject.Find("EventManager").GetComponent<EventManager>();
		nextTick = Time.time + tickInterval;
		Tick();
	}
	
	void AddHappinessEvent() {
		happinessEvents.Add("Olympics won", 4f);
	}
	public void ActivateNation() {
		Debug.Log("Revenue: " + GetRevenueString());
	}
	//other metrics calculated every tick (week? month?)
	public void Tick() {
		//happiness = food surplus + baseline + employment + stability + debt + tax rate
		//	spending power (earning/costs)
		//  costs = food/population
		//  earning = gdp / population
		//  special events: win a war; land on another planet; colonize new moon
		happiness = food / population + employment / population - taxRate;
        foreach (var hap in happinessEvents.Values)
        {
            happiness += hap;
        }

        //production = rate of making things at the nation level
        production = employment * (happiness + productivity);
		//	population, happiness level, research boosts, policy boosts
		float revenue = taxRate * GetGDP();
		treasury += revenue;
		population += population * .2f * (food / population - .9f) * (1-taxRate) * (employment / population);
		Debug.Log("Happiness: " + happiness);
		Debug.Log("Production: " + OVTools.FormatNumber(production));
		Debug.Log("Revenue: " + OVTools.FormatMoney(revenue));
		Debug.Log("Population: " + OVTools.FormatNumber(population));
	}
	//player actions: build ships, build buildings
	public void Spend(float amount)
	{
		treasury -= amount;
	}

	//financials calculated every year
	public void NewYear() {
		interest = interestRate * debt;
		float revenue = GetRevenue();
		float spending = GetSpending();
        float deficit = spending - GetRevenue();
		treasury -= deficit;

		if (treasury < 0)
		{
			debt = -treasury;
		}
	}
	// Update is called once per frame
	void Update () {
		if (eventManager.GetSimTime() > nextTick) {
			nextTick += OVTools.minute;
			Tick();
		}
	}
}
