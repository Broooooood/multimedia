using UnityEngine;

public class Grappling : MonoBehaviour
{
    public Camera fpsCam;              // Câmera FPS
    public float maxGrappleDistance = 100f;  // Distância máxima do grappling hook
    public LayerMask grappleMask;          // Máscara para detectar superfícies válidas
    public float grappleSpeed = 10f;       // Velocidade de aproximação do jogador
    public float grapplePullSpeed = 30f;   // Aumentando a velocidade do puxão do grappling hook (ajustado)
    public float grappleCooldown = 0.5f;   // Tempo de cooldown entre disparos

    public AudioClip grappleFireSound;     // Som do disparo do grappling hook
    public AudioClip grappleRetractSound;  // Som do grappling hook se retraindo
    public ParticleSystem grappleParticles; // Efeitos de partículas para o grappling hook

    private LineRenderer lineRenderer;      // Para desenhar a linha do grappling hook
    private Vector3 grapplePoint;           // Ponto de impacto onde o gancho se agarra
    private bool isGrappling = false;       // Verifica se o jogador está usando o grappling hook
    private Rigidbody rb;                   // Referência para o Rigidbody do jogador
    private float grappleTime;              // Tempo de movimento do grappling hook
    private AudioSource audioSource;        // Fonte de áudio para os sons
    private bool canGrapple = true;         // Se o grappling hook pode ser disparado novamente

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Verifica se o LineRenderer foi anexado ao objeto
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;  // Inicialmente, a linha está desabilitada
        }
        else
        {
            Debug.LogError("LineRenderer component is missing from the game object.");
        }

        // Obter o componente de áudio e o Rigidbody
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();  // Certifique-se de que o Rigidbody está anexado ao corpo do jogador
    }

    void Update()
    {
        // Disparar o grappling hook com o botão direito do mouse
        if (Input.GetMouseButtonDown(1) && canGrapple)
        {
            if (isGrappling)
            {
                EndGrapple();  // Se o jogador já estiver grappling, cancela o grappling
            }
            else
            {
                FireGrapplingHook();  // Dispara o grappling hook
            }
        }

        // Se o jogador estiver usando o grappling hook, movê-lo
        if (isGrappling)
        {
            GrappleMovement();
        }
    }

    // Método que dispara o grappling hook
    void FireGrapplingHook()
    {
        RaycastHit hit;
        Vector3 direction = fpsCam.transform.forward;  // Direção que o jogador está olhando

        // Lança um Raycast para verificar se há uma superfície válida
        if (Physics.Raycast(fpsCam.transform.position, direction, out hit, maxGrappleDistance, grappleMask))
        {
            // Se o Raycast acertar uma superfície válida
            grapplePoint = hit.point;
            isGrappling = true;
            canGrapple = false;

            // Ativa o LineRenderer e desenha a linha entre o jogador e o ponto de impacto
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, fpsCam.transform.position);  // Posição inicial (câmera do jogador)
            lineRenderer.SetPosition(1, grapplePoint);  // Ponto final do grappling hook

            // Inicia o som de disparo e as partículas
            if (audioSource && grappleFireSound)
                audioSource.PlayOneShot(grappleFireSound);
            if (grappleParticles)
                grappleParticles.Play();

            // Impede o jogador de usar o grappling hook novamente por um curto tempo
            Invoke("ResetGrappleCooldown", grappleCooldown);
        }
    }

    // Movimento do grappling hook, puxando o jogador para o ponto de impacto
    void GrappleMovement()
    {
        // Calcula a direção para o ponto de grappling
        Vector3 direction = grapplePoint - rb.position;

        // Aplica uma força para puxar o jogador em direção ao ponto de grappling
        rb.linearVelocity = direction.normalized * grapplePullSpeed;  // Ajustando velocidade de puxão

        // Atualiza a linha do grappling hook
        lineRenderer.SetPosition(0, rb.position);

        // Verifica se o jogador chegou perto o suficiente do ponto de grappling
        if (Vector3.Distance(rb.position, grapplePoint) <= 1f)
        {
            EndGrapple();  // Encerra o grappling quando o jogador chegar ao ponto
        }
    }

    // Finaliza o uso do grappling hook
    void EndGrapple()
    {
        isGrappling = false;
        lineRenderer.enabled = false;  // Desativa a linha do grappling hook

        // Toca o som de retração do grappling hook
        if (audioSource && grappleRetractSound)
            audioSource.PlayOneShot(grappleRetractSound);

        // Para as partículas
        if (grappleParticles)
            grappleParticles.Stop();
    }

    // Reseta o cooldown do grappling hook
    void ResetGrappleCooldown()
    {
        canGrapple = true;
    }
}
