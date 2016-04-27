using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The object pool should be created locally, because sending lots of graphics over the network isn't very efficient.
/// </summary>
public class ObjectPool_GunfireGraphics : MonoBehaviour
{

    [SerializeField]
    private Transform gunfireParent;

    [SerializeField]
    private GameObject rifleGunfirePrefab;
    [SerializeField]
    private GameObject shotgunGunfirePrefab;

    [SerializeField]
    private int rifleGunfireAmount = 10;
    [SerializeField]
    private int shotgunGunfireAmount = 50;

    private List<GunfireGraphics_Rifle> ggRifleList;
    private List<GunfireGraphics_Shotgun> ggShotgunList;

    // Use this for initialization
    void Start()
    {
        InitRifle();
        InitShotgun();
    }

    /// <summary>
    /// Create the ggRifleList and fill it with graphics scripts.
    /// </summary>
    private void InitRifle()
    {
        ggRifleList = new List<GunfireGraphics_Rifle>();

        for (int i = 0; i < rifleGunfireAmount; i++)
        {
            ggRifleList.Add((GunfireGraphics_Rifle)InstantiateGraphics(rifleGunfirePrefab));
        }
    }

    /// <summary>
    /// Create the ggShotgunList and fill it with graphics scripts.
    /// </summary>
    private void InitShotgun()
    {
        ggShotgunList = new List<GunfireGraphics_Shotgun>();

        for (int i = 0; i < shotgunGunfireAmount; i++)
        {
            ggShotgunList.Add((GunfireGraphics_Shotgun)InstantiateGraphics(shotgunGunfirePrefab));
        }
    }

    private GunfireGraphics InstantiateGraphics(GameObject prefab)
    {
        GameObject go;

        // Instantiate the gameobject far away from the play-area
        go = (GameObject)Instantiate(prefab, new Vector3(1000, 1000, 1000), gunfireParent.rotation);

        // Set the parent of this new gameobject for maintainability purposes.
        go.transform.SetParent(gunfireParent);

        GunfireGraphics gg = go.GetComponent<GunfireGraphics>();
        return gg;
    }

    /// <summary>
    /// This method is called by Player_GraphicsManager;
    /// </summary>
    /// <param name="origin"></param>
    public void DisplayRifleFireAtLocation(Transform origin, Vector3 targetLocation)
    {
        // This will determine the length of the laser ray.
        float distance = Vector3.Distance(origin.position, targetLocation);

        // Get a random ggr from the pool.
        GunfireGraphics_Rifle ggr = ggRifleList[Random.Range(0, ggRifleList.Count)];

        // Set its rotation.
        ggr.transform.rotation = origin.rotation;

        // Set its length.
        ggr.SetLineRendererLength(distance);

        // Display it.
        ggr.Display(origin);
    }

    public void DisplayShotgunFireAtLocation(Transform origin, Vector3 targetLocation)
    {
        GunfireGraphics_Shotgun ggs = ggShotgunList[Random.Range(0, ggShotgunList.Count)];

        // Set linerenderer length
        float length = Vector3.Distance(origin.position, targetLocation);
        ggs.SetLineRendererLength(length);

        // Set rotation
        Vector3 direction = targetLocation - origin.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        ggs.transform.rotation = rotation;

        ggs.Display(origin);
    }

}
