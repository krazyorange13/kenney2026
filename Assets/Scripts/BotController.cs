using System;
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

    public GameObject target { get; set; }
    public GameObject threat { get; set; }
    public float closestTargetDist { get; set; }
    public float closestThreatDist { get; set; }
    private GameObject lateTarget { get; set; }
    private GameObject lateThreat { get; set; }

    const float dangerRadius = 15f;

    void Start()
    {
        targetRotation = transform.rotation;
        boxCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
    }

    // OnDrawGizmosSelected
    void OnDrawGizmos()
    {
        if (lateTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, lateTarget.transform.position);
        }

        if (lateThreat)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, lateThreat.transform.position);
        }
    }

    void LateUpdate()
    {
        lateTarget = target;
        lateThreat = threat;
        target = null;
        threat = null;
        closestTargetDist = Mathf.Infinity;
        closestThreatDist = Mathf.Infinity;
    }

    void Update()
    {
        float currentScale = GetComponent<Edible>().scale;
        transform.localScale = Vector3.one * currentScale;

        // target = null;
        // threat = null;
        // closestTargetDist = Mathf.Infinity;
        // closestThreatDist = Mathf.Infinity;

        // foreach (GameObject edible in GameObject.FindGameObjectsWithTag("Edible"))
        // {
        //     if (edible == gameObject)
        //         continue;

        //     // AI
        //     float dist = Vector3.Distance(transform.position, edible.transform.position);

        //     // AI: find closest threat
        //     if (GetScale(edible) > GetScale(gameObject) && dist < dangerRadius)
        //     {
        //         if (dist < closestThreatDist)
        //         {
        //             closestThreatDist = dist;
        //             threat = edible;
        //         }
        //     }

        //     // AI: find closest target
        //     else if (GetScale(edible) < GetScale(gameObject))
        //     {
        //         if (dist < closestTargetDist)
        //         {
        //             closestTargetDist = dist;
        //             target = edible;
        //         }
        //     }
        // }


        if (threat != null && closestThreatDist <= closestTargetDist)
        {
            // run away
            Vector3 dir = (transform.position - threat.transform.position).normalized;
            dir.y = 0;
            dir = Quaternion.Euler(0, UnityEngine.Random.Range(-20f, 20f), 0) * dir;
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
                Vector2 random = UnityEngine.Random.insideUnitCircle.normalized;
                Vector3 direction = new Vector3(random.x, 0f, random.y);
                targetRotation = Quaternion.LookRotation(direction);
                timeToDirChange = UnityEngine.Random.Range(2f, 5f);
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

    void OnCollisionStay(Collision collision)
    {
        GameObject other = collision.gameObject;
        Edible otherEdible = other.GetComponent<Edible>();
        if (otherEdible != null)
        {
            BoxCollider otherBox = other.GetComponent<BoxCollider>();

            // check that the two opposite corners are contained within this bot's collider
            if (ContainsBot2D(boxCollider, otherBox))
            {
                Destroy(other);
                Edible myEdible = GetComponent<Edible>();
                float prevScale = myEdible.scale;
                myEdible.scale += otherEdible.scale / 2;
                Debug.Log($"Eating before: {prevScale} after: {myEdible.scale}");
            }
        }
    }


    public static float GetScale(GameObject gameObject)
    {
        Vector3 scale = gameObject.transform.localScale;
        return scale.x * scale.y * scale.z;
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
