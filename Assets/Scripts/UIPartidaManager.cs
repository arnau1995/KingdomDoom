using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIPartidaManager : MonoBehaviour
{
    private Transform temps;
    private Text textTemps;
    private Image imatgeTemps;
    private GameObject uiStart;
    private GameObject uiBotonsAtacs;
    private GameObject accionsInfo;
    [SerializeField] private Player player;

    private string accioActiva = "";

    void Awake()
    {
        temps = transform.Find("Temps").transform;
        textTemps = temps.Find("Text Temps").GetComponent<Text>();
        imatgeTemps = temps.Find("Imatge Temps").GetComponent<Image>();
        uiStart = transform.Find("Start").gameObject;
        uiBotonsAtacs = transform.Find("Atacs").gameObject;
        accionsInfo = transform.Find("Info").gameObject;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.M))
        {
            AccioOnClick("moviment");
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            finalitzaTornPlayer();
        }
        else
        {
            int keycode = -1;
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                keycode = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                keycode = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                keycode = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                keycode = 4;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                keycode = 5;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                keycode = 6;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                keycode = 7;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                keycode = 8;
            }
            
            if (keycode != -1) uiBotonsAtacs.transform.GetChild(keycode-1).GetChild(0).GetComponent<Button>().onClick.Invoke();
        }
    }

    public void mostrarBotoStart()
    {
        uiStart.SetActive(true);
    }

    public void AccioOnClick(string tipus)
    {
        PartidaManager.Instance.mostraAccioUsuari(tipus);
    }

    public void AccioPointerEnterExit(string tipus)
    {
        if (accioActiva == tipus)
        {
            accioActiva = "";

            accionsInfo.SetActive(false);
        }
        else
        {
            accioActiva = tipus;

            Transform boxInfo = accionsInfo.transform.Find("Box");
            // Mostrar dades atac:
            boxInfo.Find("Titol").Find("Text").GetComponent<Text>().text = AtacManager.Instance.getDescripcioAtac(accioActiva);

            boxInfo.Find("Mana Cost").GetComponent<Text>().text = AtacManager.Instance.getManaAtac(accioActiva).ToString();

            int damage = AtacManager.Instance.getDamageAtac(accioActiva, player.getNivell()); // Agafem el "damage" base de l'atac
            damage += (damage * player.getAtacEquipament() / 100);                           //  Sumem el damage de l'equipament
            boxInfo.Find("Atac").GetComponent<Text>().text = damage.ToString();

            int heal = AtacManager.Instance.getHealAtac(accioActiva, player.getNivell()); // Agafem el "heal" base de l'atac
            heal += (heal * player.getAtacEquipament() / 100);                           //  Sumem el heal de l'equipament (atac = heal)
            boxInfo.Find("Cura").GetComponent<Text>().text = heal.ToString();

            boxInfo.Find("Emputxar").GetComponent<Text>().text = AtacManager.Instance.getHabilitatEspecial(accioActiva, 3).ToString();

            boxInfo.Find("Mana").GetComponent<Text>().text = AtacManager.Instance.getHabilitatEspecial(accioActiva, 1).ToString();

            boxInfo.Find("Passos").GetComponent<Text>().text = AtacManager.Instance.getHabilitatEspecial(accioActiva, 2).ToString();
            
            Transform rangAtac = boxInfo.Find("Rang Atac");
            rangAtac.GetComponent<Text>().text = AtacManager.Instance.getRangAtacMin(accioActiva).ToString()+"-"+AtacManager.Instance.getRangAtacMax(accioActiva).ToString();
            char tipusAtac = AtacManager.Instance.getTipusAtac(accioActiva);
            if (tipusAtac == 'N' || tipusAtac == 'I') // En area
            {
                rangAtac.Find("Area").gameObject.SetActive(true);
                rangAtac.Find("Linia").gameObject.SetActive(false);
            }
            else // En linea
            {
                rangAtac.Find("Area").gameObject.SetActive(false);
                rangAtac.Find("Linia").gameObject.SetActive(true);
            }

            if (tipusAtac == 'N' || tipusAtac == 'L' || tipusAtac == 'R') // Necesita linia de visió
            {
                rangAtac.Find("Visio").gameObject.SetActive(true);
                rangAtac.Find("No Visio").gameObject.SetActive(false);
            }
            else // Sense linia de visió
            {
                rangAtac.Find("Visio").gameObject.SetActive(false);
                rangAtac.Find("No Visio").gameObject.SetActive(true);
            }

            Transform rangArea = boxInfo.Find("Rang Area");
            rangArea.GetComponent<Text>().text = AtacManager.Instance.getAreaAtacMin(accioActiva).ToString()+"-"+AtacManager.Instance.getAreaAtacMax(accioActiva).ToString();
            char tipusAreaAtac = AtacManager.Instance.getTipusAreaAtac(accioActiva);
            if (tipusAreaAtac == 'N' || tipusAreaAtac == 'I') // En area
            {
                rangArea.Find("Area").gameObject.SetActive(true);
                rangArea.Find("Linia").gameObject.SetActive(false);
            }
            else // En linea
            {
                rangArea.Find("Area").gameObject.SetActive(false);
                rangArea.Find("Linia").gameObject.SetActive(true);
            }
            
            accionsInfo.GetComponent<RectTransform>().position = uiBotonsAtacs.transform.Find(accioActiva).GetComponent<RectTransform>().position;
            accionsInfo.SetActive(true);
        }
    }

    public void actualitzaTemps(float tempsActual, float maxTemps)
    {
        textTemps.text = ((int) (maxTemps - tempsActual)).ToString();
        imatgeTemps.fillAmount = (maxTemps - tempsActual) / maxTemps;
    }

    public void finalitzaTornPlayer()
    {
        if (PartidaManager.Instance.tornActualPlayer())
        {
            PartidaManager.Instance.finalitzarTornActual();
        }
    }

    public void iniciaLluita()
    {
        PartidaManager.Instance.finalitzarPosicionament();
    }

    public void amagaStart()
    {
        uiStart.SetActive(false);
    }

    public void cambiaUiAtacsArma(string[] ordreAtacs, Dictionary <string, GameObject> uiAtacs)
    {
        gameObject.SetActive(true);

        int i = 1;
        foreach(string atac in ordreAtacs)
        {
            Transform boto = uiBotonsAtacs.transform.GetChild(i-1);
            boto.name = atac;

            // Eliminem els fills existents
            foreach (Transform child in boto) {
                Destroy(child.gameObject);
            }

            foreach (EventTrigger.Entry entry in boto.GetComponent<EventTrigger>().triggers)
            {
                entry.callback.RemoveAllListeners();
                entry.callback.AddListener( ( data ) => { AccioPointerEnterExit( atac ); } );
            }

            GameObject nouAtac = Instantiate(uiAtacs[atac], boto.position, Quaternion.identity, boto);
            nouAtac.GetComponent<Button>().onClick.AddListener(delegate{AccioOnClick(atac);});

            i++;
        }

        gameObject.SetActive(false);
    }
}
