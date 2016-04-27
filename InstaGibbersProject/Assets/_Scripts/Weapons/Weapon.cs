using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public abstract class Weapon : NetworkBehaviour
{
    #region Fields
    // This is used in Player_Equipment to determine if the player already has this weapon or not.
    public WeaponType weaponType;

    // This bool is used to determine if the weapon is owned by the local player. This bool is required because this script is attached to a child of the local player (not the player itself)
    public bool ownedByLocalPlayer = false;

    // This bool is used to determine whether or not the player has obtained the weapon already.
    public bool obtainedByLocalPlayer = false;

    // This image will be displayed at the center of the screen.
    [SerializeField]
    private Texture2D crosshairImage;

    // How many projectiles can this weapon fire before reloading.
    [SerializeField]
    private int clipSize = 10;

    // How many projectiles can this weapon carry at any given time.
    [SerializeField]
    private int ammoMax = 30;

    // The total amount of ammo this weapon currently has available to reload.
    private int ammoSupply;

    // Each weapon has its own implementation of how it fires its projectiles.
    // This delegate is therefore used in the Shoot() method.
    protected delegate void ShootImplementation();
    protected ShootImplementation shootImplementation;

    // Same story as ShootImplementation;
    protected delegate void ZoomImplementation();
    protected ZoomImplementation zoomInImplementation;
    protected ZoomImplementation zoomOutImplementation;

    // How many projectiles does the weapon currently have left in its clip.
    [SerializeField]
    protected int ammoCurrent;

    // How many times can the weapon fire in 1 second.
    [SerializeField]
    protected float fireRate = 1f;

    // How long it takes for the weapon to fill its clip in seconds.
    [SerializeField]
    protected float reloadTime = 3f;

    // This is used to determine if the reload coroutine has been started.
    protected bool isReloading = false;

    // This is used inside the reload coroutine.
    private bool performingReloadChecks = false;

    // This bool will be set to true everytime the weapon has fired. It will reset after 'fireRate' has elapsed.
    protected bool isOnCooldown = false;

    // This is the transform of the camera from which raycasts are made.
    protected Transform viewportTransform;

    protected Camera viewportCam;

    // This is the hit that will be used during raycasts.
    protected RaycastHit hit;

    // The Player_State is used to send commands to kill other players.
    [SerializeField]
    protected Player_State playerState;

    // This is the location at which gunfire will be displayed.
    [SerializeField]
    protected Transform gunfireOrigin;

    // This is used to display e.g. gunfire on all clients.
    [SerializeField]
    protected Player_GraphicsManager graphicsManager;

    // This is used to play sounds related to this weapon across the network.
    [SerializeField]
    protected Player_SoundManager soundManager;

    #endregion

    public enum WeaponType
    {
        Rifle,
        Shotgun,
        Laucher,
        Grenade
    }

    #region Automatic stuff

    void Update()
    {
        AimAtTarget();
    }

    /// <summary>
    /// Initialize important stuff.
    /// </summary>
    void OnEnable()
    {
        // Set initial ammo values when the weapon is first obtained..
        if (!obtainedByLocalPlayer)
        {
            // Mark the weapon as 'obtained'.
            obtainedByLocalPlayer = true;

            // Reload full clip.
            ammoCurrent = clipSize;

            // Determine how much ammo the weapon has available.
            ammoSupply = ammoMax - ammoCurrent;            
        }

        // Set origin for future gunfire display
        graphicsManager.SetGunfireOrigin(gunfireOrigin);

        // Link weapon activities to the proper user input.
        Player_InputManager.OnLMBPressed += Shoot;
        Player_InputManager.OnButtonPressed_R += Reload;
        Player_InputManager.OnRMBPressed += ZoomIn;
        Player_InputManager.OnRMBReleased += ZoomOut;
    }

    void OnDisable()
    {
        // Remove all weapon activities to prevent weird stuff from happening.
        Player_InputManager.OnLMBPressed -= Shoot;
        Player_InputManager.OnButtonPressed_R -= Reload;
        Player_InputManager.OnRMBPressed -= ZoomIn;
        Player_InputManager.OnRMBReleased -= ZoomOut;

        //====================================
        // Hotfix: Don't hardcode default FOV
        //====================================
        viewportCam.ResetFieldOfView();
    }

    #endregion

    #region Getters n setters

    public void SetViewportTransform(Transform viewportTransform)
    {
        this.viewportTransform = viewportTransform;
        this.viewportCam = this.viewportTransform.GetComponent<Camera>();
    }

    public bool GetIsOnCooldown()
    {
        return isOnCooldown;
    }

    public bool GetIsReloading()
    {
        return isReloading;
    }

    #endregion

    /// <summary>
    /// Fire the weapon if it has ammo and isn't on cooldown.
    /// Also play gunfire / no ammo sound.
    /// Only execute the shooting part if the method was called by the local player.
    /// </summary>
    [ClientCallback]
    private void Shoot()
    {
        if (!isOnCooldown)
        {
            // Prevent the code to be executed on non-local players.
            if (!ownedByLocalPlayer) return;

            // You can't shoot while reloading...                                                                                                                                                                                            Or can you?
            if (isReloading) return;

            // Do not fire the weapon if there is no ammo in the clip
            if (ammoCurrent <= 0)
            {
                // Play the 'no ammo' sound.
                soundManager.PlayNoAmmoSound(weaponType);
                return;
            }

            // Play the gunfire sound.
            soundManager.SendGunfireSoundCommand(weaponType);

            if (shootImplementation != null)
            {
                // Fire the weapon
                shootImplementation();
                StartCoroutine(WaitForCooldown());

                // Update the HUD
                graphicsManager.UpdateHUD();

                // Reload when clip is empty.
                if (ammoCurrent <= 0)
                {
                    Reload();
                }

            }
        }
    }

    /// <summary>
    /// Point the gun at whatever the crosshair is currently over.
    /// </summary>
    private void AimAtTarget()
    {
        if (ownedByLocalPlayer)
        {
            RaycastHit target;
            if (Physics.Raycast(viewportTransform.TransformPoint(0, 0, 0.5f), viewportTransform.forward, out target))
            {
                Vector3 targetDir = target.point - transform.position;
                float step = 3 * Time.deltaTime;
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
                Debug.DrawRay(transform.position, newDir, Color.red);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(newDir), 30 * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Make sure the weapon can be used again after it is disabled.
    /// </summary>
    public void DisableWeapon()
    {
        // 'Disable' the weapon for the local player.
        if (ownedByLocalPlayer)
        {
            // Tell the weapon it has to be picked up again in order to function properly.
            obtainedByLocalPlayer = false;

            // Reset these bools to ensure the weapon can be used again after being disabled.
            isReloading = false;
            isOnCooldown = false;
            performingReloadChecks = false;
        }

        // 'Hide' the weapon on all clients
        gameObject.SetActive(false);
    }

    #region Zooming

    private void ZoomIn()
    {
        if (!ownedByLocalPlayer) return;
        if(zoomInImplementation != null)zoomInImplementation();
    }

    private void ZoomOut()
    {
        if (!ownedByLocalPlayer) return;
        if (zoomOutImplementation != null) zoomOutImplementation();
    }

    private IEnumerator WaitForCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(fireRate);
        isOnCooldown = false;

    }
    #endregion

    #region Ammo related

    /// <returns> A dictionary that contains the current ammo and available supply.</returns>
    public Dictionary<string, int> getAmmo()
    {
        Dictionary<string, int> ammo = new Dictionary<string, int>();
        ammo.Add("current", ammoCurrent);
        ammo.Add("supply", ammoSupply);

        return ammo;
    }

    /// <summary>
    /// Set the currentAmmo variable to clipSize if there is enough ammo left to do so.
    /// </summary>
    private void Reload()
    {
        // Don't reload twice at the same time.
        if (isReloading) return;

        // Don´t reload if the clip is full.
        if (ammoCurrent == clipSize) return;

        // Don't reload if there is no ammo supply.
        if (ammoSupply <= 0) return;

        // After all you've been through, you may finally reload.        
        if (isActiveAndEnabled) StartCoroutine("ReloadAfterTime");
    }

    /// <summary>
    /// Wait until the reloadTime passes before actually reloading.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReloadAfterTime()
    {
        Debug.Log("Reloading...");

        // Used outside this method
        isReloading = true;

        // Used inside this method
        performingReloadChecks = true;

        // Play sound
        soundManager.PlayReloadStartSound(this.weaponType);

        yield return new WaitForSeconds(reloadTime);

        while (performingReloadChecks)
        {
            // First move ammo from the supply to the clip
            ammoCurrent++;
            ammoSupply--;

            // If there is no ammoSupply left, exit the while loop
            if (ammoSupply <= 0)
            {
                // Make sure limits aren't getting crossed.
                ammoSupply = 0;

                // Set to false in order to not enter the while loop again after the yield.
                performingReloadChecks = false;

                yield return null;
            }

            // If the clip is full, exit the while loop;
            if (ammoCurrent >= clipSize)
            {
                // Make sure limits aren't getting crossed.
                ammoCurrent = clipSize;

                // Set to false in order to not enter the while loop again after the yield.
                performingReloadChecks = false;

                yield return null;
            }
        }

        // Set to false so you may reload again.
        isReloading = false;

        // Update the HUD
        graphicsManager.UpdateHUD();

        // Play sound
        soundManager.PlayReloadEndSound(this.weaponType);

        Debug.Log("Reload complete.");
    }

    /// <summary>
    /// Refill the weapon's ammo supply until it cannot carry any more.
    /// </summary>
    /// <param name="amount"></param>
    public void RestockAmmo(int amount)
    {
        Debug.Log("Restocking ammo...");
        for (int i = 0; i < amount; i++)
        {
            // If the weapon is carrying it's ammo capacity, return.
            if ((ammoSupply + ammoCurrent) >= ammoMax) return;

            ammoSupply++;
        }

        // Update the HUD
        graphicsManager.UpdateHUD();

        // Reload if the clip was empty.
        if (ammoCurrent <= 0)
        {
            Reload();
        }
    }

    #endregion

    /// <summary>
    /// The crosshair can vary depending on which weapon the player has equipped.
    /// This method will set the correct crosshair.
    /// </summary>
    void OnGUI()
    {
        if (ownedByLocalPlayer)
        {
            float xMin = (Screen.width / 2) - (crosshairImage.width / 2);
            float yMin = (Screen.height / 2) - (crosshairImage.height / 2);
            GUI.DrawTexture(new Rect(xMin, yMin, crosshairImage.width, crosshairImage.height), crosshairImage);
        }
    }

}
