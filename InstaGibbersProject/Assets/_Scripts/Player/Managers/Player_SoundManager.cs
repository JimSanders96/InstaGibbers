using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player_SoundManager : NetworkBehaviour
{
    [SerializeField]
    private AudioSource weaponSource;

    [SerializeField]
    private AudioSource playerSource;

    [SerializeField]
    private AudioClip death;

    [SerializeField]
    private AudioClip rifleGunfire;

    [SerializeField]
    private AudioClip rifleNoAmmo;

    [SerializeField]
    private AudioClip rifleReloadStart;

    [SerializeField]
    private AudioClip rifleReloadEnd;

    [SerializeField]
    private AudioClip shotgunGunfire;

    [SerializeField]
    private AudioClip shotgunNoAmmo;

    [SerializeField]
    private AudioClip shotgunReloadStart;

    [SerializeField]
    private AudioClip shotgunReloadEnd;

    private bool muted = false;

    void Start()
    {
        // Do this for all players in the client scene.
        Player_InputManager.OnButtonPressed_Mute += Mute;
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        Player_InputManager.OnButtonPressed_Mute -= Mute;
    }

    // Mute
    private void Mute()
    {
        if (!muted)
        {
            muted = true;

            playerSource.Pause();
            playerSource.volume = 0;

            weaponSource.Pause();
            weaponSource.volume = 0;
        }
        else
        {
            muted = false;

            playerSource.UnPause();
            playerSource.volume = playerSource.maxDistance;

            weaponSource.UnPause();
            weaponSource.volume = weaponSource.maxDistance;
        }
    }


    #region Player related methods
    [ClientRpc]
    private void RpcPlayDeathSound()
    {
        playerSource.PlayOneShot(death);
    }

    [Command]
    void CmdPlayDeathSound()
    {
        RpcPlayDeathSound();
    }

    public void PlayDeathsound()
    {
        CmdPlayDeathSound();
    }

    #endregion

    #region Weapon related methods

    [ClientRpc]
    private void RpcPlayGunfireSound(Weapon.WeaponType weaponFired, string shooterID)
    {
        if (isLocalPlayer) return;

        // Find the shooter and play sound at their location.
        Player_SoundManager sm = GameObject.Find(shooterID).GetComponent<Player_SoundManager>();
        sm.PlayGunfireSound(weaponFired);
    }

    /// <summary>
    /// Play the sound locally before sending a the command to play it everywhere.
    /// </summary>
    /// <param name="weaponFired"></param>
    public void SendGunfireSoundCommand(Weapon.WeaponType weaponFired)
    {
        if (isLocalPlayer)
        {
            PlayGunfireSound(weaponFired);

            CmdPlayGunfireSound(weaponFired, gameObject.name);
        }
    }

    [Command]
    void CmdPlayGunfireSound(Weapon.WeaponType weaponFired, string shooterID)
    {
        RpcPlayGunfireSound(weaponFired, shooterID);
    }

    public void PlayGunfireSound(Weapon.WeaponType weaponFired)
    {
        switch (weaponFired)
        {
            case Weapon.WeaponType.Rifle:
                weaponSource.PlayOneShot(rifleGunfire);
                break;
            case Weapon.WeaponType.Shotgun:
                weaponSource.PlayOneShot(shotgunGunfire);
                break;
        }
    }

    /// <summary>
    /// Play this sound for the local player only.
    /// The 'islocalplayer' is being checked before this function is called.
    /// </summary>
    /// <param name="weaponFired"></param>
    public void PlayNoAmmoSound(Weapon.WeaponType weaponFired)
    {
        switch (weaponFired)
        {
            case Weapon.WeaponType.Rifle:
                weaponSource.PlayOneShot(rifleNoAmmo);
                break;
            case Weapon.WeaponType.Shotgun:
                weaponSource.PlayOneShot(shotgunNoAmmo);
                break;
        }
    }

    public void PlayReloadStartSound(Weapon.WeaponType weaponFired)
    {
        switch (weaponFired)
        {
            case Weapon.WeaponType.Rifle:
                weaponSource.PlayOneShot(rifleReloadStart);
                break;
            case Weapon.WeaponType.Shotgun:
                weaponSource.PlayOneShot(shotgunReloadStart);
                break;
        }
    }

    public void PlayReloadEndSound(Weapon.WeaponType weaponFired)
    {
        switch (weaponFired)
        {
            case Weapon.WeaponType.Rifle:
                weaponSource.PlayOneShot(rifleReloadEnd);
                break;
            case Weapon.WeaponType.Shotgun:
                weaponSource.PlayOneShot(shotgunReloadEnd);
                break;
        }
    }

    #endregion
}
