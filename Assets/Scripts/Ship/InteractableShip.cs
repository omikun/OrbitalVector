﻿using UnityEngine;
using System.Collections;
using VRTK;

public class InteractableShip : VRTK_InteractableObject {

    float duration = 0.5f;
    public GameObject controller;
    VRTK_ControllerTooltips debugToolTip;
    public GameObject triggerObj;
    public GameObject functionalShip;
    public override void StartUsing(GameObject currentUsingObject)
    {
        base.StartUsing(currentUsingObject);

        Debug.Log(name + "selected!");
        UXStateManager.SelectUnit(gameObject);
    }

    public override void StopUsing(GameObject previousUsingObject)
    {
        base.StopUsing(previousUsingObject);
        debugToolTip.triggerText = "cleared!";
                debugToolTip.triggerInitialised = false;
        UXStateManager.ClearSource();
        triggerObj.SetActive(false);
    }
    // Use this for initialization
    void Start () {
        base.Start();
        foreach (var controller in GameObject.FindGameObjectsWithTag("GameController") )
        {
            Debug.Log("finding controller");
            var toolTips = controller.GetComponent<VRTK_ControllerTooltips>();
            if (toolTips != null)
            {
                debugToolTip = toolTips;
                debugToolTip.triggerText = "default";
                debugToolTip.triggerInitialised = false;
                break;
            }
        }
        if (debugToolTip == null)
        {
            Debug.Log("debug tool tip on controller not found");
            debugToolTip = controller.GetComponent<VRTK_ControllerTooltips>();
            if (debugToolTip == null)
                Debug.Log("not found again??");
        }
        triggerObj.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
	}
}
