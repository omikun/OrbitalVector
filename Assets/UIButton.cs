using UnityEngine;
using System.Collections;
using VRTK;
using UnityEngine.Events;

public class UIButton : VRTK_InteractableObject {

    GameObject tooltip;
    public UnityEvent OnEvent;
    int count = 0;
    bool hover = false;
    bool prevHover = false;
    bool falseUsing = false;

    public override void StartUsing(GameObject currentUsingObject)
    {
        //Debug.Log("using moveIntercept: "+count++);
        tooltip.SetActive(true);
        hover = true;
    }
    public override void StopUsing(GameObject currentUsingObject)
   { 
        Debug.Log("NOT using moveIntercept");
        tooltip.SetActive(false);
        falseUsing = true;
    }
    public override void OnInteractableObjectTouched (InteractableObjectEventArgs e)
    {
        //Debug.Log("touching!");
    }
	// Use this for initialization
	void Start () {
        base.Start();
        tooltip = transform.Find("Tooltip").gameObject;
        tooltip.SetActive(false);
	}

	public void Click() {
		OnEvent.Invoke ();
		tooltip.SetActive (false);
	}
	// Update is called once per frame
    //FIXME HACK should not need to use update to get both hover and click
	void Update () {
        if (prevHover && !hover && !falseUsing)
        {
			Click ();
        }
        prevHover = hover;
        hover = false;
        falseUsing = false;
    }
}
