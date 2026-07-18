using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{

    private BotSpawner spawner;
    private float speed = 6;
    private float turnSpeed = 90f;
    private float timeToDirChange = 0;
    private Quaternion targetRotation;
    private BoxCollider boxCollider;

    private AudioSource audioSource;
    public AudioClip eatClip;

    GameObject target = null;
    GameObject threat = null;

    float closestTargetDist = Mathf.Infinity;
    float closestThreatDist = Mathf.Infinity;

    const float dangerRadius = 15f;

    void Start()
    {
        targetRotation = transform.rotation;
        this.boxCollider = this.GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float currentScale = GetComponent<Edible>().scale;
        transform.localScale = Vector3.one * currentScale;

        target = null;
        threat = null;
        closestTargetDist = Mathf.Infinity;
        closestThreatDist = Mathf.Infinity;

        foreach (GameObject edible in GameObject.FindGameObjectsWithTag("Edible"))
        {
            if (edible == gameObject)
                continue;

            BoxCollider other = edible.GetComponent<BoxCollider>();
            float otherScale = edible.GetComponent<Edible>().scale;

            // eating
            if (boxCollider.bounds.Intersects(other.bounds))
            {

                // check that the two opposite corners are contained within this bot's collider
                if (ContainsBot2D(this.boxCollider, other))
                {
                    // if its a player, handle respawn
                    RespawnManager player = edible.GetComponent<RespawnManager>();
                    if (player != null)
                    {
                        player.ToggleMenu();
                    }

                    audioSource.PlayOneShot(eatClip);
                    Destroy(edible);
                    GetComponent<Edible>().scale += otherScale / 2;
                    continue;
                }
            }

            // AI
            BotController otherController = edible.GetComponent<BotController>();
            Player otherControllerIfPlayer = edible.GetComponent<Player>();
            if (otherController == null && otherControllerIfPlayer == null) continue;

            float dist = Vector3.Distance(transform.position, edible.transform.position);


            // AI: find closest threat
            if (otherScale > currentScale && dist < dangerRadius)
            {
                if (dist < closestThreatDist)
                {
                    closestThreatDist = dist;
                    threat = edible;
                }
            }

            // AI: find closest target
            else if (otherScale < currentScale)
            {
                if (dist < closestTargetDist)
                {
                    closestTargetDist = dist;
                    target = edible;
                }
            }
        }

        if (threat != null && closestThreatDist <= closestTargetDist)
        {
            // run away
            Vector3 dir = (transform.position - threat.transform.position).normalized;
            dir = Quaternion.Euler(0, Random.Range(-20f, 20f), 0) * dir;
            targetRotation = Quaternion.LookRotation(dir);
        }
        else if (target != null)
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(dir);
        }
        else
        {
            // wander normally
            timeToDirChange -= Time.deltaTime;

            if (timeToDirChange <= 0)
            {
                Vector2 random = Random.insideUnitCircle.normalized;

                Vector3 direction = new Vector3(random.x, 0f, random.y);
                targetRotation = Quaternion.LookRotation(direction);

                timeToDirChange = Random.Range(2f, 5f);
            }

        }


        // update position based on rotation
        Vector3 movement = transform.forward * speed * Time.deltaTime;
        Vector3 nextPosition = transform.position + movement;

        // can't pass borders
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


        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        transform.position = nextPosition;

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
