using UnityEngine;

public class AutoTransparency : MonoBehaviour
{
    public MaterialPropertyBlock materialPropertyBlock { get; set; }

    void Start()
    {
        materialPropertyBlock = new MaterialPropertyBlock();
    }
}
