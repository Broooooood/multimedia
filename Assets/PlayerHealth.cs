using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public Text lifeText; // Referência ao componente Text do Canvas

    void Start()
    {
        currentHealth = maxHealth;
        UpdateLifeText();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateLifeText();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
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

