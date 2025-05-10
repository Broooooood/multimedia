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
    public float slideForce = 400f;
    public float slideDuration = 0.5f;
    private bool sliding;

    [Header("Slopes")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    [Header("Slope Acceleration")]
    public float slopeAcceleration = 30f;
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

        // Definir a gravidade personalizada
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
            if (state == MovementState.sprinting && grounded)
            {
                StartSlide();
            }
            else
            {
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
        if (Input.GetKey(crouchKey) && !sliding) // Verificar se o jogador está a agachar e não está a deslizar
        {
            if (grounded && Input.GetKey(sprintKey)) // Se o jogador está no chão e a correr
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
        else if (grounded && Input.GetKey(sprintKey))
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
        MovePlayer();  // Movimento normal do jogador

        // Verificar se o jogador está a deslizar e se está num declive
        if (sliding && OnSlope())
        {
            // Aumentar a velocidade de deslize enquanto desce um declive
            Vector3 slideDirection = GetSlopeMoveDirection();
            float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
            float slopeMultiplier = Mathf.Clamp(slopeAngle / maxSlopeAngle, 0.5f, 2f); // Multiplicador com base no ângulo do declive

            // Ajustar a força do deslize com base no ângulo do declive
            float boostedForce = slideForce * slopeMultiplier;

            // Aplicar a força de deslizar na direção do declive
            rb.AddForce(slideDirection * boostedForce, ForceMode.Force);  // Usar ForceMode.Force para evitar "explodir" a velocidade
        }
        else if (!OnSlope() && sliding)
        {
            // Se o jogador não estiver num declive, parar o deslize
            StopSlide();
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
        // Verificar se o jogador está no chão e se está a correr
        if (grounded && Input.GetKey(sprintKey) && !sliding)
        {
            sliding = true;

            // Reduzir a altura do jogador para o agachamento durante o deslize
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

            // Caso esteja num declive, deslizar na direção do declive
            if (OnSlope() && verticalInput > 0)
            {
                Vector3 slideDirection = GetSlopeMoveDirection();

                // Obter o ângulo do declive
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);

                // Multiplicador para a força de deslize com base no ângulo do declive
                float slopeMultiplier = Mathf.Clamp(slopeAngle / maxSlopeAngle, 0.5f, 2f);

                // Ajustar a força de deslize com base no declive
                float boostedForce = slideForce * slopeMultiplier;

                // Aplicar a força de deslize na direção do declive
                rb.AddForce(slideDirection * boostedForce, ForceMode.Force);
            }
            else
            {
                // Se não estiver num declive, deslizar na direção em que o jogador está a olhar (plano)
                Vector3 slideDirection = orientation.forward;

                // Aplicar a força de deslize
                rb.AddForce(slideDirection * slideForce, ForceMode.Force);
            }
        }
    }

    private void StopSlide()
    {
        // Parar o deslize e restaurar a altura original do jogador
        sliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

}
