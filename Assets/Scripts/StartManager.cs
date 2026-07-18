using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartManager : MonoBehaviour
{
    public Slider volumeSlider;

    public void StartGame() {
        int index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(index + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetVolume(float sliderValue) {
        AudioListener.volume = Mathf.Pow(sliderValue, 2f);
        // PlayerPreferences.volume = sliderValue;
    }
}
