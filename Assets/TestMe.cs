using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class TestMe : MonoBehaviour {

    Thread _thread;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		_thread = new Thread(Porkchop);
        _thread.Start();
	}

    void Porkchop()
    {
#if false
        Debug.Log("Porkchop thread started!");
        while (_threadRunning)
        {
            if (_triggerPork)
            {
                _triggerPork = false;
                Debug.Log("in separate thread, detected _triggerPork");
                GeneratePorkChop();
                porkDone = true;
            }
        }
#endif
    }
}
