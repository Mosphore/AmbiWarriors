using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{

    public int cam_ID;
    Camera cam;

    GameObject arm;

    public Rigidbody bullet;

    public Texture2D crosshairImage;

    //vitesse de deplacement de la cam
    public float speedH = 2.0f;
    public float speedV = 2.0f;
    public float bulletSpeed = 50.0f;

    // les rotations de camera
    float yaw = 0.0f;
    float pitch = 0.0f;

    //values for internal use
    private Quaternion lookRotation;
    private Vector3 direction;

    // pour la conversion des inputs en deplacement de viseur
    float prev_x;
    float prev_y;
    float x;
    float y;

    float mov_x;
    float mov_y;

    // pour positionner le viseur
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
        GUI.DrawTexture(new Rect(camViewportCenter.x + targetPosX - crosshairImage.width/2, camViewportCenter.y - crosshairImage.height/2 + targetPosY, crosshairImage.width, crosshairImage.height), crosshairImage);
        //GUI.Box(new Rect(Screen.width / 2, Screen.height / 2, 10, 10), "");
    }

    // Update is called once per frame
    void Update()
    {
        ////find the vector pointing from our position to the target
        //direction = (Target.position - transform.position).normalized;

        ////create the rotation we need to be in to look at the target
        //lookRotation = Quaternion.LookRotation(direction);

        ////rotate us over time according to speed until we are in the required rotation
        //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotationSpeed);
        if (cam_ID == 0)
        {
            Debug.Log(x = Input.GetAxis("Horizontal"));

            Debug.Log(y = Input.GetAxis("Vertical"));
        }
        else if (cam_ID == 2)
        {
            Debug.Log(x = Input.GetAxis("Horizontal2"));

            Debug.Log(y = Input.GetAxis("Vertical2"));
        }
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
            prev_y = y;


            targetPosX = x * 300;// mov_x * 200;
            targetPosY = y * 300;//mov_y * 200;

            if (x == 0 && y == 0)
            {
                targetPosX = targetPosY = 0;
            }

            //yaw += speedH * mov_x * 20;
            //pitch += speedV * mov_y * 20;
            //transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);


            ///////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////

            ////yaw += speedH * Input.GetAxis("Mouse X");
            ////pitch -= speedV * Input.GetAxis("Mouse Y");
            //x = Input.GetAxis("Mouse X");
            //y = Input.GetAxis("Mouse Y");

            //mov_x = x - prev_x;
            //mov_y = y - prev_y;

            //targetPosX += mov_x * 10;
            //targetPosY -= mov_y * 10;

            ////transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        

        Vector3 p = cam.ScreenToWorldPoint(new Vector3(camViewportCenter.x + targetPosX - crosshairImage.width / 2, camViewportCenter.y - targetPosY - crosshairImage.height / 2, 100));

        Rigidbody bulletClone = (Rigidbody)Instantiate(bullet, transform.position, Quaternion.identity);
        bulletClone.velocity = p.normalized * bulletSpeed;

    }
}
