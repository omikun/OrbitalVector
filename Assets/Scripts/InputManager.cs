using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using XboxCtrlrInput;		// Be sure to include this if you want an object to have Xbox input

public class InputManager : MonoBehaviour {
public XboxController controller;
    private const float MAX_TRG_SCL = 1.21f;
    public void QueryControllers()
    {
        int queriedNumberOfCtrlrs = XCI.GetNumPluggedCtrlrs();

        if (queriedNumberOfCtrlrs == 1)
        {
            Debug.Log("Only " + queriedNumberOfCtrlrs + " Xbox controller plugged in.");
        }
        else if (queriedNumberOfCtrlrs == 0)
        {
            Debug.Log("No Xbox controllers plugged in!");
        }
        else
        {
            Debug.Log(queriedNumberOfCtrlrs + " Xbox controllers plugged in.");
        }

        XCI.DEBUG_LogControllerNames();

        // This code only works on Windows
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            Debug.Log("Windows Only:: Any Controller Plugged in: " + XCI.IsPluggedIn(XboxController.Any).ToString());

            Debug.Log("Windows Only:: Controller 1 Plugged in: " + XCI.IsPluggedIn(XboxController.First).ToString());
            Debug.Log("Windows Only:: Controller 2 Plugged in: " + XCI.IsPluggedIn(XboxController.Second).ToString());
            Debug.Log("Windows Only:: Controller 3 Plugged in: " + XCI.IsPluggedIn(XboxController.Third).ToString());
            Debug.Log("Windows Only:: Controller 4 Plugged in: " + XCI.IsPluggedIn(XboxController.Fourth).ToString());
        }
    }
    bool lastPrint = false;
    public delegate void ActionDelegate();
    public ActionDelegate PressA, PressB, PressX, PressY;
    public ActionDelegate PressRightStick, pressLeftStick;
    public Action<float, float> MoveLeftStick, MoveRightStick;
    public Action<float> PressLeftTrigger, PressRightTrigger;

    public void GetController()
    {
        //RightStick
        if (XCI.GetButtonDown(XboxButton.RightStick, controller))
        {
            //TODO call rightstick delegate, which can be remapped to func
            PressRightStick();
        }
        if (XCI.GetButtonDown(XboxButton.A, controller))
        {
            PressA();
        }
        if (XCI.GetButtonDown(XboxButton.B, controller))
        {
            PressB();
        }
        if (XCI.GetButtonDown(XboxButton.X, controller))
        {
            PressX();
        }
        if (XCI.GetButtonDown(XboxButton.Y, controller))
        {
        }

        {
            // Left stick movement
            float axisY = XCI.GetAxis(XboxAxis.LeftStickY, controller);
            //if (axisY != 0) Debug.Log("axisY: " + axisY);
            float axisX = -XCI.GetAxis(XboxAxis.LeftStickX, controller);
            //if (axisX != 0) Debug.Log("axisX: " + axisX);
            //actual camera+ship rotation
            MoveLeftStick(axisY, axisX);
            //just ship rotation, for show
        }
        {
            // Right stick movement
            float axisY = XCI.GetAxis(XboxAxis.RightStickY, controller);
            //if (axisY != 0) Debug.Log("axisY: " + axisY);
            float axisX = -XCI.GetAxis(XboxAxis.RightStickX, controller);
            //if (axisX != 0) Debug.Log("axisX: " + axisX);
            //rotate from default pos by up to 180 degrees on each axis
            MoveRightStick(axisY, axisY);
        }

        {
            // Trigger input
            float leftTrigHeight = ( XCI.GetAxis(XboxAxis.LeftTrigger, controller));
            PressLeftTrigger(-leftTrigHeight);
            //Debug.Log("left trigger: " + leftTrigHeight);
            float rightTrigHeight = (XCI.GetAxis(XboxAxis.RightTrigger, controller));
            PressLeftTrigger(rightTrigHeight);


            // Bumper input
            if (XCI.GetButtonDown(XboxButton.LeftBumper, controller))
            {
            }
            if (XCI.GetButtonDown(XboxButton.RightBumper, controller))
            {
            }
        }

    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
