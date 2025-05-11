using UnityEngine;
using UnityEngine.AI;

public class Rotation : MonoBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        // Pegamos a referência ao NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        RotateTowardsMovementDirection();
    }

    // Método que vai girar o inimigo na direção do movimento
    private void RotateTowardsMovementDirection()
    {
        Vector3 movementDirection = agent.velocity.normalized; // Direção de movimento

        if (movementDirection != Vector3.zero)
        {
            // Calcula a rotação que o inimigo deve ter para se mover na direção correta
            Quaternion rotation = Quaternion.LookRotation(movementDirection);

            // Faz a rotação de maneira suave
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
        }
    }
}