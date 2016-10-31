using UnityEngine;
using System.Collections;
using VRTK;

public class InteractableShip : VRTK_InteractableObject {

    bool onSelect = false;
    float timeOnSelect;
    float duration = 0.5f;
    public GameObject controller;
    VRTK_ControllerTooltips debugToolTip;
    public GameObject triggerObj;
    GameObject selection;
    public override void StartUsing(GameObject currentUsingObject)
    {
        base.StartUsing(currentUsingObject);
        onSelect = !onSelect;
        timeOnSelect = Time.time;

        Debug.Log(name + "selected!");
        //debugToolTip.triggerText = "selected!";
        //        debugToolTip.triggerInitialised = false;
        UXStateManager.SelectUnit(gameObject);
        selection.SetActive(onSelect);
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
        var baseObj = Resources.Load("SelectionIcon");
        if (baseObj == null)
            Debug.Log("can't find config anywhere");
        selection = (GameObject)Instantiate(baseObj);
        Debug.Log("instantiated selection gameobject");
        selection.transform.parent = transform;
        selection.transform.localPosition = Vector3.zero;
        selection.SetActive(false);


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
	    if (onSelect)
        {
        }
	}
}
