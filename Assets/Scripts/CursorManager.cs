using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }
}
