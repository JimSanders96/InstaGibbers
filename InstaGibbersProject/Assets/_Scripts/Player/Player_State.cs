using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player_State : NetworkBehaviour
{
    [SyncVar(hook = "OnDeathStateChanged")]
    private bool isDead = false;

    private bool respawning = false;

    [SerializeField]
    private Player_Equipment equipment;

    [SerializeField]
    private Player_SoundManager soundManager;

    private NetworkManager netManager;

    private Vector3 lastSpawnPoint;

    void Start()
    {
        netManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    #region Death
    /// <summary>
    /// Hook function for te isDead SyncVar.
    /// In case the player is the local player, relocate them and tell the server they are alive again.
    /// </summary>
    /// <param name="isDead"></param>
    private void OnDeathStateChanged(bool isDead)
    {
        this.isDead = isDead;


        if (isLocalPlayer)
        {
            if (isDead)
            {
                Respawn();
            }
        }
    }

    /// <summary>
    /// Remove all aquired equipment from the player.
    /// TODO: Keep track of how often the player has died & other score stuff.
    /// </summary>
    public void Die(string killerID)
    {
        Debug.Log(gameObject.name + " got zapped!");
        equipment.EquipWeapon(-1);

        soundManager.PlayDeathsound();

        isDead = true;
    }

    /// <summary>
    /// Award a kill to the killer and a death to the victim.
    /// </summary>
    /// <param name="killerID"></param>
    /// <param name="myID"></param>
    [Command]
    void CmdUpdateLeaderboard(string killerID, string myID)
    {
        GameState gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        gameState.AwardKill(killerID);
        gameState.AwardDeath(myID);

    }

    #endregion

    #region Killing another player.
    /// <summary>
    /// This method is called in the Weapon scripts.
    /// </summary>
    /// <param name="playerID"></param>
    public void KillTargetPlayer(string playerID)
    {
        CmdKillTargetPlayer(playerID, gameObject.name);
    }

    [Command]
    void CmdKillTargetPlayer(string targetID, string myID)
    {
        GameObject player = GameObject.Find(targetID);
        player.GetComponent<Player_State>().Die(myID);

        // Give myself a kill and my target a death.
        CmdUpdateLeaderboard(myID, targetID);
    }

    #endregion

    #region Respawning

    [Command]
    void CmdResetDeathState()
    {
        isDead = false;
    }

    [Client]
    public void Respawn()
    {
        Vector3 newSpawnPoint = netManager.GetStartPosition().position;

        // Keep searching for another spawn point if the new one is the same as the last.
        while (newSpawnPoint == lastSpawnPoint)
        {
            newSpawnPoint = netManager.GetStartPosition().position;
        }

        // Move the player to a spawn point.
        transform.position = newSpawnPoint;

        // Keep track of the last spawn point.
        lastSpawnPoint = newSpawnPoint;

        CmdResetDeathState();
    }

    #endregion

}
