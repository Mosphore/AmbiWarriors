using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{

    public int cam_ID;
    Camera cam;

    GameObject arm;

    public Rigidbody bullet;

    public Texture2D crosshairImage;

    //vitesse de deplacement de la camera
    public float speedH = 2.0f;
    public float speedV = 2.0f;
    public float bulletSpeed = 50.0f;

    //les rotations de camera
    float yaw = 0.0f;
    float pitch = 0.0f;
    Quaternion lookRotation;
    Vector3 direction;

    // pour la conversion des inputs en deplacement de viseur
    float prev_x;
    float prev_y;
    float x;
    float y;
    float mov_x;
    float mov_y;

    //pour positionner le viseur
    Vector2 camViewportCenter;
    float targetPosX = 0;        // la position en x de la cible depuis le centre de la cam
    float targetPosY = 0;

    // Use this for initialization
    void Start()
    {
        cam = GetComponent<Camera>();
        camViewportCenter = new Vector2(cam.rect.position.x * Screen.width + cam.rect.width * Screen.width / 2, Screen.height / 2);
        arm = GetComponentInChildren<Transform>().gameObject;


    }

    void OnDrawGizmosSelected()
    {

        //Vector3 p = cam.ScreenToWorldPoint(new Vector3(camViewportCenter.x + targetPosX, camViewportCenter.y - targetPosY, 10));
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(p, 0.1F);
    }


    void OnGUI()
    {
        //Debug.Log(cam.rect.position.x + " " + cam.rect.position.y);
        GUI.DrawTexture(new Rect(camViewportCenter.x + targetPosX - crosshairImage.width / 2, camViewportCenter.y - crosshairImage.height / 2 + targetPosY, crosshairImage.width, crosshairImage.height), crosshairImage);
        //GUI.Box(new Rect(Screen.width / 2, Screen.height / 2, 10, 10), "");
    }

    // Update is called once per frame
    void Update()
    {

        if (cam_ID == 0)
        {
            x = Input.GetAxis("Horizontal");

            y = Input.GetAxis("Vertical");
        }
        else if (cam_ID == 2)
        {
            x = Input.GetAxis("Horizontal2");

            y = Input.GetAxis("Vertical2");
        }


        /*EN CAS DE RETOUR AU CONTROLLES SOURIS SANS POINT DE RETOUR IL FAUT GARDER LE TAS DE CODE COMMENTE EN DESSOUS
            if (x != 0 && prev_x != 0)
                mov_x = x - prev_x;

            if (y != 0 && prev_y != 0)
                mov_y = y - prev_y;

            if (x == 0 && prev_x == 0)
                mov_x = 0;

            if (y == 0 && prev_y == 0)
            {
                mov_y = 0;
            }

            prev_x = x;
            prev_y = y;*/


        targetPosX = x * 300;// mov_x * 200;
        targetPosY = y * 300;//mov_y * 200;

        if (x == 0 && y == 0)
        {
            targetPosX = targetPosY = 0;
        }

        // ROTATION DU BRAS SUR LE VISEUR A FAIRE
        ////find the vector pointing from our position to the target
        //direction = (Target.position - transform.position).normalized;

        ////create the rotation we need to be in to look at the target
        //lookRotation = Quaternion.LookRotation(direction);

        ////rotate us over time according to speed until we are in the required rotation
        //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotationSpeed);

        // MOUVEMENT DE CAMERA A FAIRE

        //yaw += speedH * mov_x * 20;
        //pitch += speedV * mov_y * 20;
        //transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);


        // TIRS A FAIRE (
        //Vector3 p = cam.ScreenToWorldPoint(new Vector3(camViewportCenter.x + targetPosX - crosshairImage.width / 2, camViewportCenter.y - targetPosY - crosshairImage.height / 2, 100));

        //Rigidbody bulletClone = (Rigidbody)Instantiate(bullet, transform.position, Quaternion.identity);
        //bulletClone.velocity = p.normalized * bulletSpeed;

    }
}
