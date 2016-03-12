using UnityEngine;
using System.Collections;

public class joysticktest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log(Input.GetAxis("Horizontal2"));
        transform.Translate(Input.GetAxis("Horizontal2") *20* Time.deltaTime, 0, 0);
	}
}
