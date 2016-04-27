using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameStartup : NetworkBehaviour
{
    [SerializeField]
    private int playersRequiredBeforeCountdown = 2;

    [SerializeField]
    private int countdownTime = 5;

    [SerializeField]
    private GameObject PreGameCamObject;

    [SerializeField]
    private Text countdownText;

    private GameObject countdownObject;

    [SerializeField]
    private Text waitingForPlayersText;

    private List<string> playersInPreGame = new List<string>();

    private GameState gameState;

    public override void OnStartServer()
    {
        base.OnStartServer();

        gameState = GetComponent<GameState>();

        StartCoroutine("WaitForPlayers");        
    }

    void Start()
    {
        countdownObject = countdownText.gameObject;
        countdownObject.SetActive(false);
    }

    private IEnumerator WaitForPlayers()
    {
        while(playersInPreGame.Count < playersRequiredBeforeCountdown)
        {
            yield return null;
        }

        StartCoroutine("StartGame");
    }

    private IEnumerator StartGame()
    {
        Debug.Log("Starting match...");
        InitializeGameState();
        RpcSetGameStartingUI();        

        yield return new WaitForSeconds(countdownTime);

        StartGameState();
        gameState.StartEndOfMatchTimer();
    }

    public void RegisterPlayer(string playerID)
    {
        playersInPreGame.Add(playerID);
    }

    private void InitializeGameState()
    {
        // Notify GameState which players are in the game.
        gameState.SetPlayersInGame(playersInPreGame);        
    }

    private void StartGameState()
    {
        gameState.gameStarted = true;

        // TODO: Do not allow any more players to join the match. They should instead become spectators.
        gameState.waitingForPlayers = false;
    }

    [ClientRpc]
    private void RpcSetGameStartingUI()
    {
        waitingForPlayersText.text = "Starting match...";

        StartCoroutine("DisplayCountdownText");
    }

    private IEnumerator DisplayCountdownText()
    {
        countdownObject.SetActive(true);
        for (int i = countdownTime; i > 0; i--)
        {
            SetCountdownText(i + "");
            yield return new WaitForSeconds(1);
        }
    }

    private void SetCountdownText(string text)
    {
        countdownText.text = text;
    }

    public void DisablePreGameCam()
    {
        PreGameCamObject.SetActive(false);
    }
}
