using System.Collections.Generic;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    public List<GameObject> botPrefabs = new List<GameObject>();
    public GameObject botPrefab;

    public int botCount = 100;

    public float minX = -10f;
    public float maxX = 10f;

    public float minZ = -10f;
    public float maxZ = 10f;

    public int minScale = 1;
    public int maxScale = 5;

    void Start()
    {
        for (int i = 0; i < botCount; i++)
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);

            Vector3 spawnPosition = new Vector3(x, 0, z);

            GameObject newBot = Instantiate(botPrefab, spawnPosition, Quaternion.identity, transform);
            float scale = Random.Range(minScale, maxScale);
            newBot.GetComponent<BotController>().setSpawner(this).GetComponent<Edible>().scale = scale;
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
