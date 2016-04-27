using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

public class Player_HUD : NetworkBehaviour
{

    // The transform of the HUD is required in order to find every required child (e.g. the ammo text).
    [SerializeField]
    private Transform HUD;

    // This is used to get and display this weapon's stats.
    private Weapon currentWeapon;

    // This text will display how much ammo the currently equipped weapon has left (e.g. 6 / 14).
    private Text ammoDisplayText;

    private Text matchTimerText;

    private bool timeAlmostUp = false;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        ammoDisplayText = HUD.FindChild("Ammo").GetComponentInChildren<Text>();
        matchTimerText = HUD.FindChild("Match Timer").GetComponentInChildren<Text>();

        Player_InputManager.OnLMBPressed += UpdateHUD;
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        Player_InputManager.OnLMBPressed -= UpdateHUD;
    }

    public void SetCurrentWeapon(Weapon weapon)
    {
        this.currentWeapon = weapon;
    }

    /// <summary>
    /// Update the HUD.
    /// May add more functionality in the future.
    /// </summary>
    public void UpdateHUD()
    {
        StartCoroutine(UpdateHUDAfterTime());
    }

    private IEnumerator UpdateHUDAfterTime()
    {
        // Wait a fraction of a second to ensure all fields have been updated properly.
        yield return new WaitForSeconds(.1f);

        // Update weapon HUD items only if the player has a weapon equipped.
        if (currentWeapon != null)
        {
            // Get the ammo values for the currently equipped weapon.
            Dictionary<string, int> ammo = ((Weapon)currentWeapon.GetComponent(typeof(Weapon))).getAmmo();

            ammoDisplayText.text = createAmmoText(ammo);
        }
        else
        {
            ammoDisplayText.text = "- / -";
        }

    }

    private string createAmmoText(Dictionary<string, int> ammo)
    {
        int current;
        int max;

        ammo.TryGetValue("current", out current);
        ammo.TryGetValue("supply", out max);

        return current + " / " + max;
    }

    [ClientRpc]
    public void RpcUpdateMatchTimerText(int newVal)
    {
        if (isLocalPlayer)
        {
            // When the time is up, disable all HUD elements except the leaderboard.
            if(newVal == 0)
            {
                matchTimerText.enabled = false;
                ammoDisplayText.enabled = false;
                return;
            }

            // When the time is almost up, display the time in red.
            if(newVal <= 10 && !timeAlmostUp)
            {
                timeAlmostUp = true;
                matchTimerText.color = Color.red;
            }

            matchTimerText.text = newVal + "";
        }        
    }
}
