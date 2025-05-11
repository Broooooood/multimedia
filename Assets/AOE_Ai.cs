using UnityEngine;

public class AoE_AI : EnemyBase
{
    [Header("AoE Settings")]
    public float explosionRadius = 3f;
    public GameObject explosionEffect;

    protected override void Awake()
    {
        base.Awake();
        maxHealth = 150;  // Definindo vida específica para o AoE
        moveSpeed = 2.5f;  // Definindo velocidade específica para o AoE
    }

    public override void Attack()
    {
        if (Vector3.Distance(transform.position, target.position) <= explosionRadius)
        {
            Debug.Log("Inimigo AoE explodiu!");

            if (explosionEffect)
                Instantiate(explosionEffect, transform.position, Quaternion.identity);

            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);  // Conversão de dano para int

            Die(); // Auto-destruição após explosão
        }
    }
}