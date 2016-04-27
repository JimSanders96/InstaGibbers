using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

[NetworkSettings(channel = 0, sendInterval = 0.033f)]
public class Player_SmoothPositioning : NetworkBehaviour
{
    // A hook will automatically supply the hook function (SyncPositionValues) with a synchronized variable (syncPos)
    [SyncVar(hook = "SyncPositionValues")]
    private Vector3 syncPos;

    [SyncVar]
    public bool useLerping = true;

    [SerializeField]
    Transform myTransform;

    [SerializeField]
    private bool useHistoricalLerping = false;

    float lerpRate;
    private float normalLerpRate = 20f;
    private float fasterLerpRate = 27f;

    private Vector3 lastPos;
    private float threshold = 0.1f;

    private List<Vector3> syncPosList = new List<Vector3>();
    private float closeEnough = 0.1f;

    void Start()
    {
        lerpRate = normalLerpRate;
    }

    void FixedUpdate()
    {
        TransmitPosition();
    }

    void Update()
    {
        if (useLerping)
            LerpPosition();
        else
            HardPosition();
    }

    #region Teleport

    void HardPosition()
    {
        if (isLocalPlayer)
        {
            myTransform.position = syncPos;
            CmdUseLerping(true);
        }
           
    }

    public void UseLerping(bool yesOrNo)
    {
        CmdUseLerping(yesOrNo);
    }

    [Command]
    void CmdUseLerping(bool yerOrNo)
    {
        useLerping = yerOrNo;
    }

    #endregion

    /// <summary>
    /// Use lerping only for PlayerCharacterControllers that are not controlled by you.
    /// This is because you will receive the updated transform data of others in increments.
    /// Lerping smooths out those increments (reduce glitchy behaviour).
    /// </summary>
    void LerpPosition()
    {
        if (!isLocalPlayer)
        {
            if (useHistoricalLerping)
            {
                HistoricalLerping();
            }
            else
            {
                OrdinaryLerping();
            }

        }
    }

    /// <summary>
    /// Commands always start with Cmd.
    /// </summary>
    /// <param name="pos"></param>
    [Command]
    void CmdProvidePositionToServer(Vector3 pos)
    {
        syncPos = pos;
    }

    [ClientCallback]
    void TransmitPosition()
    {
        // Send command only if the player moved more than the threshold.
        if (isLocalPlayer && Vector3.Distance(myTransform.position, lastPos) > threshold)
        {
            CmdProvidePositionToServer(myTransform.position);
            lastPos = myTransform.position;
        }
    }

    [Client]
    void SyncPositionValues(Vector3 latestPos)
    {
        syncPos = latestPos;
        syncPosList.Add(syncPos);
    }

    void OrdinaryLerping()
    {
        // Lerp the position.
        myTransform.position = Vector3.Lerp(myTransform.position, syncPos, Time.deltaTime * lerpRate);
    }

    /// <summary>
    /// Use a list of received positions to iterate through.
    /// This is used to smooth player movement when high latency is causing players to lag / stutter.
    /// </summary>
    void HistoricalLerping()
    {
        if (syncPosList.Count > 0)
        {
            // Lerp towards the first position in the list
            myTransform.position = Vector3.Lerp(myTransform.position, syncPosList[0], Time.deltaTime * lerpRate);

            // Remove the position from the list.
            if (Vector3.Distance(myTransform.position, syncPosList[0]) < closeEnough)
            {
                syncPosList.RemoveAt(0);
            }

            // If the list is getting too big (the player's view of other players is too far behind in time), speed up the lerping.
            if (syncPosList.Count > 10)
            {
                lerpRate = fasterLerpRate;
            }
            else
            {
                lerpRate = normalLerpRate;
            }
        }
    }
}
