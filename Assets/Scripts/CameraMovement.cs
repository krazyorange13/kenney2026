using UnityEngine;

public class CameraMovement : MonoBehaviour
{
  public GameObject player;
  private Vector3 offset = new Vector3(-75f, 150f, -35f);
  public float cameraDistance = 10f;
  public float sizeMultiplier = 2f;
  public float smoothSpeed = 0.02f;

  private Vector3 velocity = Vector3.zero;

  void LateUpdate()
  {
    float playerSize = player.GetComponent<Edible>().scale;

    this.GetComponent<Camera>().orthographicSize = cameraDistance + playerSize * sizeMultiplier;

    Vector3 targetPosition = player.transform.position + offset;

    transform.position = Vector3.SmoothDamp(
        transform.position,
        targetPosition,
        ref velocity,
        smoothSpeed
    );
  }
}
