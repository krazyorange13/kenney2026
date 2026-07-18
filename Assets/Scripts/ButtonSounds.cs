using UnityEngine;

public class ButtonSounds : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip buttonclick;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
    }

    public void OnClick()
    {
        audioSource.PlayOneShot(buttonclick);
    }
}
