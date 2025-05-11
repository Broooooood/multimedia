using UnityEngine;

public class Target : MonoBehaviour
{
    private EnemyBase enemy;

    void Awake()
    {
        enemy = GetComponent<EnemyBase>();
    }

    public void TakeDamage(float amount)
    {
        if (enemy != null)
        {
            enemy.TakeDamage(Mathf.RoundToInt(amount));
        }
    }
}