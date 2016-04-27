using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player_InputManager : NetworkBehaviour
{
    public delegate void ButtonPressed();
    public delegate void ButtonReleased();


    public static event ButtonPressed OnLMBPressed;
    public static event ButtonPressed OnRMBPressed;    
    public static event ButtonPressed OnButtonPressed_R;
    public static event ButtonPressed OnButtonPressed_Tab;
    public static event ButtonPressed OnButtonPressed_Mute;

    public static event ButtonReleased OnRMBReleased;
    public static event ButtonReleased OnButtonReleased_Tab;

    void Start()
    {
        // Disable this script if the object it's attached to isn't the local player.
        if (!isLocalPlayer)
        {
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckForClicks();
        CheckForButtons();
    }

    /// <summary>
    /// Trigger the corresponding mouse events.
    /// </summary>
    private void CheckForClicks()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Only trigger the event if there are methods listening to it.
            if (OnLMBPressed != null) OnLMBPressed();
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (OnRMBPressed != null) OnRMBPressed();
        }
        if (Input.GetMouseButtonUp(1))
        {
            if (OnRMBReleased != null) OnRMBReleased();
        }

    }

    private void CheckForButtons()
    {
        if (Input.GetButtonDown("Reload"))
        {
            if (OnButtonPressed_R != null) OnButtonPressed_R();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (OnButtonPressed_Tab != null) OnButtonPressed_Tab();
        }
        if (Input.GetButtonDown("Mute"))
        {
            if (OnButtonPressed_Mute != null) OnButtonPressed_Mute();
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (OnButtonReleased_Tab != null) OnButtonReleased_Tab();
        }
    }
}
