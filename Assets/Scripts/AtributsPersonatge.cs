using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AtributsPersonatge
{
    private int nivell;
    private ulong experiencia;
    private ulong experienciaActual;

    private int vidaInicial = 10;
    private int vidaTotal = 10;
    private int manaTotal = 5;
    private int movimentsTotal = 3;

    private int vidaActual = 10;
    private int manaActual = 5;
    private int movimentsActual = 3;

    private Dictionary<int, int> estats;
    // id: tipus
     // 1: Stunt
     // 2: BoostBoss1
     // 3: BoostBoss2
     // 4: 

    public AtributsPersonatge (int lvl, ulong xp, int vid, int man, int mov)
    {
        nivell = lvl;
        experiencia = xp;
        experienciaActual = 0;
        vidaInicial = vid;
        calculaVidaTotal();
        manaTotal = man;
        movimentsTotal = mov;

        estats = new Dictionary<int, int>();

        iniciaAtributs();
    }

    public void iniciaAtributs()
    {
        vidaActual = vidaTotal;
        manaActual = manaTotal;
        movimentsActual = movimentsTotal;
    }

    public void reiniciaAtributsLluita()
    {
        manaActual = manaTotal;
        movimentsActual = movimentsTotal;
    }

    public void reiniciaAtributsTorn()
    {
        manaActual = manaTotal;
        movimentsActual = movimentsTotal;

        // disminueix els torns dels estats
        List<int> keys = estats.Keys.ToList();
        foreach (int estat in keys)
        {
            estats[estat] -= 1;
            if (estats[estat] <= 0)
            {
                estats.Remove(estat);
            }
        }
    }

    private void calculaVidaTotal()
    {
        vidaTotal = vidaInicial + (int) Mathf.Ceil( (vidaInicial * 0.1f) * (nivell - 1)); // Vida augmenta un 10% per nivell
    }

    public void setVidaActual(int vid)
    {
        vidaActual = vid;
    }

    public void setEstat(int idEstat, int tornsEstat)
    {
        if (!estats.ContainsKey(idEstat))
        {
            estats.Add(idEstat, tornsEstat);
        }
        else if (estats[idEstat] < tornsEstat)
        {
            estats[idEstat] = tornsEstat;
        }
    }

    public int getEstat(int idEstat)
    {
        if (estats.ContainsKey(idEstat)) return estats[idEstat];
        return 0;
    }

    public int getNivell()
    {
        return nivell;
    }

    public ulong getExperiencia()
    {
        return experiencia * (ulong) nivell;
    }
    public ulong getExperienciaNivell()
    {
        ulong xpLvl = (ulong) Mathf.Pow(nivell*2, 2f);
        return xpLvl * experiencia;
    }
    public ulong getExperienciaActual()
    {
        return experienciaActual;
    }

    public void takeExperiencia(ulong experienciaGuanyada)
    {
        experienciaActual += experienciaGuanyada;

        if (experienciaActual >= getExperienciaNivell())
        {
            experienciaActual -= getExperienciaNivell();
            nivell++;
            
            calculaVidaTotal();
            vidaActual = vidaActual + (int) Mathf.Ceil(vidaActual * (nivell / 10));
        }
    }

    public int getVida()
    {
        return vidaTotal;
    }

    public int getVidaActual()
    {
        return vidaActual;
    }

    public int takeDamage(int damage)
    {
        int damaged = damage;
        vidaActual -= damage;
        if (vidaActual < 0) 
        {
            damaged = Mathf.Abs(vidaActual + damage);
            vidaActual = 0;
        }

        return damaged;
    }

    public int takeHeal(int heal)
    {
        int healed = heal;
        vidaActual += heal;
        if (vidaActual > vidaTotal) 
        {
            healed = Mathf.Abs(vidaActual - vidaTotal - heal);
            vidaActual = vidaTotal;
        }

        return healed;
    }

    public int getMoviments()
    {
        return movimentsTotal;
    }

    public int getMovimentsActual()
    {
        return movimentsActual;
    }

    public void aplicaMoviment(int desplacament)
    {
        movimentsActual -= desplacament;
    }

    public void aplicaMana(int mana)
    {
        manaActual -= mana;
    }

    public int getManaActual()
    {
        return manaActual;
    }
}
