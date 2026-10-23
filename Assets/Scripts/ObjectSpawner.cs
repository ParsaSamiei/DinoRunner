using UnityEngine;

/// <summary>
/// Spawns obstacles and coin rows. Uses distance travelled (not time), so spacing stays fair
/// as the game speeds up, and it pauses automatically with the game.
/// Each prefab keeps its own Y position (set it in the prefab).
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private Interactable[] obstaclePrefabs;   // GroundObstacle + AirObstacle prefabs
    [SerializeField] private Interactable coinPrefab;
    [SerializeField] private float spawnX = 12f;
    [SerializeField] private float minGap = 8f;
    [SerializeField] private float maxGap = 15f;
    [Range(0f, 1f)] [SerializeField] private float coinChance = 0.35f;
    [SerializeField] private int coinsPerRow = 3;
    [SerializeField] private float coinSpacing = 1.2f;

    private float travelled;
    private float nextGap;

    private void Start()
    {
        nextGap = Random.Range(minGap, maxGap);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        travelled += GameManager.Instance.CurrentSpeed * Time.deltaTime;
        if (travelled < nextGap) return;

        travelled = 0f;
        nextGap = Random.Range(minGap, maxGap);
        SpawnNext();
    }

    private void SpawnNext()
    {
        if (coinPrefab != null && Random.value < coinChance)
        {
            for (int i = 0; i < coinsPerRow; i++)
                Spawn(coinPrefab, spawnX + i * coinSpacing);
            return;
        }

        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;
        Spawn(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], spawnX);
    }

    private void Spawn(Interactable prefab, float x)
    {
        Vector3 pos = new Vector3(x, prefab.transform.position.y, 0f);
        Instantiate(prefab, pos, Quaternion.identity);
    }
}
