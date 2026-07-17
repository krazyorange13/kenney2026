using UnityEngine;

public class CameraMovement : MonoBehaviour
{
  public GameObject player;
  public Vector3 offset;

  void LateUpdate()
  {
      transform.position = player.transform.position + offset;
  }
}
