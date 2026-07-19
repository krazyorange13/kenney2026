using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;

public class Player : MonoBehaviour
{
    public Camera camera;
    public LayerMask groundLayer;
    public float movementSpeed = 5.0f;
    public float sprintSpeed = 10.0f;
    private float speedSizeMult = 1.01f;

    private float staminaCount;
    private float staminaTotal = 5f;

    private List<GameObject> currentObstructions = new List<GameObject>();
    private float fadeAlpha = 0.3f;
    private BoxCollider boxCollider;

    private AudioSource audioSource;
    public AudioClip growthAudio;

    public TextMeshProUGUI scoreText;

    public Slider staminaSlider;
    public BotManager botManager;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
        staminaCount = staminaTotal;
    }

    void Update()
    {
        float scale = GetComponent<Edible>().scale;
        transform.localScale = Vector3.one * ((float) Math.Sqrt(scale));
        scoreText.text = ((int)scale).ToString();

        Movement(scale);
        Transparent();
        DebugThreshold();
    }

    void DebugThreshold()
    {
        BoxCollider mine = gameObject.GetComponent<BoxCollider>();

        Vector2 random = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 direction = new Vector3(random.x, 0f, random.y);

        float radius = Vector2.Distance(new Vector2(mine.transform.localScale.x * mine.size.x, mine.transform.localScale.z * mine.size.z), Vector2.zero);
        float threshold = radius * 0.5f;

        Debug.DrawLine(mine.transform.position, mine.transform.position + direction * threshold, Color.yellowNice, 0.2f, false);

    }

    void Movement(float scale)
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

                float scaledSpeed = speed * (float) Math.Pow(speedSizeMult, scale);
                if (scaledSpeed > 100)
                {
                    scaledSpeed = 100;
                }

                Vector3 newPosition = Vector3.MoveTowards(
                    transform.position,
                    target,
                    scaledSpeed * Time.deltaTime
                );

                // prevent going outside border
                if (botManager != null)
                {
                    float margin = 0.5f;

                    newPosition.x = Mathf.Clamp(
                        newPosition.x,
                        botManager.getMinX() + margin,
                        botManager.getMaxX() - margin
                    );

                    newPosition.z = Mathf.Clamp(
                        newPosition.z,
                        botManager.getMinZ() + margin,
                        botManager.getMaxZ() - margin
                    );
                }

                transform.position = newPosition;

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
            staminaCount += Time.deltaTime * 0.7f;
            staminaCount = Math.Min(staminaCount, staminaTotal);
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
                audioSource.PlayOneShot(growthAudio);
                Destroy(other);
                Edible myEdible = GetComponent<Edible>();
                float prevScale = myEdible.scale;
                myEdible.scale += otherEdible.scale / 2;
                // Debug.Log($"Eating before: {prevScale} after: {myEdible.scale}");
            }
        }
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

    bool ContainsBot2D(BoxCollider mine, BoxCollider other)
    {
        if (GetBoxScale(mine) <= GetBoxScale(other)) return false;

        Vector3 myPos2D = mine.transform.position;
        myPos2D.y = 0;
        Vector3 urPos2D = other.transform.position;
        urPos2D.y = 0;

        float dist = Vector3.Distance(myPos2D, urPos2D);
        float radius = Vector2.Distance(new Vector2(mine.transform.localScale.x * mine.size.x, mine.transform.localScale.z * mine.size.z), Vector2.zero);
        float threshold = radius * 0.5f;

        Debug.DrawLine(mine.transform.position, mine.transform.position + Vector3.forward * threshold, Color.yellowNice, 0.02f, false);

        if (dist >= threshold) return false;

        return true;
    }

    public void Die()
    {
        SendScore();

        RespawnManager respawnManager = FindAnyObjectByType<RespawnManager>();
        respawnManager.ToggleMenu();
    }

    public void SendScore()
    {
        int score = (int)gameObject.GetComponent<Edible>().scale;
        StartCoroutine(Leaderboard.SubmitScore(score));
    }
}

