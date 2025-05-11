using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class MeeleAi : EnemyBase
{
    public float chargeForce = 20f;
    public float jumpForce = 8f;
    public float airThreshold = 1.5f; // Altura para considerar que o jogador está no ar

    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        maxHealth = 200;  // Vida do inimigo Meele
        moveSpeed = 4f;   // Velocidade do inimigo Meele
        rb = GetComponent<Rigidbody>();
    }

    public override void Attack()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;

        // Se o jogador estiver no ar, pula
        float playerHeight = target.position.y;
        float groundHeight = transform.position.y;
        bool playerInAir = (playerHeight - groundHeight) > airThreshold;

        if (playerInAir)
        {
            Debug.Log("Jumped After");
            rb.AddForce(new Vector3(direction.x, 1, direction.z) * jumpForce, ForceMode.Impulse);
        }
        else
        {
            Debug.Log("Attak");
            agent.enabled = false; 
            rb.AddForce(direction * chargeForce, ForceMode.Impulse);
            Invoke(nameof(ReenableNavMesh), 1.5f); 
        }

        
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
        }
    }

    void ReenableNavMesh()
    {
        agent.enabled = true;
    }
}
