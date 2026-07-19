using UnityEngine;
using System;

public class CameraMovement : MonoBehaviour
{
    public GameObject player;
    public float sizeBasis = 10f;
    public float cameraDistance = 150f;
    public float sizeMultiplier = 2f;
    public float smoothSpeed = 0.02f;

    private Vector3 velocity = Vector3.zero;

    private Camera camera;

    void Start()
    {
        camera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player == null) return;
        float playerSize = player.GetComponent<Edible>().scale;

        camera.orthographicSize = sizeBasis + ((float) Math.Sqrt(playerSize)) * sizeMultiplier;

        Vector3 targetPosition = player.transform.position - transform.forward * cameraDistance;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothSpeed
        );
    }
}
