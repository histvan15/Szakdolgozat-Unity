using System.Collections;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public int rows = 15;
    public int columns = 15;
    public float spacing = 3.0f;
    public int batchSize = 50;
    public bool enableStaticBatching = false;

    private const int MaxSafeStaticBatchCount = 225;

    public bool IsReady { get; private set; }

    private IEnumerator Start()
    {
        if (prefabToSpawn == null)
        {
            IsReady = true;
            yield break;
        }

        int spawnedInBatch = 0;

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < columns; z++)
            {
                Vector3 position = new Vector3(x * spacing, 0f, z * spacing);
                Instantiate(prefabToSpawn, position, Quaternion.identity, transform);
                spawnedInBatch++;

                if (spawnedInBatch >= batchSize)
                {
                    spawnedInBatch = 0;
                    yield return null;
                }
            }
        }

        if (enableStaticBatching)
        {
            if (rows * columns <= MaxSafeStaticBatchCount)
            {
                StaticBatchingUtility.Combine(gameObject);
            }
            else
            {
                Debug.LogWarning("Static Batching skipped: grid exceeds safe limit (225).");
            }
        }

        IsReady = true;
    }
}