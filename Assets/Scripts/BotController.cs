using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{

    public List<GameObject> botPrefabs = new List<GameObject>();
    private float speed = 6;
    private float turnSpeed = 90f;
    public float scale = 1;
    private float timeToDirChange = 0;
    private Quaternion targetRotation;

    void Start()
    {
        SpawnPrefabAsChild();
        scale = Random.Range(1f, 5f);
        transform.localScale = Vector3.one * scale;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        timeToDirChange -= Time.deltaTime;

        if (timeToDirChange <= 0)
        {
            Vector2 random = Random.insideUnitCircle.normalized;

            Vector3 direction = new Vector3(random.x, 0f, random.y);
            targetRotation = Quaternion.LookRotation(direction);

            timeToDirChange = Random.Range(2f, 5f);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        transform.position += transform.forward * speed * scale * Time.deltaTime;
    }

    public void SpawnPrefabAsChild()
    {
        if (botPrefabs.Count == 0) return;
        int rng = Random.Range(0, botPrefabs.Count - 1);
        GameObject prefabToSpawn = botPrefabs[rng];
        GameObject go = Instantiate(prefabToSpawn, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), this.transform);
        go.transform.localPosition = new Vector3(0, 0, 0);
    }
}