using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class Portal : NetworkBehaviour
{
    #region Fields
    [SyncVar]
    private bool isTeleportingPlayer = false;

    [SerializeField]
    private float teleportCooldown = 2f;

    // This is the gameobject that contains the camera, exit point and portal object.
    [SerializeField]
    private GameObject targetPortalObject;

    // This is the location a player can be sent to when they enter the portal linked to this one.
    [SerializeField]
    private Transform myExitPoint;

    // This is the camera whose view will be displayed on the linked portal.
    [SerializeField]
    private Camera myCamera;

    // The Renderer that contains the RenderTexture.
    [SerializeField]
    private MeshRenderer myRenderer;

    // Set this to true if you want to display what the other portal sees in front of it on this portal's portal object.
    // WARNING: Setting this to 'true' causes massive FPS drop.
    [SerializeField]
    private bool useCamera = false;

    // Just in case I ever need this.
    private Portal targetPortal;

    // This is the location the player entering the portal will be sent to.
    private Transform targetExitPoint;

    private Collider myTrigger;

    #endregion

    public Transform GetExitPoint()
    {
        return myExitPoint;
    }

    public void SetPortalView(RenderTexture renderTexture)
    {
        myCamera.targetTexture = renderTexture;
    }


    void Start()
    {
        if (targetPortalObject == null)
        {
            Debug.Log("No target portal assigned. Skipping portal setup.");
            return;
        }

        if (targetPortalObject.tag == "Portal")
        {
            targetPortal = targetPortalObject.GetComponent<Portal>();
            targetExitPoint = targetPortal.GetExitPoint();
            myTrigger = (Collider)GetComponent(typeof(Collider));

            // Only use the rendered texture when allowed.
            if (useCamera) targetPortal.SetPortalView((RenderTexture)myRenderer.material.mainTexture);
            else myCamera.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Teleport the player when they enter the portal.
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            if (isServer)
                Teleport(col.gameObject.GetComponent<Player_Movement>());
        }
    }

    /// <summary>
    /// Relocate the player to the exit point of the linked portal.
    /// </summary>
    [Server]
    public void Teleport(Player_Movement player)
    {
        if (!isTeleportingPlayer)
        {
            Debug.Log("Teleporting player");
            StartCoroutine(TeleportCooldown());
            player.RpcTeleport(targetExitPoint.position, targetExitPoint.rotation);
        }

    }

    private IEnumerator TeleportCooldown()
    {
        targetPortal.isTeleportingPlayer = true;
        isTeleportingPlayer = true;
        yield return new WaitForSeconds(teleportCooldown);

        isTeleportingPlayer = false;
        targetPortal.isTeleportingPlayer = false;
    }


}
