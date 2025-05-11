using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public int recover = 5;

    public float timeToStartHealing = 3f;
    public float healInterval = 1f;

    private float healTimer = 0f;
    private float idleTimer = 0f;
    private bool isHealing = false;

    public Text lifeText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateLifeText();
    }

    void Update()
    {
        if (currentHealth < maxHealth)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= timeToStartHealing)
            {
                isHealing = true;
            }

            if (isHealing)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= healInterval)
                {
                    Heal();
                    healTimer = 0f;
                }
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateLifeText();

        idleTimer = 0f;
        healTimer = 0f;
        isHealing = false;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal()
    {
        currentHealth += recover;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateLifeText();
    }

    void UpdateLifeText()
    {
        if (lifeText != null)
        {
            lifeText.text = "Health: " + currentHealth.ToString();
        }
    }

    void Die()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Morte");
        Debug.Log("Died");
        gameObject.SetActive(false);
    }
}
