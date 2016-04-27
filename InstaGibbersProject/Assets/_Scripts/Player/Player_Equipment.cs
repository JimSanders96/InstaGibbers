using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player_Equipment : NetworkBehaviour
{
    #region Fields
    private int equippedWeaponIndex = -1;

    // This array contains all weapons the player could potentially aquire in the game.
    [SerializeField]
    private GameObject[] weapons;

    // The location at which the weapon is displayed.
    [SerializeField]
    private Transform weaponSlot;

    // The actual weapon the player has qeuipped.
    [SerializeField]
    private GameObject currentWeapon;

    // Used to display stats of equipped items.
    private Player_HUD HUD;

    // A list of weapons the player has at their disposal, but are not currently using.
    private List<GameObject> inactiveWeapons = new List<GameObject>();
    #endregion   

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        HUD = GetComponent<Player_HUD>();
    }

    /// <returns> 999 if the player has no weapon equipped. Else equippedWeaponIndex.</returns>
    public int GetEquippedWeaponIndex()
    {
        if (currentWeapon != null) return equippedWeaponIndex;
        else return 999;
    }

    public Weapon GetCurrentWeapon()
    {
        Weapon weapon = (Weapon)currentWeapon.GetComponent(typeof(Weapon));
        return weapon;
    }

    // Used in the Spawner_Weapon script in order to restock the requested weapon's ammo.
    public Weapon GetWeaponByIndex(int index)
    {
        Weapon weapon = (Weapon)weapons[index].GetComponent(typeof(Weapon));
        return weapon;
    }

    /// <summary>
    /// This method is the hook function for the SyncVar 'equippedWeaponIndex'.
    /// It will be called every time the SyncVar changes on a client.
    /// </summary>
    /// <param name="weaponIndex"></param>
    [ClientRpc]
    private void RpcEquipWeapon(int weaponIndex)
    {
        // -1 means there should be no weapon equipped.
        if (weaponIndex == -1)
        {
            UnequipWeapon();
            // Don't equip any weapon (code below)
            return;
        }

        Debug.Log("Equipping weapon " + weaponIndex + " for " + gameObject.name);

        if (currentWeapon != null) UnequipWeapon();

        currentWeapon = weapons[weaponIndex];

        currentWeapon.SetActive(true);

        // Make the weapon controllable by the local player only.
        if (isLocalPlayer)
        {
            // Tell the weapon that its owner is the local player.
            Weapon weaponScript = currentWeapon.GetComponent(typeof(Weapon)) as Weapon;
            weaponScript.ownedByLocalPlayer = true;

            // Set the weapons camTransform
            weaponScript.SetViewportTransform(GetComponentInChildren<Camera>().transform);

            // Tell the HUD which weapon is equipped.
            HUD.SetCurrentWeapon(weaponScript);

            // Update the HUD.
            HUD.UpdateHUD();

            Debug.Log("Equipped weapon should be ready to fire");
        }
    }

    /// <summary>
    /// Disable the current weapon.
    /// </summary>
    private void UnequipWeapon()
    {
        // Unequip the weapon;
        if (currentWeapon != null)
        {
            Weapon weapon = (Weapon)currentWeapon.GetComponent(typeof(Weapon));
            weapon.DisableWeapon();

            if (isLocalPlayer)
            {
                // Update the HUD.
                HUD.SetCurrentWeapon(null);
                HUD.UpdateHUD();
            }

            currentWeapon = null;
        }
    }

    /// <summary>
    /// Refill the weapon's ammo supply by a certain amount and equip it.
    /// NOTE: The equipping part may be removed in the future.
    /// </summary>
    /// <param name="weaponIndex"></param>
    public void ObtainWeapon(int weaponIndex)
    {        
        if (isLocalPlayer) HUD.UpdateHUD();

        //TODO: Design a system in which the weapon is only equipped on pick-up if it is better than the currently equipped weapon?
        EquipWeapon(weaponIndex);
    }

    public void EquipWeapon(int weaponIndex)
    {
        // Don't equip the weapon if it is already equipped.
        if (weaponIndex == equippedWeaponIndex) return;

        equippedWeaponIndex = weaponIndex;

        CmdEquipWeapon(weaponIndex);
    }

    [Command]
    void CmdEquipWeapon(int weaponIndex)
    {
        RpcEquipWeapon(weaponIndex);
    }   
    
}
