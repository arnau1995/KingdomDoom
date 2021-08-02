using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtributsAtac
{
    /* 
    Tipus Arees
    - N: Normal                                 (necesita linia de visió)
    - I: Invariable                             (sense linia de visió)
    - L: En linia (en X)                        (necesita linia de visió)
    - X: En linia (en X) invaiable              (sense linia de visió)
    - R: Recte (només a un costat)              (necesita linia de visió)
    - I: Recte (només a un costat) invariable   (sense linia de visió)
    */
    private string descAtac = "";
    private int manaAtac = 3;
    private int damageAtac = 3;
    private int healAtac = 0;
    private int rangAtacMin = 1;
    private int rangAtacMax = 1;
    private char tipusAtac = 'N';
    private int areaAtacMin = 0;
    private int areaAtacMax = 0;
    private char tipusAreaAtac = 'N';
    private int escalatAtacNivell = 5;
    private int escalatCuraNivell = 5;


    private Dictionary <int, int> habilitatsEspecials;
    private Dictionary <int, int> habilitatsEspecialsSelf;
    /*
    Habilitats Especials
    - 1: Treure manà
    - 2: Treure passos
    - 3: Emputxar
    - 4: BoostBoss1
    - 5: BoostBoss2
    */
    private int escalatHabEspeNivell = 5;
    private int estatNecessari = 0;

    private GameObject efecte;
    private GameObject ui;

    public AtributsAtac ()
    {
        descAtac = "";
        manaAtac = 0;
        damageAtac = 0;
        healAtac = 0;
        rangAtacMin = 0;
        rangAtacMax = 0;
        tipusAtac = 'N';
        areaAtacMin = 0;
        areaAtacMax = 0;
        tipusAreaAtac = 'N';
        
        escalatAtacNivell = 0;
        escalatCuraNivell = 0;
        escalatHabEspeNivell = 0;
        habilitatsEspecials = new Dictionary <int, int>();
        habilitatsEspecialsSelf = new Dictionary <int, int>();
        estatNecessari = 0;
    }

    public AtributsAtac (string desc, int mana, int damage, int heal, int rangMin, int rangMax, char tipus, int areaMin, int areaMax, char tipusArea) // Pensat per els atacs i cures
    {
        descAtac = desc;
        manaAtac = mana;
        damageAtac = damage;
        healAtac = heal;
        rangAtacMin = rangMin;
        rangAtacMax = rangMax;
        tipusAtac = tipus;
        areaAtacMin = areaMin;
        areaAtacMax = areaMax;
        tipusAreaAtac = tipusArea;
        
        escalatAtacNivell = 5;
        escalatCuraNivell = 5;
        escalatHabEspeNivell = 0;
        habilitatsEspecials = new Dictionary <int, int>();
        habilitatsEspecialsSelf = new Dictionary <int, int>();
        estatNecessari = 0;
    }

    public AtributsAtac (string desc, int mana, int rangMin, int rangMax, char tipus, int areaMin, int areaMax, char tipusArea, Dictionary <int, int> habEspecials) // Pensat per les habilitats especials
    {
        descAtac = desc;
        manaAtac = mana;
        damageAtac = 0;
        healAtac = 0;
        rangAtacMin = rangMin;
        rangAtacMax = rangMax;
        tipusAtac = tipus;
        areaAtacMin = areaMin;
        areaAtacMax = areaMax;
        tipusAreaAtac = tipusArea;
        
        escalatAtacNivell = 0;
        escalatCuraNivell = 0;
        escalatHabEspeNivell = 5;
        habilitatsEspecials = habEspecials;
        habilitatsEspecialsSelf = new Dictionary <int, int>();
        estatNecessari = 0;
    }

    // GET ///////////////////////////////////////
    public string getDescripcio()
    {
        return descAtac;
    }
    public int getManaAtac()
    {
        return manaAtac;
    }

    public int getDamageAtac()
    {
        return damageAtac;
    }

    public int getDamageAtacNivell(int nivell)
    {
        int damageByLevel = (int) Mathf.Ceil( damageAtac * ((((float) escalatAtacNivell) / 100) * (nivell - 1)) );
        return damageByLevel;
    }

    public int getCuraAtac()
    {
        return healAtac;
    }

    public int getCuraAtacNivell(int nivell)
    {
        return (int) healAtac + (int) Mathf.Ceil(((healAtac * (escalatCuraNivell / 100)) * (nivell - 1)));
    }

    public int getRangAtacMin()
    {
        return rangAtacMin;
    }
    public int getRangAtacMax()
    {
        return rangAtacMax;
    }

    public char getTipusAtac()
    {
        return tipusAtac;
    }

    public char getAtacTipusSpec()
    {
        var tipusSpec = 'A';
        if (damageAtac > 0) tipusSpec = 'A';
        else if (healAtac > 0) tipusSpec = 'H';
        else 
        {
            tipusSpec = 'P';

            int[] habilitatsAgresives = new int[3] {1, 2, 3};
            foreach (int habA in habilitatsAgresives)
            {
                if (habilitatsEspecials.ContainsKey(habA)) 
                {
                    tipusSpec = 'A';
                }
            }

            if (tipusSpec == 'A' && habilitatsEspecialsSelf.Count > 0)
            {
                tipusSpec = 'P';
            }
        }

        return tipusSpec;
    }

    public int getAreaAtacMin()
    {
        return areaAtacMin;
    }
    public int getAreaAtacMax()
    {
        return areaAtacMax;
    }

    public char getTipusAreaAtac()
    {
        return tipusAreaAtac;
    }

    public Dictionary <int, int> getHabilitatsEspecials()
    {
        return habilitatsEspecials;
    }

    public int getHabilitatEspecial(int tipus)
    {
        if (habilitatsEspecials.ContainsKey(tipus)) return habilitatsEspecials[tipus];
        return 0;
    }

    public Dictionary <int, int> getHabilitatsEspecialsSelf()
    {
        return habilitatsEspecialsSelf;
    }

    public int getHabilitatEspecialSelf(int tipus)
    {
        if (habilitatsEspecialsSelf.ContainsKey(tipus)) return habilitatsEspecialsSelf[tipus];
        return 0;
    }

    public int getHabEspecialNivell(int tipus, int nivell)
    {
        return (int) getHabilitatEspecial(tipus) + (int) Mathf.Ceil(((getHabilitatEspecial(tipus) * (escalatHabEspeNivell / 100)) * (nivell - 1)));
    }

    public int getEstatNecessari()
    {
        return estatNecessari;
    }

    // SET ///////////////////////////////////////
    public void setEscalatAtacNivell(int escalat)
    {
        escalatAtacNivell = escalat;
    }

    public void setEscalatCuraNivell(int escalat)
    {
        escalatCuraNivell = escalat;
    }

    public void setHabilitatsEspecials(Dictionary <int, int> habEspecials)
    {
        habilitatsEspecials = habEspecials;
    }

    public void afegeixHabilitatEspecial(int tipus, int valor)
    {
        habilitatsEspecials.Add(tipus,valor);
    }

    public void setHabilitatsEspecialsSelf(Dictionary <int, int> habEspecials)
    {
        habilitatsEspecialsSelf = habEspecials;
    }

    public void afegeixHabilitatEspecialSelf(int tipus, int valor)
    {
        habilitatsEspecialsSelf.Add(tipus,valor);
    }

    public void setEstatNecessari(int estat)
    {
        estatNecessari = estat;
    }

    public void setEfecte(GameObject newEfecte)
    {
        efecte = newEfecte;
    }

    public GameObject getEfecte()
    {
        return efecte;
    }

    public void setUi(GameObject newUi)
    {
        ui = newUi;
    }

    public GameObject getUi()
    {
        return ui;
    }
}
