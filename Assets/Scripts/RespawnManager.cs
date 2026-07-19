using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RespawnManager : MonoBehaviour
{
    private bool isOpen;
    public GameObject respawnMenu;
    public Slider volumeSlider;

    void Start()
    {
        respawnMenu.SetActive(false);
        isOpen = false;
        volumeSlider.value = PlayerPreferences.volume;
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(0);
        ToggleMenu();
    }

    public void SetVolume(float sliderValue)
    {
        PlayerPreferences.volume = sliderValue;
        AudioListener.volume = Mathf.Pow(sliderValue, 2f);
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;
        respawnMenu.SetActive(isOpen);
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            Debug.Log("Stop Player");
            player.enabled = !isOpen;
        }

        if (isOpen)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void Respawn()
    {
        ToggleMenu();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
