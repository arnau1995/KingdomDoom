using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private Slider audioModifier;

    void Start()
    {
        if (PlayerPrefs.GetInt("tutorial", 0) == 0)
        {
            transform.Find("Canvas/Continuar").GetComponent<Button>().interactable = false;
            transform.Find("Canvas/Continuar/Text").GetComponent<Text>().color = Color.gray;
        }
        else
        {
            transform.Find("Canvas/Continuar").GetComponent<Button>().interactable = true;
            transform.Find("Canvas/Continuar/Text").GetComponent<Text>().color = Color.white;
        }

        audioModifier = transform.Find("Canvas/Audio Modifier").GetComponent<Slider>();
        float audioLevel = PlayerPrefs.GetFloat("audioLevel", 1);
        audioModifier.value = audioLevel;
        modifyAudio();
        AudioListener.pause = false;
    }
    public void onClickIniciar()
    {
        float audioLevel = PlayerPrefs.GetFloat("audioLevel", 1);
        PlayerPrefs.DeleteAll();
        audioModifier.value = audioLevel;
        modifyAudio();
        SaveSystem.DeleteMissionsInfo(); // Eliminar el fitxer missions.info
        onClickContinuar();
    }
    public void onClickContinuar()
    {
        SceneManager.LoadScene("MonObert", LoadSceneMode.Single);
        Time.timeScale = 1.0f;
    }
    public void onClickSortir()
    {
        Application.Quit();
    }

    public void modifyAudio()
    {
        float audioLevel = audioModifier.value * audioModifier.value;
        PlayerPrefs.SetFloat("audioLevel", audioModifier.value);
        AudioListener.volume = audioLevel;
    }
}
