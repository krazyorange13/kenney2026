using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BotManager : MonoBehaviour
{
    private List<GameObject> bots = new List<GameObject>();
    public List<GameObject> botPrefabs = new List<GameObject>();
    public GameObject botPrefab;
    public int botCount = 100;
    public float minX = -10f;
    public float maxX = 10f;
    public float minZ = -10f;
    public float maxZ = 10f;
    public int minScale = 1;
    public int maxScale = 5;
    public float awarenessRadius = 5.0f;
    public int maxHitsPerBot = 16;
    private JobHandle jobHandle;
    NativeArray<OverlapSphereCommand> commands;
    NativeArray<ColliderHit> results;
    int nQueries;
    int nResults;
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
            bots.Add(newBot);
        }
    }

    public void SpawnBotModel(GameObject parent)
    {
        if (botPrefabs.Count == 0) return;
        int rng = Random.Range(0, botPrefabs.Count);
        GameObject prefabToSpawn = botPrefabs[rng];
        GameObject go = Instantiate(prefabToSpawn, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), parent.transform);
        go.transform.localPosition = new Vector3(0, 0, 0);
    }

    void Update()
    {
        bots.RemoveAll(bot => bot == null);

        nQueries = bots.Count;
        nResults = nQueries * maxHitsPerBot;

        if (nQueries == 0) return;

        // Debug.Log($"[BotManager] Preparing batch nQueries: {nQueries} nResults: {nResults}");

        commands = new NativeArray<OverlapSphereCommand>(nQueries, Allocator.TempJob);
        results = new NativeArray<ColliderHit>(nResults, Allocator.TempJob);

        for (int i = 0; i < nQueries; i++)
        {
            Vector3 position = bots[i].transform.position;
            QueryParameters queryParameters = new QueryParameters(-1, false, QueryTriggerInteraction.Collide, false);
            float radius = bots[i].transform.localScale.x + 15.0f * Mathf.Sqrt(bots[i].transform.localScale.x);
            commands[i] = new OverlapSphereCommand(position, radius, queryParameters);
        }

        jobHandle = OverlapSphereCommand.ScheduleBatch(commands, results, 64, maxHitsPerBot, default);
    }

    void LateUpdate()
    {
        if (!commands.IsCreated || !results.IsCreated) return;

        try
        {
            jobHandle.Complete();
            ProcessPhysicsResults();
        }
        finally
        {
            commands.Dispose();
            results.Dispose();
        }
    }

    void ProcessPhysicsResults()
    {
        // Debug.Log($"[BotManager] ProcessPhysicsResults nQueries: {nQueries}");

        for (int i = 0; i < nQueries; i++)
        {
            NativeSlice<ColliderHit> hits = results.Slice(i * maxHitsPerBot, maxHitsPerBot);
            GameObject bot = bots[i];
            if (bot == null) continue;
            BotController botController = bot.GetComponent<BotController>();
            botController.ProcessTriggers(hits);
        }
    }
}
