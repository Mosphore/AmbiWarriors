using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CharacterLife : NetworkBehaviour {

    [SyncVar]
    public float life = 1000;
    float prevLife = 1000;

    Texture2D texHealth = null;
    GUIStyle texHealthStyle = null;

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

    void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        //barre de vie a l'arrache
        GUI.Box(new Rect(Screen.width / 4, Screen.height - Screen.height / 6, 200, 40), GUIContent.none, texHealthStyle);
    }

    // Use this for initialization
    void Start ()
    {
        if (texHealth == null)
            texHealth = new Texture2D(1, 1);

        if (texHealthStyle == null)
            texHealthStyle = new GUIStyle();

        texHealth.SetPixel(0, 0, Color.green);
        texHealth.Apply();
        texHealthStyle.normal.background = texHealth;
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateHealthBar();
    }
}
