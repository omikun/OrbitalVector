using UnityEngine;
using System.Collections;
using VRTK;

public class InteractablePlot : VRTK_InteractableObject {
    GameObject selector;
    PorkChopPlot pcp;
    public override void StartUsing(GameObject currentUsingObject)
    {
        base.StartUsing(currentUsingObject);

        Ray ray = new Ray(currentUsingObject.transform.position, currentUsingObject.transform.forward);
        RaycastHit hit;
        if (GetComponent<Collider>().Raycast(ray, out hit, 100))
        {
            //SetSelectorPosition(hit.point);
            selector.transform.position = new Vector3(hit.point.x, hit.point.y, selector.transform.position.z);
            var plotCoord = new Vector2(hit.point.x, hit.point.y);
            plotCoord.x *= 1 / transform.localScale.x;
            plotCoord.y -= transform.position.y;
            plotCoord.y *= 1 / transform.localScale.y;
            pcp.SelectedTrajectory(plotCoord);
        }
    }
    public void StartMouseUsing(RaycastHit hitInfo)
    {
        {
            selector.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, selector.transform.position.z);
            var fixedZ = selector.transform.localPosition;
            fixedZ.z = -.05f;
            selector.transform.localPosition = fixedZ;
            var plotCoord = new Vector2(hitInfo.point.x, hitInfo.point.y);
            plotCoord.x *= 1 / transform.localScale.x;
            plotCoord.y -= transform.position.y;
            plotCoord.y *= 1 / transform.localScale.y;
            pcp.SelectedTrajectory(plotCoord);
        }
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

        pcp = GetComponent<PorkChopPlot>();
        if (pcp == null)
            Debug.Log("can't find porkchopplot component");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
