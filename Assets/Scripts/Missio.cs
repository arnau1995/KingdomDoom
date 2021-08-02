using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Missio
{
    private int[] enemics_needed;
    private int[] enemics_actuals;
    private int monedes;
    private int monedesXEnemic = 6;
    private int experiencia;
    private int experienciaXEnemic = 20;


    public Missio(int numEnemics, int min_enemics, int max_enemics)
    {
        // crear una missio aleatoria
        enemics_needed = new int[numEnemics];
        enemics_actuals = new int[numEnemics];
        monedes = 0;
        experiencia = 0;

        // Seleccionem un numero aleatori entre min i max
        int totalEnemics = Random.Range(min_enemics, max_enemics+1);
        // mentres la suma de enemics_needed sigui menor al numero aleatori creat
        int total_enemics_actual = total_enemics_needed();
        while (total_enemics_actual < totalEnemics)
        {
            //  guardem una id (la id és la pos a l'array) de enemics i enemics_needed entre 1 i el nombre aleatori
            int randNumEnemics = Random.Range(1, (totalEnemics-total_enemics_actual)+1);
            enemics_needed[Random.Range(0, numEnemics)] += randNumEnemics;

            int mitjanaMonedes = randNumEnemics * monedesXEnemic;
            monedes += Random.Range( (int) (mitjanaMonedes * .7f), (int) ((mitjanaMonedes * 1.2f)+1) );

            int mitjanaExperiencia = randNumEnemics * experienciaXEnemic;
            experiencia += Random.Range( (int) (mitjanaExperiencia * .7f), (int) ((mitjanaExperiencia * 1.2f)+1) );

            total_enemics_actual = total_enemics_needed();
        }
    }

    public bool esMissioCompletada()
    {
        bool completa = true;
        int i = 0;
        while (completa && i < enemics_needed.Length)
        {
            if (enemics_actuals[i] < enemics_needed[i]) completa = false;
            i++;
        }
        return completa;
    }

    public int total_enemics_needed()
    {
        int total = 0;
        if (enemics_needed.Length > 0) 
        {
            foreach (int needed in enemics_needed) total += needed;
        }
        return total;
    }

    public int getTotalEnemicsId()
    {
        return enemics_needed.Length;
    }

    public int getTotalEnemicsNeeded()
    {
        int totalEnemicsNeeded = 0;
        for (int i = 0; i < enemics_needed.Length; i++)
        {
            totalEnemicsNeeded += enemics_needed[i];
        }
        return totalEnemicsNeeded;
    }

    public int getEnemicsNeeded(int id_enemic)
    {
        return enemics_needed[id_enemic];
    }

    public int getEnemicsActuals(int id_enemic)
    {
        return enemics_actuals[id_enemic];
    }

    public int getMonedes()
    {
        return monedes;
    }

    public int getExperiencia()
    {
        return experiencia;
    }

    public void takeEnemicId(int id_enemic)
    {
        if (enemics_actuals[id_enemic] < enemics_needed[id_enemic]) 
        {
            enemics_actuals[id_enemic] ++;
        }
    }
}
