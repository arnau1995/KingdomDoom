using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PartidaManager : MonoBehaviour
{
    private static PartidaManager _instance;
    public static PartidaManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("PartidaManager is NULL");

            return _instance;
        }
    }

    //public Camera camaraEscena;
    private Vector2 cellSpacing;
    [SerializeField] private Vector2 cellZoneIniciLluita;
    [SerializeField] private Player player;
    private Enemic[] enemics;

    [SerializeField] private float tornMaxTemps;
    [SerializeField] private float minTempsPosicionament = 2.5f;
    private int torn;
    private float tornTemps;

    private GameObject objUserInteraction;
    [SerializeField] private UIPartidaManager uiManager;

    [SerializeField] private GameObject[] uiWorldObjects;

    private Enemic enemicInfo;
    [SerializeField] private GameObject objEnemicInfo;
    private GameObject objEnemicInfo_tmp;
    [SerializeField] private float uiSpeed = 8f;

    // Variables privades per augmentar rendiment dels calculs
    Dictionary<Vector2, List<Vector2>> movimentsValids = new Dictionary<Vector2, List<Vector2>>();

    void Awake()
    {
        _instance = this;

        enabled = false;
    }

    void Start()
    {
        objEnemicInfo_tmp = null;
        cellSpacing = WorldManager.Instance.getCellSpacing();
    }

    public void startFight()
    {
        torn = -2;
        tornTemps = -1;
    
        objUserInteraction = null;
        enabled = true;

        StartCoroutine(LateStart(3));
    }

    public void endFight()
    {
        if (torn == -1) // Torn del player
        {
            player.finalitzaTorn();
        }
        else if (torn >= 0) // Torn dels enemics
        {
            enemics[torn].finalitzaTorn();
        }
        torn = -2;
        tornTemps = -1;
        StartCoroutine(LateFinish(4));
    }

    public void reviuPlayer()
    {
        StartCoroutine(LateReviuPlayer(2));
    }

    IEnumerator LateReviuPlayer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        player.reviuPlayer();
        WorldManager.Instance.habilitaManager();
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        uiManager.gameObject.SetActive(true);
        uiManager.mostrarBotoStart();

        player.iniciaLluita();
        foreach(Enemic enemic in enemics)
        {
            enemic.iniciaLluita();
        }
        tornTemps = 0;

        // Torn 0 per colocar els personatges
        int zona = Random.Range(0, 4);
        Vector2 posLluita = WorldManager.Instance.worldCameraPosition();

        Vector2 newPlayerPos;
        List<Vector2> posOcupades = new List<Vector2>();
        switch (zona)
        {
            case 0: // Partim mapa en X
                newPlayerPos = getRandomPosition(0, (int) cellZoneIniciLluita.x, (int) (-cellZoneIniciLluita.y), (int) cellZoneIniciLluita.y, posLluita, posOcupades);
                WorldManager.Instance.setPlayerTarget(newPlayerPos);
                posOcupades.Add(newPlayerPos);
                
                foreach(Enemic enemic in enemics)
                {
                    Vector2 newEnemicPos = getRandomPosition((int) (-cellZoneIniciLluita.x), 0, (int) (-cellZoneIniciLluita.y), (int) cellZoneIniciLluita.y, posLluita, posOcupades);
                    WorldManager.Instance.setAgentTarget(enemic.gameObject.GetComponent<AgentScript>(), newEnemicPos);
                    posOcupades.Add(newEnemicPos);
                }
            break;
            case 1: // Partim mapa en -X
                newPlayerPos = getRandomPosition((int) (-cellZoneIniciLluita.x), 0, (int) (-cellZoneIniciLluita.y), (int) cellZoneIniciLluita.y, posLluita, posOcupades);
                WorldManager.Instance.setPlayerTarget(newPlayerPos);
                posOcupades.Add(newPlayerPos);
                
                foreach(Enemic enemic in enemics)
                {
                    Vector2 newEnemicPos = getRandomPosition((int) cellZoneIniciLluita.x, 0, (int) (-cellZoneIniciLluita.y), (int) cellZoneIniciLluita.y, posLluita, posOcupades);
                    WorldManager.Instance.setAgentTarget(enemic.gameObject.GetComponent<AgentScript>(), newEnemicPos);
                    posOcupades.Add(newEnemicPos);
                }
            break;
            case 2: // Partim mapa en Y
                newPlayerPos = getRandomPosition((int) (-cellZoneIniciLluita.x), (int) cellZoneIniciLluita.x, 0, (int) cellZoneIniciLluita.y, posLluita, posOcupades);
                WorldManager.Instance.setPlayerTarget(newPlayerPos);
                posOcupades.Add(newPlayerPos);
                
                foreach(Enemic enemic in enemics)
                {
                    Vector2 newEnemicPos = getRandomPosition((int) (-cellZoneIniciLluita.x), (int) cellZoneIniciLluita.x, (int) (-cellZoneIniciLluita.y), 0, posLluita, posOcupades);
                    WorldManager.Instance.setAgentTarget(enemic.gameObject.GetComponent<AgentScript>(), newEnemicPos);
                    posOcupades.Add(newEnemicPos);
                }
            break;
            case 3: // Partim mapa en -Y
                newPlayerPos = getRandomPosition((int) (-cellZoneIniciLluita.x), (int) cellZoneIniciLluita.x, (int) (-cellZoneIniciLluita.y), 0, posLluita, posOcupades);
                WorldManager.Instance.setPlayerTarget(newPlayerPos);
                posOcupades.Add(newPlayerPos);
                
                foreach(Enemic enemic in enemics)
                {
                    Vector2 newEnemicPos = getRandomPosition((int) (-cellZoneIniciLluita.x), (int) cellZoneIniciLluita.x, 0, (int) cellZoneIniciLluita.y, posLluita, posOcupades);
                    WorldManager.Instance.setAgentTarget(enemic.gameObject.GetComponent<AgentScript>(), newEnemicPos);
                    posOcupades.Add(newEnemicPos);
                }
            break;
        }
    }

    IEnumerator LateFinish(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        enabled = false;
        deleteEnemicInfo();
        uiManager.gameObject.SetActive(false);
        player.finalitzaLluita();

        WorldManager.Instance.finishFight();
    }

    private Vector2 getRandomPosition(int minX, int maxX, int minY, int maxY, Vector2 posLluita, List<Vector2> posOcupades)
    {
        Vector2 newPos = new Vector2();
        do {
            Vector2 celes = new Vector2();
            do {
                celes.Set( (int) Random.Range( minX, maxX ), (int) Random.Range( minY, maxY ) );
            } while ( !(celes.x % 2 == 0 && celes.y % 2 == 0) && !(celes.x % 2 != 0 && celes.y % 2 != 0) );
            newPos.Set(posLluita.x + (celes.x * cellSpacing.x), posLluita.y + (celes.y * cellSpacing.y));
            newPos = WorldManager.Instance.roundPos(newPos);
        } while (!esPosicioValida(newPos) || posOcupades.Contains(newPos));

        return newPos;
    }

    public void setEnemics(Enemic[] newEnemics)
    {
        enemics = newEnemics;
    }

    void Update()
    {
        Vector3 mousePos = WorldManager.Instance.mouseToWorldPointPersp(Input.mousePosition, 0);
        RaycastHit2D hitEnemic = objecteAPosicioAmbTag(mousePos, "Enemic", false);
        if(hitEnemic.collider != null && !hitEnemic.collider.isTrigger && hitEnemic.transform.tag == "Enemic")
        {
            Enemic enemicInfo_tmp = hitEnemic.transform.GetComponent<Enemic>();
            if (enemicInfo_tmp != enemicInfo) 
            {
                deleteEnemicInfo();
                enemicInfo = enemicInfo_tmp;
                showEnemicInfo();
            }

            if (enemicInfo != null)
            {
                objEnemicInfo_tmp.transform.position = Vector3.Lerp(objEnemicInfo_tmp.transform.position, enemicInfo.transform.position, uiSpeed * Time.deltaTime);
            }
        }
        else 
        {
            // Eliminar l'objecte creat
            deleteEnemicInfo();
        }

        if (tornTemps != -1)
        {
            // GESTIO TORN //
            tornTemps += Time.deltaTime;

            uiManager.actualitzaTemps(tornTemps, tornMaxTemps);

            if (tornTemps >= tornMaxTemps)
            {
                tornTemps = 0;

                if (torn == -1) // Torn del player
                {
                    player.finalitzaTorn();
                }
                else if (torn >= 0) // Torn dels enemics
                {
                    enemics[torn].finalitzaTorn();
                }
                else if (torn == -2)
                {
                    uiManager.amagaStart();
                }

                do
                {
                    torn++;
                    if (torn >= enemics.Length) torn = -1;
                } while (torn != -1 && !enemics[torn].isAlive());

                if (torn == -1) 
                {
                    if (!player.isAlive()) endFight();
                    else player.iniciaTorn();
                }
                else 
                {
                    enemics[torn].iniciaTorn();
                }
            }
            // FI GESTIO TORN //
            
            // Interacció amb les caselles mostrades al player
            //Vector3 mousePos = WorldManager.Instance.mouseToWorldPointPersp(Input.mousePosition, 0);
            RaycastHit2D hit = objecteAPosicioAmbTag(mousePos, "User Interaction");
            if (objUserInteraction != null && (hit.collider == null || hit.collider.gameObject != objUserInteraction || EventSystem.current.IsPointerOverGameObject()))
            {
                //objUserInteraction.GetComponent<SpriteRenderer>().color = objUserInteraction.GetComponent<UserInteractable>().colorDefecte;
                
                if (player.accioEsValida())
                {
                    if (player.accioEsMoviment())
                    {
                        // posar per defecte totes les celes
                        //List<Vector2> camiTo = buscaCamiPersonatge(player.getPosition(), new Vector2(objUserInteraction.transform.position.x, objUserInteraction.transform.position.y), player.getMovimentsActual());
                        List<Vector2> camiTo = player.getCamiTo(new Vector2(objUserInteraction.transform.position.x, objUserInteraction.transform.position.y));

                        foreach(Vector2 posValides in camiTo)
                        {
                            RaycastHit2D obj = objecteAPosicioAmbTag(posValides, "User Interaction");
                            if (obj.collider != null && obj.transform.GetComponent<SpriteRenderer>())
                            {
                                obj.transform.GetComponent<SpriteRenderer>().color = obj.transform.GetComponent<UserInteractable>().colorDefecte;
                            }
                        }
                    }
                    else
                    {
                        List<Vector2> posicionsValides = new List<Vector2>();
                        // agafar les posicions de l'atac amb el rang i tipus de l'atac
                        if (player.getTipusAreaAtacActual() != 'R' && player.getTipusAreaAtacActual() != 'L')
                        {
                            posicionsValides = buscaRangAtacPersonatge(new Vector2(objUserInteraction.transform.position.x, objUserInteraction.transform.position.y), player.getAreaAtacMinActual(), player.getAreaAtacMaxActual(), player.getTipusAreaAtacActual());
                        }
                        else
                        {
                            posicionsValides = buscaRangAtacPersonatge(new Vector2(objUserInteraction.transform.position.x, objUserInteraction.transform.position.y), player.getPosition(), player.getAreaAtacMinActual(), player.getAreaAtacMaxActual(), player.getTipusAreaAtacActual());
                        }

                        // per cada posicio:
                        foreach(Vector2 posValides in posicionsValides)
                        {
                            RaycastHit2D obj = objecteAPosicioAmbTag(posValides, "User Interaction");
                            if (obj.collider != null && obj.transform.GetComponent<SpriteRenderer>())
                            {
                                obj.transform.GetComponent<SpriteRenderer>().color = obj.transform.GetComponent<UserInteractable>().colorDefecte;
                            }
                        }

                        // Eliminar els tmp creats
                        player.eliminaCelesAtacTmp();
                    }
                }

                objUserInteraction = null;
            }

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if(hit.collider != null && hit.collider.tag == "User Interaction" && objUserInteraction == null)
                {
                    objUserInteraction = hit.collider.gameObject;
                    //objUserInteraction.GetComponent<SpriteRenderer>().color = objUserInteraction.GetComponent<UserInteractable>().colorMouseOn;
                    
                    if (player.accioEsMoviment())
                    {
                        // buscar cami i aplicar color a celes
                        //List<Vector2> camiTo = buscaCamiPersonatge(player.getPosition(), new Vector2(hit.transform.position.x, hit.transform.position.y), player.getMovimentsActual());
                        List<Vector2> camiTo = player.getCamiTo(new Vector2(hit.transform.position.x, hit.transform.position.y));

                        foreach(Vector2 posValides in camiTo)
                        {
                            RaycastHit2D obj = objecteAPosicioAmbTag(posValides, "User Interaction");
                            if (obj.collider != null && obj.transform.GetComponent<SpriteRenderer>())
                            {
                                obj.transform.GetComponent<SpriteRenderer>().color = obj.transform.GetComponent<UserInteractable>().colorMouseOn;
                            }
                        }
                    }
                    else
                    {
                        List<Vector2> posicionsValides = new List<Vector2> ();
                        // agafar les posicions de l'atac amb el rang i tipus de l'atac
                        if (player.getTipusAreaAtacActual() != 'R' && player.getTipusAreaAtacActual() != 'L')
                        {
                            posicionsValides = buscaRangAtacPersonatge(new Vector2(hit.transform.position.x, hit.transform.position.y), player.getAreaAtacMinActual(), player.getAreaAtacMaxActual(), player.getTipusAreaAtacActual());
                        }
                        else
                        {
                            posicionsValides = buscaRangAtacPersonatge(new Vector2(hit.transform.position.x, hit.transform.position.y), player.getPosition(), player.getAreaAtacMinActual(), player.getAreaAtacMaxActual(), player.getTipusAreaAtacActual());
                        }
                        
                        // per cada posicio:
                        foreach(Vector2 posValides in posicionsValides)
                        {
                            RaycastHit2D obj = objecteAPosicioAmbTag(posValides, "User Interaction");
                            if (obj.collider != null && obj.transform.GetComponent<SpriteRenderer>())
                            {
                                obj.transform.GetComponent<SpriteRenderer>().color = obj.transform.GetComponent<UserInteractable>().colorMouseOn;
                            }
                            else // No hi ha cel·la, es crea un tmp 
                            {
                                GameObject tmp_obj = player.afegirCelaAtacTmp(posValides);
                                tmp_obj.transform.GetComponent<SpriteRenderer>().color = tmp_obj.transform.GetComponent<UserInteractable>().colorMouseOn;
                            }
                        }
                    }
                }

                // Click objecte
                if (torn != -2 && player.tornActiu() && Input.GetMouseButtonDown(0) && objUserInteraction != null) 
                {
                    //objUserInteraction.GetComponent<SpriteRenderer>().color = objUserInteraction.GetComponent<UserInteractable>().colorMouseClick;
                    Vector3 posAccio = new Vector3();

                    if (player.accioEsMoviment())
                    {
                        posAccio = objUserInteraction.transform.position;
                        player.accioUsuari(objUserInteraction.transform.position);
                    }
                    else
                    {
                        // Aplicar rotacio al personatge
                        player.setRotation(WorldManager.Instance.distanceCells(player.getPosition(), objUserInteraction.transform.position));

                        List<Vector2> posicionsValides = new List<Vector2> ();
                        // agafar les posicions de l'atac amb el rang i tipus de l'atac
                        if (player.getTipusAreaAtacActual() != 'R' && player.getTipusAreaAtacActual() != 'L')
                        {
                            posicionsValides = buscaRangAtacPersonatge(new Vector2(hit.transform.position.x, hit.transform.position.y), player.getAreaAtacMinActual(), player.getAreaAtacMaxActual(), player.getTipusAreaAtacActual());
                        }
                        else
                        {
                            posicionsValides = buscaRangAtacPersonatge(new Vector2(hit.transform.position.x, hit.transform.position.y), player.getPosition(), player.getAreaAtacMinActual(), player.getAreaAtacMaxActual(), player.getTipusAreaAtacActual());
                        }

                        player.aplicaManaAtac();

                        // per cada posicio:
                        foreach(Vector2 posValides in posicionsValides)
                        {
                            player.accioUsuari(new Vector2(posValides.x, posValides.y));
                        }
                        checkFinish();

                        posAccio = hit.transform.position;
                    }

                    player.mostraEfecteAccio(posAccio);

                    // Eliminar els tmp creats
                    player.eliminaCelesAtacTmp();

                    // Reiniciem les accions
                    player.reiniciaAccions();
                }
            }
        }
    }

    private void showEnemicInfo()
    {
        Vector3 pos = new Vector3(enemicInfo.transform.parent.position.x + Mathf.Pow(enemicInfo.transform.localPosition.x,2), enemicInfo.transform.parent.position.y + Mathf.Pow(enemicInfo.transform.localPosition.y,2), 0);
        objEnemicInfo_tmp = Instantiate(objEnemicInfo, pos, Quaternion.identity);
        objEnemicInfo_tmp.transform.position = enemicInfo.transform.position;

        // Actualitzar text
        string textBoss = "";
        if (enemicInfo.isBoss()) textBoss += "★";
        
        objEnemicInfo_tmp.transform.Find("Text Boss").GetComponent<Text>().text = textBoss;
        objEnemicInfo_tmp.transform.Find("Text Nom").GetComponent<Text>().text = enemicInfo.getNom();
        objEnemicInfo_tmp.transform.Find("Text Nivell").GetComponent<Text>().text = "LVL."+enemicInfo.getNivell().ToString();

        objEnemicInfo_tmp.transform.Find("Text Vida").GetComponent<Text>().text = enemicInfo.getVidaActual().ToString()+"/"+enemicInfo.getVidaTotal().ToString();
        objEnemicInfo_tmp.transform.Find("Text Mana").GetComponent<Text>().text = enemicInfo.getManaActual().ToString();
        objEnemicInfo_tmp.transform.Find("Text Moviments").GetComponent<Text>().text = enemicInfo.getMovimentsActual().ToString();
    }

    private void deleteEnemicInfo()
    {
        if (objEnemicInfo_tmp != null) Destroy(objEnemicInfo_tmp);
        enemicInfo = null;
        objEnemicInfo_tmp = null;
    }

    public void finalitzarTornActual()
    {
        bool finalitzarTornActual = true;

        if (torn == -1) 
        {
            finalitzarTornActual = !player.getEnMoviment();
        }
        else if (torn >= 0) 
        {
            finalitzarTornActual = !enemics[torn].getEnMoviment();
        }

        if (finalitzarTornActual) tornTemps = tornMaxTemps;
    }

    public void finalitzarPosicionament()
    {
        if (torn == -2 && tornTemps >= minTempsPosicionament)
        {
            finalitzarTornActual();
            uiManager.amagaStart();
        }
    }

    public bool tornActualPlayer()
    {
        return torn == -1;
    }

    public Dictionary<Vector2, List<Vector2>> buscaMovimentsPersonatge(Vector2 posicio, int moviments)
    {
        movimentsValids = new Dictionary<Vector2, List<Vector2>>(); // reiniciem viariable
        comprovaPosicionsMoviment(new List<Vector2>(), posicio, 0, moviments); // retornem totes les posicions possibles
        return movimentsValids;
    }

    private void comprovaPosicionsMoviment(List<Vector2> posicioPath, Vector2 posicio, int movimentsActuals, int maxMoviments)
    {
        posicio = WorldManager.Instance.roundPos(posicio); // Arrodonim a 2 decimals
        
        if (esPosicioValida(posicio) || movimentsActuals == 0) // Si casella correcte o és la primera
        {
            List<Vector2> posicioPath_tmp = new List<Vector2>(posicioPath);
            bool continuar = false;
            
            if (movimentsActuals == 0) continuar = true;
            else if (!movimentsValids.ContainsKey(posicio)) // Afegim casella (la primera no s'agafa)
            {
                posicioPath_tmp.Add(posicio);
                movimentsValids.Add(posicio, posicioPath_tmp);
                continuar = true;
            }
            else if (posicioPath_tmp.Count+1 < movimentsValids[posicio].Count)
            {
                posicioPath_tmp.Add(posicio);
                movimentsValids[posicio] = new List<Vector2>(posicioPath_tmp);
                continuar = true;
            }

            if (continuar)
            {
                movimentsActuals++;
                if (movimentsActuals <= maxMoviments)
                {
                    //Vector2 posTopRight = new Vector2(posicio.x + cellSpacing.x, posicio.y + cellSpacing.y);
                    Vector2 posTopRight = new Vector2(posicio.x - cellSpacing.x, posicio.y);
                    comprovaPosicionsMoviment(posicioPath_tmp, posTopRight, movimentsActuals, maxMoviments);

                    //Vector2 posBotRight = new Vector2(posicio.x + cellSpacing.x, posicio.y - cellSpacing.y);
                    Vector2 posBotRight = new Vector2(posicio.x + cellSpacing.x, posicio.y);
                    comprovaPosicionsMoviment(posicioPath_tmp, posBotRight, movimentsActuals, maxMoviments);

                    //Vector2 posBotLeft = new Vector2(posicio.x - cellSpacing.x, posicio.y - cellSpacing.y);
                    Vector2 posBotLeft = new Vector2(posicio.x, posicio.y - cellSpacing.y);
                    comprovaPosicionsMoviment(posicioPath_tmp, posBotLeft, movimentsActuals, maxMoviments);

                    //Vector2 posTopLeft = new Vector2(posicio.x - cellSpacing.x, posicio.y + cellSpacing.y);
                    Vector2 posTopLeft = new Vector2(posicio.x, posicio.y + cellSpacing.y);
                    comprovaPosicionsMoviment(posicioPath_tmp, posTopLeft, movimentsActuals, maxMoviments);
                }
            }
        }
    }
    
    public List<Vector2> buscaRangAtacPersonatge(Vector2 posicio, int rangMin, int rangMax, char tipus)
    {
        List<Vector2> posicionsFinals = new List<Vector2>();
        // segons el tipus fer una cosa o altra
        if (tipus == 'N' || tipus == 'I')
        {
            List<Vector2> posicionsValides = retornaPosicionsRang(posicio, tipus, rangMin, rangMax);
            List<Vector2> posicionsInvalides = retornaPosicionsRangInvalides(posicio, posicionsValides, tipus);
            posicionsFinals = eliminaPosicionsRangInvalides(posicio, posicionsValides, posicionsInvalides, tipus, rangMax);
            posicionsFinals = posicionsValides;
        }
        else if (tipus == 'L' || tipus == 'X')
        {
            posicionsFinals = retornaPosicionsRang(posicio, tipus, rangMin, rangMax);
        }

        return posicionsFinals;
    }

    public List<Vector2> buscaRangAtacPersonatge(Vector2 posicio, Vector2 posicioPersonatge, int rangMin, int rangMax, char tipus)
    {
        List<Vector2> posicionsFinals = new List<Vector2>();
        
        // agafar la direcció
        Vector2 direccio = new Vector2();
        Vector2 distancia = WorldManager.Instance.distanceCells(posicioPersonatge, posicio);
        //if (distancia.x >= 0) direccio.x = -cellSpacing.x;
        //else direccio.x = cellSpacing.x;
        //if (distancia.y >= 0) direccio.y = -cellSpacing.y;
        //else direccio.y = cellSpacing.y;
        if (distancia.x > 0) direccio.x = -cellSpacing.x;
        else if (distancia.x < 0) direccio.x = cellSpacing.x;
        else if (distancia.y > 0) direccio.y = -cellSpacing.y;
        else if (distancia.y < 0) direccio.y = cellSpacing.y;


        // segons el tipus fer una cosa o altra
        if (tipus == 'R' || tipus == 'I')
        {
            posicionsFinals = retornaPosicionsRang(posicio, direccio, tipus, rangMin, rangMax);
        }

        return posicionsFinals;
    }

    private List<Vector2> retornaPosicionsRang(Vector2 posicio, char tipus, int minRang, int maxRang)
    {
        List<Vector2> posicionsRang = new List<Vector2>();

        if (tipus == 'N' || tipus == 'I')
        {
            Vector2 posInicial1 = posicio;
            Vector2 posInicial2 = posicio;
            for (int i = 0; i <= maxRang; i++)
            {
                Vector2 posRang1_1 = posInicial1;
                Vector2 posRang1_2 = posInicial1;

                Vector2 posRang2_1 = posInicial2;
                Vector2 posRang2_2 = posInicial2;

                for (int j = 0; j <= (maxRang - i); j++)
                {
                    if ((i+j) >= minRang && (i+j) <= maxRang) posicionsRang.Add(posRang1_1);
                    //posRang1_1 = new Vector2(posRang1_1.x + cellSpacing.x, posRang1_1.y - cellSpacing.y); // Right Down
                    posRang1_1 = new Vector2(posRang1_1.x + cellSpacing.x, posRang1_1.y); // Right Down
                    
                    if (j > 0) 
                    {
                        if ((i+j) >= minRang && (i+j) <= maxRang) posicionsRang.Add(posRang1_2);
                    }
                    //posRang1_2 = new Vector2(posRang1_2.x - cellSpacing.x, posRang1_2.y + cellSpacing.y); // Left Up
                    posRang1_2 = new Vector2(posRang1_2.x - cellSpacing.x, posRang1_2.y); // Left Up
                    
                    if (i > 0)
                    {
                        if ((i+j) >= minRang && (i+j) <= maxRang) posicionsRang.Add(posRang2_1);
                        //posRang2_1 = new Vector2(posRang2_1.x + cellSpacing.x, posRang2_1.y - cellSpacing.y); // Right Down
                        posRang2_1 = new Vector2(posRang2_1.x + cellSpacing.x, posRang2_1.y); // Right Down

                        if (j > 0) 
                        {
                            if ((i+j) >= minRang && (i+j) <= maxRang) posicionsRang.Add(posRang2_2);
                        }
                        //posRang2_2 = new Vector2(posRang2_2.x - cellSpacing.x, posRang2_2.y + cellSpacing.y); // Left Up
                        posRang2_2 = new Vector2(posRang2_2.x - cellSpacing.x, posRang2_2.y); // Left Up
                    }
                }

                //posInicial1 = new Vector2(posInicial1.x + cellSpacing.x, posInicial1.y + cellSpacing.y); // Right Up
                //posInicial2 = new Vector2(posInicial2.x - cellSpacing.x, posInicial2.y - cellSpacing.y); // Left Down
                posInicial1 = new Vector2(posInicial1.x, posInicial1.y + cellSpacing.y); // Right Up
                posInicial2 = new Vector2(posInicial2.x, posInicial2.y - cellSpacing.y); // Left Down
            }
        }
        else if (tipus == 'L')
        {
            //Vector2[] posicions = {new Vector2(1,0),new Vector2(-1,0),new Vector2(0,1),new Vector2(0,-1)};
            Vector2[] posicions = {new Vector2(cellSpacing.x,0),new Vector2(-cellSpacing.x,0),new Vector2(0,cellSpacing.y),new Vector2(0,-cellSpacing.y)};
            
            for (int i = 0; i < posicions.Length; i++)
            {
                Vector2 posRang = posicio + posicions[i];
                int j = 1;
                while (esPosicioValida(posRang) && j <= maxRang)
                {
                    if (j >= minRang && j <= maxRang) posicionsRang.Add(posRang);
                    posRang += posicions[i];
                    j++;
                }

                if (!esPosicioValida(posRang) && getTagPos(posRang) != "Mapa" && j <= maxRang) posicionsRang.Add(posRang);
            }
        }
        else if (tipus == 'X')
        {
            //Vector2[] posicions = {new Vector2(1,0),new Vector2(-1,0),new Vector2(0,1),new Vector2(0,-1)};
            Vector2[] posicions = {new Vector2(cellSpacing.x,0),new Vector2(-cellSpacing.x,0),new Vector2(0,cellSpacing.y),new Vector2(0,-cellSpacing.y)};

            for (int i = 0; i < posicions.Length; i++)
            {
                Vector2 posRang = posicio + posicions[i];
                int j = 1;
                while (j < maxRang)
                {
                    if (esPosicioValida(posRang) || (!esPosicioValida(posRang) && getTagPos(posRang) != "Mapa"))
                    {
                        if (j >= minRang && j <= maxRang) posicionsRang.Add(posRang);
                    }
                    posRang += posicions[i];
                    j++;
                }
            }
        }

        if (minRang == 0) 
        {
            if (!posicionsRang.Contains(posicio)) posicionsRang.Add(posicio);
        }
        else posicionsRang.Remove(posicio);

        return posicionsRang;
    }

    private List<Vector2> retornaPosicionsRang(Vector2 posicio, Vector2 direccio, char tipus, int minRang, int maxRang)
    {
        List<Vector2> posicionsRang = new List<Vector2>();
        
        if (tipus == 'R')
        {
            Vector2 posRang = posicio + direccio;
            int i = 1;
            
            while (i <= maxRang && esPosicioValida(posRang))
            {
                if (i >= minRang && i <= maxRang) posicionsRang.Add(posRang);
                posRang += direccio;
                i++;
            }
            
            if (!esPosicioValida(posRang) && getTagPos(posRang) != "Mapa" && i < maxRang) posicionsRang.Add(posRang);
        }
        else if (tipus == 'I')
        {
            Vector2 posRang = posicio + direccio;

            for (int i = 1; i <= maxRang; i++)
            {
                if (esPosicioValida(posRang) || (!esPosicioValida(posRang) && getTagPos(posRang) != "Mapa"))
                {
                    if (i >= minRang && i <= maxRang) posicionsRang.Add(posRang);
                }
                posRang += direccio;
            }
        }

        if (minRang == 0) 
        {
            if (!posicionsRang.Contains(posicio)) posicionsRang.Add(posicio);
        }
        else posicionsRang.Remove(posicio);

        return posicionsRang;
    }

    private List<Vector2> retornaPosicionsRangInvalides(Vector2 posicio, List<Vector2> posicionsValides, char tipus)
    {
        List<Vector2> posicionsInvalides = new List<Vector2>();
        if (tipus == 'N' || tipus == 'I')
        {
            foreach (Vector2 posValida in posicionsValides)
            {
                if (!esPosicioValida(posValida))
                {
                    if (posValida != posicio) posicionsInvalides.Add(posValida);
                }
                
            }
        }

        return posicionsInvalides;
    }

    private List<Vector2> eliminaPosicionsRangInvalides(Vector2 posicio, List<Vector2> posicionsValides, List<Vector2> posicionsInvalides, char tipus, int maxRang)
    {
        List<Vector2> posicionsFinals = posicionsValides;

        if (tipus == 'N')
        {
            foreach (Vector2 posInvalida in posicionsInvalides)
            {
                Vector2 distancia = WorldManager.Instance.distanceCells(posicio, posInvalida);
                // Distancia:
                // x = -1 (Dreta)
                // x = 1 (Esquerra)
                // y = 1 (Abaix)
                // y = -1 (Adal)
                
                if (distancia.x == 0 || distancia.y == 0) // Es troba a un costat (amunt, aball, esquerra o dreta), en linia recta
                {
                    // Eliminar en forma de T
                    // Si 1,1, la forma s'aplica cada casella
                    // Si 2,2, la forma s'aplica cada 2 caselles, etc.

                    int distanciaTmp = 0;
                    if (distancia.y == 0) distanciaTmp = (int) Mathf.Abs(distancia.x);
                    else distanciaTmp = (int) Mathf.Abs(distancia.y);
                    
                    float desplacamentX = cellSpacing.x;
                    if (distancia.x > 0) desplacamentX = -desplacamentX;
                    float desplacamentY = cellSpacing.y;
                    if (distancia.y > 0) desplacamentY = -desplacamentY;
                    
                    Vector2 diagonal1 = posInvalida;
                    Vector2 diagonal2 = diagonal1;
                    for (int i = 0; i <= (maxRang - distanciaTmp); i++) // calculem les diagonals que hem de fer
                    {
                        // diagonals
                        Vector2 tmpDiagonal1 = diagonal1;
                        Vector2 tmpDiagonal2 = diagonal2;
                        for (int j = 0; j <= (maxRang - distanciaTmp) - i; j++) // comprovem les linies de la diagonal
                        {
                            if (tmpDiagonal1 != posInvalida || ( tmpDiagonal1 == posInvalida && getTagPos(tmpDiagonal1) == "Mapa")) posicionsFinals.Remove(tmpDiagonal1);
                            if (tmpDiagonal2 != posInvalida || ( tmpDiagonal2 == posInvalida && getTagPos(tmpDiagonal2) == "Mapa")) posicionsFinals.Remove(tmpDiagonal2);

                            if (distancia.y == 0) // Diagonals en X
                            {
                                tmpDiagonal1 = new Vector2(tmpDiagonal1.x + desplacamentX, tmpDiagonal1.y);
                                tmpDiagonal2 = new Vector2(tmpDiagonal2.x + desplacamentX, tmpDiagonal2.y);
                            }
                            else // Diagonals en Y
                            {
                                tmpDiagonal1 = new Vector2(tmpDiagonal1.x, tmpDiagonal1.y + desplacamentY);
                                tmpDiagonal2 = new Vector2(tmpDiagonal2.x, tmpDiagonal2.y + desplacamentY);
                            }
                        }
                        
                        for (int w = 0; w < distanciaTmp; w++) // avançem quant calgui per fer la diagonal
                        {
                            if (distancia.y == 0) // Diagonals en X
                            {
                                diagonal1 = new Vector2(diagonal1.x + desplacamentX, diagonal1.y);
                                diagonal2 = new Vector2(diagonal2.x + desplacamentX, diagonal2.y);
                            }
                            else // Diagonals en Y
                            {
                                diagonal1 = new Vector2(diagonal1.x, diagonal1.y + desplacamentY);
                                diagonal2 = new Vector2(diagonal2.x, diagonal2.y + desplacamentY);
                            }
                        }

                        // avançem en diagonal
                        if (distancia.y == 0) // Diagonals en X
                        {
                            diagonal1 = new Vector2(diagonal1.x, diagonal1.y + desplacamentY);
                            diagonal2 = new Vector2(diagonal2.x, diagonal2.y - desplacamentY);
                        }
                        else // Diagonals en Y
                        {
                            diagonal1 = new Vector2(diagonal1.x + desplacamentX, diagonal1.y);
                            diagonal2 = new Vector2(diagonal2.x - desplacamentX, diagonal2.y);
                        }
                        //diagonal1 = new Vector2(diagonal1.x + (desplacamentX * 2), diagonal1.y);
                        //diagonal2 = new Vector2(diagonal2.x, diagonal2.y + (desplacamentY * 2));
                    }
                } 
                else if (Mathf.Abs(distancia.x) == Mathf.Abs(distancia.y)) // Es troba en una diagonal
                {
                    // Avançem en diagonal, (per saber els passos que comprovem: distancia en diagonal - desplaçaments laterals)

                    // Si distancia = 2 (1,1)
                    //  0, Diagonal 1 Costat, 2, 2, 3, 4, 4, 5, 6, 6, 7...
                    //  sumem 3 vegades, 1 no sumem, repetim

                    // Si distancia = 4 (2,2) (o més)
                    //  0, Diagonal 1 Costat, 1, 2, 2, 3, 3, 4, 4, 5, 5...
                    //  sumem 2 vegades, 1 no sumem, repetim
                    
                    int distanciaDiagonal = (int) Mathf.Abs(distancia.x) + (int) Mathf.Abs(distancia.y);
                    int bloquejarDespLaterals = 3;
                    if (distanciaDiagonal >= 4) bloquejarDespLaterals = 2;

                    // Distancia:
                    // x = -1 (Dreta)
                    // x = 1 (Esquerra)
                    // y = 1 (Abaix)
                    // y = -1 (Adal)
                    float desplacamentX = cellSpacing.x;
                    if (distancia.x > 0) desplacamentX = -desplacamentX;
                    float desplacamentY = cellSpacing.y;
                    if (distancia.y > 0) desplacamentY = -desplacamentY;

                    // Si diagonal = -X: -x, -y i -x, y
                    // Si diagonal = X: x, y i x, -y
                    // Si diagonal = -Y: -x, -y i -x, y
                    // Si diagonal = Y: x, y i x, -y
                    float desplacamentLateralX = cellSpacing.x;
                    if (distancia.x < 0) desplacamentLateralX = -desplacamentLateralX;
                    float desplacamentLateralY = cellSpacing.y;
                    if (distancia.y < 0) desplacamentLateralY = -desplacamentLateralY;

                    Vector2 diagonal = posInvalida;
                    int desplacamentsDiagonals = 0;
                    int desplacamentsLaterals = 0;
                    while ( (distanciaDiagonal - desplacamentsLaterals) <= maxRang)
                    {
                        if (diagonal != posInvalida || (diagonal == posInvalida && getTagPos(diagonal) == "Mapa")) posicionsFinals.Remove(diagonal);
                        
                        for (int i = 1; i <= desplacamentsLaterals; i++) // avançem de costat
                        {
                            Vector2 costat1 = new Vector2(diagonal.x + (desplacamentLateralX * i), diagonal.y);
                            posicionsFinals.Remove(costat1);
                            
                            Vector2 costat2 = new Vector2(diagonal.x, diagonal.y + (desplacamentLateralY * i));
                            posicionsFinals.Remove(costat2);
                        }

                        // avançem en diagonal
                        diagonal = new Vector2(diagonal.x + desplacamentX, diagonal.y + desplacamentY);
                        distanciaDiagonal += 1;

                        desplacamentsDiagonals++;
                        if (desplacamentsDiagonals >= bloquejarDespLaterals) desplacamentsDiagonals = 0;
                        else desplacamentsLaterals++;
                    }
                }
                else // Es troba en una diagonal no perfecta
                {
                    int distX = (int) Mathf.Abs(distancia.x);
                    int distY = (int) Mathf.Abs(distancia.y);
                    int distanciaActual = Mathf.Max(distX, distY);

                    int distanciaLateral = Mathf.Max(distX, distY);
                    if (distanciaActual >= 5 && Mathf.Min(distX, distY) < (Mathf.Max(distX, distY) / 1.5)) // > 5 caselles amb el player i min < 2/3 de max
                    {
                        distanciaLateral = Mathf.Max(distX, distY) - (Mathf.Max(distX, distY) - Mathf.Min(distX, distY));
                    }

                    // Desplaçament Lateral:
                    float desplacamentX = cellSpacing.x;
                    if (distancia.x > 0) desplacamentX = -desplacamentX;
                    float desplacamentY = cellSpacing.y;
                    if (distancia.y > 0) desplacamentY = -desplacamentY;

                    // Repetim de distanciaActual fins maxRang
                    int desplacamentsVerticals = 0;
                    int i = 0;
                    Vector2 posicioLateral = posInvalida;
                    while (distanciaActual <= maxRang)
                    {
                        if (posicioLateral != posInvalida || (posicioLateral == posInvalida && getTagPos(posicioLateral) == "Mapa")) posicionsFinals.Remove(posicioLateral);

                        // Pas 1: Avançem de posActual per l'eix més gran (distanciaX > distancia Y) x vegades
                        if (i > 0)
                        {
                            for (int j = 1; j <= desplacamentsVerticals; j++)
                            {
                                Vector2 posicioVertical = new Vector2();
                                if (distX > distY) posicioVertical = new Vector2(posicioLateral.x + (desplacamentX * j), posicioLateral.y);
                                else posicioVertical = new Vector2(posicioLateral.x, posicioLateral.y + (desplacamentY * j));
                                posicionsFinals.Remove(posicioVertical);
                            }
                        }

                        // Pas 2: Avançem sobre l'eix més gran o el menor
                        if (i >= distanciaLateral) // Avançem en l'eix contrari
                        {
                            if (distX > distY) posicioLateral = new Vector2(posicioLateral.x, posicioLateral.y + desplacamentY);
                            else posicioLateral = new Vector2(posicioLateral.x + desplacamentX, posicioLateral.y);
                            desplacamentsVerticals--;
                            i = 0;
                        }
                        else // Avançem en l'eix més gran
                        {
                            if (distX > distY) posicioLateral = new Vector2(posicioLateral.x + desplacamentX, posicioLateral.y);
                            else posicioLateral = new Vector2(posicioLateral.x, posicioLateral.y + desplacamentY);
                            desplacamentsVerticals++;
                            i++;
                        }

                        distanciaActual++;
                    }
                }
            }
        }
        else if (tipus == 'I')
        {
            foreach (Vector2 posInvalida in posicionsInvalides)
            {
                if (getTagPos(posInvalida) == "Mapa") posicionsFinals.Remove(posInvalida);
            }
        }

        return posicionsFinals;
    }

    public void checkFinish()
    {
        if (!personatgesSonVius()) endFight();
    }

    public void dealDamageTo(int damage, Vector2 posicio) // mirar qui hi ha a la posicio, aplicar el "damage"
    {
        List<Transform> personatgesHited = getPlayersFrom(posicio);
        foreach (Transform personatge in personatgesHited)
        {
            int damaged = 0;
            bool personatgeCorrecte = true;
            if (personatge.tag == player.tag)
            {
                Player p = personatge.GetComponent<Player>();
                damaged = p.takeDamage(damage);
            }
            else if (personatge.tag == "Enemic")
            {
                Enemic e = personatge.GetComponent<Enemic>();
                damaged = e.takeDamage(damage);
            }
            else personatgeCorrecte = false;

            if (personatgeCorrecte)
            {
                mostraUIWorldObjectByType("Atac", new Vector3(posicio.x, posicio.y, -0.01f), damaged.ToString());
            }
        }
    }

    public void healTo(int heal, Vector2 posicio) // mirar qui hi ha a la posicio, aplicar el "heal"
    {
        List<Transform> personatgesHited = getPlayersFrom(posicio);
        foreach (Transform personatge in personatgesHited)
        {
            int healed = 0;
            bool personatgeCorrecte = true;
            if (personatge.tag == player.tag)
            {
                Player p = personatge.GetComponent<Player>();
                healed = p.takeHeal(heal);
            }
            else if (personatge.tag == "Enemic")
            {
                Enemic e = personatge.GetComponent<Enemic>();
                healed = e.takeHeal(heal);
            }
            else personatgeCorrecte = false;

            if (personatgeCorrecte)
            {
                mostraUIWorldObjectByType("Cura", new Vector3(posicio.x, posicio.y, -0.01f), healed.ToString());
            }
        }
    }

    public void aplicaHabilitatsTo(Dictionary <int, int> habilitatsEspecials, Vector2 posicioPersonatge, Vector2 posicio)
    {
        if (habilitatsEspecials.Count > 0)
        {
            // Per cada habilitat, fer el que calgui
            List<Transform> personatgesHited = getPlayersFrom(posicio);
            foreach (Transform personatge in personatgesHited)
            {
                Player p = personatge.GetComponent<Player>();
                Enemic e = personatge.GetComponent<Enemic>();

                if (p || e)
                {
                    foreach(KeyValuePair<int, int> habilitatEspecial in habilitatsEspecials)
                    {
                        switch (habilitatEspecial.Key)
                        {
                            case 1: // Treure Mana
                                Debug.Log("Treure Mana: "+habilitatEspecial.Value);
                                if (p) p.takeRestaMana(habilitatEspecial.Value);
                                if (e) e.takeRestaMana(habilitatEspecial.Value);
                            break;
                            case 2: // Treure Passos
                                Debug.Log("Treure Passos: "+habilitatEspecial.Value);
                                if (p) p.takeRestaPassos(habilitatEspecial.Value);
                                if (e) e.takeRestaPassos(habilitatEspecial.Value);
                            break;
                            case 3: // Emputxar
                                // agafar la direcció
                                Vector2 direccio = new Vector2();
                                Vector2 distancia = WorldManager.Instance.distanceCells(posicioPersonatge, posicio);
                                if (distancia.x != 0) 
                                {
                                    if (distancia.x > 0) direccio.x = -cellSpacing.x;
                                    else direccio.x = cellSpacing.x;
                                }
                                else if (distancia.y != 0) 
                                {
                                    if (distancia.y > 0) direccio.y = -cellSpacing.y;
                                    else direccio.y = cellSpacing.y;
                                }
                                /*if (distancia.x >= 0) direccio.x = -cellSpacing.x;
                                else direccio.x = cellSpacing.x;
                                if (distancia.y >= 0) direccio.y = -cellSpacing.y;
                                else direccio.y = cellSpacing.y;*/

                                Vector2 posicioObjectiu = (posicio);
                                int i = 0;
                                while (esPosicioValida(posicioObjectiu + direccio) && i < habilitatEspecial.Value)
                                {
                                    posicioObjectiu += direccio;
                                    i++;
                                }

                                if (p) p.takeEmputxarTo(posicioObjectiu);
                                if (e) e.takeEmputxarTo(posicioObjectiu);
                            break;
                            case 4: //BoostBoss1
                                if (p) p.takeEstat(habilitatEspecial.Key, habilitatEspecial.Value);
                                if (e) e.takeEstat(habilitatEspecial.Key, habilitatEspecial.Value);
                            break;
                            case 5: //BoostBoss2
                                if (p) p.takeEstat(habilitatEspecial.Key, habilitatEspecial.Value);
                                if (e) e.takeEstat(habilitatEspecial.Key, habilitatEspecial.Value);
                            break;
                        }
                    }
                }
            }
        }
    }

    private RaycastHit2D objecteAPosicio(Vector2 pos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);
        foreach(RaycastHit2D hit in hits)
        {
            if (!hit.collider.isTrigger) return hit;
        }
        return new RaycastHit2D();
    }

    public RaycastHit2D objecteAPosicioAmbTag(Vector2 pos, string tag)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);
        int i = 0;
        while (i < hits.Length) 
        {
            if (hits[i].transform.tag == tag) return hits[i];
            i++;
        }

        return new RaycastHit2D();
    }

    public RaycastHit2D objecteAPosicioAmbTag(Vector2 pos, string tag, bool trigger)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);
        int i = 0;
        while (i < hits.Length) 
        {
            if (hits[i].transform.tag == tag && hits[i].collider.isTrigger == trigger) return hits[i];
            i++;
        }

        return new RaycastHit2D();
    }

    public RaycastHit2D objecteAPosicioAmbTag(Vector2 pos, List<string> tags)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);
        int i = 0;
        while (i < hits.Length) 
        {
            if (tags.Contains(hits[i].transform.tag)) return hits[i];
            i++;
        }

        return new RaycastHit2D();
    }

    public List<GameObject> buscaPersonatgesAreaAtac(List<Vector2> posicionsArea) // Retorna els objectes dins una area
    {
        List<string> tagsPersonatges = new List<string>(new string[] { player.tag,"Enemic" });
        List<GameObject> personatges = new List<GameObject>();

        foreach(Vector2 posicioArea in posicionsArea)
        {
            RaycastHit2D personatge = objecteAPosicioAmbTag(posicioArea, tagsPersonatges);
            if (personatge.collider != null) personatges.Add(personatge.transform.gameObject);
        }

        return personatges;
    }

    public bool liniaDeVisio(Vector2 p1, Vector2 p2, string tagPersonatge)
    {
        RaycastHit2D h = Physics2D.Linecast(p1, p2);
        return (h.transform.tag != "Mapa" && h.transform.tag != tagPersonatge);
    }

    public bool liniaDeVisioToPlayer(Vector2 p1, string tagPersonatge)
    {
        Vector3 playerPos = player.getPosition();
        Vector2 p2 = new Vector2(playerPos.x, playerPos.y);
        return liniaDeVisio(p1, p2, tagPersonatge);
    }

    private bool esPosicioValida(Vector2 posicio)
    {
        // Comprovem si està fora de camera
        if (!WorldManager.Instance.pointInCamera(posicio)) return false;

        // Comprovem els hits
        RaycastHit2D[] hits = Physics2D.RaycastAll(posicio, Vector2.zero);
        int i = 0;
        while (i < hits.Length) 
        {
            if (hits[i].collider != null && !hits[i].collider.isTrigger) return false;
            i++;
        }
        return true;
    }

    private string getTagPos(Vector2 posicio)
    {
        RaycastHit2D hit = objecteAPosicio(posicio);
        if (hit)
        {
            return hit.transform.tag;
        }
        
        return "";
    }

    private List<Transform> getPlayersFrom(Vector2 posicio) // retornar tots els players i enemics tocats per l'atac
    {
        List<Transform> personatgesHited = new List<Transform>();
        RaycastHit2D[] rh2d = Physics2D.RaycastAll(posicio, Vector2.zero);
        foreach (RaycastHit2D hit in rh2d)
        {
            if (!hit.collider.isTrigger && (hit.transform.tag == "Enemic" || hit.transform.tag == player.tag))
            {
                if (!personatgesHited.Contains(hit.transform)) personatgesHited.Add(hit.transform);
            }
        }
        return personatgesHited;
    }

    public bool isPlayerIn(Vector2 posicio)
    {
        return getTagPos(posicio) == player.tag;
    }

    public bool isPlayerIn(List<Vector2> posicions)
    {
        foreach (Vector2 posicio in posicions)
        {
            if (isPlayerIn(posicio)) return true;
        }
        return false;
        //return posicions.Contains(player.getPosition());
    }

    public bool isTagIn(Vector2 posicio, string tag)
    {
        return getTagPos(posicio) == player.tag;
    }

    public int distanceToPlayer(Vector2 posicio)
    {
        int distX = (int) (Mathf.Abs( posicio.x - player.transform.position.x ) / cellSpacing.x);
        int distY = (int) (Mathf.Abs( posicio.y - player.transform.position.y ) / cellSpacing.y);
        
        //int distancia = 0;
        //if (distX == distY) distancia = distX;
        //else distancia = distX + distY;
        int distancia = distX + distY;
        
        return distancia;
    }

    public int distanceTo(Vector2 posicio1, Vector2 posicio2)
    {
        int distX = (int) (Mathf.Abs( posicio1.x - posicio2.x ) / cellSpacing.x);
        int distY = (int) (Mathf.Abs( posicio1.y - posicio2.y ) / cellSpacing.y);
        
        //int distancia = 0;
        //if (distX == distY) distancia = distX;
        //else distancia = distX + distY;
        int distancia = distX + distY;
        
        return distancia;
    }

    public void mostraAccioUsuari(string tipus)
    {
        player.mostraAccioUsuari(tipus);
    }

    private GameObject getUIWorldObjectByType(string tipus)
    {
        GameObject objecte = null;

        int i = 0;
        while (objecte == null && i < uiWorldObjects.Length)
        {
            if (uiWorldObjects[i].name == ("UIWorld_"+tipus+"Text"))
            {
                objecte = uiWorldObjects[i];
            }
            i++;
        }

        return objecte;
    }

    public void mostraUIWorldObjectByType(string tipus, Vector3 posicio, string valor)
    {
        GameObject uiWorldText = Instantiate(getUIWorldObjectByType(tipus), new Vector3(posicio.x + Random.Range(-0.2f, 0.2f), posicio.y + Random.Range(-0.2f, 0.2f), -0.01f + Random.Range(-0.25f, 0.25f)), Quaternion.identity);
        uiWorldText.transform.Find("Text").GetComponent<Text>().text = valor;
    }

    private bool personatgesSonVius()
    {
        // Si player no està viu tornem false
        if (!player.isAlive()) return false;

        // Si queda 1 enemic viu tornem true, si no en queda cap viu tornem false
        int i = 0;
        bool enemicViu = false;
        while (i < enemics.Length && !enemicViu) 
        {
            if (enemics[i].isAlive()) enemicViu = true;
            i++;
        }
        return enemicViu;
    }

    public bool playerIsAlive()
    {
        return player.isAlive();
    }

    public UIPartidaManager getUIPartidaManager()
    {
        return uiManager;
    }

    public void playerCompletaMissio(int monedes, int experiencia)
    {
        player.takeMonedes(monedes);
        player.takeExperiencia((ulong) experiencia);
    }
}
