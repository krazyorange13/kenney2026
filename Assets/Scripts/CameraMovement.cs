using UnityEngine;

public class CameraMovement : MonoBehaviour
{
  public GameObject player;
  public Vector3 offset = new Vector3(-6f, 14f, -3f);
  public float smoothSpeed = 0.02f;

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
