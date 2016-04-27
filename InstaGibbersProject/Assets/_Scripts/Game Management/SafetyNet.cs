using UnityEngine;
using System.Collections;

public class SafetyNet : MonoBehaviour {

    /// <summary>
    /// Respawn the player when they touch the safety net.
    /// </summary>
    /// <param name="col"></param>
	void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Player")
        {
            Player_State player = col.GetComponent<Player_State>();
            player.Respawn();
        }
    }
}
