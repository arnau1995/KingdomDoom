using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveMissionsInfo
{
    public int missioActiva;
    public Missio[] missions;

    public SaveMissionsInfo (MissionsInfo missionsInfo)
    {
        missioActiva = missionsInfo.getMissioActiva();

        missions = new Missio[missionsInfo.maxMissions()];
        for (int i = 0; i < missions.Length; i++)
        {
            missions[i] = missionsInfo.getMissio(i);
        }
    }
}
