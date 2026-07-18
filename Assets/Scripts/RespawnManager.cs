using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RespawnManager : MonoBehaviour
{
    public GameObject respawnMenu;
    public Slider volumeSlider;

    void Start()
    {
        respawnMenu.SetActive(false);
        volumeSlider.value = PlayerPreferences.volume;
        // Time.timeScale = 1f;
        // Player player = FindAnyObjectByType<Player>();
        // if (player != null) {
        //     player.enabled = true;
        // }
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(0);
        ToggleMenu();
    }

    public void SetVolume(float sliderValue) {
        PlayerPreferences.volume = sliderValue;
        AudioListener.volume = Mathf.Pow(sliderValue, 2f);
    }

    public void ToggleMenu()
    {
        Time.timeScale = 0f;
        Player player = FindAnyObjectByType<Player>();
        if (player != null) {
            player.enabled = false;
        }
    }

    public void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
