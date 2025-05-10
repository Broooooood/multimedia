using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;

    public float jumpForce;
    public float jumpCoolDown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    public float startYScale;

    [Header("Sliding")]
    public float slideForce = 400f;
    public float slideDuration = 0.5f;
    private bool sliding;


    [Header("Slopes")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air
    }

    // Variáveis de controlo de gravidade
    public float gravityScale = 2.5f;  // Gravidade personalizada
    private float originalGravity = -9.81f; // Gravidade original do Unity (padrão)

    private int jumpCount = 0; // Contador de saltos (1 para o salto normal, 1 para o duplo salto)
    private const int maxJumps = 2; // Máximo de saltos (1 salto normal + 1 duplo salto)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        jumpCount = 0;

        startYScale = transform.localScale.y;

        // Definir a gravidade global no início
        Physics.gravity = new Vector3(0, originalGravity * gravityScale, 0);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Salto
        if (Input.GetKey(jumpKey) && readyToJump && (grounded || jumpCount < maxJumps))
        {
            readyToJump = false;

            Jump();

            if (!grounded)
            {
                jumpCount++; // Incrementa o contador de saltos se estiver no ar
            }

            Invoke(nameof(ResetJump), jumpCoolDown);
        }

        // Agachar e deslisar
        if (Input.GetKeyDown(crouchKey))
            {
                if (state == MovementState.sprinting && grounded)
                {
                    StartSlide();
                }
                else
                {
                    // Agachar normal
                    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                    rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                }
            }
            else if (Input.GetKeyUp(crouchKey) && !sliding)
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }

        if (sliding)
        {
            state = MovementState.sliding;
            moveSpeed = sprintSpeed; // Mantém a velocidade
        }
    }

    // Update é chamado uma vez por frame
    private void Update()
    {
        // Verificar se está no chão
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        Debug.Log("Grounded: " + grounded);

        MyInput();
        SpeedControl();
        StateHandler();

        // Aderência
        if (grounded)
        {
            rb.linearDamping = groundDrag;
            jumpCount = 0; // Reseta o contador de saltos ao tocar o chão
        }
        else
        {
            rb.linearDamping = 0;
        }

        // Ajuste da gravidade no ar
        if (!grounded)
        {
            AdjustGravityInAir();
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    public void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // No chão
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

private void AdjustGravityInAir()
{
    // Gravidade mais leve ao cair
    if (rb.linearVelocity.y < 0)
    {
        rb.AddForce(Vector3.up * originalGravity * gravityScale * 1.0f, ForceMode.Acceleration); // Mais suave ao cair
    }
    // Gravidade ainda mais leve ao subir
    else if (rb.linearVelocity.y > 0)
    {
        rb.AddForce(Vector3.up * originalGravity * gravityScale * 0.3f, ForceMode.Acceleration); // Subida mais flutuante
    }
}


    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void StartSlide()
    {
        sliding = true;
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(orientation.forward * slideForce, ForceMode.Impulse);

        Invoke(nameof(StopSlide), slideDuration);
    }

    private void StopSlide()
    {
        sliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }
}
