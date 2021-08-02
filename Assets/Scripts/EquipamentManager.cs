using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipamentManager : MonoBehaviour
{
    private static EquipamentManager _instance;
    public static EquipamentManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("EquipamentManager is NULL");

            return _instance;
        }
    }

    private List<AtributsEquipament> equipament;
    [SerializeField] private GameObject[] objectesEquipament;

    void Awake()
    {
        _instance = this;
        
        equipament = new List<AtributsEquipament>();
        // AtributsEquipament(char tip, int ata, int def, string[] atas, int obj)
        AtributsEquipament espasaInicial = new AtributsEquipament("Espasa Inicial", 'A', 5, 0, new string[] {"Espasada", "EspasadaPoderosa", "EspasadaEmputxadora", "EspasadaX", "Estocada"}, 0);
        equipament.Add(espasaInicial); // id = 0

        AtributsEquipament espasaLlarga = new AtributsEquipament("Espasa Llarga", 'A', 10, 0, new string[] {"Espasada", "EspasadaPoderosa", "EspasadaEmputxadora", "EspasadaX", "EspasadaContigua"}, 1);
        equipament.Add(espasaLlarga); // id = 1

        AtributsEquipament varaLlarga = new AtributsEquipament("Vara Llarga", 'A', 10, 0, new string[] {"FletxaPoderosa", "FletxaBuscadora", "FletxaLineal", "FletxaExplosiva", "Curacio"}, 2);
        equipament.Add(varaLlarga); // id = 1
    }

    public GameObject getObjecteEquipament(int id)
    {
        return objectesEquipament[equipament[id].getObjecte()];
    }

    public int getAtac(int id)
    {
        return equipament[id].getAtac();
    }

    public int getDefensa(int id)
    {
        return equipament[id].getDefensa();
    }

    public string[] getAtacs(int id)
    {
        return equipament[id].getAtacs();
    }

    public Dictionary <string, GameObject> getUiEquipament(int id)
    {
        Dictionary <string, GameObject> uiEquipament = new Dictionary<string, GameObject>();

        foreach (string atac in equipament[id].getAtacs())
        {
            uiEquipament[atac] = AtacManager.Instance.getUiAtac(atac);
        }

        return uiEquipament;
    }
}
