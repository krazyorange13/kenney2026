using System.Collections.Generic;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    public List<GameObject> botPrefabs = new List<GameObject>();
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

            GameObject newBot = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
            float scale = Random.Range(1, 3);
            newBot.GetComponent<BotController>().setSpawner(this).scale = scale;
            SpawnBotModel(newBot);
        }
    }

    public void SpawnBotModel(GameObject child)
    {
        if (botPrefabs.Count == 0) return;
        int rng = Random.Range(0, botPrefabs.Count);
        GameObject prefabToSpawn = botPrefabs[rng];
        GameObject go = Instantiate(prefabToSpawn, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), child.transform);
        go.transform.localPosition = new Vector3(0, 0, 0);
    }
}