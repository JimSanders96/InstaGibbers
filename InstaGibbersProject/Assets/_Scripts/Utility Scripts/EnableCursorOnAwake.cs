using UnityEngine;
using System.Collections;

public class EnableCursorOnAwake : MonoBehaviour {

	void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
