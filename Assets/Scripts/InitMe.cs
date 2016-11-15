using UnityEngine;
using System.Collections;

public class InitMe : MonoBehaviour {
	private static InitMe instance;
    public bool ShowVRPreview = true;
    public bool EnableVR = true;
	public static InitMe GetInstance() {
		return instance;
	}
	// Use this for initialization
	void Awake () {
		if (!instance) {
			instance = this;
		} else {
			return;
		}
	}
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
        GameObject camera = GameObject.Find("Camera");
        camera.SetActive(!ShowVRPreview);
        GameObject cameraRig = GameObject.Find("[CameraRig]");
        cameraRig.SetActive(EnableVR);
        GameObject steamVR = GameObject.Find("[SteamVR]");
        steamVR.SetActive(EnableVR);
    }

    public void ToggleTargetSelection()
    {
        UXStateManager.ToggleTargetSelection();
    }
	// Update is called once per frame
	void Update () {
	
	}
}
