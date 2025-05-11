using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyWaveEntry
    {
        public GameObject enemyPrefab;
        public int count = 1;
    }

    [Header("Spawn Settings")]
    public List<EnemyWaveEntry> waveEnemies;
    public Transform[] spawnPoints;
    public float timeBetweenWaves = 5f;

    [Header("Difficulty Scaling Settings")]
    public int waveNumber = 1;
    public int enemyHealthMultiplier = 1;
    public int enemyDamageMultiplier = 1;
    public int countIncreasePerWave = 1;

    [Header("UI")]
    public Text waveText;

    // Expose uma variável pública para o jogador no Editor
    public GameObject player; // Esta será a referência ao jogador no Editor

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        UpdateWaveText();
        StartCoroutine(SpawnWaveLoop());
    }

    IEnumerator SpawnWaveLoop()
    {
        while (true)
        {
            while (activeEnemies.Count > 0)
            {
                yield return null;
            }

            yield return new WaitForSeconds(timeBetweenWaves);

            Debug.Log($"Wave {waveNumber} started!");
            SpawnWave();
            IncreaseDifficulty();
            UpdateWaveText();
        }
    }

    void SpawnWave()
    {
        // Aqui, agora usamos o jogador arrastado no editor diretamente
        if (player == null)
        {
            Debug.LogError("Jogador não foi atribuído no Spawner! Por favor, arraste o jogador no Editor.");
            return;
        }

        foreach (var entry in waveEnemies)
        {
            for (int i = 0; i < entry.count; i++)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject enemy = Instantiate(entry.enemyPrefab, spawnPoint.position, Quaternion.identity);

                activeEnemies.Add(enemy);

                EnemyBase stats = enemy.GetComponent<EnemyBase>();
                if (stats != null)
                {
                    stats.spawner = this;
                    stats.target = player.transform; // Agora usamos a referência do jogador arrastado no Editor
                    stats.maxHealth *= enemyHealthMultiplier;
                    stats.damage *= enemyDamageMultiplier;
                    stats.TakeDamage(0); // Atualiza a saúde inicial
                }
            }
        }
    }

    public void OnEnemyDeath(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
    }

    void IncreaseDifficulty()
    {
        waveNumber++;
        foreach (var entry in waveEnemies)
        {
            entry.count += countIncreasePerWave;
        }

        enemyHealthMultiplier += 1;  
        enemyDamageMultiplier += 1;
    }

    void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {waveNumber}";
        }
    }
}
