using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Spawner_Weapon : NetworkBehaviour
{

    [SyncVar(hook = "SpawnNewWeapon")]
    public bool weaponTaken = false;

    // This is the index of the weapons list in Player_Equipment.
    [SerializeField]
    private int weaponIndex = 0;

    [SerializeField]
    private GameObject weaponGraphics;

    // The time it takes before a weapon respawns in seconds.
    [SerializeField]
    private float respawnTime = 60f;

    [SerializeField]
    private int ammoRestockedOnPickup = 10;

    /// <summary>
    /// Call the GrantWeapon method in the Spawner_Weapon when a player collides with the trigger.
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            Player_Equipment pe = col.gameObject.GetComponent<Player_Equipment>();
            GrantWeapon(pe);

        }
    }

    /// <summary>
    /// Give the player the weapon attached to this spawner.
    /// Start the respawn cycle after the weapon has been picked up.
    /// </summary>
    /// <param name="col"></param>
    public void GrantWeapon(Player_Equipment pe)
    {
        if (!weaponTaken)
        {
            // Resupply the ammo.
            ResupplyAmmo(pe.gameObject.name);

            // Grant the player the weapon.
            pe.ObtainWeapon(weaponIndex);

            weaponTaken = true;
        }
    }

    /// <summary>
    /// This method is the hook function for the weaponTaken SyncVar.
    /// </summary>
    /// <param name="taken"></param>
    private void SpawnNewWeapon(bool taken)
    {
        this.weaponTaken = taken;

        if (taken)
        {
            // Start the respawn cycle.
            StartCoroutine(Spawn());
        }
    }

    /// <summary>
    /// Disable the graphics, wait a bit, enable the graphics again.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Spawn()
    {
        weaponGraphics.SetActive(false);

        yield return new WaitForSeconds(respawnTime);

        weaponGraphics.SetActive(true);
        weaponTaken = false;
    }

    /// <summary>
    /// Resupply the ammo for the weapon this spawner contains.
    /// </summary>
    /// <param name="pe"></param>
    private void ResupplyAmmo(string playerID)
    {
        Player_Equipment pe = GameObject.Find(playerID).GetComponent<Player_Equipment>();
        pe.GetWeaponByIndex(weaponIndex).RestockAmmo(ammoRestockedOnPickup);

    }

}
