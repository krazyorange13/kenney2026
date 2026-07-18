using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
  public Camera camera;
  public LayerMask groundLayer;
  public float movementSpeed = 5.0f;

  private List<Renderer> currentObstructions = new List<Renderer>();
  private float fadeAlpha = 0.3f;

  void Update()
  {
    Movement();
    Transparent();
  }

  void Movement() {
    Vector2 mousePos = Mouse.current.position.ReadValue();

    Ray ray = camera.ScreenPointToRay(mousePos);

    Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    if (groundPlane.Raycast(ray, out float distance))
    {
      Vector3 target = ray.GetPoint(distance);

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

  void Transparent() {
    foreach (Renderer rend in currentObstructions) {
      SetAlpha(rend, 1f);
    }

    currentObstructions.Clear();


    Vector3 origin = camera.transform.position;
    Vector3 destination = transform.position;
    Vector3 direction = destination - origin;
  
    RaycastHit[] hits = Physics.RaycastAll(origin, direction.normalized, direction.magnitude);
    
    foreach (RaycastHit hit in hits) {
      Renderer rend = hit.collider.GetComponent<Renderer>();

      if (rend != null) {
        currentObstructions.Add(rend);
        SetAlpha(rend, fadeAlpha);

        Debug.Log("BuildingRenderer Found");
      }
    }
  }

  void SetAlpha(Renderer rend, float alpha) {
    Material mat = rend.material;

    Color color = mat.color;
    color.a = alpha;
    mat.color = color;
  }
}

