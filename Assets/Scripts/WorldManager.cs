using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WorldManager : MonoBehaviour
{
    private static WorldManager _instance;
    public static WorldManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("WorldManager is NULL");

            return _instance;
        }
    }
    
    // VARIABLES

    [SerializeField] private UIWorldManager uiWorldManager;
    [SerializeField] private SmoothFollowCamera worldCamera;
    [SerializeField] private Camera cameraWorldCamera;
    private Camera cameraUICamera;
    [SerializeField] private float cameraCombatFov = 8f;
    private float cameraWorldFov;
    [SerializeField] private Vector2 cellSpacing;
    [SerializeField] private AgentScript player;
    [SerializeField] private GameObject worldMoviment;
    private GameObject mobActiu;
    private Transform enemicHit;
    private MobInfo mobInfo;
    [SerializeField] private GameObject objMobInfo;
    private GameObject objMobInfo_tmp;
    [SerializeField] private float uiSpeed = 8f;
    private bool move = false;

    public AudioClip[] audioSources;
    
    void Awake()
    {
        _instance = this;

        mobActiu = null;
        enemicHit = null;
        mobInfo = null;
        objMobInfo_tmp = null;
        worldCamera.setTarget(player.transform);
        cameraUICamera = cameraWorldCamera.transform.Find("UI Camera").GetComponent<Camera>();
        cameraWorldFov = cameraWorldCamera.fieldOfView;

        // Iniciem la musica
        modifyAudioSource(0);
    }

    /*void Start()
    {
        Application.targetFrameRate = 60;
    }*/

    void Update()
    {
        Vector3 mousePos = mouseToWorldPointPersp(Input.mousePosition, 0);

        RaycastHit2D hit = objecteAPosicio(mousePos);
        if(hit.collider != null && hit.transform.tag == "Enemic" && hit.transform.parent.tag == "Mob")
        {
            enemicHit = hit.transform;
            MobInfo mobInfo_tmp = enemicHit.parent.GetComponent<MobInfo>();
            if (mobInfo_tmp != mobInfo) 
            {
                deleteMobInfo();
                mobInfo = mobInfo_tmp;
                showMobInfo(new Vector3(hit.transform.position.x, hit.transform.position.y, 0));
            }

            if (enemicHit != null)
            {
                objMobInfo_tmp.transform.position = Vector3.Lerp(objMobInfo_tmp.transform.position, enemicHit.position, uiSpeed * Time.deltaTime);
            }
        }
        else 
        {
            // Eliminar l'objecte creat
            deleteMobInfo();
        }

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if(Input.GetMouseButtonDown(0))
            {
                move = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                setPlayerTarget(mousePos);
                GameObject worldMovimentInstance = Instantiate(worldMoviment, WorldManager.Instance.positionToCellSpace(mousePos, player.getTag()), Quaternion.identity);
                move = false;
            }
        }
        else 
        {
            move = false;
        }

        if (move)
        {
            setPlayerTarget(mousePos);
        }
    }

    private void showMobInfo(Vector3 posicio)
    {
        objMobInfo_tmp = Instantiate(objMobInfo, posicio, Quaternion.identity);
        
        // Actualitzar text
        string textBoss = "";
        string textEnemics = "";
        string lvlEnemics = "";
        int numEnemics = 0;

        foreach (Enemic enemic in mobInfo.getMobEnemics())
        {
            if (enemic.isBoss()) textBoss += "★";
            textBoss += "\n";
            textEnemics += enemic.getNom()+"\n";
            lvlEnemics += "LVL."+enemic.getNivell().ToString()+"\n";
            numEnemics++;
        }

        if (textBoss != "") textBoss = textBoss.Substring(0, textBoss.Length - 1);
        if (textEnemics != "") textEnemics = textEnemics.Substring(0, textEnemics.Length - 1);
        if (lvlEnemics != "") lvlEnemics = lvlEnemics.Substring(0, lvlEnemics.Length - 1);

        objMobInfo_tmp.transform.Find("Text Boss").GetComponent<Text>().text = textBoss;
        objMobInfo_tmp.transform.Find("Text Noms").GetComponent<Text>().text = textEnemics;
        objMobInfo_tmp.transform.Find("Text Nivells").GetComponent<Text>().text = lvlEnemics;

        float tamanyReq = 60 * numEnemics + 10;
        RectTransform imatge_tmp = objMobInfo_tmp.transform.Find("Image").GetComponent<RectTransform>();
        imatge_tmp.sizeDelta = new Vector2(imatge_tmp.sizeDelta.x, tamanyReq);

        RectTransform borderImatge_tmp = objMobInfo_tmp.transform.Find("Border Image").GetComponent<RectTransform>();
        borderImatge_tmp.sizeDelta = new Vector2(borderImatge_tmp.sizeDelta.x, tamanyReq);
    }

    private void deleteMobInfo()
    {
        if (objMobInfo_tmp != null) Destroy(objMobInfo_tmp);
        enemicHit = null;
        mobInfo = null;
        objMobInfo_tmp = null;
    }

    private void modifyAudioSource(int audio)
    {
        // 0: Audio del món
        // 1: Audio de combat
        AudioSource aSource = cameraWorldCamera.GetComponent<AudioSource>();
        aSource.clip = audioSources[audio];
        aSource.Play();
    }

    public void pauseAudio(bool pause)
    {
        AudioListener.pause = pause;
    }

    public void setPlayerTarget(Vector2 postition)
    {
        player.setTarget(postition);
    }

    public void setAgentTarget(AgentScript agent, Vector2 postition)
    {
        agent.setTarget(postition);
    }

    public Vector2 getCellSpacing()
    {
        return cellSpacing;
    }

    public void startFight(GameObject mob) // Iniciem lluita
    {
        if (enabled)
        {
            // Guardem mob
            mobActiu = mob;
            mobActiu.GetComponent<MobInfo>().setEnabled(false);
            PartidaManager.Instance.setEnemics(mobActiu.GetComponent<MobInfo>().getMobEnemics());

            // Eliminem UI
            deleteMobInfo();

            // La camara delimita el combat
            activarCamaraCombat();
            cameraWorldCamera.fieldOfView = cameraUICamera.fieldOfView = cameraCombatFov;

            // Iniciem Lluita
            PartidaManager.Instance.startFight();

            // Desactivem l'update del món
            enabled = false;

            // Activem la animació inicial
            uiWorldManager.iniciaLluita();

            // Modifiquem la musica
            modifyAudioSource(1);
        }
    }

    public void finishFight() // Finalitzem Lluita
    {
        if(!enabled)
        {
            // Moure camera
            worldCamera.setTarget(player.transform);
            cameraWorldCamera.fieldOfView = cameraUICamera.fieldOfView = cameraWorldFov;

            MobInfo mobInfo = mobActiu.GetComponent<MobInfo>();
            if (!PartidaManager.Instance.playerIsAlive()) // Ha mort el player
            {
                // Deixem el mob viu
                mobInfo.reiniciarEnemics();

                Debug.Log("Player mort, tornar al poblat");
                PartidaManager.Instance.reviuPlayer();
            }
            else // Player ha matat tots els enemics
            {
                Player p = player.GetComponent<Player>();
                p.takeExperiencia(mobInfo.getMobXp());
                p.takeMonedes(mobInfo.getMobMonedes());

                if (mobInfo.isFinishBoss())
                {
                    bool gameFinished = PlayerPrefs.GetInt("gameFinished", 0) != 0;
                    if (!gameFinished)
                    {
                        // Mostrar menu de fi de joc
                        uiWorldManager.mostraMenuFinal();
                        PlayerPrefs.SetInt("gameFinished", 1);
                    }
                }

                transform.GetComponent<MissionsInfo>().actualitzaMissioActual(mobInfo.getMobIdEnemics());
                mobInfo.eliminarEnemics();

                // Activem l'update del món
                enabled = true;
            }
            mobInfo.setEnabled(true);
            mobActiu = null;

            // Modifiquem la musica
            modifyAudioSource(0);
        }
    }

    public void habilitaManager()
    {
        enabled = true;
    }

    public Vector2 positionToCellSpace(Vector3 posicio, string tag)
    {
        // Normalitzem la posició i la arrodonim
        Vector2 posicioCell = new Vector2(Mathf.Round(posicio.x/cellSpacing.x) * cellSpacing.x, Mathf.Round(posicio.y/cellSpacing.y) * cellSpacing.y);
        if (esPosicioValida(posicioCell, tag)) return posicioCell;
        else 
        {
            // Provem posicions en X
            posicioCell = new Vector2(Mathf.Ceil(posicio.x/cellSpacing.x) * cellSpacing.x, Mathf.Round(posicio.y/cellSpacing.y) * cellSpacing.y);
            if (esPosicioValida(posicioCell, tag)) return posicioCell;

            posicioCell = new Vector2(Mathf.Floor(posicio.x/cellSpacing.x) * cellSpacing.x, Mathf.Round(posicio.y/cellSpacing.y) * cellSpacing.y);
            if (esPosicioValida(posicioCell, tag)) return posicioCell;

            // Provem posicions en Y
            posicioCell = new Vector2(Mathf.Round(posicio.x/cellSpacing.x) * cellSpacing.x, Mathf.Ceil(posicio.y/cellSpacing.y) * cellSpacing.y);
            if (esPosicioValida(posicioCell, tag)) return posicioCell;

            posicioCell = new Vector2(Mathf.Round(posicio.x/cellSpacing.x) * cellSpacing.x, Mathf.Floor(posicio.y/cellSpacing.y) * cellSpacing.y);
            if (esPosicioValida(posicioCell, tag)) return posicioCell;
        }

        return posicioCell;
    }

    public bool esPosicioValida(Vector2 posicio, string tag)
    {
        RaycastHit2D hit = objecteAPosicio(posicio);
        if(hit.collider != null && hit.collider.tag != tag && !hit.collider.isTrigger) return false;
        return true;
    }

    public bool esPosicioValidaPersonatge(Vector2 posicio)
    {
        RaycastHit2D hit = objecteAPosicio(posicio);
        if(hit.collider != null && hit.collider.tag != "Enemic" && hit.collider.tag != "Player" && !hit.collider.isTrigger) return false;
        return true;
    }

    public bool esPosicioValida(Vector2 posicio)
    {
        RaycastHit2D hit = objecteAPosicio(posicio);
        if(hit.collider != null && !hit.collider.isTrigger) return false;
        return true;
    }

    private RaycastHit2D objecteAPosicio(Vector2 pos)
    {
        return Physics2D.Raycast(pos, Vector2.zero);
    }

    public Vector2 distanceCells(Vector2 posicio1, Vector2 posicio2)
    {
        Vector2 distancia = new Vector2();
        distancia.x = (int) Mathf.Round(( posicio1.x - posicio2.x ) / cellSpacing.x);
        distancia.y = (int) Mathf.Round(( posicio1.y - posicio2.y ) / cellSpacing.y);
        return distancia;
    }

    public Vector2 roundPos(Vector2 posicio)
    {
        return new Vector2(Mathf.Round(posicio.x * 100f) / 100f, Mathf.Round(posicio.y * 100f) / 100f);
    }

    public Vector3 roundPos(Vector3 posicio)
    {
        return new Vector3(Mathf.Round(posicio.x * 100f) / 100f, Mathf.Round(posicio.y * 100f) / 100f, Mathf.Round(posicio.z * 100f) / 100f);
    }

    public void activarCamaraCombat()
    {
        // Moure camera
        worldCamera.setTarget(mobActiu.transform);
    }

    public Vector2 worldCameraPosition()
    {
        return worldCamera.transform.position;
    }

    public bool pointInCamera(Vector2 posicio)
    {
        Vector3 viewPos = cameraWorldCamera.WorldToViewportPoint(posicio);
        if (viewPos.x > 1 || viewPos.x < 0 || viewPos.y > 1 || viewPos.y < 0) return false;
        return true;
    }

    public float getRotacioAccioPersonatge(Vector2 orientacio)
    {
        float rotacio = 0;

        // Ens assegurem que siguin valors enters
        int distX = (int) Mathf.Round(orientacio.x);
        int distY = (int) Mathf.Round(orientacio.y);
        
        if (distX == 0) 
        {
            if (distY < 0) rotacio = 0;                 // Up
            else rotacio = 180;                         // Down
        }
        else if (distY == 0)
        {
            if (distX > 0) rotacio = 270;               // Right
            else rotacio = 90;                          // Left
        }
        else if (distX < 0 && distY > 0) rotacio = 135; // Right Down
        else if (distX > 0 && distY < 0) rotacio = 315; // Left Up
        else if (distX > 0 && distY > 0) rotacio = 225; // Left Down
        else if (distX < 0 && distY < 0) rotacio = 45;  // Right Up

        return rotacio;
    }

    public Vector3 mouseToWorldPointHorto(Vector3 mousePos)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos.Set(mousePos.x, mousePos.y, mousePos.z - (pos.z * 2));
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    public Vector3 mouseToWorldPointPersp(Vector3 screenPosition, float z) 
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    public MissionsInfo getMissionsInfo() // MissionsInfo pot canviar segons la zona on es trobi el player del mapa
    {
        return transform.GetComponent<MissionsInfo>();
    }

    public void mostraAvisos(string avis)
    {
        uiWorldManager.mostraAvisos(avis);
    }
}
