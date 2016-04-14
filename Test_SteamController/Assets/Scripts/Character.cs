using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Rewired;

[RequireComponent(typeof(CharacterController))]
public class Character : NetworkBehaviour
{
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

    //deplacement des cam laterales
    Transform leftCam, rightCam;
    public float aimRotationSpeed = 1;

    public float camMoveZone = 0.7f; //a partir de quel seuil sur le pad la camera doit bouger
    public float camMaxInterior = 10;// rotations min et max des cameras laterales
    public float camMaxExterior = 80;
    float yawLeft = 0.0f; 
    float yawRight = 0.0f;
    Vector2 leftCrosshairMove, rightCrosshairMove;

    //Missile et Tirs
    bool LeftParticleActive = false;
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
    }
    // Use this for initialization
    void Start()
    {

        //active la bonne camera pour le perso sur le serveur
        if (isLocalPlayer)
        {
            Cursor.visible = false;
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
                                 leftCam.GetComponent<Camera>().pixelRect.center.y - 100 + leftCrosshairMove.y - crosshairImage.height / 2,
                                 crosshairImage.width, crosshairImage.height),
                                 crosshairImage);

        GUI.DrawTexture(new Rect(rightCam.GetComponent<Camera>().pixelRect.center.x + 180 + rightCrosshairMove.x - crosshairImage.width / 2,
                                 rightCam.GetComponent<Camera>().pixelRect.center.y - 100 + rightCrosshairMove.y - crosshairImage.height / 2,
                                 crosshairImage.width, crosshairImage.height),
                                 crosshairImage);

        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), cockpit);

        //barre de vie a l'arrache
        GUI.Box(new Rect(Screen.width / 2, Screen.height / 2, 100, 10), GUIContent.none, texHealthStyle);

    }

    //on calcule la position et l'orientation du tir selon le bras qui a tiré et le type de tir avant d'envoyer le tout au serveur avec la [Command]
    //pour eviter d'envoyer les variables locales du calcul de tir au serveur.
    void CalcShoot(SHOOT_TYPE st)
    {
        if (st == SHOOT_TYPE.RIGHT_MISSILE)
        {

            Vector3 p = rightCam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(rightCam.GetComponent<Camera>().pixelRect.center.x + 180 + rightCrosshairMove.x,
                                                                                       rightCam.GetComponent<Camera>().pixelRect.center.y + 100 - rightCrosshairMove.y, 100));

            RaycastHit hit;
            if (Physics.Raycast(rightCam.position, p - rightCam.position, out hit))
            {
                if(hit.collider != null)
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
            Vector3 p = rightCam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(rightCam.GetComponent<Camera>().pixelRect.center.x + 180 + rightCrosshairMove.x,
                                                                           rightCam.GetComponent<Camera>().pixelRect.center.y + 100 - rightCrosshairMove.y, 100));
            RaycastHit hit;
            if (Physics.Raycast(rightCam.position, p - rightCam.position, out hit))
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
            Vector3 p = leftCam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(leftCam.GetComponent<Camera>().pixelRect.center.x - 180 + leftCrosshairMove.x,
                                                                                       leftCam.GetComponent<Camera>().pixelRect.center.y + 100 - leftCrosshairMove.y, 100));
            RaycastHit hit;
            if (Physics.Raycast(leftCam.position, p - leftCam.position, out hit))
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
            Vector3 p = leftCam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(leftCam.GetComponent<Camera>().pixelRect.center.x - 180 + leftCrosshairMove.x,
                                                                                       leftCam.GetComponent<Camera>().pixelRect.center.y + 100 - leftCrosshairMove.y, 100));

            RaycastHit hit;
            if (Physics.Raycast(leftCam.position, p - leftCam.position, out hit))
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
        RaycastHit hit ;
        if (Physics.Raycast(position, direction, out hit))
        {
            if (hit.collider != null)
            {

                //GameObject MissileClone = (GameObject)Instantiate(missile, position, Quaternion.identity);
                //MissileClone.GetComponent<Rigidbody>().velocity = direction * MissileSpeed;
                //NetworkServer.Spawn(MissileClone);
                //Destroy(MissileClone, 10.0f);

                GameObject ImpactClone = (GameObject)Instantiate(ImpactParticle, hit.point, Quaternion.identity);
                //MissileClone.GetComponent<Rigidbody>().velocity = direction * MissileSpeed;
                NetworkServer.Spawn(ImpactClone);
                Destroy(ImpactClone, 1);

                ImpactClone = (GameObject)Instantiate(GatlingParticleEffect, transform.position, Quaternion.identity);
                //MissileClone.GetComponent<Rigidbody>().velocity = direction * MissileSpeed;
                NetworkServer.Spawn(ImpactClone);
                Destroy(ImpactClone, 1);

                //Debug.Log("tir particule");

                if (hit.transform.tag == "Character")
                {
                    hit.transform.GetComponent<Character>().LoseLife(20);
                    Debug.Log("hitchar_gatling");
                }

                if (hit.transform.tag == "DestructBat")
                {
                    hit.transform.GetComponent<DestructObject>().TakeDamage(5);
                }
            }
        }
    }

    void PlayerControls()
    {

        if (Input.GetKey(KeyCode.E))
        {

        }

        /*********/
        //Rotation perso
        /*********/
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

        Vector2 leftInput, rightInput;

        /*********/
        //Mouvement viseur et rotation cam droite gauche
        /*********/
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
            Debug.Log("leftgatling");
        }

        if (player.GetAxis("RightGatling") != 0 && rightShootTimer >= 0.1f)
        {
            rightShootTimer = 0;
            CalcShoot(SHOOT_TYPE.RIGHT_GATLING);
            Debug.Log("rightgatling");
        }

        //if (player.GetAxis("LeftGatling") != 0 && LeftParticleActive == false)
        //{
        //    LeftParticleActive = true;
        //    ParticleSystem.EmissionModule ps = LeftGatlingParticleEffect.GetComponent<ParticleSystem>().emission;
        //    ps.enabled = true;
        //}
        //if(player.GetAxis("LeftGatling") == 0 && LeftParticleActive )
        //{
        //    LeftParticleActive = false;
        //    ParticleSystem.EmissionModule ps = LeftGatlingParticleEffect.GetComponent<ParticleSystem>().emission;
        //    ps.enabled = false;
        //}
    }

    public void LoseLife(int damage)
    {
        if (!isServer)
            return;

        life -= damage;
        //Debug.Log("life lost");
    }

    void UpdateHealthBar()
    {
        if (life != prevLife)
        {
            texHealth.SetPixel(0, 0, Color.Lerp(Color.red, Color.green, life / 1000.0f));
            texHealth.Apply();
            texHealthStyle.normal.background = texHealth;
        }
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

        PlayerControls();
        UpdateHealthBar();
        // fonction pour shoot a la mitraille


    }
}
