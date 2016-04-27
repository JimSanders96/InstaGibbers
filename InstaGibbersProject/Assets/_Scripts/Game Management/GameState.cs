using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GameState : NetworkBehaviour
{
    #region Fields
    [SerializeField]
    private int maxPlayers = 5;

    [SerializeField]
    private int matchTimerSeconds = 120;

    [SerializeField]
    private int exitMatchAfterSeconds = 10;

    [SyncVar]
    public bool gameStarted = false;
    [SyncVar]
    public bool waitingForPlayers = true;
    [SyncVar]
    public bool gameEnded = false;
    [SyncVar]
    public int timeUntilEnd;

    private NetworkManager networkManager;

    // This list contains all player ID's
    private SyncListString playersInGame = new SyncListString();

    // The index of the played in the playersInGame List is the same as the index in these lists.
    private SyncListInt killsByPlayerIndex = new SyncListInt();
    private SyncListInt deathsByPlayerIndex = new SyncListInt();

    private List<Player_HUD> playerHUDs = new List<Player_HUD>();

    #endregion

    public override void OnStartServer()
    {
        base.OnStartServer();

        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        timeUntilEnd = matchTimerSeconds;

        InitializeKillDeathArrays();
        
    }

    #region Player tracking

    public void AddPlayerToGame(string playerID)
    {
        if (isServer)
        {
            this.playersInGame.Add(playerID);
        }

    }

    public void SetPlayersInGame(List<string> players)
    {
        if (isServer)
        {
            foreach (string id in players)
            {
                playersInGame.Add(id);
            }
        }

    }

    #endregion

    #region Leaderboard stuff

    /// <summary>
    /// TODO: Somehow return the playerID and kills / deaths linked to the leaderboard rank.
    /// </summary>
    /// <param name="playerLeaderboardRank"></param>
    public List<LeaderboardData> GetLeaderboardData()
    {
        if (isClient)
        {
            return CreateLeaderboardData();
        }

        return null;
    }

    public void AwardKill(string playerID)
    {
        if (isServer)
        {
            int index = playersInGame.IndexOf(playerID);
            killsByPlayerIndex[index]++;
        }
    }

    public void AwardDeath(string playerID)
    {
        if (isServer)
        {
            int index = playersInGame.IndexOf(playerID);
            deathsByPlayerIndex[index]++;
        }
    }

    /// <summary>
    /// Pre-generate '0 / 0' scores for the maximum amount of players.
    /// </summary>
    private void InitializeKillDeathArrays()
    {
        if (isServer)
        {
            // Kills
            for (int i = 0; i < maxPlayers; i++)
            {
                killsByPlayerIndex.Add(0);
            }

            // Deaths
            for (int i = 0; i < maxPlayers; i++)
            {
                deathsByPlayerIndex.Add(0);
            }
        }        
    }

    /// <summary>
    /// Create a LeaderboardData for every player in the game and return them as a list.
    /// </summary>
    /// <returns></returns>
    private List<LeaderboardData> CreateLeaderboardData()
    {
        List<LeaderboardData> data = new List<LeaderboardData>();

        foreach (string playerID in playersInGame)
        {
            // Get the kill / death values for this player
            int kills = killsByPlayerIndex[playersInGame.IndexOf(playerID)];
            int deaths = deathsByPlayerIndex[playersInGame.IndexOf(playerID)];

            // Create a new LeaderboardData and add it to the list.
            LeaderboardData newData = new LeaderboardData(playerID, kills, deaths);
            data.Add(newData);
        }

        return data;
    }

    #endregion

    private void UpdateAllHUDTimers(int timeRemaining)
    {
        foreach (Player_HUD hud in playerHUDs)
        {
            hud.RpcUpdateMatchTimerText(timeRemaining);
        }
    }

    #region Match ending

    public void StartEndOfMatchTimer()
    {
        // Get all player_HUDs so they can be updated with the most recent timer value.
        foreach(string playerID in playersInGame)
        {
            Player_HUD hud = GameObject.Find(playerID).GetComponent<Player_HUD>();
            playerHUDs.Add(hud);
            hud.RpcUpdateMatchTimerText(matchTimerSeconds);
        }

        StartCoroutine("EndOfMatchTimer");
    }

    private void EndMatch()
    {
        gameEnded = true;
        DisablePlayers();

        StartCoroutine("DisconnectAfterTime");
    }

    private IEnumerator EndOfMatchTimer()
    {
        int timeRemaining = matchTimerSeconds;

        while(timeRemaining > 0)
        {
            yield return new WaitForSeconds(1);
            timeRemaining--;

            // Update player HUD timer values.
            UpdateAllHUDTimers(timeRemaining);
        }

        EndMatch();
    }

    /// <summary>
    /// Find all players and call their RpcDisablePlayer funtion.
    /// </summary>
    private void DisablePlayers()
    {
        foreach(string playerID in playersInGame)
        {
            GameObject.Find(playerID).GetComponent<Player_Setup>().RpcDisablePlayer();
        }
    }

    /// <summary>
    /// Close all network connections.
    /// Load the menu scene after disconnecting everything.
    /// </summary>
    private void DisconnectPlayers()
    {
        if (isServer)
        {
            Debug.Log("Disconnecting...");
            for (int i = 0; i < Network.connections.Length; i++)
            {
                Debug.Log("Disconnecting: " + Network.connections[i].ipAddress + ":" + Network.connections[i].port);
                Network.CloseConnection(Network.connections[i], true);
            }

            Network.Disconnect();
            MasterServer.UnregisterHost();

            if (Network.connections.Length > 0)
            {
                Debug.LogWarning("Not all connections were closed");
                return;
            }

            Debug.Log("Disconnected");

            networkManager.StopClient();
            networkManager.StopServer();
        }        
    }

    private IEnumerator DisconnectAfterTime()
    {
        yield return new WaitForSeconds(exitMatchAfterSeconds);

        DisconnectPlayers();
    }



    #endregion

}
