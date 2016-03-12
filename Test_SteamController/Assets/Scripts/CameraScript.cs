using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{

    public int cam_ID;
    Camera cam;

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private float yaw2 = 0.0f;
    private float pitch2 = 0.0f;
    // Use this for initialization
    void Start()
    {
        cam = GetComponent<Camera>();

        if (cam_ID == 0)
        {
            cam.rect = new Rect(0, 0, 0.35f, 1);
        }
        else if (cam_ID == 1)
        {
            cam.rect = new Rect(0.35f, 0, 0.3f, 1);
        }
        else if (cam_ID == 2)
        {
            cam.rect = new Rect(0.65f, 0, 0.35f, 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(cam_ID == 0)
        {
            yaw2 += speedH * Input.GetAxis("Horizontal");
            pitch2 -= speedV * Input.GetAxis("Vertical");

            transform.eulerAngles = new Vector3(pitch2, yaw2, 0.0f);
        }
        else if (cam_ID == 2)
        {
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

    }
}
