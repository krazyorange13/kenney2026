using UnityEngine;

public class RandomRotate : MonoBehaviour
{
    public int steps = 4;

    void Start()
    {
        int stepI = Random.Range(0, steps);
        int stepAngle = 360 / 4;
        int rotation = stepI * stepAngle;
        Vector3 angles = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(angles.x, rotation, angles.z);
    }
}
