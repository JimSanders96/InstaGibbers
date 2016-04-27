using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[NetworkSettings(channel = 0, sendInterval = 0.033f)]
public class Player_SmoothRotation : NetworkBehaviour
{

    [SyncVar]
    private Quaternion syncPlayerRotation;
    [SyncVar]
    private Quaternion syncCamRotation;
    [SyncVar]
    public bool useLerping = true;

    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private Transform camTransform;
    [SerializeField]
    private float lerpRate = 15;

    private Quaternion lastPlayerRot;
    private Quaternion lastCamRot;
    private float threshhold = 1f;
        
    void FixedUpdate()
    {
        TransmitRotations();
    }

    void Update()
    {
        LerpRotations();
    }


    #region Teleport
  

    #endregion
    /// <summary>
    /// Lerp rotations for non-local players.
    /// </summary>
    void LerpRotations()
    {
        if (!isLocalPlayer)
        {
            playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, syncPlayerRotation, Time.deltaTime * lerpRate);
            camTransform.rotation = Quaternion.Lerp(camTransform.rotation, syncCamRotation, Time.deltaTime * lerpRate);
        }

    }

    /// <summary>
    /// A command can only be invoked by the owner of the player.
    /// </summary>
    /// <param name="playerRot"></param>
    /// <param name="camRot"></param>
    [Command]
    void CmdProvideRotationsToServer(Quaternion playerRot, Quaternion camRot)
    {
        syncPlayerRotation = playerRot;
        syncCamRotation = camRot;
    }

    /// <summary>
    /// Send rotation data from client to server.
    /// </summary>
    [Client]
    void TransmitRotations()
    {
        if (isLocalPlayer)
        {
            if (Quaternion.Angle(playerTransform.rotation, lastPlayerRot) > threshhold || Quaternion.Angle(camTransform.rotation, lastCamRot) > threshhold)
            {
                CmdProvideRotationsToServer(playerTransform.rotation, camTransform.rotation);
                lastPlayerRot = playerTransform.rotation;
                lastCamRot = camTransform.rotation;
            }
        }
    }
}