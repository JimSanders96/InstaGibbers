using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterController))]
public class Player_Movement : NetworkBehaviour
{
    #region Fields
    public bool lockCursor = true;
    public bool mouseLookEnabled = true;
    public bool movementEnabled = true;
    public bool jumpingEnabled = true;
    public bool dodgingEnabled = true;
    public float forwardSpeed = 10f;
    public float sidewaysSpeed = 10f;
    public float jumpSpeed = 7f;
    public float lookSpeed = 2f;
    public float upDownRange = 80f;
    public float dodgeSpeedMultiplier = 3f;
    [Range(0.1f, 1f)]
    public float dodgeDurationSeconds = 0.05f;
    public float dodgeCooldownSeconds = 1f;
    public int maxJumps = 1;

    [SerializeField]
    private Camera viewport;
    [SerializeField]
    private CharacterController characterController;
    [SerializeField]
    private float gravityMultiplier = 1f;
    private Player_SmoothPositioning smoothPosScript;
    private Player_SmoothRotation smoothRotScript;
    private bool syncPosRot = false;
    private float rotX = 0;
    private float verticalVelocity = 0;
    private float currentJumps = 0;
    private bool isFalling = false;
    private bool cursorIsLocked = true;
    private bool isTeleporting = false;
    private bool isDodging = false;
    #endregion

    void Start()
    {
        // Try to get the position & rotation synchronizing scripts. Don't sync if those scripts can't be found.
        try
        {
            smoothPosScript = GetComponent<Player_SmoothPositioning>();
            smoothRotScript = GetComponent<Player_SmoothRotation>();
            syncPosRot = true;
        }
        catch
        {
            Debug.LogWarning("No Player_SmoothPositioning or Player_Smoothrotation components found. Position & rotation will not be sent across network.");
        }
    }

    void Update()
    {
        // Jumping and gravity are in the Update function because gravity requires deltaTime and Jumping relies on being on the ground.
        CheckForJump();
        if (!characterController.isGrounded) verticalVelocity += Physics.gravity.y * Time.deltaTime * gravityMultiplier;

        // Also relies on Time.deltaTime
        CheckForDodge();
    }

    void FixedUpdate()
    {
        Rotate();
        Move();
    }

    /// <summary>
    /// Move the player with the WASD keys.
    /// Only move if it is allowed.
    /// Check for jumps here to get the new verticalVelocity value right after the jump method changes it.
    /// </summary>
    private void Move()
    {
        if (movementEnabled)
        {
            float forward = Input.GetAxis("Vertical");
            float sideways = Input.GetAxis("Horizontal");

            Vector3 speed = new Vector3(sideways * sidewaysSpeed, verticalVelocity, forward * forwardSpeed);
            speed = transform.rotation * speed;

            characterController.Move(speed * Time.deltaTime);
        }
    }

    #region Jumping
    /// <summary>
    /// Jump when the jump button is pressed.
    /// Only jump if the player is allowed to.
    /// Only jump if the player is grounded or hasn't reached their jump limit.
    /// </summary>
    private void CheckForJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (jumpingEnabled)
            {
                if (!isFalling)
                {
                    Jump();
                    StartCoroutine("CheckIsFalling");
                }
                else if (currentJumps < maxJumps)
                {
                    Jump();
                }
            }
        }
    }

    /// <summary>
    /// Set the vertical velocity and add 1 to the current jumps.
    /// </summary>
    private void Jump()
    {
        verticalVelocity = jumpSpeed;
        currentJumps++;
    }

    /// <summary>
    /// Set the isFalling bool to true.
    /// Set it to false when the player touches the ground again.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckIsFalling()
    {
        isFalling = true;

        while (!characterController.isGrounded)
        {
            yield return null;
        }

        isFalling = false;

    }

    #endregion

    #region Dodging

    private void CheckForDodge()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (dodgingEnabled)
            {
                if (!isDodging)
                {
                    StartCoroutine("Dodge");
                }
            }
        }
    }

    /// <summary>
    /// Temporarily increase movement speed.
    /// TODO: Gradually decrease it back to it's normal value.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Dodge()
    {
        isDodging = true;

        float multiplier = dodgeSpeedMultiplier;
        float baseSpeedF = forwardSpeed;
        float baseSpeedS = sidewaysSpeed;

        this.forwardSpeed = baseSpeedF * multiplier;
        this.sidewaysSpeed = baseSpeedS * multiplier;

        float elapsedTime = 0;

        while (elapsedTime < dodgeDurationSeconds)
        {
            this.forwardSpeed = Mathf.Lerp(forwardSpeed, baseSpeedF, (elapsedTime / dodgeDurationSeconds));
            this.sidewaysSpeed = Mathf.Lerp(sidewaysSpeed, baseSpeedS, (elapsedTime / dodgeDurationSeconds));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        forwardSpeed = baseSpeedF;
        sidewaysSpeed = baseSpeedS;

        yield return new WaitForSeconds(dodgeCooldownSeconds);

        isDodging = false;
    }

    #endregion

    /// <summary>
    /// Rotate the gameobject on the Y axis when the mouse is moved left and right.
    /// Rotate the viewport (camera) on the X axis when the mouse is moved up and down.
    /// </summary>
    private void Rotate()
    {
        if (mouseLookEnabled)
        {
            // Rotate gameObject on Y axis
            float rotY = Input.GetAxis("Mouse X") * lookSpeed;
            transform.Rotate(0, rotY, 0);

            // Rotate viewport on X axis between certain values
            rotX -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotX = Mathf.Clamp(rotX, -upDownRange, upDownRange);
            viewport.transform.localRotation = Quaternion.Euler(rotX, 0, 0);
        }

        UpdateCursorLock();
    }

    /// <summary>
    /// Set the player's position & rotation to the targetLocation's position & rotation.
    /// Currently called by the Portal script, which will only perform actions if it's a Server object.
    /// </summary>
    /// <param name="targetLocation"></param>
    [ClientRpc]
    public void RpcTeleport(Vector3 targetPos, Quaternion targetRot)
    {
        isTeleporting = true;

        transform.position = targetPos;
        transform.rotation = targetRot;

        if (isLocalPlayer)
        {
            smoothPosScript.UseLerping(false);
        }

        isTeleporting = false;
    }

    #region Cursor locking (source: Standard Assets MouseLook)
    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {//we force unlock the cursor if the user disable the cursor locking helper
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        //if the user set "lockCursor" we check & properly lock the cursos
        if (lockCursor)
            InternalLockUpdate();
    }

    private void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            cursorIsLocked = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            cursorIsLocked = true;
        }

        if (cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    #endregion
}
