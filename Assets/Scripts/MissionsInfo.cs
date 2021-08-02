using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionsInfo : MonoBehaviour
{
    [SerializeField] private SpawnerInfo spawnerInfo;
    [SerializeField] private int minEnemics = 5;
    [SerializeField] private int maxEnemics = 15;
    private Missio[] missions;
    private int missioActiva;

    [SerializeField] private float tempsNovaMissioTotal = 180; // 180 sec = 3 min
    private float tempsNovaMissio;
    private float tempsMissio=0;

    // UI //////////////////////////
    [SerializeField]private GameObject missionsUI;
    private Text missionsUIDescripcio;
    private Text missionsUIMonedes;
    private Text missionsUIExperiencia;

    void Start()
    {
        //SaveSystem.DeleteMissionsInfo(); // Eliminar el fitxer missions.info

        tempsNovaMissio = tempsNovaMissioTotal;
        if (PlayerPrefs.GetInt("tutorial", 0) == 0)
        {
            //tempsNovaMissio /= 3;
            tempsNovaMissio = 10;
        }
        
        missionsUIDescripcio = missionsUI.transform.Find("Descripcio").GetComponent<Text>();
        missionsUIMonedes = missionsUI.transform.Find("Monedes").GetComponent<Text>();
        missionsUIExperiencia = missionsUI.transform.Find("Experiencia").GetComponent<Text>();

        LoadMissionsInfo();

        actualitzaUIMissioActiva();
    }

    void Update()
    {
        tempsMissio += Time.deltaTime;
        if (tempsMissio >= tempsNovaMissio)
        {
            int newMissio = missioNoCreada();
            if (newMissio != -1)
            {
                missions[newMissio] = new Missio(spawnerInfo.numEnemics(), minEnemics, maxEnemics);
                WorldManager.Instance.mostraAvisos("Nova Missio");
                SaveMissionsInfo();
            }

            tempsMissio = 0;
            if (tempsNovaMissio != tempsNovaMissioTotal) tempsNovaMissio = tempsNovaMissioTotal;
        }
    }

    public void SaveMissionsInfo()
    {
        SaveSystem.SaveMissionsInfo(this);
    }

    public void LoadMissionsInfo()
    {
        SaveMissionsInfo loadedInfo = SaveSystem.LoadMissionsInfo();
        if (loadedInfo != null)
        {
            missions = new Missio[loadedInfo.missions.Length];
            for (int i = 0; i < missions.Length; i++)
            {
                missions[i] = loadedInfo.missions[i];
            }

            missioActiva = loadedInfo.missioActiva;
        }
        else
        {
            missions = new Missio[3];
            int i = 0;
            while (i < missions.Length)
            {
                missions[i] = null;
                i++;
            }
            missioActiva = -1;
        }
    }

    private int missioNoCreada()
    {
        int i = 0;
        while (i < missions.Length)
        {
            if (missions[i] == null) return i;
            i++;
        }
        return -1;
    }

    private void actualitzaUIMissioActiva()
    {
        string missioText = "";
        string monedesText = "";
        string experienciaText = "";

        float heightMissions = 18;
        float lineHeight = 7.5f;
        if (missioActiva != -1)
        {
            missioText += getMissioTitol(missioActiva)+"\n\n";
            heightMissions += (lineHeight*2);
            
            missioText += getMissioDescripcio(missioActiva);
            for (int i = 0; i < missions[missioActiva].getTotalEnemicsId(); i++)
            {
                if (missions[missioActiva].getEnemicsNeeded(i) > 0) heightMissions += lineHeight;
            }

            monedesText = getMissioMonedes(missioActiva).ToString();
            experienciaText = getMissioExperiencia(missioActiva).ToString();
        }
        
        RectTransform rtMissionsUI = missionsUI.transform.GetComponent<RectTransform>();
        rtMissionsUI.sizeDelta = new Vector2(rtMissionsUI.sizeDelta.x, heightMissions);
        missionsUIDescripcio.text = missioText;
        missionsUIMonedes.text = monedesText;
        if (monedesText == "") missionsUIMonedes.gameObject.SetActive(false);
        else missionsUIMonedes.gameObject.SetActive(true);
        missionsUIExperiencia.text = experienciaText;
        if (experienciaText == "") missionsUIExperiencia.gameObject.SetActive(false);
        else missionsUIExperiencia.gameObject.SetActive(true);
    }

    public int maxMissions()
    {
        return missions.Length;
    }

    public Missio getMissio(int idMissio)
    {
        return missions[idMissio];
    }

    public string getMissioTitol(int idMissio)
    {
        return "Matar " + missions[idMissio].getTotalEnemicsNeeded().ToString() + " enemics";
    }

    public string getMissioDescripcio(int idMissio)
    {
        string missioDescripcio = "";
        for (int i = 0; i < missions[idMissio].getTotalEnemicsId(); i++)
        {
            if (missions[idMissio].getEnemicsNeeded(i) > 0)
            {
                missioDescripcio += "    "+spawnerInfo.getEnemicNom(i)+": "+missions[idMissio].getEnemicsActuals(i).ToString()+" / "+missions[idMissio].getEnemicsNeeded(i).ToString()+"\n";
            }
        }
        if (missioDescripcio != "") missioDescripcio = missioDescripcio.Substring(0, missioDescripcio.Length-1);
        return missioDescripcio;
    }

    public bool esMissioActual(int idMissio)
    {
        return idMissio == missioActiva;
    }

    public bool esMissioCompletada(int idMissio)
    {
        return missions[idMissio].esMissioCompletada();
    }

    public int getMissioMonedes(int idMissio)
    {
        return missions[idMissio].getMonedes();
    }

    public int getMissioExperiencia(int idMissio)
    {
        return missions[idMissio].getExperiencia();
    }

    public bool esMissioActiva(int idMissio)
    {
        return missions[idMissio] != null;
    }

    public int getMissioActiva()
    {
        return missioActiva;
    }
    public void setMissioActiva(int newMissio)
    {
        missioActiva = newMissio;
        SaveMissionsInfo();
        // Actualitzar UI
        actualitzaUIMissioActiva();
    }

    public void eliminaMissio(int idMissio)
    {
        missions[idMissio] = null;
        SaveMissionsInfo();
    }

    public void actualitzaMissioActual(int[] idEnemics)
    {
        if (missions.Length > 0 && missioActiva != -1)
        {
            for (int i = 0; i < idEnemics.Length; i++)
            {
                missions[missioActiva].takeEnemicId(idEnemics[i]);
            }
            SaveMissionsInfo();
            actualitzaUIMissioActiva();
        }
    }
}
