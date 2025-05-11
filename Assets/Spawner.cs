using UnityEngine;
using UnityEngine.UI; // Importando para usar o Legacy UI Text
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyWaveEntry
    {
        public GameObject enemyPrefab; // Prefab do inimigo
        public int count = 1; // Número de inimigos a spawnar na onda
    }

    [Header("Spawn Settings")]
    public List<EnemyWaveEntry> waveEnemies; // Lista de inimigos para cada onda
    public Transform[] spawnPoints; // Pontos onde os inimigos podem aparecer
    public float timeBetweenWaves = 5f; // Tempo entre ondas

    [Header("Difficulty Scaling Settings")]
    public int waveNumber = 1; // Número da onda atual
    public float enemyHealthMultiplier = 1.2f; // Multiplicador de vida dos inimigos
    public float enemyDamageMultiplier = 1.1f; // Multiplicador de dano dos inimigos
    public int countIncreasePerWave = 1; // Quantidade de inimigos a mais por onda

    [Header("UI")]
    public Text waveText; // Referência para o texto da UI que exibe a onda atual

    void Start()
    {
        StartCoroutine(SpawnWaveLoop()); // Inicia o ciclo de spawn das ondas
    }

    IEnumerator SpawnWaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenWaves); // Espera o tempo entre ondas

            Debug.Log($"Wave {waveNumber} started!"); // Mostra no log quando a onda começa
            SpawnWave(); // Chama a função para spawnar os inimigos
            IncreaseDifficulty(); // Aumenta a dificuldade para a próxima onda
            UpdateWaveText(); // Atualiza o texto da UI com a onda atual
        }
    }

    void SpawnWave()
    {
        // Para cada tipo de inimigo na lista da onda
        foreach (var entry in waveEnemies)
        {
            // Cria o número de inimigos especificado para o tipo
            for (int i = 0; i < entry.count; i++)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)]; // Ponto de spawn aleatório
                GameObject enemy = Instantiate(entry.enemyPrefab, spawnPoint.position, Quaternion.identity); // Instancia o inimigo

                // Obtém o componente EnemyBase do inimigo para ajustar a vida e o dano
                EnemyBase stats = enemy.GetComponent<EnemyBase>();
                if (stats != null)
                {
                    // Ajusta a vida máxima para ser um número inteiro, aplicando o multiplicador
                    stats.maxHealth = Mathf.RoundToInt(stats.maxHealth * enemyHealthMultiplier);
                    stats.TakeDamage(0); // Resetando a saúde do inimigo com base no valor multiplicado

                    // Multiplica o dano do inimigo
                    stats.damage = Mathf.RoundToInt(stats.damage * enemyDamageMultiplier); // Cast para int
                }
            }
        }
    }

    void IncreaseDifficulty()
    {
        waveNumber++; // Aumenta o número da onda

        // Aumenta a quantidade de inimigos por tipo
        foreach (var entry in waveEnemies)
        {
            entry.count += countIncreasePerWave;
        }

        // Escala a vida e o dano dos inimigos para aumentar a dificuldade
        enemyHealthMultiplier += 0.1f;
        enemyDamageMultiplier += 0.05f;
    }

    // Método para atualizar o texto da UI
    void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {waveNumber}"; // Exibe o número da onda no texto
        }
    }
}
