using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public sealed class Player : MonoBehaviour
{
    [SerializeField]
    private GameObject highlightBlock;
    private GameObject highlight;

    [HorizontalLine]

    new private Transform camera;
    private Vector3 cameraStandingPosition;
    private Vector3 cameraCrouchingPosition;

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
        highlight.SetActive(true);
    }

    private void Update()
    {
        GetPlayerInputs();
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
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Crouch"))
        {
            IsCrouching = true;
            IsRunning = false;
        }
        if (Input.GetButtonUp("Crouch"))
        {
            IsCrouching = false;
        }
        if (Input.GetButtonDown("Run"))
        {
            IsRunning = true;
            IsCrouching = false;
        }
        if (Input.GetButtonUp("Run"))
        {
            IsRunning = false;
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
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
