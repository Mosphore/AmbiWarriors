﻿using UnityEngine;

using System.Collections;

public class MissileScript : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Character")
        {
            //collision.transform.GetComponent<Character>().LoseLife(10);
            Destroy(gameObject);
        }
        //else
        //    Destroy(gameObject);
    }
}
