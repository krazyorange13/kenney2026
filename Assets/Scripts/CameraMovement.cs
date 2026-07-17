using UnityEngine;

public class CameraMovement : MonoBehaviour
{
  public GameObject player;
  public Vector3 offset;
  public float smoothSpeed = 0.05f;

  private Vector3 velocity = Vector3.zero;

  void LateUpdate()
  {
    transform.position = Vector3.SmoothDamp(
        transform.position,
        player.transform.position + offset,
        ref velocity,
        smoothSpeed
    );
  }
}
