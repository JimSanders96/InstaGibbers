using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Weapon_Rifle : Weapon
{
    [SerializeField]
    [Range(15, 50)]
    private float zoomInFOV = 20;

    [SerializeField]
    [Range(60, 100)]
    private float zoomOutFOV = 60;

    [SerializeField]
    private float zoomSpeed = 20f;

    void Awake()
    {
        base.shootImplementation = FireBeam;
        base.zoomInImplementation = ZoomInBehaviour;
        base.zoomOutImplementation = ZoomOutBehaviour;
    }

    /// <summary>
    /// Fire an instant laser beam towards the crosshair.
    /// Instagib the first player the beam encounters.
    /// </summary>
    private void FireBeam()
    {
        if (ownedByLocalPlayer)
        {
            // Reduce ammo in clip by 1;
            ammoCurrent--;

            // Cast a ray from the center of the camera towards the crosshair
            if (Physics.Raycast(viewportTransform.TransformPoint(0, 0, 0.5f), viewportTransform.forward, out hit))
            {
                Debug.DrawLine(viewportTransform.TransformPoint(0, 0, 0.5f), hit.point);
                if (hit.transform.tag == "Player")
                {
                    string uIdentity = hit.transform.name;
                    playerState.KillTargetPlayer(uIdentity);

                    Debug.Log("Hit " + uIdentity);

                }
            }

            // Display the gunfire
            graphicsManager.FireWeapon(this.weaponType, hit.transform.position);
        }
    }

    private void ZoomInBehaviour()
    {
        StartCoroutine("ZoomInSmoothly");
    }

    private void ZoomOutBehaviour()
    {
        StartCoroutine("ZoomOutSmoothly");
    }

    private IEnumerator ZoomInSmoothly()
    {
        StopCoroutine("ZoomOutSmoothly");

        while (Camera.main.fieldOfView > zoomInFOV)
        {
            Camera.main.fieldOfView -= zoomSpeed * Time.deltaTime;
            yield return null;
        }

    }

    private IEnumerator ZoomOutSmoothly()
    {
        StopCoroutine("ZoomInSmoothly");
        while (Camera.main.fieldOfView < zoomOutFOV)
        {
            Camera.main.fieldOfView += zoomSpeed * Time.deltaTime;
            yield return null;
        }
    }

}
