using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtributsEquipament
{
    private string descripcio;
    /* Tipus:
    A: Arma
    E: Escut
    */
    private char tipus;
    private int atac;
    private int defensa;
    private string[] atacs;
    private int objecte;

    public AtributsEquipament()
    {
        descripcio = "";
        tipus = 'A';
        atac = 0;
        defensa = 0;
        atacs = new string[0];
        objecte = -1;
    }

    public AtributsEquipament(string desc, char tip, int ata, int def, string[] atas, int obj)
    {
        descripcio = desc;
        tipus = tip;
        atac = ata;
        defensa = def;
        atacs = atas;
        objecte = obj;
    }

    public string getDescripcio()
    {
        return descripcio;
    }

    public int getAtac()
    {
        return atac;
    }
    public void setAtac(int ata)
    {
        atac = ata;
    }

    public int getDefensa()
    {
        return defensa;
    }
    public void setDefensa(int def)
    {
        defensa = def;
    }

    public string[] getAtacs()
    {
        return atacs;
    }
    public void setAtacs(string[] atas)
    {
        atacs = atas;
    }

    public int getObjecte()
    {
        return objecte;
    }
    public void setObjecte(int obj)
    {
        objecte = obj;
    }
}
