using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player_GraphicsManager : NetworkBehaviour
{

    // This Transform is the origin of the rifle's gunfire.
    [SerializeField]
    private Transform gunfireOrigin;

    // This is used to display the currently equipped weapon's info.
    [SerializeField]
    protected Player_HUD HUD;

    private ObjectPool_GunfireGraphics gunfireGraphics;

    void Start()
    {
        // Get the object pool that contains the GunfireGraphics (This is done on all clients, which contain their own GameManager)
        gunfireGraphics = GameObject.Find("GameManager").GetComponent<ObjectPool_GunfireGraphics>();
    }

    /// <summary>
    /// This is called from the Weapon script.
    /// The reason this is here is to keep the link to the HUD out of the Weapon script.
    /// </summary>
    public void UpdateHUD()
    {
        if (!isLocalPlayer) return;

        if (HUD != null) HUD.UpdateHUD();
    }

    #region Gunfire related
    /// <summary>
    /// Display the correct gunfire graphics according to which weapon was fired.
    /// </summary>
    /// <param name="weaponFired"></param>
    [ClientRpc]
    private void RpcDisplayGunfire(Weapon.WeaponType weaponFired, Vector3 targetLocation)
    {
        // Only display graphics via RPC for non-local players. Local players should see gunfire feedback instantly, rather than after a network delay.
        // This basically prevents gunfire from being displayed twice on the local player.
        DisplayLocalGunfire(weaponFired, targetLocation);
    }

    public void SetGunfireOrigin(Transform origin)
    {
        this.gunfireOrigin = origin;
    }

    /// <summary>
    /// If the local player called this, instantly display gunfire for that player.
    /// Tell the server to display this player's gunfire on other clients aswell.
    /// </summary>
    /// <param name="weaponFired"></param>
    public void FireWeapon(Weapon.WeaponType weaponFired, Vector3 targetLocation)
    {
        if (isLocalPlayer)
        {
            DisplayLocalGunfire(weaponFired, targetLocation);

            CmdDisplayGunfire(weaponFired, targetLocation);
        }        
    }

    private void DisplayLocalGunfire(Weapon.WeaponType weaponFired, Vector3 targetLocation)
    {
        switch (weaponFired)
        {
            case Weapon.WeaponType.Rifle:
                gunfireGraphics.DisplayRifleFireAtLocation(gunfireOrigin, targetLocation);
                break;
            case Weapon.WeaponType.Shotgun:
                gunfireGraphics.DisplayShotgunFireAtLocation(gunfireOrigin, targetLocation);
                break;
        }
    }

    [Command]
    void CmdDisplayGunfire(Weapon.WeaponType weaponFired, Vector3 targetLocation)
    {
        RpcDisplayGunfire(weaponFired, targetLocation);
    }
    #endregion
}
