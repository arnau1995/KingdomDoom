using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerInfo : MonoBehaviour
{
    [SerializeField] private GameObject[] enemics;
    [SerializeField] private GameObject[] bosses;

    public GameObject getEnemic(int idEnemic)
    {
        return enemics[idEnemic];
    }

    public GameObject getBoss(int idBoss)
    {
        return bosses[idBoss];
    }

    public GameObject[] getEnemics()
    {
        return enemics;
    }

    public int numEnemics()
    {
        return enemics.Length;
    }

    public GameObject[] getBosses()
    {
        return bosses;
    }

    public int numBosses()
    {
        return bosses.Length;
    }

    public string getEnemicNom(int id)
    {
        return enemics[id].GetComponent<Enemic>().getNom();
    }
}
