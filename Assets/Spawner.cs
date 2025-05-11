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

    [Header("References")]
    public Transform player; // Arrasta o jogador aqui no Inspector

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("O jogador não foi atribuído no Spawner! Por favor, arraste o jogador no Inspector.");
        }

        UpdateWaveText();
        StartCoroutine(SpawnWaveLoop());
    }

    IEnumerator SpawnWaveLoop()
    {
        while (true)
        {
            while (activeEnemies.Count > 0)
                yield return null;

            yield return new WaitForSeconds(timeBetweenWaves);

            Debug.Log($"Wave {waveNumber} started!");
            SpawnWave();
            IncreaseDifficulty();
            UpdateWaveText();
        }
    }

    void SpawnWave()
    {
        if (player == null) return;

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
                    stats.target = player;
                    stats.maxHealth *= enemyHealthMultiplier;
                    stats.damage *= enemyDamageMultiplier;
                    stats.TakeDamage(0);
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

        enemyHealthMultiplier++;
        enemyDamageMultiplier++;
    }

    void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {waveNumber}";
        }
    }
}
