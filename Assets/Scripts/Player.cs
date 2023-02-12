using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public sealed class Player : MonoBehaviour
{
    new private Transform camera;

    [SerializeField]
    private World world;
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

    [HorizontalLine]

    [SerializeField]
    private float walkSpeed = 4.317f;
    [SerializeField]
    private float runSpeed = 5.612f;
    [SerializeField]
    private float jumpForce = 8.125f;

    private bool jumpRequest;
    private bool isGrounded;
    private bool isRunning;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum;

    private void Start()
    {
        camera = Camera.main.transform;
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

        if (Input.GetButtonDown("Run"))
        {
            isRunning = true;
        }
        if (Input.GetButtonUp("Run"))
        {
            isRunning = false;
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

    private void CalculateVelocity()
    {
        if (verticalMomentum > gravity)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity * acceleration;
        }

        if (isRunning)
        {
            velocity = runSpeed * Time.fixedDeltaTime * ((transform.forward * vertical) + (transform.right * horizontal));
        }
        else
        {
            velocity = walkSpeed * Time.fixedDeltaTime * ((transform.forward * vertical) + (transform.right * horizontal));
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
            world.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth))
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
            world.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
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
            return  world.CheckForVoxel(new(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                    world.CheckForVoxel(new(transform.position.x, transform.position.y + (playerHeight / 2.0f), transform.position.z + playerWidth));
        }
    }

    public bool Back
    {
        get
        {
            return  world.CheckForVoxel(new(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                    world.CheckForVoxel(new(transform.position.x, transform.position.y + (playerHeight / 2.0f), transform.position.z - playerWidth));
        }
    }

    public bool Right
    {
        get
        {
            return  world.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                    world.CheckForVoxel(new(transform.position.x + playerWidth, transform.position.y + (playerHeight / 2.0f), transform.position.z));
        }
    }

    public bool Left
    {
        get
        {
            return  world.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                    world.CheckForVoxel(new(transform.position.x - playerWidth, transform.position.y + (playerHeight / 2.0f), transform.position.z));
        }
    }
}
