using UnityEngine;
using System.Collections;

public abstract class GunfireGraphics : MonoBehaviour
{

    [SerializeField]
    protected float displayTime = .25f;

    [SerializeField]
    protected LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer.enabled = false;
    }

    /// <summary>
    /// Move this object to origin and display the graphics.
    /// </summary>
    /// <param name="origin"></param>
    public void Display(Transform origin)
    {
        transform.position = origin.position;
        StartCoroutine(DisplayGraphics());
    }

    /// <summary>
    /// Enable the LineRenderer for displayTime seconds to simulate a laser effect.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DisplayGraphics()
    {
        lineRenderer.enabled = true;
        yield return new WaitForSeconds(displayTime);
        lineRenderer.enabled = false;
    }


}
