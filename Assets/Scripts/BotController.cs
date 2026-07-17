using UnityEngine;

public class BotController : MonoBehaviour
{
    private float speed = 6;
    private float turnSpeed = 90f;
    public float scale = 1;
    private float timeToDirChange = 0;
    private Quaternion targetRotation;

    void Start()
    {
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

            timeToDirChange = 3f;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        transform.position += transform.forward * speed * scale * Time.deltaTime;
    }
}