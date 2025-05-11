using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    public Spawner spawner;
    
    [Header("Stats")]
    public int maxHealth = 100;
    public float moveSpeed = 3.5f;
    public int damage = 10;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    [Header("References")]
    public Transform target;
    protected NavMeshAgent agent;
    protected int currentHealth;
    protected float lastAttackTime = -Mathf.Infinity;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
    }

    protected virtual void Update()
    {
        if (target)
        {
            agent.SetDestination(target.position);

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    public virtual void Die()
    {
        spawner?.OnEnemyDeath(gameObject);
        Destroy(gameObject);
    }

    public virtual void Attack()
    {
        Debug.Log("EnemyBase attack (genérico). Deve ser sobrescrito.");
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
