﻿using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Character")
        {
            collision.transform.GetComponent<Character>().life -= 10;
            Destroy(gameObject);
        }
    }
}
