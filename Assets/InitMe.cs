using UnityEngine;
using System.Collections;

public class InitMe : MonoBehaviour {
    public bool ShowVRPreview = true;
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

        //IMPORTANT disables VR preview, enables regular camera
        UnityEngine.VR.VRSettings.showDeviceView = ShowVRPreview;
        if (ShowVRPreview)
        {
            GameObject camera = GameObject.Find("Camera");
            camera.SetActive(false);
        }
    }

    public void EnableTargetSelection()
    {
        UXStateManager.EnableTargetSelection();
    }
	// Update is called once per frame
	void Update () {
	
	}
}
