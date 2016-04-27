using UnityEngine;
using System.Collections;

public class LeaderboardData {

    public LeaderboardData(string playerID, int kills, int deaths)
    {
        this.playerID = playerID;
        this.kills = kills;
        this.deaths = deaths;
    }

    public string playerID { get; set; }

    public int kills { get; set; }

    public int deaths { get; set; }
}
