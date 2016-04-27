using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon_Shotgun : Weapon
{
    [SerializeField]
    private float effectiveDistance = 10f;

    [SerializeField]
    private int deathRaysFired = 5;

    [SerializeField]
    private float spread = 30f;

    // This list is used within the ShootRay function to determine whether or not a player has been shot already.
    private List<string> playersHit = new List<string>();

    void Awake()
    {
        base.shootImplementation = Fire;
    }

    private void Fire()
    {
        if (ownedByLocalPlayer)
        {
            // Reduce ammo in clip by 1;
            ammoCurrent--;            

            // Fire deathRaysFired amount of rays in a spread pattern.
            for (int i = 0; i < deathRaysFired; i++)
            {
                ShootRay();
            }

            // Reset the playerHit list.
            playersHit.Clear();
        }
    }

    /// <summary>
    /// Source: http://answers.unity3d.com/questions/467742/how-can-i-create-raycast-bullet-innaccuracy-as-a-c.html
    /// </summary>
    void ShootRay()
    {      
        //  Generate a random XY point inside a circle:
        Vector3 direction = Random.insideUnitCircle * spread;
        direction.z = effectiveDistance; // circle is at effectiveDistance units.
        direction = transform.TransformDirection(direction.normalized);

        //Raycast and debug
        Ray r = new Ray(transform.position, direction);
        RaycastHit newHit;
        if (Physics.Raycast(r, out newHit))
        {
            // If a ray hits a player, kill them.
            if (newHit.transform.tag == "Player")
            {

                string playerID = newHit.transform.name;
                if (!playersHit.Contains(playerID))
                {
                    playerState.KillTargetPlayer(playerID);
                    playersHit.Add(playerID);
                }
            }

            // Display gunfire
            graphicsManager.FireWeapon(this.weaponType, newHit.point);

            Debug.DrawLine(transform.position, newHit.point);
        }
    }
}
