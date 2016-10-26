using UnityEngine;
using System.Collections;

public class EnableRadialMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (UXStateManager.getUXState() == UXStates.IDLE)
        {
            var menu = GetComponent<RadialMenu>();
            menu.ShowMenu();
        } else if (UXStateManager.getUXState() == UXStates.HIDDEN)
        {
            var menu = GetComponent<RadialMenu>();
            menu.HideMenu(true);
        }
	}
}
