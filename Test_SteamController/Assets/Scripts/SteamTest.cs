using UnityEngine;
using System.Collections;
using Steamworks;

public class SteamTest : MonoBehaviour {

    // Use this for initialization
    
    ControllerHandle_t[] controllerTab;
    ControllerActionSetHandle_t controllerSet;
    void Start()
    {
        if (SteamManager.Initialized)
        {
            string name = SteamFriends.GetPersonaName();
            Debug.Log(name);

            SteamController.Init();
            controllerTab = new ControllerHandle_t[2];

            controllerSet = SteamController.GetActionSetHandle("title");

            Debug.Log(SteamController.GetConnectedControllers(controllerTab));
            Debug.Log(controllerTab[0].ToString());
            

        }
    }
	
	// Update is called once per frame
	void Update () {

	}
}
