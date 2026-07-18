using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{

    private BotSpawner spawner;
    private float speed = 6;
    private float turnSpeed = 90f;
    public float scale = 1;
    private float timeToDirChange = 0;
    private Quaternion targetRotation;
    private BoxCollider boxCollider;


    void Start()
    {
        targetRotation = transform.rotation;
        this.boxCollider = this.GetComponent<BoxCollider>();
    }

    void Update()
    {
        transform.localScale = Vector3.one * scale;

        timeToDirChange -= Time.deltaTime;

        if (timeToDirChange <= 0)
        {
            Vector2 random = Random.insideUnitCircle.normalized;

            Vector3 direction = new Vector3(random.x, 0f, random.y);
            targetRotation = Quaternion.LookRotation(direction);

            timeToDirChange = Random.Range(2f, 5f);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        Vector3 movement = transform.forward * speed * Time.deltaTime;
        Vector3 nextPosition = transform.position + movement;

        if (spawner != null)
        {
            if (nextPosition.x < spawner.minX 
            || nextPosition.x > spawner.maxX 
            || nextPosition.z < spawner.minZ 
            || nextPosition.z > spawner.maxZ)
            {
                Vector3 center = new Vector3((spawner.minX + spawner.maxX) / 2f, transform.position.y, (spawner.minZ + spawner.maxZ) / 2f);
                targetRotation = Quaternion.LookRotation(center - transform.position);
            }
        }

        transform.position = nextPosition;


        foreach (GameObject bot in GameObject.FindGameObjectsWithTag("Edible"))
        {
            if (bot == gameObject)
                continue;

            BoxCollider other = bot.GetComponent<BoxCollider>();

            if (boxCollider.bounds.Intersects(other.bounds))
            {
                
                // check that the two opposite corners are contained within this bot's collider
                if (ContainsBot2D(this.boxCollider, other)) {
                    Destroy(bot);
                    float myVolume = scale * scale * scale;
                    BotController otherController = bot.GetComponent<BotController>();
                    float otherVolume = otherController.scale * otherController.scale * otherController.scale;
                    scale = Mathf.Pow(myVolume + otherVolume, 1f / 3f);
                }
            }
        }
    
    }

    public BotController setSpawner(BotSpawner spawner)
    {
        this.spawner = spawner;
        return this;
    }

    bool ContainsBot2D(BoxCollider mine, BoxCollider other)
    {
        Vector3 half = other.size * 0.5f;

        Vector3[] corners =
        {
            new(-half.x, 0, -half.z),
            new(-half.x, 0,  half.z),
            new( half.x, 0, -half.z),
            new( half.x, 0,  half.z),
        };

        Vector3 myHalf = mine.size * 0.5f;
        Vector3 myMin = mine.center - myHalf;
        Vector3 myMax = mine.center + myHalf;

        foreach (Vector3 corner in corners)
        {
            // Corner in world space
            Vector3 world = other.transform.TransformPoint(other.center + corner);

            // Convert to this bot's local space
            Vector3 local = mine.transform.InverseTransformPoint(world);

            // Check only X and Z
            if (local.x < myMin.x || local.x > myMax.x ||
                local.z < myMin.z || local.z > myMax.z)
            {
                return false;
            }
        }

        return true;
    }
}