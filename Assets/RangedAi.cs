using UnityEngine;

public class RangedAi : EnemyBase
{
    [Header("Ranged Settings")]
    public Transform shootPoint;  // Ponto de origem do disparo
    public GameObject laserEffectPrefab;  // Prefab do efeito visual do laser

    [Header("Laser Settings")]
    public float laserRange = 10f;  // Distância do laser
    public LayerMask targetLayer;  // Camada do jogador, para garantir que o laser só atinja o jogador

    protected override void Awake()
    {
        base.Awake();
        maxHealth = 120;  // Vida do inimigo Ranged
        moveSpeed = 3f;   // Velocidade do inimigo Ranged
    }

    public override void Attack()
    {
        if (CanSeePlayer())  // Só ataca se o inimigo vê o jogador
        {
            Debug.Log("Inimigo Ranged disparou laser!");

            if (shootPoint != null && laserEffectPrefab != null)
            {
                // Criando o efeito visual do laser
                GameObject laserEffect = Instantiate(laserEffectPrefab, shootPoint.position, Quaternion.identity);
                LaserEffect laser = laserEffect.GetComponent<LaserEffect>();

                if (laser != null)
                {
                    Vector3 direction = (target.position - shootPoint.position).normalized;
                    laser.InitializeLaser(shootPoint.position, direction, laserRange);
                }

                // Dispara o raycast para verificar colisão com o jogador
                RaycastHit hit;
                if (Physics.Raycast(shootPoint.position, (target.position - shootPoint.position).normalized, out hit, laserRange, targetLayer))
                {
                    if (hit.collider.CompareTag("Player"))  // Verifica se o alvo do raycast é o jogador
                    {
                        PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                        if (playerHealth != null)
                        {
                            playerHealth.TakeDamage(damage);
                            Debug.Log("Jogador atingido pelo laser!");
                        }
                    }
                }
            }
        }
    }

    // Função que verifica se o inimigo pode ver o jogador (com base em linha de visão)
    protected bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (target.position - shootPoint.position).normalized;
        float distanceToPlayer = Vector3.Distance(shootPoint.position, target.position);

        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, directionToPlayer, out hit, laserRange, targetLayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;  // Se o raio atingir o jogador, significa que o inimigo pode vê-lo
            }
        }

        return false;  // Se não atingir o jogador, o inimigo não consegue vê-lo
    }
}