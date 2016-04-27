using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player_Leaderboard : NetworkBehaviour {

    // This array contains the score tables on which to display player information.
    [SerializeField]
    private GameObject[] scoreTables;

    [SerializeField]
    private GameObject leaderboardCanvas;

    private GameState gameState;
    
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Player_InputManager.OnButtonPressed_Tab += DisplayLeaderboard;
        Player_InputManager.OnButtonReleased_Tab += HideLeaderboard;

        this.gameState = GameObject.Find("GameManager").GetComponent<GameState>();
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        Player_InputManager.OnButtonPressed_Tab -= DisplayLeaderboard;
        Player_InputManager.OnButtonReleased_Tab -= HideLeaderboard;
    }

    public void DisplayLeaderboard()
    {
        List<LeaderboardData> data = gameState.GetLeaderboardData();

        foreach(LeaderboardData lbd in data)
        {
            SetScoreTableData(lbd, data.IndexOf(lbd));
        }

        leaderboardCanvas.SetActive(true);
    }

    public void HideLeaderboard()
    {
        leaderboardCanvas.SetActive(false);
    }

    private void SetScoreTableData(LeaderboardData data, int scoreTableIndex)
    {
        // Get the required components from the scoreTable
        GameObject scoreTable = this.scoreTables[scoreTableIndex];
        Text name = scoreTable.transform.FindChild("Player Name").GetComponent<Text>();
        Text kills = scoreTable.transform.FindChild("Kills Text").GetComponent<Text>();
        Text deaths = scoreTable.transform.FindChild("Deaths Text").GetComponent<Text>();

        // Set the text values.
        name.text = data.playerID;
        kills.text = data.kills + "";
        deaths.text = data.deaths + "";

        scoreTable.SetActive(true);
    }
}
