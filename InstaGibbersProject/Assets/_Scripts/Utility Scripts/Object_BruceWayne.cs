using UnityEngine;
using System.Collections;

public class Object_BruceWayne : MonoBehaviour {

    /// <summary>
    /// When this object becomes active, set its parent to null.
    /// </summary>
	void Awake()
    {
        transform.SetParent(null);
    }
}
