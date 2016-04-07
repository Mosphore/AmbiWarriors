using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Character : NetworkBehaviour
{
    enum SHOOT_TYPE
    {
        LEFT_MISSILE = 0,
        LEFT_MACHINEGUN,
        RIGHT_MISSILE,
        RIGHT_MACHINEGUN
    };
    //[SyncVar]
    public float life = 1000;
    Texture2D texHealth = null;
    GUIStyle texHealthStyle = null;

    public GameObject missile;
    public Texture2D crosshairImage;
    public Texture2D cockpit;

    //deplacement du personnage
    public float rotationSpeed = 1;
    public float moveSpeed = 20;

    //deplacement des cam laterales
    Transform leftCam, rightCam;
    public float aimRotationSpeed = 1;
    
    public float camMoveZone = 0.7f; //a partir de quel seuil sur le pad la camera doit bouger
    public float camMaxInterior = 10;
    public float camMaxExterior = 80;
    float yawLeft = 0.0f;
    float yawRight = 0.0f;
    Vector2 leftCrosshairMove, rightCrosshairMove;

    //Missile et Tirs
    public Transform shootSpawnLeft, shootSpawnRight;
    public float MissileSpeed = 50.0f;
    float leftShootTimer=0,
        rightShootTimer=0;
    // Use this for initialization
    void Start()
    {

        //active la bonne camera pour le perso sur le serveur
        if (isLocalPlayer)
        {
            if (texHealth == null)
                texHealth = new Texture2D(1, 1);

            if (texHealthStyle == null)
                texHealthStyle = new GUIStyle();

            texHealth.SetPixel(0, 0, Color.green);
            texHealth.Apply();
            texHealthStyle.normal.background = texHealth;

            leftCam = transform.FindChild("Camera_Left");
            rightCam = transform.FindChild("Camera_Right");

            GetComponentsInChildren<Camera>()[0].enabled = true;
            GetComponentsInChildren<Camera>()[1].enabled = true;
            GetComponentsInChildren<Camera>()[2].enabled = true;

            GetComponentInChildren<AudioListener>().enabled = true;
        }
    }

    void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        //dessin des cibles (crosshair)
        GUI.DrawTexture(new Rect(leftCam.GetComponent<Camera>().pixelRect.center.x - 180 + leftCrosshairMove.x - crosshairImage.width / 2,
                                 leftCam.GetComponent<Camera>().pixelRect.center.y -100 + leftCrosshairMove.y - crosshairImage.height / 2,
                                 crosshairImage.width, crosshairImage.height),
                                 crosshairImage);

        GUI.DrawTexture(new Rect(rightCam.GetComponent<Camera>().pixelRect.center.x +180+ rightCrosshairMove.x - crosshairImage.width / 2,
                                 rightCam.GetComponent<Camera>().pixelRect.center.y -100+ rightCrosshairMove.y - crosshairImage.height / 2,
                                 crosshairImage.width, crosshairImage.height),
                                 crosshairImage);

        GUI.DrawTexture(new Rect(0, 0,Screen.width, Screen.height),cockpit);

        //barre de vie a l'arrache


        GUI.Box(new Rect(Screen.width/2,Screen.height/2,100,10), GUIContent.none, texHealthStyle);
        //GUI.DrawTexture(new Rect(Screen.width / 2, Screen.height / 2, 100, 20),texHealth, ScaleMode.ScaleAndCrop);

    }

    //on calcule la position et l'orientation du tir selon le bras qui a tiré et le type de tir avant d'envoyer le tout au serveur avec la [Command]
    //pour eviter d'nevoyer les variables locales au serveur.
    void CalcShoot(SHOOT_TYPE st)
    {
        if (st == SHOOT_TYPE.RIGHT_MISSILE)
        {

            Vector3 p = rightCam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(rightCam.GetComponent<Camera>().pixelRect.center.x +180 + rightCrosshairMove.x,
                                                                                       rightCam.GetComponent<Camera>().pixelRect.center.y +100 -rightCrosshairMove.y, 100));
            Vector3 shootDir = p - shootSpawnRight.position;
            shootDir.Normalize();
            CmdShootMissile(shootSpawnRight.position, shootDir);
        }
        if (st == SHOOT_TYPE.LEFT_MISSILE)
        {
            Debug.Log("shootleft");
            Vector3 p = leftCam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(leftCam.GetComponent<Camera>().pixelRect.center.x - 180 + leftCrosshairMove.x,
                                                                                       leftCam.GetComponent<Camera>().pixelRect.center.y + 100 - leftCrosshairMove.y, 100));
            Vector3 shootDir = p - shootSpawnLeft.position;
            shootDir.Normalize();
            CmdShootMissile(shootSpawnLeft.position, shootDir);
        }
    }

    [Command]
    void CmdShootMissile(Vector3 position, Vector3 direction)
    {
        //tir d'objet physique (missile )
        GameObject MissileClone = (GameObject)Instantiate(missile, position, Quaternion.identity);
        MissileClone.GetComponent<Rigidbody>().velocity = direction * MissileSpeed;
        NetworkServer.Spawn(MissileClone);
        Destroy(MissileClone, 10.0f);

        //if (Input.GetAxisRaw("Fire4") == 1 && cam_ID == 0)
        //{
        //    Vector3 p = cam.ScreenToWorldPoint(new Vector3(cam.pixelRect.center.x + targetPos.x, cam.pixelRect.center.y - targetPos.y, 100));
        //    Vector3 shootDir = p - transform.position;

        //    //tir d'objet physique (missile )
        //    Rigidbody bulletClone = (Rigidbody)Instantiate(bullet, transform.position, Quaternion.identity);
        //    bulletClone.velocity = shootDir.normalized * MissileSpeed;
        //}



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

    void MoveCam()
    {
        Vector2 leftInput, rightInput;

        leftInput.x = Input.GetAxis("Horizontal");

        leftInput.y = Input.GetAxis("Vertical");


        rightInput.x = Input.GetAxis("Horizontal2");

        rightInput.y = Input.GetAxis("Vertical2");

        leftCrosshairMove.x = leftInput.x * 300;
        leftCrosshairMove.y = leftInput.y * 300;

        rightCrosshairMove.x = rightInput.x * 300;
        rightCrosshairMove.y = rightInput.y * 300;

        if (leftInput.x == 0 && leftInput.y == 0)
        {
            leftCrosshairMove.x = leftCrosshairMove.y = 0;
        }

        if (rightInput.x == 0 && rightInput.y == 0)
        {
            rightCrosshairMove.x = rightCrosshairMove.y = 0;
        }

        if (leftInput.x > camMoveZone || leftInput.x < -camMoveZone)
        {

            yawLeft += leftInput.x * aimRotationSpeed;

            if (yawLeft < -camMaxExterior)
            {
                yawLeft = -camMaxExterior;
            }
            else if (yawLeft > camMaxInterior)
            {
                yawLeft = camMaxInterior;
            }


            leftCam.localEulerAngles = new Vector3(0, yawLeft, 0.0f);
        }

        if (rightInput.x > camMoveZone || rightInput.x < -camMoveZone)
        {

            yawRight += rightInput.x * aimRotationSpeed;

            if (yawRight > camMaxExterior)
            {
                yawRight = camMaxExterior;
            }
            else if (yawRight < -camMaxInterior)
            {
                yawRight = -camMaxInterior;
            }


            rightCam.localEulerAngles = new Vector3(0, yawRight, 0.0f);
        }
    }

    public void LoseLife(int damage)
    {
        life -= damage;
        texHealth.SetPixel(0, 0, Color.Lerp(Color.red, Color.green, life/1000.0f));
        texHealth.Apply();
        texHealthStyle.normal.background = texHealth;
        //Debug.Log("life lost");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;

        //if (Input.GetKey(KeyCode.T))
        //{
        //    LoseLife(5);
        //}
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

        MoveCam();

        leftShootTimer += Time.deltaTime;
        rightShootTimer += Time.deltaTime;

        if (Input.GetAxisRaw("Fire1") > 0 && rightShootTimer >= 0.1f)
        {
            rightShootTimer = 0;
            CalcShoot(SHOOT_TYPE.RIGHT_MISSILE);
        }
        if (Input.GetAxisRaw("Fire3") > 0 && leftShootTimer >= 0.1f)
        {
            leftShootTimer = 0;
            CalcShoot(SHOOT_TYPE.LEFT_MISSILE);
        }
        if (Input.GetAxisRaw("Fire2") > 0)
            CalcShoot(SHOOT_TYPE.RIGHT_MACHINEGUN);

        // fonction pour shoot a la mitraille
        //if (st == SHOOT_TYPE.RIGHT_MACHINEGUN)
        //{
        //    Vector3 p = rightCam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(rightCam.GetComponent<Camera>().pixelRect.center.x + rightCrosshairMove.x,
        //                                                                               rightCam.GetComponent<Camera>().pixelRect.center.y - rightCrosshairMove.y, 100));

        //    Vector3 shootDir = p - rightCam.position;

        //    //tir gatling
        //    RaycastHit hit;
        //    if (Physics.Raycast(rightCam.position, shootDir, out hit))
        //    {
        //        if (hit.transform.tag == "Character")
        //        {
        //            hit.transform.GetComponent<Character>().LoseLife(2);
        //            Debug.Log("hitchar_RIGHT");
        //        }
        //    }
        //}

    }
}
