using UnityEngine;

public class CameraPan : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float duration;

    void Update()
    {
        float t = Mathf.PingPong(Time.time / duration, 1.0f);
        transform.position = Vector3.Lerp(startPosition, endPosition, t);
    }
}
