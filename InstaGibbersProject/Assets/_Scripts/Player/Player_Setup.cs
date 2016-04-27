using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player_Setup : NetworkBehaviour
{

    [SerializeField]
    Camera FPSCharacterCam;
    [SerializeField]
    AudioListener audioListener;
    [SerializeField]
    GameObject HUD;
    [SerializeField]
    Player_Equipment equipment;
    [SerializeField]
    Player_Movement playerMovement;
    [SerializeField]
    Player_Leaderboard leaderboard;

    private GameState gameState;
    private GameStartup gameStartup;

    public override void OnStartLocalPlayer()
    {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        gameStartup = GameObject.Find("GameManager").GetComponent<GameStartup>();

        // Notify the server that this player has entered the game.
        CmdRegisterToGameStartup(gameObject.name);

        StartCoroutine("EnablePlayerAfterGameStart");
    }

    private void EnablePlayer()
    {
        if (isLocalPlayer)
        {
            // Disable the pre-game cam.
            gameStartup.DisablePreGameCam();

            // Enable essential scripts on this player.
            GetComponent<Player_InputManager>().enabled = true;
            FPSCharacterCam.enabled = true;
            audioListener.enabled = true;
            playerMovement.enabled = true;

            HUD.SetActive(true);
        }
    }

    [ClientRpc]
    public void RpcDisablePlayer()
    {
        if (isLocalPlayer)
        {
            // Enable essential scripts on this player.
            GetComponent<Player_InputManager>().enabled = false;
            audioListener.enabled = true;
            playerMovement.enabled = false;
            leaderboard.DisplayLeaderboard();

            // Disable the script after the player has been disabled.
            this.enabled = false;
        }
    }

    [Command]
    void CmdRegisterToGameStartup(string myID)
    {
        GameObject.Find("GameManager").GetComponent<GameStartup>().RegisterPlayer(myID);
    }

    private IEnumerator EnablePlayerAfterGameStart()
    {
        while (!gameState.gameStarted)
        {
            yield return null;
        }

        EnablePlayer();
    }
}
