using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
  public Camera camera;
  public LayerMask groundLayer;
  public float movementSpeed = 5.0f;

  void Update()
  {
    Movement();
  }

  void Movement() {
    Vector2 mousePos = Mouse.current.position.ReadValue();
    Ray ray = camera.ScreenPointToRay(mousePos);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer)) {
      Vector3 target = hit.point;
      target.y = transform.position.y;
    
      if (target.magnitude > 0.01f) {

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            movementSpeed * Time.deltaTime
        );

        Vector3 direction = target - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            10f * Time.deltaTime
        );
      }
    }
  }
}
