using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Rewired;

[RequireComponent(typeof(CharacterController))]
public class Character : NetworkBehaviour
{
    bool showcockpit = true;
    enum SHOOT_TYPE
    {
        LEFT_MISSILE = 0,
        LEFT_GATLING,
        RIGHT_MISSILE,
        RIGHT_GATLING
    };

    public int playerId = 0; // The Rewired player id of this character
    private Player player; // The Rewired Player

    [SyncVar]
    public float life = 1000;
    float prevLife = 1000;

    Texture2D texHealth = null;
    GUIStyle texHealthStyle = null;

    //prefabs
    public GameObject missile;

    public GameObject GatlingParticleEffect;
    public GameObject ImpactParticle;

    //sprites HUD
    public Texture2D crosshairImage;
    public Texture2D cockpit;

    //deplacement du personnage
    public float rotationSpeed = 1;
    public float moveSpeed = 20;

    //Camera
    Transform  camMinimap, camTPS, camFPS, camFPSLeft, camFPSRight, // chaque camera du personnage (1ere pers 3eme pers et la minimap
               camPlayer, camLeftAim, camRightAim;// ces 3 dernieres cam servent de couche d'abstraction pour switcher les cameras

    //Viseurs
    public Vector2 leftCrosshairInitPos, rightCrosshairInitPos;
    Vector2 leftCrosshairMove, rightCrosshairMove;
    Vector2 leftInput, rightInput, prevLeftInput, prevRightInput;


    //Bras
    [SyncVar]
    Vector3 leftArmLookAt;
    [SyncVar]
    Vector3 rightArmLookAt;

    public Transform LeftArmTransform, RightArmTransform;
    Vector3 aimPosRight, prevAimPosRight;
    Vector3 aimPosLeft, prevAimPosLeft;

    //Missile et Tirs
    public Transform shootSpawnLeft, shootSpawnRight;
    public float MissileSpeed = 50.0f;
    float leftShootTimer = 0,
          rightShootTimer = 0,
          leftMissileTimer = 0,
          rightMissileTimer = 0;

    void Awake()
    {
        // Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
        player = ReInput.players.GetPlayer(playerId);
        aimPosRight = aimPosLeft = Vector3.zero;
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("Start");
        //active la bonne camera pour le perso sur le serveur
        if (isLocalPlayer)
        {
            leftCrosshairInitPos = rightCrosshairInitPos = new Vector2(Screen.width / 2, Screen.height / 2);
            Cursor.visible = false;
            if (texHealth == null)
                texHealth = new Texture2D(1, 1);

            if (texHealthStyle == null)
                texHealthStyle = new GUIStyle();

            texHealth.SetPixel(0, 0, Color.green);
            texHealth.Apply();
            texHealthStyle.normal.background = texHealth;

            //leftCam = transform.FindChild("Camera_Left");
            //rightCam = transform.FindChild("Camera_Right");
            camFPS = transform.FindChild("Cameras").FindChild("CamFPS");
            camFPSLeft = transform.FindChild("Cameras").FindChild("CamFPSLeft");
            camFPSRight = transform.FindChild("Cameras").FindChild("CamFPSRight");

            camTPS = transform.FindChild("CamTPS");
            camMinimap = transform.FindChild("CamMinimap");
            camPlayer = camFPS;
            camLeftAim = camFPSLeft;
            camRightAim = camFPSRight;

            camPlayer.GetComponent<Camera>().enabled = true;
            camMinimap.GetComponent<Camera>().enabled = true;
            camLeftAim.GetComponent<Camera>().enabled = true;
            camRightAim.GetComponent<Camera>().enabled = true;

            // GetComponentsInChildren<Camera>()[0].enabled = true;
            //GetComponentsInChildren<Camera>()[1].enabled = true;
            //GetComponentsInChildren<Camera>()[2].enabled = true;
            //GetComponentsInChildren<Camera>()[3].enabled = true;// le 4eme c'est la minimap apparement


            GetComponentInChildren<AudioListener>().enabled = true;
        }
    }

