using System;
using Unity.Collections;
using UnityEngine;

public class BotController : MonoBehaviour
{
    private BotManager spawner;
    private float speed = 7.0f;
    private float sizeSpeedMult = 0.2f;
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

    void Start()
    {
        targetRotation = transform.rotation;
        boxCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
    }

    // OnDrawGizmosSelected
    void OnDrawGizmos()
    {
        float radius = transform.localScale.x + 15.0f * Mathf.Sqrt(transform.localScale.x);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);

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

    void Update()
    {
        float currentScale = GetComponent<Edible>().scale;
        transform.localScale = Vector3.one * ((float)Math.Sqrt(currentScale * Math.Pow(currentScale, 0.2f)));

        float speedBoost = 1.0f;

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
            if (target.name != "Player")
            {
                speedBoost = 1.2f;
            }
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
                timeToDirChange = UnityEngine.Random.Range(3f, 5f);
            }
        }

        // update position based on rotation
        // float scaledSpeed = speed * (float)Math.Pow(sizeSpeedMult, currentScale);
        float scaledSpeed = speed + sizeSpeedMult * currentScale;
        if (scaledSpeed >= 100)
        {
            scaledSpeed = 100;
        }
        // Debug.Log(scaledSpeed);
        Vector3 movement = transform.forward * scaledSpeed * speedBoost * Time.deltaTime;
        Vector3 nextPosition = transform.position + movement;

        // can't pass borders
        if (spawner != null)
        {
            if (nextPosition.x < spawner.getMinX()
            || nextPosition.x > spawner.getMaxX()
            || nextPosition.z < spawner.getMinZ()
            || nextPosition.z > spawner.getMaxZ())
            {
                Vector3 center = new Vector3((spawner.getMinX() + spawner.getMaxX()) / 2f, transform.position.y, (spawner.getMinZ() + spawner.getMaxZ()) / 2f);
                targetRotation = Quaternion.LookRotation(center - transform.position);
            }
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        transform.position = nextPosition;

        lateTarget = target;
        lateThreat = threat;
        target = null;
        threat = null;
        closestTargetDist = Mathf.Infinity;
        closestThreatDist = Mathf.Infinity;
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
                if (other.layer == 2)
                {
                    other.GetComponent<Player>().Die();
                }
                else
                {
                    Destroy(other);
                    Edible myEdible = GetComponent<Edible>();
                    float prevScale = myEdible.scale;
                    myEdible.scale += otherEdible.scale / 2;
                }
            }
        }
    }

    public static float GetTransformScale(GameObject obj)
    {
        Vector3 scale = obj.transform.localScale;
        return scale.x * scale.y * scale.z;
    }

    public static float GetBoxScale(GameObject obj)
    {
        if (!obj.TryGetComponent<BoxCollider>(out var box)) return 0;
        Vector3 scale = obj.transform.localScale;
        return scale.x * scale.y * scale.z * box.size.x * box.size.y * box.size.z;
    }

    public static float GetBoxScale(BoxCollider box)
    {
        Vector3 scale = box.transform.localScale;
        return scale.x * scale.y * scale.z * box.size.x * box.size.y * box.size.z;
    }

    public BotController setSpawner(BotManager spawner)
    {
        this.spawner = spawner;
        return this;
    }

    bool ContainsBot2D(BoxCollider mine, BoxCollider other)
    {
        if (GetBoxScale(mine) <= GetBoxScale(other)) return false;

        Vector3 myPos2D = mine.transform.position;
        myPos2D.y = 0;
        Vector3 urPos2D = other.transform.position;
        urPos2D.y = 0;

        float dist = Vector3.Distance(myPos2D, urPos2D);
        float radius = Vector2.Distance(new Vector2(mine.transform.localScale.x * mine.size.x, mine.transform.localScale.z * mine.size.z), Vector2.zero);
        float threshold = radius * 0.4f;

        if (dist >= threshold) return false;

        return true;
    }

    public void ProcessTriggers(NativeSlice<ColliderHit> others)
    {
        foreach (ColliderHit other in others)
        {
            if (other.collider == null) continue;
            ProcessTrigger(other.collider);
            Debug.DrawLine(transform.position, other.collider.transform.position, Color.orange);
        }
    }

    void ProcessTrigger(Collider other)
    {
        if (!other.gameObject.CompareTag("Edible")) return;

        float dist = Vector3.Distance(transform.position, other.transform.position);
        bool fullySentient = other.TryGetComponent<BotController>(out _);
        bool partiallySentient = other.TryGetComponent<Player>(out _);

        if (GetBoxScale(other.gameObject) >= GetBoxScale(gameObject))
        {
            if (gameObject == other.gameObject) return;
            if (!(fullySentient || partiallySentient)) return;

            if (dist < closestThreatDist)
            {
                closestThreatDist = dist;
                threat = other.gameObject;
            }
        }
        else
        {
            if (partiallySentient)
            {
                // yes eat the player please
                if (dist < closestTargetDist * 5.0f)
                {
                    closestTargetDist = 0.2f;
                    target = other.gameObject;
                }
            }
            else if (fullySentient)
            {
                // incentivise eating other stuff over unfruitful chases
                if (dist < closestTargetDist * 0.5f)
                {
                    closestTargetDist = dist * 2.0f;
                    target = other.gameObject;
                }
            }
            else if (dist < closestTargetDist)
            {
                closestTargetDist = dist;
                target = other.gameObject;
            }
        }
    }
}
