using UnityEngine;
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
    public float slideForce = 50f;
    public float slideDuration = 0.03f;
    private bool sliding;

    [Header("Slide Conditions")]
    public float minSlideSpeed = 20f;  
    public float slideDecay = 10f;  
    public float uphillSlideDecay = 15f; 

    [Header("Slopes")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    [Header("Slope Acceleration")]
    public float slopeAcceleration = 20f;
    public float maxSlopeSpeed = 20f;

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

    [Header("Custom Gravity")]
    public float gravityScale = 2.5f;
    private float originalGravity = -9.81f;

    private int jumpCount = 0;
    private const int maxJumps = 2;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        jumpCount = 0;

        startYScale = transform.localScale.y;

        // Definir a gravidade 
        Physics.gravity = new Vector3(0, originalGravity * gravityScale, 0);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Saltar
        if (Input.GetKey(jumpKey) && readyToJump && (grounded || jumpCount < maxJumps))
        {
            readyToJump = false;
            Jump();

            if (!grounded) jumpCount++;

            Invoke(nameof(ResetJump), jumpCoolDown);
        }

        // Agachar ou deslizar
        if (Input.GetKeyDown(crouchKey))
        {
            if (state == MovementState.sprinting && grounded && rb.linearVelocity.magnitude > minSlideSpeed) // Verifica se está a correr e com velocidade suficiente
            {
                StartSlide();
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            StopSlide();  // Garante que o slide é interrompido
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchKey) && sliding) // Verificar se o jogador está a agachar e não está a deslizar
        {
            if (grounded && Input.GetKey(sprintKey) && rb.linearVelocity.magnitude > minSlideSpeed) // Se o jogador está no chão, a correr e com velocidade suficiente
            {
                state = MovementState.sliding; // Definir estado para deslizar
                moveSpeed = sprintSpeed; // Manter a velocidade de corrida durante o deslize
                Debug.Log("State: Sliding");
                StartSlide(); // Iniciar o deslize
            }
            else if (grounded)
            {
                state = MovementState.crouching;
                moveSpeed = crouchSpeed; // Reduzir a velocidade para agachar
                Debug.Log("State: Crouching");
            }
        }
        else if (grounded && Input.GetKey(sprintKey) && !Input.GetKey(crouchKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
            Debug.Log("State: Sprinting");
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            Debug.Log("State: Walking");
        }
        else if (!grounded)
        {
            state = MovementState.air;
            Debug.Log("State: In Air");
        }
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        MyInput();
        SpeedControl();
        StateHandler();

        // Aderência ao chão
        if (grounded)
        {
            rb.linearDamping = groundDrag;
            jumpCount = 0;
        }
        else
        {
            rb.linearDamping = 0;
        }

        if (!grounded)
        {
            AdjustGravityInAir();
        }

        // Debug da velocidade horizontal
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Debug.Log("Velocidade horizontal: " + flatVel.magnitude.ToString("F2") + " u/s");
    }

    private void FixedUpdate()
    {
        MovePlayer();

        if (sliding)
        {
            Vector3 slideDirection = OnSlope() ? GetSlopeMoveDirection() : moveDirection;

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);

                // Se estiver a subir (slopeDirection contra a normal do declive)
                if (Vector3.Dot(slideDirection, slopeHit.normal) > 0)
                {
                    // Desacelera ao subir
                    rb.linearVelocity *= uphillSlideDecay;
                }
            }
            else
            {
                // Desacelera em plano
                rb.linearVelocity *= slideDecay;
            }

            // Parar slide se velocidade for muito baixa
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVel.magnitude < 1f)
            {
                StopSlide();
            }
        }
    }



    public void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (OnSlope() && rb.linearVelocity.y <= 0.1f)
        {
            Vector3 slopeDirection = GetSlopeMoveDirection();

            // Aceleração extra ao descer um declive
            if (rb.linearVelocity.magnitude < maxSlopeSpeed)
            {
                rb.AddForce(slopeDirection * slopeAcceleration, ForceMode.Force);
                Debug.Log("A descer declive - velocidade atual: " + rb.linearVelocity.magnitude.ToString("F2"));
            }
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float currentMaxSpeed = moveSpeed;

        // Permitir mais velocidade ao deslizar numa rampa
        if (sliding && OnSlope())
        {
            currentMaxSpeed += 10f; 
        }

        if (flatVel.magnitude > currentMaxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMaxSpeed;
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
        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.up * originalGravity * gravityScale * 1.0f, ForceMode.Acceleration);
        }
        else if (rb.linearVelocity.y > 0)
        {
            rb.AddForce(Vector3.up * originalGravity * gravityScale * 0.3f, ForceMode.Acceleration);
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
    Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

    // Verifica se está no chão, a correr e com velocidade suficiente
    if (grounded && Input.GetKey(sprintKey) && !sliding && flatVelocity.magnitude >= minSlideSpeed)
    {
        sliding = true;
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

        Vector3 slideDirection = OnSlope() && verticalInput > 0 ? GetSlopeMoveDirection() : orientation.forward;

        float slideBoost = slideForce;
        if (OnSlope())
        {
            float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
            float slopeMultiplier = Mathf.Clamp(slopeAngle / maxSlopeAngle, 0.5f, 2f);
            slideBoost *= slopeMultiplier;
        }

        rb.AddForce(slideDirection * slideBoost, ForceMode.Force);
    }
}

    private void StopSlide()
    {
        // Parar o deslize e restaurar a altura original do jogador
        sliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }
}
