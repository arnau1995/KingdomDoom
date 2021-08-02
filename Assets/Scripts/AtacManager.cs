using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtacManager : MonoBehaviour
{
    private static AtacManager _instance;
    public static AtacManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("AtacManager is NULL");

            return _instance;
        }
    }

    private Dictionary <string, AtributsAtac> atacs;
    [SerializeField] private GameObject[] efectesAtacs;
    [SerializeField] private GameObject[] uiAtacs;

    void Awake()
    {
        _instance = this;
        
        atacs = new Dictionary<string, AtributsAtac>();
        
        // Inicialitzar atacs i cures
        // AtributsAtac (string desc, int mana, int damage, int heal, int rangMin, int rangMax, char tipus, int areaMin, int areaMax, char tipusArea)

        // Inicialitzar habilitats especials
        // AtributsAtac (string desc, int mana, int rangMin, int rangMax, char tipus, int areaMin, int areaMax, char tipusArea, Dictionary <char, int> habEspecials)

        // PLAYER ////////////////////////////////////////////////////////////////////////////////
        // Atacs a distancia
        atacs.Add("FletxaPoderosa", new AtributsAtac("Fletxa Poderosa", 5, 5, 0, 1, 4, 'N', 0, 0, 'N'));
        atacs["FletxaPoderosa"].setEfecte(efectesAtacs[0]);
        atacs["FletxaPoderosa"].setUi(uiAtacs[0]);
        atacs.Add("FletxaBuscadora", new AtributsAtac("Fletxa Buscadora", 4, 2, 0, 1, 6, 'I', 0, 0, 'N'));
        atacs["FletxaBuscadora"].setEfecte(efectesAtacs[1]);
        atacs["FletxaBuscadora"].setUi(uiAtacs[1]);
        atacs.Add("FletxaLineal", new AtributsAtac("Fletxa Lineal", 5, 6, 0, 1, 4, 'L', 0, 2, 'R'));
        atacs["FletxaLineal"].setEfecte(efectesAtacs[2]);
        atacs["FletxaLineal"].setUi(uiAtacs[2]);
        atacs.Add("FletxaExplosiva", new AtributsAtac("Fletxa Explosiva", 5, 4, 0, 1, 3, 'N', 0, 2, 'N'));
        atacs["FletxaExplosiva"].setEfecte(efectesAtacs[6]);
        atacs["FletxaExplosiva"].setUi(uiAtacs[3]);

        atacs.Add("EspasadaX", new AtributsAtac("Espasada X", 4, 4, 0, 1, 2, 'N', 0, 1, 'I'));
        atacs["EspasadaX"].setEfecte(efectesAtacs[2]);
        atacs["EspasadaX"].setUi(uiAtacs[8]);
        
        // Atacs cos a cos
        atacs.Add("Espasada", new AtributsAtac("Espasada", 3, 3, 0, 1, 1, 'N', 0, 0, 'N'));
        atacs["Espasada"].setEfecte(efectesAtacs[4]);
        atacs["Espasada"].setUi(uiAtacs[5]);

        atacs.Add("EspasadaPoderosa", new AtributsAtac("Espasada Poderosa", 5, 7, 0, 1, 1, 'N', 0, 0, 'N'));
        atacs["EspasadaPoderosa"].setEfecte(efectesAtacs[4]);
        atacs["EspasadaPoderosa"].setUi(uiAtacs[6]);

        atacs.Add("EspasadaEmputxadora", new AtributsAtac("Espasada Emputxadora", 4, 3, 0, 1, 1, 'N', 0, 0, 'N'));
        atacs["EspasadaEmputxadora"].setEfecte(efectesAtacs[4]);
        atacs["EspasadaEmputxadora"].setUi(uiAtacs[7]);
        Dictionary <int, int> habEspPoderosa = new Dictionary <int, int>();
        habEspPoderosa.Add(3, 1); // Emputxa 1 casella
        atacs["EspasadaEmputxadora"].setHabilitatsEspecials(habEspPoderosa);

        atacs.Add("Estocada", new AtributsAtac("Estocada", 4, 6, 0, 0, 2, 'L', 0, 0, 'N'));
        atacs["Estocada"].setEfecte(efectesAtacs[4]);
        atacs["Estocada"].setUi(uiAtacs[9]);

        atacs.Add("EspasadaContigua", new AtributsAtac("Espasada Contigua", 5, 4, 0, 0, 0, 'N', 1, 1, 'I'));
        atacs["EspasadaContigua"].setEfecte(efectesAtacs[4]);
        atacs["EspasadaContigua"].setUi(uiAtacs[10]);
        
        // Habilitats de curacio
        atacs.Add("Curacio", new AtributsAtac("Curacio", 4, 0, 3, 0, 3, 'N', 0, 0, 'N'));
        atacs["Curacio"].setEfecte(efectesAtacs[5]);
        atacs["Curacio"].setUi(uiAtacs[4]);

        // Habilitats especials
        Dictionary <int, int> habEspTreureMana = new Dictionary <int, int>();
        habEspTreureMana.Add(1, 1);
        atacs.Add("TreureMana", new AtributsAtac("Treure Mana", 2, 1, 3, 'N', 0, 0, 'N', habEspTreureMana));
        atacs["TreureMana"].setEfecte(efectesAtacs[7]);

        Dictionary <int, int> habEspTreurePassos = new Dictionary <int, int>();
        habEspTreurePassos.Add(2, 1);
        atacs.Add("TreurePassos", new AtributsAtac("Treure Passos", 2, 1, 3, 'N', 0, 0, 'N', habEspTreurePassos));
        atacs["TreurePassos"].setEfecte(efectesAtacs[8]);

        Dictionary <int, int> habEspEmputxar = new Dictionary <int, int>();
        habEspEmputxar.Add(3, 2);
        atacs.Add("Emputxar", new AtributsAtac("Emputxar", 2, 1, 3, 'L', 0, 0, 'N', habEspEmputxar));
        atacs["Emputxar"].setEfecte(efectesAtacs[9]);

        // ENEMICS ///////////////////////////////////////////////////////////////////////////////
        // Atacs a distancia

        // Atacs cos a cos
        atacs.Add("Mossegada", new AtributsAtac("Mossegada", 4, 3, 0, 1, 1, 'L', 0, 0, 'N'));
        atacs["Mossegada"].setEfecte(efectesAtacs[4]);
        atacs.Add("Picada", new AtributsAtac("Embestida", 3, 2, 0, 1, 1, 'L', 0, 0, 'N'));
        atacs["Picada"].setEfecte(efectesAtacs[4]);
        atacs.Add("Embestida", new AtributsAtac("Embestida", 5, 4, 0, 1, 1, 'L', 0, 0, 'N'));
        atacs["Embestida"].afegeixHabilitatEspecial(3, 2); // Embestida emputxa 2 caselles
        atacs["Embestida"].setEfecte(efectesAtacs[4]);
        atacs.Add("Cornada", new AtributsAtac("Cornada", 4, 3, 0, 1, 1, 'L', 0, 0, 'N'));
        atacs["Cornada"].setEfecte(efectesAtacs[4]);

        // Habilitats especials
        Dictionary <int, int> habEspReduccioMana = new Dictionary <int, int>();
        habEspReduccioMana.Add(1, 1);
        atacs.Add("ReduccioMana", new AtributsAtac("Reduccio Mana", 3, 1, 2, 'N', 0, 0, 'N', habEspReduccioMana));
        atacs["ReduccioMana"].setEfecte(efectesAtacs[7]);

        Dictionary <int, int> habEspReduccioMoviment = new Dictionary <int, int>();
        habEspReduccioMoviment.Add(2, 1);
        atacs.Add("ReduccioMoviment", new AtributsAtac("Reduccio Moviment", 3, 1, 2, 'N', 0, 0, 'N', habEspReduccioMoviment));
        atacs["ReduccioMoviment"].setEfecte(efectesAtacs[8]);

        // BOSSES ///////////////////////////////////////////////////////////////////////////////
        // Atacs a distancia

        // Atacs cos a cos
        atacs.Add("AtacBoss1", new AtributsAtac("Atac Boss 1", 5, 15, 0, 1, 1, 'N', 0, 0, 'N'));
        atacs["AtacBoss1"].setEstatNecessari(4); // Es necesita l'estat BoostBoss1
        atacs["AtacBoss1"].setEfecte(efectesAtacs[4]);

        atacs.Add("AtacBoss2", new AtributsAtac("Atac Boss 2", 5, 5, 0, 1, 1, 'N', 0, 0, 'N'));
        atacs["AtacBoss2"].setEfecte(efectesAtacs[4]);

        // Habilitats especials
        Dictionary <int, int> habEspBuffBoss1 = new Dictionary <int, int>();
        habEspBuffBoss1.Add(4, 2);
        atacs.Add("BoostBoss1", new AtributsAtac("Boost Boss 1", 3, 0, 0, 'N', 0, 0, 'N', habEspBuffBoss1));
        atacs["BoostBoss1"].setEfecte(efectesAtacs[3]);

        Dictionary <int, int> habEspBuffBoss2 = new Dictionary <int, int>();
        habEspBuffBoss2.Add(1, 99);
        habEspBuffBoss2.Add(2, 99);
        atacs.Add("BoostBoss2", new AtributsAtac("Boost Boss 2", 0, 1, 30, 'I', 0, 0, 'N', habEspBuffBoss2));
        Dictionary <int, int> habEspSelfBuffBoss2 = new Dictionary <int, int>();
        habEspSelfBuffBoss2.Add(5, 2);
        atacs["BoostBoss2"].setHabilitatsEspecialsSelf(habEspSelfBuffBoss2);
        atacs["BoostBoss2"].setEfecte(efectesAtacs[3]);
    }

    public List<Vector2> buscaRangAtacPersonatge(Vector2 posicio, string tipus)
    {
        return PartidaManager.Instance.buscaRangAtacPersonatge(posicio, atacs[tipus].getRangAtacMin(), atacs[tipus].getRangAtacMax(), atacs[tipus].getTipusAtac());
    }

    public List<Vector2> buscaRangAreaAtacPersonatge(Vector2 posicio, string tipus)
    {
        return PartidaManager.Instance.buscaRangAtacPersonatge(posicio, atacs[tipus].getAreaAtacMin(), atacs[tipus].getAreaAtacMax(), atacs[tipus].getTipusAreaAtac());
    }

    public List<GameObject> buscaPersonatgesAreaAtac(List<Vector2> posicionsArea)
    {
        return PartidaManager.Instance.buscaPersonatgesAreaAtac(posicionsArea);
    }

    public string getDescripcioAtac(string tipus)
    {
        return atacs[tipus].getDescripcio();
    }

    public int getManaAtac(string tipus)
    {
        return atacs[tipus].getManaAtac();
    }

    public int getHabilitatEspecial(string tipus, int hab)
    {
        return atacs[tipus].getHabilitatEspecial(hab);
    }

    public int getEstatNecessari(string tipus)
    {
        return atacs[tipus].getEstatNecessari();
    }

    public char getAtacTipusSpec(string tipus)
    {
        return atacs[tipus].getAtacTipusSpec();
    }

    public int getDamageAtac(string tipus, int nivell)
    {
        int damageBase = atacs[tipus].getDamageAtac();
        int damageNivell = atacs[tipus].getDamageAtacNivell(nivell);
        return damageBase + damageNivell;
    }

    public int getHealAtac(string tipus, int nivell)
    {
        int healBase = atacs[tipus].getCuraAtac();
        int healNivell = atacs[tipus].getCuraAtacNivell(nivell);
        return healBase + healNivell;
    }

    public int getRangAtacMin(string tipus)
    {
        return atacs[tipus].getRangAtacMin();
    }
    public int getRangAtacMax(string tipus)
    {
        return atacs[tipus].getRangAtacMax();
    }

    public int getAreaAtacMin(string tipus)
    {
        return atacs[tipus].getAreaAtacMin();
    }
    public int getAreaAtacMax(string tipus)
    {
        return atacs[tipus].getAreaAtacMax();
    }

    public char getTipusAtac(string tipus)
    {
        return atacs[tipus].getTipusAtac();
    }

    public char getTipusAreaAtac(string tipus)
    {
        return atacs[tipus].getTipusAreaAtac();
    }

    public Dictionary <int, int> getHabilitatsEspecials(string tipus)
    {
        return atacs[tipus].getHabilitatsEspecials();
    }

    public Dictionary <int, int> getHabilitatsEspecialsSelf(string tipus)
    {
        return atacs[tipus].getHabilitatsEspecialsSelf();
    }

    public GameObject getUiAtac(string tipus)
    {
        return atacs[tipus].getUi();
    }

    public void mostraEfecteAtac(string tipus, Vector3 posicio)
    {
        GameObject efecteAtac = atacs[tipus].getEfecte();
        if (efecteAtac != null) 
        {
            GameObject tmp_efecte = Instantiate(efecteAtac, posicio, Quaternion.identity, null);
        }
    }
}
