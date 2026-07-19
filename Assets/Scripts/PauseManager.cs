using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseManager : MonoBehaviour
{
    private bool isPaused;
    public GameObject pauseMenu;
    public Slider volumeSlider;

    private InputActionAsset inputAsset;
    private InputAction escapeAction;
    private InputAction tabAction;

    private AudioSource audioSource;
    public AudioClip audioClip;

    void Start()
    {
        pauseMenu.SetActive(false);
        volumeSlider.value = PlayerPreferences.volume;
        isPaused = false;
        inputAsset = InputSystem.actions;
        tabAction = inputAsset.FindAction("Menu");
        escapeAction = inputAsset.FindAction("Escape");
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (escapeAction.WasPressedThisFrame())
        {
            TogglePause();
        }
        if (tabAction.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(0);
        TogglePause();
    }

    public void SetVolume(float sliderValue)
    {
        PlayerPreferences.volume = sliderValue;
        AudioListener.volume = Mathf.Pow(sliderValue, 2f);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        audioSource.PlayOneShot(audioClip);
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            Debug.Log("Stop Player");
            player.enabled = !isPaused;
        }

        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
