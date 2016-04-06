using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{

    public int cam_ID; // 0 cam gauche, 2 cam droite
    Camera cam;

    GameObject arm;

    public Rigidbody bullet;
    public Texture2D crosshairImage;

    //vitesse de deplacement de la camera
    public float aimRotationSpeed = 1;
    public float bulletSpeed = 50.0f;
    public float camMoveZone = 0.7f; //a partir de quel seuil sur le pad la camera doit bouger
    public float camMaxInterior = 10;
    public float camMaxExterior = 80;

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
    Vector2 camScreenToViewportCenter;
    Vector2 camViewportCenter;
    Vector2 targetPos; // position de la cible au centre de chaque camera

    // Use this for initialization
    void Start()
    {
        cam = GetComponent<Camera>();
        camScreenToViewportCenter = new Vector2(cam.rect.position.x * Screen.width + cam.rect.width * Screen.width / 2, Screen.height / 2);
        arm = GetComponentInChildren<Transform>().gameObject;

        //camViewportCenter = cam.Rect.center;


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
        GUI.DrawTexture(new Rect(cam.pixelRect.center.x + targetPos.x - crosshairImage.width, cam.pixelRect.center.y + targetPos.y - crosshairImage.height, crosshairImage.width, crosshairImage.height), crosshairImage);
        //GUI.Box(new Rect(Screen.width / 2, Screen.height / 2, 10, 10), "");
    }

    void Shoot()
    {
        if (Input.GetAxisRaw("Fire1") == 1 && cam_ID == 2)
        {
            Vector3 p = cam.ScreenToWorldPoint(new Vector3(cam.pixelRect.center.x + targetPos.x, cam.pixelRect.center.y - targetPos.y, 100));
            Vector3 shootDir = p - transform.position;

            //tir d'objet physique (missile )
            Rigidbody bulletClone = (Rigidbody)Instantiate(bullet, transform.position, Quaternion.identity);
            bulletClone.velocity = shootDir.normalized * bulletSpeed;
        }
        //if (Input.GetAxisRaw("Fire4") == 1 && cam_ID == 0)
        //{
        //    Vector3 p = cam.ScreenToWorldPoint(new Vector3(cam.pixelRect.center.x + targetPos.x, cam.pixelRect.center.y - targetPos.y, 100));
        //    Vector3 shootDir = p - transform.position;

        //    //tir d'objet physique (missile )
        //    Rigidbody bulletClone = (Rigidbody)Instantiate(bullet, transform.position, Quaternion.identity);
        //    bulletClone.velocity = shootDir.normalized * bulletSpeed;
        //}

        if (Input.GetAxisRaw("Fire2") > 0 && cam_ID == 0 )
        {
            Vector3 p = cam.ScreenToWorldPoint(new Vector3(cam.pixelRect.center.x + targetPos.x, cam.pixelRect.center.y - targetPos.y, 100));
            Vector3 shootDir = p - transform.position;
            //tir gatling
            RaycastHit hit;
            if (Physics.Raycast(transform.position, shootDir, out hit))
            {
                if (hit.transform.tag == "Character")
                {
                    hit.transform.GetComponent<Character>().life -= 2;
                    Debug.Log("hitchar" + cam_ID);
                }
            }
        }

        //if (Input.GetAxisRaw("Fire5") > 0 && cam_ID == 2)
        //{
        //    Vector3 p = cam.ScreenToWorldPoint(new Vector3(cam.pixelRect.center.x + targetPos.x, cam.pixelRect.center.y - targetPos.y, 100));
        //    Vector3 shootDir = p - transform.position;
        //    //tir gatling
        //    RaycastHit hit;d
        //    if (Physics.Raycast(transform.position, shootDir, out hit))
        //    {
        //        if (hit.transform.tag == "Character")
        //        {
        //            hit.transform.GetComponent<Character>().life -= 2;
        //            Debug.Log("hitchar" + cam_ID);
        //        }
        //    }
        //}
    }

    void Move()
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

        targetPos.x = x * 300;// mov_x * 200;
        targetPos.y = y * 300;//mov_y * 200;

        if (x == 0 && y == 0)
        {
            targetPos.x = targetPos.y = 0;
        }

        if (x > camMoveZone || x < -camMoveZone)
        {

            yaw += x * aimRotationSpeed;
            if (cam_ID == 0)
            {
                if (yaw < -camMaxExterior)
                {
                    yaw = -camMaxExterior;
                }
                else if (yaw > camMaxInterior)
                {
                    yaw = camMaxInterior;
                }
            }
            if (cam_ID == 2)
            {
                if (yaw > camMaxExterior)
                {
                    yaw = camMaxExterior;
                }
                else if (yaw < -camMaxInterior)
                {
                    yaw = -camMaxInterior;
                }
            }

            transform.eulerAngles = new Vector3(0, yaw, 0.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {

        Move();
        Shoot();
        // ROTATION DU BRAS SUR LE VISEUR A FAIRE
        ////find the vector pointing from our position to the target
        //direction = (Target.position - transform.position).normalized;

        ////create the rotation we need to be in to look at the target
        //lookRotation = Quaternion.LookRotation(direction);

        ////rotate us over time according to speed until we are in the required rotation
        //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotationSpeed);

        // TIRS

        //Vector3 p = cam.ScreenToWorldPoint(new Vector3(camViewportCenter.x + targetPosX - crosshairImage.width / 2, camViewportCenter.y - targetPosY - crosshairImage.height / 2, 100));

        //Rigidbody bulletClone = (Rigidbody)Instantiate(bullet, transform.position, Quaternion.identity);
        //bulletClone.velocity = p.normalized * bulletSpeed;

    }
}
