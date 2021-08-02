using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIWorldManager : MonoBehaviour
{
    private GameObject vida;
    private Text textVida;
    private Image imatgeVida;
    private GameObject experiencia;
    private Text textExperiencia;
    private Image imatgeExperiencia;
    private GameObject nivell;
    private Text textNivell;
    private GameObject mana_moviment;
    private Text textMana;
    private Text textMoviment;
    private GameObject monedes;
    private Text textMonedes;

    private GameObject pause_menu;
    private Slider audioModifier;
    private GameObject fight_menu;

    private GameObject tutorial_menu;

    private GameObject avisos;
    [SerializeField] private GameObject[]  uiPreferences;
    
    void Awake()
    {
        vida = transform.Find("Vida").gameObject;
        textVida = vida.transform.Find("Text").GetComponent<Text>();
        imatgeVida = vida.transform.Find("Fill").GetComponent<Image>();
        
        experiencia = transform.Find("Experiencia").gameObject;
        textExperiencia = experiencia.transform.Find("Text").GetComponent<Text>();
        imatgeExperiencia = experiencia.transform.Find("Fill").GetComponent<Image>();

        nivell = transform.Find("Nivell").gameObject;
        textNivell = nivell.transform.Find("Text").GetComponent<Text>();

        mana_moviment = transform.Find("Mana_Moviments").gameObject;
        textMana = mana_moviment.transform.Find("Text Mana").GetComponent<Text>();
        textMoviment = mana_moviment.transform.Find("Text Moviments").GetComponent<Text>();

        monedes = transform.Find("Monedes").gameObject;
        textMonedes = monedes.transform.Find("Text").GetComponent<Text>();

        pause_menu = transform.Find("Pause Menu").gameObject;
        pause_menu.SetActive(false);

        fight_menu = transform.Find("Fight Menu").gameObject;
        fight_menu.SetActive(false);

        avisos = transform.Find("Avisos").gameObject;

        tutorial_menu = transform.Find("Tutorial").gameObject;
        if (PlayerPrefs.GetInt("tutorial", 0) == 0) // comprovar si cal mostrar el tuto !!
        {
            iniciaTutorial();
        }

        audioModifier = pause_menu.transform.Find("Audio Modifier").GetComponent<Slider>();
        float audioLevel = PlayerPrefs.GetFloat("audioLevel", 1);
        audioModifier.value = audioLevel;
        modifyAudio();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool openPause = true;
            int i = 0;
            while (openPause && i < uiPreferences.Length)
            {
                if (uiPreferences[i].activeSelf) 
                {
                    uiPreferences[i].SetActive(false);
                    openPause = false;
                }
                i++;
            }

            if (openPause)
            {
                if (!pause_menu.activeSelf) pauseGame();
                else resumeGame();
            }
        }
    }

    public void pauseGame()
    {
        WorldManager.Instance.pauseAudio(true);
        Time.timeScale = 0f;
        pause_menu.SetActive(true);
    }

    public void resumeGame()
    {
        WorldManager.Instance.pauseAudio(false);
        pause_menu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void mainMenuGame()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        Time.timeScale = 1.0f;
    }

    public void iniciaLluita()
    {
        fight_menu.SetActive(true);
    }

    public void actualitzaVida(float vida, float maxVida)
    {
        textVida.text = vida.ToString()+" / "+maxVida.ToString();
        imatgeVida.fillAmount = vida / maxVida;
    }

    public void actualitzaExperiencia(float experiencia, float maxExperiencia)
    {
        textExperiencia.text = experiencia.ToString()+" / "+maxExperiencia.ToString();
        imatgeExperiencia.fillAmount = experiencia / maxExperiencia;
    }

    public void actualitzaNivell(int nivell)
    {
        textNivell.text = nivell.ToString();
    }

    public void actualitzaMana(int mana)
    {
        textMana.text = mana.ToString();
    }

    public void actualitzaMoviments(int moviments)
    {
        textMoviment.text = moviments.ToString();
    }

    public void actualitzaMonedes(int monedes)
    {
        textMonedes.text = monedes.ToString();
    }

    public void mostraMenuFinal()
    {
        Time.timeScale = 0f;
        transform.Find("Finish Menu").gameObject.SetActive(true);
    }

    public void tencaMenuFinal()
    {
        transform.Find("Finish Menu").gameObject.SetActive(false);
        Time.timeScale = 1.0f;
    }

    public void mostraAvisos(string avis)
    {
        avisos.transform.Find(avis).gameObject.SetActive(true);
    }

    public void iniciaTutorial()
    {
        tutorial_menu.SetActive(true);
    }

    public void tutorialDesactivaPagina(int numPagina)
    {
        tutorial_menu.transform.Find("Pagina "+numPagina).gameObject.SetActive(false);
    }

    public void tutorialActivaPagina(int numPagina)
    {
        tutorial_menu.transform.Find("Pagina "+numPagina).gameObject.SetActive(true);
    }

    public void finalitzaTutorial()
    {
        // Guardar l'int tutorial
        PlayerPrefs.SetInt("tutorial", 1);
        tutorial_menu.SetActive(false);
    }

    public void modifyAudio()
    {
        float audioLevel = audioModifier.value * audioModifier.value;
        PlayerPrefs.SetFloat("audioLevel", audioModifier.value);
        AudioListener.volume = audioLevel;
    }
}
