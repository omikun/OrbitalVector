using UnityEngine;
using System.Collections;

public static class DataStore { 

    //public static GameObject userSelection; //TODO: make this an array or linked list
    public static bool simpleMovement = false;
}

public enum SelectStates
{
    SELECT_SOURCE,
    SELECT_TARGET
}
public enum UXStates
{
    IDLE,
    MOVE,
    ATTACK,
    SCAN,
    JAM,
    CONFIG
}
public static class UXStateManager
{
    static SelectStates selectState;
    static UXStates uxState;
    static GameObject selectedSource, selectedTarget;
    public static void SelectUnit(GameObject unit)
    {
        switch (selectState)
        {
            case SelectStates.SELECT_SOURCE:
                selectedSource = unit;
                selectState = SelectStates.SELECT_TARGET;
                ClearTarget();
                break;
            case SelectStates.SELECT_TARGET:
                selectedTarget = unit;
                selectState = SelectStates.SELECT_SOURCE;
                break;
        }
    }
    public static void ClearSource() { selectedSource = null; }
    public static void ClearTarget() { selectedTarget = null; }
    public static GameObject GetSource() { return selectedSource; }
    public static GameObject GetTarget() { return selectedTarget; }

    static void Update()
    {
        switch (uxState)
        {
            case UXStates.IDLE:
                //if 
                break;
            case UXStates.MOVE:
                break;
            case UXStates.ATTACK:
                break;
            case UXStates.SCAN:
                break;
            case UXStates.JAM:
                break;
            case UXStates.CONFIG:
                break;
        }
    }
}
