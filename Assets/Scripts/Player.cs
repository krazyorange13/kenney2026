using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public Camera camera;
    public LayerMask groundLayer;
    public float movementSpeed = 5.0f;

    private List<GameObject> currentObstructions = new List<GameObject>();
    private float fadeAlpha = 0.3f;
    public float scale = 2;
    private BoxCollider boxCollider;

    private AudioSource audioSource;
    public AudioClip growthAudio;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        transform.localScale = Vector3.one * scale;

        Movement();
        Transparent();
        Eating();
    }

    void Movement()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = camera.ScreenPointToRay(mousePos);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 target = ray.GetPoint(distance);

            target.y = transform.position.y;

            if (target.magnitude > 0.01f)
            {

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    movementSpeed * Time.deltaTime
                );

                Vector3 direction = target - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    10f * Time.deltaTime
                );
            }
        }
    }

    void Transparent()
    {
        foreach (GameObject obj in currentObstructions)
        {
            SetAlpha(obj, 1.0f);
            Debug.Log($"Obstruction defaded: {obj.name}");
        }

        currentObstructions.Clear();

        Vector3 origin = camera.transform.position;
        Vector3 destination = transform.position;
        Vector3 direction = destination - origin;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction.normalized, direction.magnitude);

        foreach (RaycastHit hit in hits)
        {
            GameObject obj = hit.collider.gameObject;

            Debug.Log($"Obstruction found: {obj.name}");

            if (obj.GetComponent<Renderer>() != null)
            {
                currentObstructions.Add(obj);
                SetAlpha(obj, fadeAlpha);
                Debug.Log($"Obstruction faded: {obj.name}");
            }
        }
    }

    void SetAlpha(GameObject obj, float alpha)
    {
        AutoTransparency autoTransparency = obj.GetComponent<AutoTransparency>();
        if (autoTransparency == null) return;
        Color color = new Color(1.0f, 1.0f, 1.0f, alpha);
        autoTransparency.materialPropertyBlock.SetColor("_Base_Color", color);
        obj.GetComponent<Renderer>().SetPropertyBlock(autoTransparency.materialPropertyBlock);

    }

    void Eating()
    {
        foreach (GameObject bot in GameObject.FindGameObjectsWithTag("Edible"))
        {
            if (bot == gameObject)
                continue;

            BoxCollider other = bot.GetComponent<BoxCollider>();

            // check that the two opposite corners are contained within this bot's collider
            if (ContainsBot2D(this.boxCollider, other))
            {
                audioSource.PlayOneShot(growthAudio);
                Destroy(bot);
                BotController otherController = bot.GetComponent<BotController>();
                scale = scale + otherController.scale;
            }
        }
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

