using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Rewired;

public class CharacterMove : NetworkBehaviour {

    //deplacement du personnage
    float shootMoveTimer = 0;
    float speedMultiplier = 1;

    public int playerId = 0; // The Rewired player id of this character
    private Player player; // The Rewired Player
    public float TweakMoveSpeed = 20;
    public float TweakSpeedMultiplier = 1;
    public float TweakRotationSpeed = 1;

    void Awake()
    {
            // Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
            player = ReInput.players.GetPlayer(playerId); 
    }
    // Use this for initialization
    void Start () {
	
	}

    public void SlowMovement()
    {
        shootMoveTimer = 0.3f;
    }
	
    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(0, TweakRotationSpeed, 0);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Rotate(0, -TweakRotationSpeed, 0);
            }

            /*********/
            //ralentissement quand on tire
            /*********/
            if (shootMoveTimer > 0)
            {
                speedMultiplier = TweakSpeedMultiplier;
                shootMoveTimer -= Time.deltaTime;
            }
            else
            {
                speedMultiplier = 1;
            }

            /*********/
            //Deplacement perso
            /*********/
            Vector3 moveDirection = Vector3.zero;

            moveDirection = new Vector3(player.GetAxis("MoveHorizontal"), 0, -player.GetAxis("MoveVertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= TweakMoveSpeed * speedMultiplier;

            //moveDirection.y -= 1000;
            GetComponent<Rigidbody>().AddForce(moveDirection.x, -50, moveDirection.z );
            //transform.position += (moveDirection * Time.deltaTime);
        }
    }

	// Update is called once per frame
	void Update () {

	
	}
}
