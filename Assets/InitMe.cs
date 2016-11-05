using UnityEngine;
using System.Collections;

public class InitMe : MonoBehaviour {

	// Use this for initialization
	void Start () {
        UXStateManager.selectionIcon = GameObject.Find("SelectionIcon");
        Debug.Log("selectionIcon: " + UXStateManager.selectionIcon);
        UXStateManager.selectionIcon.SetActive(false);
        UXStateManager.targetIcon = GameObject.Find("TargetIcon");
        Debug.Log("TargetIcon: " + UXStateManager.targetIcon);
        UXStateManager.targetIcon.SetActive(false);

        var root = GameObject.Find("HoloRoot");
        root.transform.localScale *= .4f;
    }

    public void EnableTargetSelection()
    {
        UXStateManager.EnableTargetSelection();
    }
	// Update is called once per frame
	void Update () {
	
	}
}
