using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab; // assign PatrolEnemy prefab here
    public Transform leftSpawn;
    public Transform rightSpawn;

    [Header("Wave settings")]
    public float timeBetweenWaves = 5f;
    public bool loopWaves = false;

    // predefined waves (configured in code for now)
    [System.Serializable]
    public class WaveDefinition
    {
        public string name;
        public int totalEnemies;
        public float spawnInterval; // seconds between spawns (or between spawn groups)
        public SpawnMode spawnMode = SpawnMode.Single; // Single = one at a time, Pair = two at once
        public int extraHealth = 0;
        public float extraSpeed = 0f;
    }

    public enum SpawnMode { Single, Pair }

    public WaveDefinition[] waves;

    // optional UI (will be found automatically if null)
    public WaveUI waveUI;

    [Tooltip("Half-width (in world units) of the patrol area created around each spawn point")]
    public float patrolHalfWidth = 3f;

    // runtime counters
    private int enemiesAlive = 0;
    private int enemiesKilledThisWave = 0;
    private int totalSpawnedThisWave = 0;

    void OnEnable()
    {
        EnemyScript.OnEnemyDied += HandleEnemyDied;
    }

    void OnDisable()
    {
        EnemyScript.OnEnemyDied -= HandleEnemyDied;
    }

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("WaveSpawner: enemyPrefab is not assigned. No waves will spawn.");
            return;
        }
        if (leftSpawn == null || rightSpawn == null)
        {
            Debug.LogWarning("WaveSpawner: leftSpawn or rightSpawn is not assigned. Please set spawn points in the Inspector.");
            return;
        }

        // find UI automatically if not assigned
        if (waveUI == null)
        {
            waveUI = FindObjectOfType<WaveUI>();
            if (waveUI == null)
            {
                // create a WaveUI GameObject so UI appears automatically
                GameObject uiGO = new GameObject("WaveUI");
                waveUI = uiGO.AddComponent<WaveUI>();
            }
        }

        // if no waves defined in inspector, create 3 default waves matching your request
        if (waves == null || waves.Length == 0)
        {
            waves = new WaveDefinition[3];
            waves[0] = new WaveDefinition() { name = "Wave 1", totalEnemies = 5, spawnInterval = 3f, spawnMode = SpawnMode.Single, extraHealth = 0, extraSpeed = 0f };
            waves[1] = new WaveDefinition() { name = "Wave 2", totalEnemies = 10, spawnInterval = 3f, spawnMode = SpawnMode.Pair, extraHealth = 0, extraSpeed = 0f };
            waves[2] = new WaveDefinition() { name = "Wave 3", totalEnemies = 20, spawnInterval = 2f, spawnMode = SpawnMode.Single, extraHealth = 30, extraSpeed = 0.5f };
        }

        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        do
        {
            for (int i = 0; i < waves.Length; i++)
            {
                WaveDefinition w = waves[i];
                Debug.Log($"Starting {w.name} ({i + 1}/{waves.Length}) - total {w.totalEnemies}");
                // reset counters BEFORE showing wave start
                enemiesKilledThisWave = 0;
                totalSpawnedThisWave = 0;
                if (waveUI != null)
                    waveUI.ShowWaveStart(i + 1, waves.Length, w.totalEnemies);

                // spawn according to definition
                yield return StartCoroutine(SpawnWaveDefinition(w));

                Debug.Log($"Wave spawned {totalSpawnedThisWave} enemies. Waiting for them to be killed...");
                // update UI AFTER spawning with correct total
                if (waveUI != null)
                    waveUI.UpdateKillCount(enemiesKilledThisWave);

                while (enemiesAlive > 0)
                {
                    if (waveUI != null)
                        waveUI.UpdateKillCount(enemiesKilledThisWave);
                    yield return null;
                }

                Debug.Log($"{w.name} ended. Enemies killed this wave: {enemiesKilledThisWave}.");
                if (waveUI != null)
                    waveUI.ShowWaveEnd(i + 1, w.name);

                yield return new WaitForSeconds(timeBetweenWaves);
            }
        } while (loopWaves);
    }

    IEnumerator SpawnWaveDefinition(WaveDefinition w)
    {
        if (enemyPrefab == null || leftSpawn == null || rightSpawn == null)
            yield break;

        // NOTE: DO NOT reset counters here - they're already reset in RunWaves before ShowWaveStart

        Player playerComponent = FindObjectOfType<Player>();
        Transform playerTransform = playerComponent != null ? playerComponent.transform : null;

        if (w.spawnMode == SpawnMode.Single)
        {
            // spawn half on left then half on right (if odd, extra on left)
            int half = w.totalEnemies / 2;
            for (int i = 0; i < half; i++)
            {
                if (SpawnEnemyAt(leftSpawn, playerTransform, true, w.extraHealth, w.extraSpeed))
                    totalSpawnedThisWave++;
                if (waveUI != null) waveUI.UpdateKillCount(enemiesKilledThisWave);
                yield return new WaitForSeconds(w.spawnInterval);
            }
            for (int i = 0; i < half; i++)
            {
                if (SpawnEnemyAt(rightSpawn, playerTransform, false, w.extraHealth, w.extraSpeed))
                    totalSpawnedThisWave++;
                if (waveUI != null) waveUI.UpdateKillCount(enemiesKilledThisWave);
                yield return new WaitForSeconds(w.spawnInterval);
            }
            if (w.totalEnemies % 2 != 0)
            {
                if (SpawnEnemyAt(leftSpawn, playerTransform, true, w.extraHealth, w.extraSpeed))
                    totalSpawnedThisWave++;
            }
        }
        else // Pair
        {
            int groups = w.totalEnemies / 2;
            for (int i = 0; i < groups; i++)
            {
                // spawn one left and one right at the same time
                int spawnedThisGroup = 0;
                if (SpawnEnemyAt(leftSpawn, playerTransform, true, w.extraHealth, w.extraSpeed)) spawnedThisGroup++;
                if (SpawnEnemyAt(rightSpawn, playerTransform, false, w.extraHealth, w.extraSpeed)) spawnedThisGroup++;
                totalSpawnedThisWave += spawnedThisGroup;
                if (waveUI != null) waveUI.UpdateKillCount(enemiesKilledThisWave);
                yield return new WaitForSeconds(w.spawnInterval);
            }
            if (w.totalEnemies % 2 != 0)
            {
                if (SpawnEnemyAt(leftSpawn, playerTransform, true, w.extraHealth, w.extraSpeed))
                    totalSpawnedThisWave++;
            }
        }
    }

    // Returns true if an enemy was actually instantiated
    bool SpawnEnemyAt(Transform spawnPoint, Transform player, bool fromLeft, int extraHealth = 0, float extraSpeed = 0f)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("WaveSpawner: enemyPrefab was destroyed or unset at runtime. Skipping spawn. Make sure you assigned a prefab from the Project (Assets) window, not a scene object from Hierarchy.");
            return false;
        }
        if (spawnPoint == null)
        {
            Debug.LogWarning("WaveSpawner: spawnPoint is null, skipping spawn.");
            return false;
        }

        GameObject go = null;
        try
        {
            go = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("WaveSpawner: failed to Instantiate enemyPrefab: " + ex.Message);
            return false;
        }

        if (go == null)
            return false;

        enemiesAlive++;
        Debug.Log($"Spawned enemy at {spawnPoint.position}. Alive: {enemiesAlive}");
        EnemyScript es = go.GetComponent<EnemyScript>();
        if (es != null)
        {
            // assign player reference if present
            if (player != null)
                es.player = player;

            // create patrol boundary points around spawn position
            GameObject pA = new GameObject("pointA");
            GameObject pB = new GameObject("pointB");
            pA.transform.position = spawnPoint.position + Vector3.left * patrolHalfWidth;
            pB.transform.position = spawnPoint.position + Vector3.right * patrolHalfWidth;
            pA.transform.parent = go.transform;
            pB.transform.parent = go.transform;
            es.pointA = pA.transform;
            es.pointB = pB.transform;

            // set initial facing: enemies spawned from left should move right (1), from right should move left (-1)
            es.SetDirection(fromLeft ? 1 : -1);

            // apply wave modifiers
            if (extraHealth != 0)
                es.maxHealth += extraHealth;
            if (Mathf.Abs(extraSpeed) > 0.0001f)
                es.moveSpeed += extraSpeed;
        }

        return true;
    }

    // helper to start a single wave from other scripts/UI
    public void StartWave()
    {
        StartCoroutine(SpawnThenWait());
    }

    IEnumerator SpawnThenWait()
    {
        if (waves != null && waves.Length > 0)
            yield return StartCoroutine(SpawnWaveDefinition(waves[0]));
        Debug.Log($"Wave spawned {totalSpawnedThisWave} enemies. Waiting for them to be killed...");
        while (enemiesAlive > 0)
            yield return null;
        Debug.Log($"Wave ended. Enemies killed this wave: {enemiesKilledThisWave}.");
    }

    void HandleEnemyDied(EnemyScript e)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        enemiesKilledThisWave++;
        Debug.Log($"Enemy died. Alive remaining: {enemiesAlive}. Killed this wave: {enemiesKilledThisWave}");
        if (waveUI != null)
            waveUI.UpdateKillCount(enemiesKilledThisWave);
    }
}
