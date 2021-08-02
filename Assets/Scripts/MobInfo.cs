using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobInfo : MonoBehaviour
{
    private string descLastBoss = "Orc";
    [SerializeField] private int[] idEnemics;
    private AgentScript[] enemics;
    private float[] timerMovEnemics;
    private float[] maxTimerMovEnemics;
    private float minRandTimeMov = 20f; // 20s
    private float maxRandTimeMov = 60f; // 60s
    [SerializeField] private int minNivell = 1;
    [SerializeField] private int maxNivell = 5;
    [SerializeField] private int minEnemics = 1;
    [SerializeField] private int maxEnemics = 2;
    [SerializeField] private Vector2 cellZone;
    private Vector2 cellSpacing;
    [SerializeField] private float tempsRespawn = 20f; // 20s
    private float tempsToSpawn;
    private bool enemicsVius = false;
    private SpawnerInfo spawner;

    [SerializeField] private int[] idBosses;
    [SerializeField] private int minNivellBoss = 1;
    [SerializeField] private int maxNivellBoss = 5;
    //private AgentScript[] bosses;
    //private float[] timerMovBosses;
    //private float[] maxTimerMovBosses;

    // Start is called before the first frame update
    void Start()
    {
        enemics = new AgentScript[0];
        timerMovEnemics = new float[0];
        maxTimerMovEnemics = new float[0];
        
        tempsToSpawn = tempsRespawn - tempsRespawn / 4; // Deixem un petit temps al inici del joc per fer spawn als mobs
        cellSpacing = WorldManager.Instance.getCellSpacing();
        enemicsVius = false;
        spawner = FindParentWithName("Spawner").GetComponent<SpawnerInfo>();
    }

    private GameObject FindParentWithName(string nom)
    {
        Transform t = transform;
        while (t.parent != null)
        {
            if (t.parent.name == nom)
            {
                return t.parent.gameObject;
            }
            t = t.parent.transform;
        }
        return null; // No s'ha trobat cap parent amb nom
    }

    // Update is called once per frame
    void Update()
    {
        if (!enemicsVius && tempsToSpawn >= tempsRespawn)
        {
            crearMobEnemics();
        }
        else 
        {
            if (tempsToSpawn < tempsRespawn)
            {
                tempsToSpawn += Time.deltaTime;
            }

            // moure enemics
            for (int i = 0; i < timerMovEnemics.Length; i++)
            {
                timerMovEnemics[i] += Time.deltaTime;

                if (timerMovEnemics[i] >= maxTimerMovEnemics[i])
                {
                    Vector2 enemicPos = getRandomPosition();
                    enemicPos = WorldManager.Instance.roundPos(enemicPos);
                    enemics[i].setTarget(enemicPos);
                    timerMovEnemics[i] = 0;
                    maxTimerMovEnemics[i] = Random.Range(minRandTimeMov, maxRandTimeMov);
                }
            }
        }
    }

    private void crearMobEnemics()
    {
        List<Vector2> posOcupades = new List<Vector2>();

        int numEnemics = 0;
        if (idEnemics.Length > 0) numEnemics = Random.Range(minEnemics, maxEnemics+1);
        
        int lengthEnemics = numEnemics + idBosses.Length;
        enemics = new AgentScript[lengthEnemics];
        timerMovEnemics = new float[lengthEnemics];
        maxTimerMovEnemics = new float[lengthEnemics];

        int enemicActual = 0;
        if (idBosses.Length > 0)
        {
            for (int i = 0; i < idBosses.Length; i++)
            {
                int idBoss = idBosses[i];
                GameObject boss = spawner.getBoss(idBoss);
                Vector2 bossPos = getRandomPosition(posOcupades);
                bossPos = WorldManager.Instance.roundPos(bossPos);
                posOcupades.Add(bossPos);
                GameObject bossTmp = Instantiate(boss, new Vector3(bossPos.x, bossPos.y, transform.position.z), Quaternion.identity, transform);
                bossTmp.GetComponent<Enemic>().setNivell(Random.Range(minNivellBoss, maxNivellBoss+1));
                enemics[enemicActual] = bossTmp.GetComponent<AgentScript>();

                // crear timers de moviment
                timerMovEnemics[enemicActual] = 0;
                maxTimerMovEnemics[enemicActual] = Random.Range(minRandTimeMov, maxRandTimeMov);

                enemicActual++;
            }
        }

        if (idEnemics.Length > 0)
        {
            for (int i = 0; i < numEnemics; i++)
            {
                int idEnemic = idEnemics[Random.Range(0, idEnemics.Length)];
                GameObject enemic = spawner.getEnemic(idEnemic);
                Vector2 enemicPos = getRandomPosition(posOcupades);
                enemicPos = WorldManager.Instance.roundPos(enemicPos);
                posOcupades.Add(enemicPos);
                GameObject enemicTmp = Instantiate(enemic, new Vector3(enemicPos.x, enemicPos.y, transform.position.z), Quaternion.identity, transform);
                enemicTmp.GetComponent<Enemic>().setNivell(Random.Range(minNivell, maxNivell+1));
                enemics[enemicActual] = enemicTmp.GetComponent<AgentScript>();

                // crear timers de moviment
                timerMovEnemics[enemicActual] = 0;
                maxTimerMovEnemics[enemicActual] = Random.Range(minRandTimeMov, maxRandTimeMov);

                enemicActual++;
            }
        }

        enemicsVius = true;
    }

    private Vector2 getRandomPosition()
    {
        Vector2 newPos = new Vector2();
        do {
            Vector2 celes = new Vector2();
            do {
                celes.Set( (int) Random.Range( -cellZone.x, cellZone.x ), (int) Random.Range( -cellZone.y, cellZone.y ) );
            } while ( !(celes.x % 2 == 0 && celes.y % 2 == 0) && !(celes.x % 2 != 0 && celes.y % 2 != 0) );
            newPos.Set(transform.position.x + (celes.x * cellSpacing.x), transform.position.y + (celes.y * cellSpacing.y));
            newPos = WorldManager.Instance.roundPos(newPos);
        } while (!WorldManager.Instance.esPosicioValida(newPos));

        return newPos;
    }

    private Vector2 getRandomPosition(List<Vector2> posOcupades)
    {
        Vector2 newPos = new Vector2();
        do {
            Vector2 celes = new Vector2();
            do {
                celes.Set( (int) Random.Range( -cellZone.x, cellZone.x ), (int) Random.Range( -cellZone.y, cellZone.y ) );
            } while ( !(celes.x % 2 == 0 && celes.y % 2 == 0) && !(celes.x % 2 != 0 && celes.y % 2 != 0) );
            newPos.Set(transform.position.x + (celes.x * cellSpacing.x), transform.position.y + (celes.y * cellSpacing.y));
            newPos = WorldManager.Instance.roundPos(newPos);
        } while (!posOcupades.Contains(newPos) && !WorldManager.Instance.esPosicioValida(newPos));

        return newPos;
    }

    public int[] getMobIdEnemics()
    {
        return idEnemics;
    }
    public Enemic[] getMobEnemics()
    {
        // agafar tots els enemics
        List<Enemic> enemics = new List<Enemic>();
        foreach(Transform child in transform)
        {
            Enemic enemic = child.GetComponent<Enemic>();
            if (enemic) enemics.Add(enemic);
        }
        return enemics.ToArray();
    }


    public ulong getMobXp()
    {
        ulong xp = 0;
        foreach(Transform enemic in transform)
        {
            Enemic e = enemic.GetComponent<Enemic>();
            if (e != null) 
            {
                xp += e.getExperiencia();
            }
        }
        return xp;
    }

    public int getMobMonedes()
    {
        int monedes = 0;
        foreach(Transform enemic in transform)
        {
            Enemic e = enemic.GetComponent<Enemic>();
            if (e != null) 
            {
                monedes += e.getMonedes();
            }
        }
        return monedes;
    }

    public bool isFinishBoss()
    {
        foreach(Transform enemic in transform)
        {
            if (enemic.GetComponent<Enemic>().getNom() == descLastBoss) return true;
        }

        return false;
    }

    public void setEnabled(bool enable)
    {
        enabled = enable;
    }

    public void reiniciarEnemics()
    {
        foreach(Transform enemic in transform)
        {
            enemic.gameObject.SetActive(true);
            enemic.GetComponent<Enemic>().iniciaAtributs();
        }
    }

    public void eliminarEnemics()
    {
        foreach(Transform enemic in transform)
        {
            GameObject.Destroy(enemic.gameObject);
        }

        enemics = new AgentScript[0];
        timerMovEnemics = new float[0];
        maxTimerMovEnemics = new float[0];
        
        tempsToSpawn = 0;
        enemicsVius = false;
    }
}
