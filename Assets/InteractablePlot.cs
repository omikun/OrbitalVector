using UnityEngine;
using System.Collections;
using VRTK;

public class InteractablePlot : VRTK_InteractableObject {
    GameObject selector;
    public override void StartUsing(GameObject currentUsingObject)
    {
        base.StartUsing(currentUsingObject);

        Ray ray = new Ray(currentUsingObject.transform.position, currentUsingObject.transform.forward);
        RaycastHit hit;
        if (GetComponent<Collider>().Raycast(ray, out hit, 100))
        {
            //SetSelectorPosition(hit.point);
            selector.transform.position = new Vector3(hit.point.x, hit.point.y, selector.transform.position.z);
        }
        Debug.Log("Using plot!");
    }
    
    void SetSelectorPosition(Vector3 point)
	{
        bool fixX = false;
        bool fixY = false;
        var temp = selector.transform.localPosition;
        selector.transform.position = point;
		selector.transform.localPosition = new Vector3(fixX ? selector.transform.localPosition.x : point.x, fixY ? selector.transform.localPosition.y : point.y, selector.transform.localPosition.z);
	}
    public override void StopUsing(GameObject previousUsingObject)
    {
        base.StopUsing(previousUsingObject);
    }
	// Use this for initialization
	void Start () {
        base.Start();
        selector = GameObject.Find("selector");
        if (selector == null)
            Debug.Log("cann't find selector in plot");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
