using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Camera camera;
    public LayerMask groundLayer;
    public float movementSpeed = 5.0f;
    public float sprintSpeed = 10.0f;

    private float staminaCount;
    private float staminaTotal = 5f;

    private List<GameObject> currentObstructions = new List<GameObject>();
    private float fadeAlpha = 0.3f;
    private BoxCollider boxCollider;

    private AudioSource audioSource;
    public AudioClip growthAudio;

    public TextMeshProUGUI scoreText;

    public Slider staminaSlider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
        staminaCount = staminaTotal;
    }

    void Update()
    {
        float scale = GetComponent<Edible>().scale;
        transform.localScale = Vector3.one * scale;
        scoreText.text = ((int)scale).ToString();

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

            Vector3 direction = target - transform.position;

            if (direction.magnitude > 0.01f)
            {
                float speed;
                if (Mouse.current.leftButton.isPressed && staminaCount > 0f)
                {
                    speed = sprintSpeed;
                    staminaCount -= Time.deltaTime;
                }
                else
                {
                    speed = movementSpeed;
                }

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    speed * Time.deltaTime
                );


                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    10f * Time.deltaTime
                );
            }
        }

        if (!Mouse.current.leftButton.isPressed)
        {
            staminaCount += Time.deltaTime * 0.5f;
            if (staminaCount > staminaTotal)
            {
                staminaCount = staminaTotal;
            }
        }

        staminaSlider.value = staminaCount / staminaTotal;
    }

    void Transparent()
    {
        foreach (GameObject obj in currentObstructions)
        {
            if (obj == null) continue;
            SetAlpha(obj, 1.0f);
            // Debug.Log($"Obstruction defaded: {obj.name}");
        }

        currentObstructions.Clear();

        Vector3 origin = camera.transform.position;
        Vector3 destination = transform.position;
        Vector3 direction = destination - origin;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction.normalized, direction.magnitude);

        foreach (RaycastHit hit in hits)
        {
            GameObject obj = hit.collider.gameObject;

            // Debug.Log($"Obstruction found: {obj.name}");
            currentObstructions.Add(obj);
            SetAlpha(obj, fadeAlpha);
            // Debug.Log($"Obstruction faded: {obj.name}");
        }
    }

    void SetAlpha(GameObject obj, float alpha)
    {
        AutoTransparency autoTransparency = obj.GetComponent<AutoTransparency>();
        if (autoTransparency == null) return;
        _SetAlpha(obj, alpha, autoTransparency);
    }

    void _SetAlpha(GameObject obj, float alpha, AutoTransparency autoTransparency)
    {
        Color color = new Color(1.0f, 1.0f, 1.0f, alpha);
        autoTransparency.materialPropertyBlock.SetColor("_Base_Color", color);
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.SetPropertyBlock(autoTransparency.materialPropertyBlock);
        }
        foreach (Transform child in obj.transform)
        {
            _SetAlpha(child.gameObject, alpha, autoTransparency);
            // renderer = child.GetComponent<Renderer>();
            // if (renderer != null)
            // {
            //     Debug.Log($"Obstruction child faded: {child.name}");
            //     renderer.SetPropertyBlock(autoTransparency.materialPropertyBlock);
            // }
        }
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
                Edible myEdible = GetComponent<Edible>();
                Edible otherEdible = bot.GetComponent<Edible>();
                float prevScale = myEdible.scale;
                myEdible.scale += otherEdible.scale / 2;
                Debug.Log($"Eating before: {prevScale} after: {myEdible.scale}");
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

