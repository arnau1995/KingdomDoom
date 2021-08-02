using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotigaScript : MonoBehaviour
{
    private bool playerPotEntrar = false;
    [SerializeField] private Player player;
    [SerializeField] private GameObject uiBotiga;
    [SerializeField] private GameObject uiWorldInteraccio;
    private GameObject uiWorldInteraccio_tmp;
    private Transform equipament;

    // Start is called before the first frame update
    void Start()
    {
        equipament = uiBotiga.transform.Find("Equipament");
    }

    // Update is called once per frame
    void Update()
    {
        if (playerPotEntrar)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                obreUIBotiga();
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

    public void obreUIBotiga()
    {
        actualitzaUIBotiga();

        uiBotiga.SetActive(true);
    }

    private void actualitzaUIBotiga()
    {
        for (int i = 0; i < equipament.childCount; i++) // i = idEquipament
        {
            modificaUIEquipament(i);
        }
    }

    private void modificaUIEquipament(int id)
    {
        Transform arma = equipament.GetChild(id);
        Transform buttonEquipar = arma.Find("Equipar");
        Transform buttonComprar = arma.Find("Comprar");

        buttonEquipar.gameObject.SetActive(false);
        buttonComprar.gameObject.SetActive(false);
        
        if (id == 0 || PlayerPrefs.GetInt("equipament_"+id, 0) != 0) 
        {
            if (player.getArmaEquipada() == id)
            {
                buttonEquipar.GetComponent<Button>().interactable = false;
            }
            else
            {
                buttonEquipar.GetComponent<Button>().interactable = true;
            }

            buttonEquipar.gameObject.SetActive(true);
        }
        else
        {
            buttonComprar.gameObject.SetActive(true);
        }
    }

    public void tencaUIBotiga()
    {
        uiBotiga.SetActive(false);
    }

    public void onCompraEquipament(int idEquipament)
    {
        // Comprem segons calgui
        int preu = int.Parse( equipament.GetChild(idEquipament).Find("Monedes").GetComponent<Text>().text.Replace(" Monedes", "") );
        if (player.getMonedes() >= preu)
        {
            player.takeMonedes(-preu);
            PlayerPrefs.SetInt("equipament_"+idEquipament, 1);
            modificaUIEquipament(idEquipament);
        }
    }

    public void onEquipaEquipament(int idEquipament)
    {
        // Equipem
        player.canviaArma(idEquipament);
        actualitzaUIBotiga();
    }
}
