using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public sealed class Player : MonoBehaviour
{
    [SerializeField, Required]
    private GameObject highlightBlock;
    private GameObject highlight;
    private Vector3 placeBlockPosition;
    [SerializeField, Range(0.01f, 1.0f)]
    private float checkIncrement = 0.1f;
    [SerializeField, MinValue(1.0f)]
    private float reach = 4.0f;
    public byte SelectedBlockID { get; private set; }
    public string SelectedBlockName { get; private set; }

    new private Transform camera;
    private Vector3 cameraStandingPosition;
    private Vector3 cameraCrouchingPosition;

    [HorizontalLine]
    [SerializeField]
    private float gravity = -9.807f;
    private const float acceleration = 2.5f;

    [HorizontalLine]

    [SerializeField, Range(0.5f, 10.0f)]
    private float mouseSensitivity = 4.0f;

    [HorizontalLine]

    [SerializeField]
    private float playerWidth = 0.6f;
    [SerializeField]
    private float playerHeight = 1.8f;
    private float playerStandingHeight;
    private float playerCrouchHeight;

    [HorizontalLine]

    [SerializeField]
    private float crouchSpeed = 1.31f;
    [SerializeField]
    private float walkSpeed = 4.317f;
    [SerializeField]
    private float runSpeed = 5.612f;
    [SerializeField]
    private float jumpForce = 8.125f;

    private bool jumpRequest;
    private bool isGrounded;
    public bool IsCrouching { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsJumping { get { return verticalMomentum > 0.0f && !isGrounded; } }
    public bool IsFalling { get { return verticalMomentum < 0.0f && !isGrounded; } }

    private float horizontal;
    private float vertical;
    private Vector3 lastMouseCoordinate = Vector3.zero;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum;

    private void Start()
    {
        camera = Camera.main.transform;

        cameraStandingPosition = new(0.0f, playerHeight, 0.0f);
        cameraCrouchingPosition = new(0.0f, playerHeight - (playerHeight / 8.0f), 0.0f);

        playerStandingHeight = playerHeight;
        playerCrouchHeight = playerHeight - (playerHeight / 8.0f);

        camera.localPosition = cameraStandingPosition;

        highlight = Instantiate(highlightBlock, transform.position, Quaternion.identity);
        highlight.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        SelectedBlockName = World.Instance.GetBlockTypes[SelectedBlockID].blockName;

        EventsManager.Instance.UpdateSelectedBlockUI();
    }

    private void Update()
    {
        GetPlayerInputs();
        UpdateCursorBlock();
    }

    private void FixedUpdate()
    {
        CalculateVelocity();

        if (jumpRequest)
        {
            Jump();
        }

        if (IsCrouching)
        {
            Crouch();
        }
        else
        {
            StandUp();
        }

        transform.Rotate(mouseHorizontal * (mouseSensitivity * Vector3.up));
        camera.Rotate(mouseSensitivity * (-mouseVertical * Vector3.right));
        transform.Translate(velocity, Space.World);
    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        if (horizontal != 0)
        {
            EventsManager.Instance.UpdateDebugScreenUI(playerGUIArea: true);
        }
        vertical = Input.GetAxis("Vertical");
        if (vertical != 0)
        {
            EventsManager.Instance.UpdateDebugScreenUI(playerGUIArea: true);
        }

        Vector3 mouseDelta = Input.mousePosition - lastMouseCoordinate;
        if (mouseDelta.x != 0 || mouseDelta.y != 0)
        {
            EventsManager.Instance.UpdateDebugScreenUI(playerGUIArea: true);
        }
        lastMouseCoordinate = Input.mousePosition;

        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Crouch"))
        {
            IsCrouching = true;
            IsRunning = false;

            EventsManager.Instance.UpdateDebugScreenUI(playerGUIArea: true);
        }
        if (Input.GetButtonUp("Crouch"))
        {
            IsCrouching = false;

            EventsManager.Instance.UpdateDebugScreenUI(playerGUIArea: true);
        }
        if (Input.GetButtonDown("Run"))
        {
            IsRunning = true;
            IsCrouching = false;

            EventsManager.Instance.UpdateDebugScreenUI(playerGUIArea: true);
        }
        if (Input.GetButtonUp("Run"))
        {
            IsRunning = false;

            EventsManager.Instance.UpdateDebugScreenUI(playerGUIArea: true);
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;

            EventsManager.Instance.UpdateDebugScreenUI(playerGUIArea: true);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                SelectedBlockID++;
            }
            else
            {
                SelectedBlockID--;
            }

            if (SelectedBlockID > (World.Instance.GetBlockTypes.Length - 1))
            {
                SelectedBlockID = 1;
            }
            if (SelectedBlockID < 1)
            {
                SelectedBlockID = (byte)(World.Instance.GetBlockTypes.Length - 1);
            }

            SelectedBlockName = World.Instance.GetBlockTypes[SelectedBlockID].blockName;

            EventsManager.Instance.UpdateDebugScreenUI(playerGUIArea: true);
            EventsManager.Instance.UpdateSelectedBlockUI();
        }

        if (highlight.activeSelf)
        {
            // Destroy the block (replace with air block)
            if (Input.GetMouseButtonDown(0))
            {
                World.Instance.GetChunkFromVector3(highlight.transform.position).EditVoxel(highlight.transform.position, 0);
            }

            // Place block (replace with new selected block ID)
            if (Input.GetMouseButtonDown(1))
            {
                World.Instance.GetChunkFromVector3(placeBlockPosition).EditVoxel(placeBlockPosition, SelectedBlockID);
            }
        }
    }

    private void UpdateCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPosition = new();

        while (step < reach)
        {
            Vector3 checkPosition = camera.position + (camera.forward * step);

            if (World.Instance.CheckForVoxel(checkPosition))
            {
                highlight.transform.position = new
                (
                    x: Mathf.FloorToInt(checkPosition.x),
                    y: Mathf.FloorToInt(checkPosition.y),
                    z: Mathf.FloorToInt(checkPosition.z)
                );
                placeBlockPosition = lastPosition;

                highlight.SetActive(true);

                return;
            }

            lastPosition = new
            (
                x: Mathf.FloorToInt(checkPosition.x),
                y: Mathf.FloorToInt(checkPosition.y),
                z: Mathf.FloorToInt(checkPosition.z)
            );
            step += checkIncrement;
        }

        highlight.SetActive(false);
    }
    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void Crouch()
    {
        camera.localPosition = cameraCrouchingPosition;
        playerHeight = playerCrouchHeight;
    }

    private void StandUp()
    {
        camera.localPosition = cameraStandingPosition;
        playerHeight = playerStandingHeight;
    }

    private void CalculateVelocity()
    {
        if (verticalMomentum > gravity)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity * acceleration;
        }

        if (IsRunning)
        {
            velocity = runSpeed * Time.fixedDeltaTime * ((transform.forward * vertical) + (transform.right * horizontal));
        }
        else
        {
            if (IsCrouching)
            {
                velocity = crouchSpeed * Time.fixedDeltaTime * ((transform.forward * vertical) + (transform.right * horizontal));
            }
            else
            {
                velocity = walkSpeed * Time.fixedDeltaTime * ((transform.forward * vertical) + (transform.right * horizontal));
            }
        }

        velocity += Time.fixedDeltaTime * verticalMomentum * Vector3.up;

        if ((velocity.z > 0 && Front) || (velocity.z < 0 && Back))
        {
            velocity.z = 0;
        }
        if ((velocity.x > 0 && Right) || (velocity.x < 0 && Left))
        {
            velocity.x = 0;
        }

        if (velocity.y < 0)
        {
            velocity.y = CheckDownSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = CheckUpSpeed(velocity.y);
        }
    }

    private float CheckUpSpeed(float upSpeed)
    {
        if (
            World.Instance.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) ||
            World.Instance.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) ||
            World.Instance.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth)) ||
            World.Instance.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth))
            )
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (
            World.Instance.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            World.Instance.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            World.Instance.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            World.Instance.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
            )
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    public bool Front
    {
        get
        {
            return  World.Instance.CheckForVoxel(new(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                    World.Instance.CheckForVoxel(new(transform.position.x, transform.position.y + (playerHeight / 2.0f), transform.position.z + playerWidth));
        }
    }

    public bool Back
    {
        get
        {
            return  World.Instance.CheckForVoxel(new(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                    World.Instance.CheckForVoxel(new(transform.position.x, transform.position.y + (playerHeight / 2.0f), transform.position.z - playerWidth));
        }
    }

    public bool Right
    {
        get
        {
            return  World.Instance.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                    World.Instance.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y + (playerHeight / 2.0f), transform.position.z));
        }
    }

    public bool Left
    {
        get
        {
            return  World.Instance.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                    World.Instance.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y + (playerHeight / 2.0f), transform.position.z));
        }
    }
}
