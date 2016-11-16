using UnityEngine;
using System.Collections;
using VRTK;
using UnityEngine.Events;

public class UIButton : VRTK_InteractableObject {

    GameObject tooltip;
    public UnityEvent OnEvent;
    public UnityEvent OnDragEvent;
    int count = 0;
    bool hover = false;
    bool prevHover = false;
    bool falseUsing = false;

    public override void StartUsing(GameObject currentUsingObject)
    {
        //Debug.Log("using moveIntercept: "+count++);
        if (tooltip)
            tooltip.SetActive(true);
        FadeOverTime(2);
        hover = true;
        Drag();
    }
    public override void StopUsing(GameObject currentUsingObject)
   { 
        Debug.Log("StopUsing button");
        if (tooltip)
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
        if (tooltip)
            tooltip.SetActive(false);
	}

    public void Hover()
    {
        if (tooltip)
            tooltip.SetActive(true);
        FadeOverTime(2);
    }
    bool clicked = false;
	public void Clicked() {
        Debug.Log("UIButton clicked received");
        clicked = true;
	}
    public void Click()
    {
		OnEvent.Invoke ();
        FadeOverTime(2);
    }
    // Update is called once per frame
    //FIXME HACK should not need to use update to get both hover and click
    float fadeTime, startTime;
    void FadeOverTime(float time)
    {
        var progress = 0;
        fadeTime = time;
        startTime = Time.time;
    }
    public void Drag()
    {
        if (OnDragEvent == null)
            return;
        OnDragEvent.Invoke();
    }
    void Update () {
        if (prevHover && !hover && !falseUsing)
        {
            clicked = true;
        }
        if (clicked)
        {
            clicked = false;
            Click();
        }
        prevHover = hover;
        hover = false;
        falseUsing = false;

        if (Time.time > fadeTime + startTime)
        {
            if (tooltip)
                tooltip.SetActive(false);
        }
    }
}