    void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        //dessin des cibles (crosshair)
        GUI.DrawTexture(new Rect(leftCrosshairInitPos.x + leftCrosshairMove.x - crosshairImage.width / 2,
                                 leftCrosshairInitPos.y + leftCrosshairMove.y - crosshairImage.height / 2,
                                 crosshairImage.width, crosshairImage.height),
                                 crosshairImage);

        GUI.DrawTexture(new Rect(rightCrosshairInitPos.x + rightCrosshairMove.x - crosshairImage.width / 2,
                                 rightCrosshairInitPos.y + rightCrosshairMove.y - crosshairImage.height / 2,
                                 crosshairImage.width, crosshairImage.height),
                                 crosshairImage);

        //Vector3 posRightCrosshairCenter = camPlayer.GetComponent<Camera>().WorldToScreenPoint(aimPosRight);

        //if (posRightCrosshairCenter.x < Screen.width / 2.0f + Screen.width * (camPlayer.GetComponent<Camera>().rect.x / 2.0f))
        //{

        //    GUI.DrawTexture(new Rect(posRightCrosshairCenter.x - crosshairImage.width / 2,
        //                      Screen.height - posRightCrosshairCenter.y - crosshairImage.height / 2,
        //                      crosshairImage.width, crosshairImage.height),
        //                      crosshairImage);
        //}

        //Vector3 posLeftCrosshairCenter = camPlayer.GetComponent<Camera>().WorldToScreenPoint(aimPosLeft);

        //if (posLeftCrosshairCenter.x > Screen.width / 2 - Screen.width * (camPlayer.GetComponent<Camera>().rect.x / 2))
        //{
        //    GUI.DrawTexture(new Rect(posLeftCrosshairCenter.x - crosshairImage.width / 2,
        //                  Screen.height - posLeftCrosshairCenter.y - crosshairImage.height / 2,
        //                  crosshairImage.width, crosshairImage.height),
        //                  crosshairImage);
        //}

        //GUI.DrawTexture(new Rect(camPlayer.GetComponent<Camera>().pixelRect.xMin - 20 + posCrosshairCenter.x * camPlayer.GetComponent<Camera>().pixelRect.width,
        //                          camPlayer.GetComponent<Camera>().pixelRect.yMax - 100*(1/0.5f) -  posCrosshairCenter.y * camPlayer.GetComponent<Camera>().pixelRect.height,
        //                          crosshairImage.width, crosshairImage.height),
        //                          crosshairImage);

