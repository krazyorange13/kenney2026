using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    public GameObject botPrefab;

    public int botCount = 10;

    public float minX = -10f;
    public float maxX = 10f;

    public float minZ = -10f;
    public float maxZ = 10f;

    void Start()
    {
        for (int i = 0; i < botCount; i++)
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);

            Vector3 spawnPosition = new Vector3(x, 0, z);

            Instantiate(botPrefab, spawnPosition, Quaternion.identity);
        }
    }
}