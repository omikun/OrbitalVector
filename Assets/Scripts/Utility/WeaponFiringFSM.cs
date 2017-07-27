using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FSM supports basic timers
// a volley timer
// a finer unit timer that runs per volley
// a reload timer
public class WeaponFiringFSM {
    Timer ReloadTimer;
    Timer VolleyTimer;
    Timer UnitTimer;
    public float ReloadRate = 2;
    public int ReloadAmount = 1;
    public float VolleyInterval = 5;
    public float MissileInterval = .2f;
    public int NumPerVolley = 4;
    public int NumPerUnit = 1;
    public int Ammo, StartingAmmo = 5;
    int NumFired = 0;
    public delegate GameObject ActionDelegate();
    ActionDelegate Action;
    GameObject UnitPrototype; //TODO send this with delegate invocation

    public WeaponFiringFSM(ActionDelegate a) {
        Initialize();
        Action = a;
        //unitPrototype = unit;
    }

    public void Initialize()
    {
        Ammo = StartingAmmo;
        NumFired = 0;
        ReloadTimer = new Timer(ReloadRate);
        VolleyTimer = new Timer(VolleyInterval);
        UnitTimer = new Timer(MissileInterval);
    }

    public void Tick(ref bool fireWeaponFlag)
    {
        //check if should and can fire volley
        if (fireWeaponFlag && VolleyTimer.isReady())
        {
            // can fire but may not necessarily fire; must use isReady above
            if (Ammo > 0 && UnitTimer.ResetIfReady())
            {
                Action();
                NumFired += NumPerUnit;
                Ammo -= NumPerUnit;
                ReloadTimer.ResetIfIdle();
                if (NumFired >= NumPerVolley || Ammo == 0)
                {
                    VolleyTimer.Reset();
                    NumFired = 0;
                    fireWeaponFlag = false;
                }
            }
        }
        //check if need to reload
        //need to reset reload timer only either when time runs out or missile fired
        //reloadTimer needs an idle state: isReady but 
        if (Ammo < StartingAmmo)
        {
            if (ReloadTimer.ResetIfReady())
            {
                Ammo += ReloadAmount;

                if (Ammo >= StartingAmmo)
                {
                    Ammo = StartingAmmo;
                    ReloadTimer.SetIdle();
                }
                Debug.Log("Reloaded ammo: " + Ammo);
            }
        }
    }
}