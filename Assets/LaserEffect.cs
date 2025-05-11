using UnityEngine;

public class LaserEffect : MonoBehaviour
{
    public float laserDuration = 0.1f;  // Duração do efeito do laser

    private void Start()
    {
        // Destroi o efeito do laser após um curto período
        Destroy(gameObject, laserDuration);
    }

    // Método para inicializar o laser (posicionamento e direção)
    public void InitializeLaser(Vector3 startPos, Vector3 direction, float range)
    {
        // Aqui você pode adicionar código para controlar o laser (ex. criar a linha visual, etc.)
        // Por exemplo, você pode usar um LineRenderer para mostrar a linha do laser
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, startPos + direction * range);
        }
    }
}