        //camPlayer.GetComponent<Camera>().WorldToScreenPoint(aimPosRight);
        if(showcockpit)
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), cockpit);

        //barre de vie a l'arrache
        GUI.Box(new Rect(Screen.width / 2, Screen.height / 2, 100, 10), GUIContent.none, texHealthStyle);

    }

    [Command]
    void CmdCheat()
    {
        LoseLife(200);
        //transform.Rotate(new Vector3(100, 100, 100));
    }

    void PlayerControls()
    {

        /*********/
        //Rotation perso
        /*********/
        if (Input.GetKey(KeyCode.K))
        {
            CmdCheat();
        }
        if (Input.GetKey(KeyCode.O))
        {
            camFPS.GetComponent<Camera>().enabled = false;
            camTPS.GetComponent<Camera>().enabled = true;
            camPlayer = camTPS;
            showcockpit = false;
        }
        if (Input.GetKey(KeyCode.P))
        {
            camFPS.GetComponent<Camera>().enabled = true;
            camTPS.GetComponent<Camera>().enabled = false;
            camPlayer = camFPS;
            showcockpit = true;
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, rotationSpeed, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, -rotationSpeed, 0);
        }

        /*********/
        //Deplacement perso
        /*********/
        CharacterController controller = GetComponent<CharacterController>();
        Vector3 moveDirection;

        moveDirection = new Vector3(player.GetAxis("MoveHorizontal"), 0, -player.GetAxis("MoveVertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= moveSpeed;

        moveDirection.y -= 1000;
        controller.Move(moveDirection * Time.deltaTime);

        //GetComponent<CharacterController>().Move(transform.right * player.GetAxis("MoveHorizontal") * moveSpeed * Time.deltaTime);
        //GetComponent<CharacterController>().Move(transform.forward * player.GetAxis("MoveVertical") * -moveSpeed * Time.deltaTime);
        //transform.position += (transform.right * player.GetAxis("MoveHorizontal") * moveSpeed * Time.deltaTime);

        transform.position += transform.forward * player.GetAxis("MoveVertical") * -moveSpeed * Time.deltaTime;

        Vector2 moveLeft, moveRight;
        moveLeft = moveRight = Vector2.zero;

        /*********/
        //Mouvement viseur et rotation cam droite gauche
        /*********/
        leftInput.x = Input.GetAxis("Horizontal");

        leftInput.y = Input.GetAxis("Vertical");

        rightInput.x = Input.GetAxis("Horizontal2");

        rightInput.y = Input.GetAxis("Vertical2");

        //EN CAS DE RETOUR AU CONTROLLES SOURIS SANS POINT DE RETOUR IL FAUT GARDER LE TAS DE CODE COMMENTE EN DESSOUS
        if (leftInput.x != 0 && prevLeftInput.x != 0)
            moveLeft.x = leftInput.x - prevLeftInput.x;

        if (leftInput.y != 0 && prevLeftInput.y != 0)
            moveLeft.y = leftInput.y - prevLeftInput.y;

        prevLeftInput.x = leftInput.x;
        prevLeftInput.y = leftInput.y;

        if (rightInput.x != 0 && prevRightInput.x != 0)
            moveRight.x = rightInput.x - prevRightInput.x;

        if (rightInput.y != 0 && prevRightInput.y != 0)
            moveRight.y = rightInput.y - prevRightInput.y;

        prevRightInput.x = rightInput.x;
        prevRightInput.y = rightInput.y;

        
        leftCrosshairMove.x += moveLeft.x * 300;
        if (leftCrosshairInitPos.x + leftCrosshairMove.x > Screen.width - Screen.width / 3 || leftCrosshairInitPos.x + leftCrosshairMove.x < 0)
            leftCrosshairMove.x -= moveLeft.x * 300;

        leftCrosshairMove.y += moveLeft.y * 300;

        rightCrosshairMove.x += moveRight.x * 300;
        if (rightCrosshairInitPos.x + rightCrosshairMove.x < Screen.width / 3 || rightCrosshairInitPos.x + rightCrosshairMove.x > Screen.width)
            rightCrosshairMove.x -= moveLeft.x * 300;

        rightCrosshairMove.y += moveRight.y * 300;

        /*********/
        //tirs
        /*********/
        leftShootTimer += Time.deltaTime;
        rightShootTimer += Time.deltaTime;
        leftMissileTimer += Time.deltaTime;
        rightMissileTimer += Time.deltaTime;

        if (Input.GetAxisRaw("RightMissile") > 0 && rightMissileTimer >= 1.0f)
        {
            rightMissileTimer = 0;
            CalcShoot(SHOOT_TYPE.RIGHT_MISSILE);
        }

        if (Input.GetAxisRaw("LeftMissile") > 0 && leftMissileTimer >= 1.0f)
        {
            leftMissileTimer = 0;
            CalcShoot(SHOOT_TYPE.LEFT_MISSILE);
        }

        if (player.GetAxis("LeftGatling") != 0 && leftShootTimer >= 0.1f)
        {
            leftShootTimer = 0;
            CalcShoot(SHOOT_TYPE.LEFT_GATLING);
        }

        if (player.GetAxis("RightGatling") != 0 && rightShootTimer >= 0.1f)
        {
            rightShootTimer = 0;
            CalcShoot(SHOOT_TYPE.RIGHT_GATLING);
        }
    }

    //on calcule la position et l'orientation du tir selon le bras qui a tiré et le type de tir avant d'envoyer le tout au serveur avec la [Command]
    //pour eviter d'envoyer les variables locales du calcul de tir au serveur.
    void CalcShoot(SHOOT_TYPE st)
    {
        if (st == SHOOT_TYPE.RIGHT_MISSILE)
        {
         //   leftCrosshairInitPos.x + leftCrosshairMove.x
            Vector3 p = camRightAim.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(rightCrosshairInitPos.x + rightCrosshairMove.x,
                                                                                       rightCrosshairInitPos.y - rightCrosshairMove.y, 100));

            RaycastHit hit;
            if (Physics.Raycast(camPlayer.position, p - camPlayer.position, out hit))
            {
                if (hit.collider != null)
                {
                    p = hit.point;
                }
            }

            Vector3 shootDir = p - shootSpawnRight.position;
            shootDir.Normalize();
            CmdShootMissile(shootSpawnRight.position, shootDir);
        }
        else if (st == SHOOT_TYPE.RIGHT_GATLING)
        {
            Vector3 p = camRightAim.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(rightCrosshairInitPos.x + rightCrosshairMove.x,
                                                                           rightCrosshairInitPos.y - rightCrosshairMove.y, 100));
            RaycastHit hit;
            if (Physics.Raycast(camPlayer.position, p - camPlayer.position, out hit))
            {
                if (hit.collider != null)
                {
                    p = hit.point;
                }
            }
            Vector3 shootDir = p - shootSpawnRight.position;
            shootDir.Normalize();
            CmdShootGatling(shootSpawnRight.position, shootDir);
        }
        if (st == SHOOT_TYPE.LEFT_MISSILE)
        {
            Vector3 p = camLeftAim.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(leftCrosshairInitPos.x + leftCrosshairMove.x,
                                                                                       leftCrosshairInitPos.y - leftCrosshairMove.y, 100));
            RaycastHit hit;
            if (Physics.Raycast(camPlayer.position, p - camPlayer.position, out hit))
            {
                if (hit.collider != null)
                {
                    p = hit.point;
                }
            }
            Vector3 shootDir = p - shootSpawnLeft.position;
            shootDir.Normalize();
            CmdShootMissile(shootSpawnLeft.position, shootDir);
        }
        else if (st == SHOOT_TYPE.LEFT_GATLING)
        {
            Vector3 p = camLeftAim.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(leftCrosshairInitPos.x + leftCrosshairMove.x,
                                                                                       leftCrosshairInitPos.y - leftCrosshairMove.y, 100));

            RaycastHit hit;
            if (Physics.Raycast(camPlayer.position, p - camPlayer.position, out hit))
            {
                if (hit.collider != null)
                {
                    p = hit.point;
                }
            }
            Vector3 shootDir = p - shootSpawnLeft.position;
            shootDir.Normalize();
            CmdShootGatling(shootSpawnLeft.position, shootDir);
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
    }

    [Command]
    void CmdShootGatling(Vector3 position, Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, direction, out hit))
        {
            if (hit.collider != null)
            {
                GameObject ImpactClone = (GameObject)Instantiate(ImpactParticle, hit.point, Quaternion.identity);
                NetworkServer.Spawn(ImpactClone);
                Destroy(ImpactClone, 0.2f);

                GameObject GatlingParticleClone = (GameObject)Instantiate(GatlingParticleEffect, position, Quaternion.identity);
                GatlingParticleClone.transform.LookAt(hit.point);
                NetworkServer.Spawn(GatlingParticleClone);
                Destroy(GatlingParticleClone, 0.2f);

                if (hit.transform.tag == "Character")
                {
                    hit.transform.GetComponent<Character>().LoseLife(20);
                }

                //if (hit.transform.tag == "DestructBat")
                //{
                //    hit.transform.GetComponent<DestructObject>().TakeDamage(5);
                //}
            }
        }
        else
        {
            GameObject GatlingParticleClone = (GameObject)Instantiate(GatlingParticleEffect, position, Quaternion.identity);
            GatlingParticleClone.transform.LookAt(position + direction * 100);
            NetworkServer.Spawn(GatlingParticleClone);
            Destroy(GatlingParticleClone, 0.2f);
        }
    }

    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            // move back to zero location
            transform.position = GameObject.Find("Spawner").transform.position;
        }
    }

    public void LoseLife(int damage)
    {
        if (!isServer)
            return;

        life -= damage;

        if (life <= 0)
        {
            life = 1000;

            // called on the server, will be invoked on the clients
            RpcRespawn();
        }
    }

    void UpdateHealthBar()
    {
        if (life != prevLife)
        {
            if (life > 500)
            {
                texHealth.SetPixel(0, 0, Color.Lerp(Color.yellow, Color.green, (life - 500) / 500));
            }
            else
            {
                texHealth.SetPixel(0, 0, Color.Lerp(Color.red, Color.yellow, life / 500));
            }
            texHealth.Apply();
            texHealthStyle.normal.background = texHealth;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(aimPosRight, 1);
        Gizmos.DrawLine(RightArmTransform.position, aimPosRight);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(aimPosLeft, 1);
        Gizmos.DrawLine(LeftArmTransform.position, aimPosLeft);

    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;

        PlayerControls();
        UpdateHealthBar();

        //assignement de la bonne camera au viseur
        if(leftCrosshairInitPos.x + leftCrosshairMove.x < (Screen.width/3))
        {
            camLeftAim = camFPSLeft;
        }
        else
        {
            camLeftAim = camFPS;
        }

        if (rightCrosshairInitPos.x + rightCrosshairMove.x > Screen.width - (Screen.width / 3))
        {
            camRightAim = camFPSRight;
        }
        else
        {
            camRightAim = camFPS;
        }

        // on recuperere le point de visée de chaque bras pour les tourner vers ce point dans late update (si on met ce bout de code dans late update y'a un nullreferenceexception qui pop u_u )
        aimPosLeft = camLeftAim.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(leftCrosshairInitPos.x + leftCrosshairMove.x,
                                                                                      leftCrosshairInitPos.y - leftCrosshairMove.y, 100));

        aimPosRight = camRightAim.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(rightCrosshairInitPos.x + rightCrosshairMove.x,
                                                                                       rightCrosshairInitPos.y - rightCrosshairMove.y, 100));

    }

    void FixedUpdate()
    {
        TransmitRotations();
    }

    //synchronise la rotation des bras du robot pour les autres clients
    void RotateArmOther()
    {
        if (!isLocalPlayer)
        {
            LeftArmTransform.LookAt(leftArmLookAt, transform.up);
            RightArmTransform.LookAt(rightArmLookAt, transform.up);
            LeftArmTransform.Rotate(LeftArmTransform.right, 90);
            RightArmTransform.Rotate(RightArmTransform.right, 90);
        }
    }

    [Command]
    void CmdProvideRotationsToServer(Vector3 leftlookAt, Vector3 rightLookAt)
    {
        leftArmLookAt = leftlookAt;
        rightArmLookAt = rightLookAt;
    }

    [Client]
    void TransmitRotations()
    {
        if (isLocalPlayer)
        {
            CmdProvideRotationsToServer(aimPosLeft, aimPosRight);
        }
    }

    // pour bouger les bras du robot apres l'animation
    void LateUpdate()
    {

        //Quaternion lookRotation = Quaternion.LookRotation(aimPosLeft - LeftArmTransform.position);
        //LeftArmTransform.rotation = lookRotation;

        LeftArmTransform.forward = aimPosLeft - LeftArmTransform.position;
        RightArmTransform.forward = aimPosRight - RightArmTransform.position;
        //LeftArmTransform.forward =  -(LeftArmTransfor.position - aimPosLeft);
        //LeftArmTransform.forward = Vector3.Cross(LeftArmTransform.right, LeftArmTransform.forward);

        RotateArmOther();
    }
}
