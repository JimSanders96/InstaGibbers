using UnityEngine;
using System.Collections;

public class TestShotgunSpread : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            CalcSpreadPoint(transform);
        }
	}

    /// <summary>
    /// Source: http://answers.unity3d.com/questions/504904/shotgun-raycast.html
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    private Vector3 CalcSpreadPoint(Transform origin)
    {
        Vector3 offset = transform.up * Random.Range(0.0f, 1);
        offset = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), transform.forward) * offset;

        Vector3 newPoint = origin.forward * 5 + offset;

        var tr = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        tr.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        tr.position = newPoint;

        return newPoint;
    }
}
