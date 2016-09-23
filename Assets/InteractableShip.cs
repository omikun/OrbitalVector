using UnityEngine;
using System.Collections;
using VRTK;

public class InteractableShip : VRTK_InteractableObject {

    bool onSelect = false;
    float timeOnSelect;
    float duration = 0.5f;
    public override void StartUsing(GameObject currentUsingObject)
    {
        base.StartUsing(currentUsingObject);
        onSelect = true;
        timeOnSelect = Time.time;

        Debug.Log("ship selected!");
        DataStore.userSelection = gameObject;
    }

    public override void StopUsing(GameObject previousUsingObject)
    {
        base.StopUsing(previousUsingObject);
    }
    // Use this for initialization
    void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
	    if (onSelect)
        {
            var t = 1 - (Time.time - timeOnSelect) / duration;
            if (t < 0)
            {
                onSelect = false;
                return;
            }
            transform.position += new Vector3(0, .1f * Mathf.Lerp(1, 0, t), 0);
        }
	}
}
