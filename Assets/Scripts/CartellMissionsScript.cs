using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CartellMissionsScript : MonoBehaviour
{
    private bool playerPotEntrar = false;
    [SerializeField] private Player player;
    [SerializeField] private GameObject uiCartell;
    private Transform missions;
    [SerializeField] private GameObject uiWorldInteraccio;
    private GameObject uiWorldInteraccio_tmp;

    // Start is called before the first frame update
    void Start()
    {
        missions = uiCartell.transform.Find("Missions");
    }

    // Update is called once per frame
    void Update()
    {
        if (playerPotEntrar)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                obreUICartell();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null && col.tag == player.tag && !col.isTrigger)
        {
            playerPotEntrar = true;
            // Mostrar Ajuda UI
            uiWorldInteraccio_tmp = Instantiate(uiWorldInteraccio, transform.position, Quaternion.identity);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col != null && col.tag == player.tag && !col.isTrigger)
        {
            playerPotEntrar = false;
            
            // Eliminar Ajuda UI
            GameObject.Destroy(uiWorldInteraccio_tmp);
        }
    }

    public void obreUICartell()
    {
        actualitzaDadesCartell();
        uiCartell.SetActive(true);
    }

    private void actualitzaDadesCartell()
    {
        amagarMissions();

        // mostrar missions actives
        MissionsInfo missionsInfo = WorldManager.Instance.getMissionsInfo();
        for (int i = 0; i < missionsInfo.maxMissions(); i++)
        {
            if (missionsInfo.esMissioActiva(i))
            {
                string titol = missionsInfo.getMissioTitol(i);
                string descripcio = missionsInfo.getMissioDescripcio(i);
                int monedes = missionsInfo.getMissioMonedes(i);
                int experiencia = missionsInfo.getMissioExperiencia(i);

                mostrarMissio(i, titol, descripcio, monedes, experiencia);
            }
        }
    }

    public void tancarUICartell()
    {
        uiCartell.SetActive(false);
    }

    public void mostrarMissio(int idMissio, string titol, string descripcio, int monedes, int experiencia) // Modificar els textos de la missio idMissio
    {
        Transform missioActual = missions.Find("Missio "+idMissio.ToString());
        missioActual.gameObject.SetActive(true);

        missioActual.Find("Nom").GetComponent<Text>().text = titol;
        missioActual.Find("Descripcio").GetComponent<Text>().text = descripcio;
        missioActual.Find("Monedes").GetComponent<Text>().text = monedes.ToString()+" Monedes";
        missioActual.Find("Experiencia").GetComponent<Text>().text = experiencia.ToString()+" XP";

        MissionsInfo missionsInfo = WorldManager.Instance.getMissionsInfo();
        if (missionsInfo.esMissioCompletada(idMissio))
        {
            missioActual.Find("Completar").gameObject.SetActive(true);
            missioActual.Find("Activar").gameObject.SetActive(false);
            
            missioActual.Find("Eliminar").GetComponent<Button>().interactable = false;
        }
        else 
        {
            missioActual.Find("Completar").gameObject.SetActive(false);
            missioActual.Find("Activar").gameObject.SetActive(true);

            if (missionsInfo.esMissioActual(idMissio))
            {
                missioActual.Find("Activar").GetComponent<Button>().interactable = false;
                missioActual.Find("Eliminar").GetComponent<Button>().interactable = false;
            }
            else
            {
                missioActual.Find("Activar").GetComponent<Button>().interactable = true;
                missioActual.Find("Eliminar").GetComponent<Button>().interactable = true;
            }
        }
    }

    public void amagarMissions()
    {
        foreach(Transform missio in missions)
        {
            missio.Find("Completar").gameObject.SetActive(false);
            missio.Find("Activar").gameObject.SetActive(true);
            
            missio.Find("Nom").GetComponent<Text>().text = "";
            missio.Find("Descripcio").GetComponent<Text>().text = "";
            missio.Find("Monedes").GetComponent<Text>().text = "0 Monedes";
            missio.Find("Experiencia").GetComponent<Text>().text = "0 XP";

            missio.Find("Activar").GetComponent<Button>().interactable = false;
            missio.Find("Eliminar").GetComponent<Button>().interactable = false;
        }
    }

    public void setInteractableMissio(int idMissio, bool inter)
    {
        Transform missioActual = missions.Find("Missio "+idMissio.ToString());
        missioActual.Find("Activar").GetComponent<Button>().interactable = inter;
        missioActual.Find("Eliminar").GetComponent<Button>().interactable = inter;
    }

    public void activarMissio(int idMissio)
    {
        // Activar la missio numero idMissio
        MissionsInfo missionsInfo = WorldManager.Instance.getMissionsInfo();
        missionsInfo.setMissioActiva(idMissio);

        actualitzaDadesCartell();
    }

    public void eliminarMissio(int idMissio)
    {
        // Eliminar la missio numero idMissio
        MissionsInfo missionsInfo = WorldManager.Instance.getMissionsInfo();
        missionsInfo.eliminaMissio(idMissio);
        actualitzaDadesCartell();
    }

    public void completarMissio(int idMissio)
    {
        MissionsInfo missionsInfo = WorldManager.Instance.getMissionsInfo();
        PartidaManager.Instance.playerCompletaMissio(missionsInfo.getMissioMonedes(idMissio), missionsInfo.getMissioExperiencia(idMissio));
        missionsInfo.setMissioActiva(-1);

        eliminarMissio(idMissio);
    }
}
