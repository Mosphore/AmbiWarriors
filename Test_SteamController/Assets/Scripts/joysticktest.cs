using UnityEngine;
using System.Collections;

public class joysticktest : MonoBehaviour
{

    public float rotationSpeed = 1;
    public float moveSpeed = 20;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, rotationSpeed, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, -rotationSpeed, 0);
        }
        //Debug.Log( Input.GetAxis("Horizontal"));
        transform.position += (transform.right * Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime);


        //Debug.Log( Input.GetAxis("Vertical"));
        transform.position += transform.forward * Input.GetAxis("Mouse Y") * -moveSpeed * Time.deltaTime;


    }
}
