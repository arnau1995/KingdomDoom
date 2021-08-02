using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    // Variables World
    [SerializeField] private UIWorldManager uIWorldManager;
    [SerializeField] private GameObject uILevelUp;
    [SerializeField] private Vector2 posInicial;
    [SerializeField] private float recuperarVida = 5f;
    private float tempsRecuperarVida;

    // Variables Equipament
    private int arma;
    private GameObject armaPrincipal;

    // Variables Player
    private int monedes;

    // Variables Lluita
    [SerializeField] private GameObject resaltarCelaMoviment;
    [SerializeField] private GameObject resaltarCelaAtac;
    [SerializeField] private GameObject tmp_resaltarCelaAtac;
    private GameObject blocMoviments;
    private GameObject blocAtac;
    private GameObject blocAtacTmp;
    private AtributsPersonatge atributs;
    private bool actiu = false;
    private bool enMoviment = false;
    private Transform mesh = null;
    private AgentScript agent = null;
    private Animator animator = null;

    // Variables Atributs (s'utilitzen per iniciar els atributs de Player)
    [SerializeField] private float speed = 2f;
    private ulong experiencia = 10; // Experiencia necessaria per superar els nivells (2^(nivell-1) * experiencia)
    [SerializeField] private int vida = 10;
    [SerializeField] private int mana = 5;
    [SerializeField] private int moviments = 5;

    private string accioActual;
    private Dictionary<Vector2, List<Vector2>> movimentsValids;

    // Variables guardar pos player
    private float timeToSavePos = 15; // Cada 15 segons es guardarà la pos del player
    private float timeElapsetdSavePos = 0;

    // Start is called before the first frame update
    void Start()
    {
        Vector2 positionIni = new Vector2(PlayerPrefs.GetFloat("playerPosX", posInicial.x), PlayerPrefs.GetFloat("playerPosY", posInicial.y));
        StartCoroutine( MoveDirectlyTo(new Vector3(positionIni.x, positionIni.y, 0)) );

        reiniciarMoviments();
        actiu = false;
        enMoviment = false;
        accioActual = "";
        blocMoviments = null;
        blocAtac = null;
        blocAtacTmp = null;
        
        monedes = PlayerPrefs.GetInt("monedes", 0);
        
        armaPrincipal = RecursiveFindChild(transform, "Arma Principal").gameObject;
        canviaArma(PlayerPrefs.GetInt("arma", 0));

        int nivell = PlayerPrefs.GetInt("nivell", 1);
        atributs = new AtributsPersonatge (nivell, experiencia, vida, mana, moviments);
        setExperiencia(ulong.Parse(PlayerPrefs.GetString("experiencia", "0")));
        atributs.reiniciaAtributsLluita();
        finalitzaTorn();

        uIWorldManager.actualitzaVida(atributs.getVidaActual(), atributs.getVida());
        uIWorldManager.actualitzaExperiencia(atributs.getExperienciaActual(), atributs.getExperienciaNivell());
        uIWorldManager.actualitzaNivell(getNivell());
        uIWorldManager.actualitzaMonedes(monedes);
        uIWorldManager.actualitzaMana(atributs.getManaActual());
        uIWorldManager.actualitzaMoviments(atributs.getMovimentsActual());

        mesh = transform.Find("Mesh");
        agent = transform.GetComponent<AgentScript>();
        animator = mesh.GetComponent<Animator>();
    }

    void Update()
    {
        if (atributs.getVidaActual() < atributs.getVida())
        {
            tempsRecuperarVida += Time.deltaTime;
            if (tempsRecuperarVida >= recuperarVida) 
            {
                takeHeal(1);
                tempsRecuperarVida = 0;
            }
        }

        timeElapsetdSavePos += Time.deltaTime;
        if (timeElapsetdSavePos >= timeToSavePos)
        {
            Vector3 position = WorldManager.Instance.positionToCellSpace(transform.position, transform.tag);
            PlayerPrefs.SetFloat("playerPosX", position.x);
            PlayerPrefs.SetFloat("playerPosY", position.y);
            timeElapsetdSavePos = 0;
        }
    }

    // SUPPORT ////////////////////////////////////
    public static Transform RecursiveFindChild(Transform parent, string childName)
    {
        Transform child = null;
        for (int i = 0; i < parent.childCount; i++)
        {
            child = parent.GetChild(i);
            if (child.name == childName)
            {
                break;
            }
            else
            {
                child = RecursiveFindChild(child, childName);
                if (child != null)
                {
                    break;
                }
            }
        }

        return child;
    }

    // WORLD //////////////////////////////////////
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null && col.transform.tag == "Enemic" && col.transform.parent.tag == "Mob")
        {
            //col.transform.parent.GetComponent<MobInfo>().getMobEnemics();
            WorldManager.Instance.startFight(col.transform.parent.gameObject);
        }
    }
    // FI WORLD ///////////////////////////////////

    // FIGHT //////////////////////////////////////

    public void reiniciaAccions()
    {
        accioActual = "";
        eliminaMovimentsPossibles();
        eliminaCelesAtacTmp();
        eliminaRangAtac();
    }

    public Vector2 getPosition()
    {
        return WorldManager.Instance.roundPos(new Vector2(transform.position.x, transform.position.y));
    }

    public int getAreaAtacMinActual()
    {
        return AtacManager.Instance.getAreaAtacMin(accioActual);
    }

    public int getAreaAtacMaxActual()
    {
        return AtacManager.Instance.getAreaAtacMax(accioActual);
    }

    public char getTipusAreaAtacActual()
    {
        return AtacManager.Instance.getTipusAreaAtac(accioActual);
    }

    public void iniciaLluita()
    {
        actiu = false;
        enMoviment = false;
        reiniciaAccions();
        reiniciarMoviments();
        atributs.reiniciaAtributsLluita();
        uIWorldManager.actualitzaMana(atributs.getManaActual());
        uIWorldManager.actualitzaMoviments(atributs.getMovimentsActual());
        enabled = false;
    }

    public void finalitzaLluita()
    {
        finalitzaTorn();
        enabled = true;
    }

    public void iniciaTorn()
    {
        actiu = true;
        enMoviment = false;
        reiniciaAccions();
        reiniciarMoviments();
    }

    public void finalitzaTorn()
    {
        actiu = false;
        enMoviment = false;
        atributs.reiniciaAtributsTorn();
        uIWorldManager.actualitzaMana(atributs.getManaActual());
        uIWorldManager.actualitzaMoviments(atributs.getMovimentsActual());
        reiniciarMoviments();
        reiniciaAccions();
    }

    public bool tornActiu()
    {
        return actiu;
    }

    public bool accioEsMoviment()
    {
        return accioActual == "moviment";
    }

    public bool accioEsValida()
    {
        return accioActual != "";
    }

    public void mostraEfecteAccio(Vector3 posicio)
    {
        if (actiu && accioEsValida() && !accioEsMoviment()) AtacManager.Instance.mostraEfecteAtac(accioActual, posicio);
    }

    public bool getEnMoviment()
    {
        return enMoviment;
    }

    public int getMovimentsActual()
    {
        return atributs.getMovimentsActual();
    }

    public GameObject afegirCelaAtacTmp(Vector2 pos)
    {
        if (!blocAtacTmp) 
        {
            blocAtacTmp = new GameObject();
            blocAtacTmp.name = "blocAtacTmp";
            blocAtacTmp.transform.SetParent(null);
        }

        GameObject tmp_novaCela = Instantiate(tmp_resaltarCelaAtac, new Vector3(pos.x, pos.y, -0.01f), Quaternion.identity, blocAtacTmp.transform);
        
        return tmp_novaCela;
    }

    public void eliminaCelesAtacTmp()
    {
        if (blocAtacTmp) Destroy(blocAtacTmp);
        blocAtacTmp = null;
    }

    private void mostraMovimentsPossibles()
    {
        if (movimentsValids.Count < 1 && atributs.getMovimentsActual() > 0)
        {
            movimentsValids = PartidaManager.Instance.buscaMovimentsPersonatge(new Vector2(transform.position.x, transform.position.y), atributs.getMovimentsActual());
        }
        
        blocMoviments = new GameObject();
        blocMoviments.name = "blocMoviments";
        foreach(KeyValuePair<Vector2, List<Vector2>> pos in movimentsValids)
        {
            GameObject novaCela = Instantiate(resaltarCelaMoviment, new Vector3(pos.Key.x, pos.Key.y, -0.01f), Quaternion.identity, blocMoviments.transform);
        }
        blocMoviments.transform.SetParent(null);
    }

    private void eliminaMovimentsPossibles()
    {
        if (blocMoviments) Destroy(blocMoviments);
        blocMoviments = null;
        if (!actiu) reiniciarMoviments();
    }

    private void mostraRangAtac(string tipus)
    {
        List<Vector2> posicionsValides = AtacManager.Instance.buscaRangAtacPersonatge(new Vector2(transform.position.x, transform.position.y), tipus);
        blocAtac = new GameObject();
        blocAtac.name = "blocAtac";
        foreach(Vector2 pos in posicionsValides)
        {
            GameObject novaCela = Instantiate(resaltarCelaAtac, new Vector3(pos.x, pos.y, -0.01f), Quaternion.identity, blocAtac.transform);
        }
        blocAtac.transform.SetParent(null);
    }

    private void eliminaRangAtac()
    {
        if (blocAtac) Destroy(blocAtac);
        blocAtac = null;
    }

    public void mostraAccioUsuari(string tipus)
    {
        if (!enMoviment)
        {
            if (accioActual == tipus)
            {
                reiniciaAccions();
            }
            else
            {
                reiniciaAccions();
                
                if (tipus == "moviment")
                {
                    mostraMovimentsPossibles();
                }
                else
                {
                    if (AtacManager.Instance.getManaAtac(tipus) <= atributs.getManaActual())
                    {
                        mostraRangAtac(tipus);
                    }
                }
                
                accioActual = tipus;
            }
        }
    }

    public void accioUsuari(Vector2 accio) // L'usuari ha accionat una posició de les que tenie possibles
    {
        if (actiu)
        {
            // Cal mirar si estavem atacant o ens moviem
            if (accioActual == "moviment")
            {
                playerMoveTo(accio);
            }
            else
            {
                playerAtacTo(accio);
            }
        }
    }

    private void reiniciarMoviments()
    {
        movimentsValids = new Dictionary<Vector2, List<Vector2>>();
    }

    public List<Vector2> getCamiTo(Vector2 pos)
    {
        if (movimentsValids.ContainsKey(pos)) return movimentsValids[pos];
        else return new List<Vector2>();
    }

    private void playerMoveTo(Vector2 moveTo)
    {
        int moviments = getCamiTo(moveTo).Count;
        atributs.aplicaMoviment(moviments);
        PartidaManager.Instance.mostraUIWorldObjectByType("Moviments", new Vector3(transform.position.x, transform.position.y, -0.01f), moviments.ToString());
        uIWorldManager.actualitzaMoviments(atributs.getMovimentsActual());
        StartCoroutine( MoveFromTo(getCamiTo(moveTo)) );
        reiniciarMoviments();
    }

    public void takeEmputxarTo(Vector2 emputxarTo)
    {
        StartCoroutine( MoveFromTo(emputxarTo) );
        reiniciarMoviments();
    }

    private IEnumerator MoveFromTo(List<Vector2> bPoints) 
    {
        if (animator != null) animator.SetBool("Run", true);

        Vector2 a = transform.position;
        enMoviment = true;
        float z = transform.position.z;

        foreach (Vector2 b in bPoints)
        {
            Vector2 distancia = WorldManager.Instance.distanceCells(a, b);
            // Aplicar rotacio al personatge
            setRotation(distancia);

            float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
            float t = 0;
            while (t <= 1.0f) 
            {
                t += step;                                  // Goes from 0 to 1, incrementing by step each time
                transform.position = Vector3.Lerp(a, b, t); // Move transform closer to b
                transform.position.Set(transform.position.x, transform.position.y, z);
                yield return new WaitForFixedUpdate();      // Leave the routine and return here in the next frame
            }
            a = b;
            transform.position = b;
        }
        enMoviment = false;

        if (animator != null) animator.SetBool("Run", false);
    }

    private IEnumerator MoveFromTo(Vector2 b) 
    {
        Vector2 a = transform.position;
        enMoviment = true;
        float z = transform.position.z;

        float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
        float t = 0;
        while (t <= 1.0f) 
        {
            t += step;                                  // Goes from 0 to 1, incrementing by step each time
            transform.position = Vector3.Lerp(a, b, t); // Move transform closer to b
            transform.position.Set(transform.position.x, transform.position.y, z);
            yield return new WaitForFixedUpdate();      // Leave the routine and return here in the next frame
        }
        a = b;
        transform.position = b;

        enMoviment = false;
    }

    public void setRotation(Vector2 orientacio)
    {
        if (mesh != null)
        {
            agent.setRotation(orientacio);
        }
    }

    private void playerAtacTo(Vector2 atacTo)
    {
        //if (animator != null) animator.SetTrigger("Atac Simple");

        //int mana = AtacManager.Instance.getManaAtac(accioActual);
        //atributs.aplicaMana(mana);
        //PartidaManager.Instance.mostraUIWorldObjectByType("Mana", new Vector3(transform.position.x, transform.position.y, -0.01f), mana.ToString());
        //uIWorldManager.actualitzaMana(atributs.getManaActual());

        // Ataca a la pos
        int damage = AtacManager.Instance.getDamageAtac(accioActual, getNivell()); // Agafem el "damage" base de l'atac
        damage += (damage * getAtacEquipament() / 100);                           //  Sumem el heal de l'equipament
        if (damage > 0) 
        {
            PartidaManager.Instance.dealDamageTo(damage, atacTo);
        }

        // Cura a la pos
        int heal = AtacManager.Instance.getHealAtac(accioActual, getNivell()); // Agafem el "heal" base de l'atac
        heal += (heal * getAtacEquipament() / 100);                           //  Sumem el heal de l'equipament (atac = heal)
        if (heal > 0) 
        {
            PartidaManager.Instance.healTo(heal, atacTo);
        }

        // Aplica atacs especials
        Dictionary <int, int> habilitatsEspecials = AtacManager.Instance.getHabilitatsEspecials(accioActual);
        if (habilitatsEspecials.Count > 0) 
        {
            PartidaManager.Instance.aplicaHabilitatsTo(habilitatsEspecials, new Vector2(transform.position.x, transform.position.y), atacTo);
        }

        // Aplica atacs especials Self
        Dictionary <int, int> habilitatsEspecialsSelf = AtacManager.Instance.getHabilitatsEspecialsSelf(accioActual);
        if (habilitatsEspecialsSelf.Count > 0) 
        {
            PartidaManager.Instance.aplicaHabilitatsTo(habilitatsEspecialsSelf, new Vector2(transform.position.x, transform.position.y), new Vector2(transform.position.x, transform.position.y));
        }
    }
    
    public void aplicaManaAtac()
    {
        if (animator != null) animator.SetTrigger("Atac Simple");
        
        int mana = AtacManager.Instance.getManaAtac(accioActual);
        atributs.aplicaMana(mana);
        PartidaManager.Instance.mostraUIWorldObjectByType("Mana", new Vector3(transform.position.x, transform.position.y, -0.01f), mana.ToString());
        uIWorldManager.actualitzaMana(atributs.getManaActual());

        reiniciarMoviments();
    }

    public void setExperiencia(ulong xp)
    {
        atributs.takeExperiencia(xp);
        PlayerPrefs.SetInt("nivell", getNivell());
        PlayerPrefs.SetString("experiencia", atributs.getExperienciaActual().ToString());

        uIWorldManager.actualitzaExperiencia(atributs.getExperienciaActual(), atributs.getExperienciaNivell());
        uIWorldManager.actualitzaNivell(getNivell());
        uIWorldManager.actualitzaVida(atributs.getVidaActual(), atributs.getVida());
    }

    public void takeExperiencia(ulong xp)
    {
        int nivell_tmp = atributs.getNivell();
        atributs.takeExperiencia(xp);
        PlayerPrefs.SetInt("nivell", getNivell());
        PlayerPrefs.SetString("experiencia", atributs.getExperienciaActual().ToString());

        uIWorldManager.actualitzaExperiencia(atributs.getExperienciaActual(), atributs.getExperienciaNivell());
        uIWorldManager.actualitzaNivell(getNivell());
        uIWorldManager.actualitzaVida(atributs.getVidaActual(), atributs.getVida());

        if (atributs.getNivell() > nivell_tmp) // Mostrem animacio de pujar nivell
        {
            Instantiate(uILevelUp);
        }
    }

    public int getNivell()
    {
        return atributs.getNivell();
    }

    public int takeDamage(int damage)
    {
        // RECALCULAR damage amb el % de defensa dels objectes
        damage -= (damage * getDefensaEquipament() / 100);
        if (damage < 0) damage = 0;

        int damaged = 0;
        damaged = atributs.takeDamage(damage);
        Debug.Log("Player: Vida actual "+atributs.getVidaActual());
        uIWorldManager.actualitzaVida(atributs.getVidaActual(), atributs.getVida());

        if (atributs.getVidaActual() <= 0)
        {
            if (animator != null) animator.SetTrigger("Die");
            Debug.Log("Player: MORT");
        }
        else if (animator != null) animator.SetTrigger("Get Hit");

        return damaged;
    }

    public void reviuPlayer()
    {
        atributs.setVidaActual(1);
        uIWorldManager.actualitzaVida(atributs.getVidaActual(), atributs.getVida());
        StartCoroutine( MoveDirectlyTo(new Vector3(posInicial.x, posInicial.y, 0)) );
        if (animator != null) animator.SetTrigger("Recover");
    }

    private IEnumerator MoveDirectlyTo(Vector3 pos)
    {
        transform.GetComponent<NavMeshAgent>().enabled = false;
        while (Vector3.Distance(transform.position, pos) > 0.5)
        {
            transform.position = pos;
            yield return 0;
        }
        transform.GetComponent<NavMeshAgent>().enabled = true;

        WorldManager.Instance.setPlayerTarget(pos);
    }

    public int takeHeal(int heal)
    {
        int healed = 0;
        healed = atributs.takeHeal(heal);
        Debug.Log("Player: Vida actual "+atributs.getVidaActual());
        uIWorldManager.actualitzaVida(atributs.getVidaActual(), atributs.getVida());
        return healed;
    }

    public bool isAlive()
    {
        return atributs.getVidaActual() > 0;
    }

    public void takeRestaMana(int mana)
    {
        atributs.aplicaMana(mana);
        PartidaManager.Instance.mostraUIWorldObjectByType("Mana", new Vector3(transform.position.x, transform.position.y, -0.01f), mana.ToString());
        uIWorldManager.actualitzaMana(atributs.getManaActual());
        Debug.Log("Player: Resta mana "+mana);
    }

    public void takeRestaPassos(int moviments)
    {
        atributs.aplicaMoviment(moviments);
        PartidaManager.Instance.mostraUIWorldObjectByType("Moviments", new Vector3(transform.position.x, transform.position.y, -0.01f), moviments.ToString());
        uIWorldManager.actualitzaMoviments(atributs.getMovimentsActual());
        reiniciarMoviments();
        Debug.Log("Player: Resta moviments "+moviments);
    }

    public void takeEstat(int idEstat, int tornsEstat)
    {
        atributs.setEstat(idEstat, tornsEstat);
        Debug.Log("Player: Nou estat "+idEstat+" durant "+tornsEstat+" torns");
    }

    private int celesDesplacades(Vector2 newPos)
    {
        Vector2 cellSpacing = WorldManager.Instance.getCellSpacing();
        return (int) Mathf.Max( Mathf.Abs( newPos.x - transform.position.x ) / cellSpacing.x, Mathf.Abs( newPos.y - transform.position.y ) / cellSpacing.y );
    }

    // FI FIGHT ///////////////////////////////////

    // VARIABLES PLAYER ///////////////////////////
    public int getMonedes()
    {
        return monedes;
    }
    public void takeMonedes(int newMonedes) // Es poden sumar o restar les monedes
    {
        monedes += newMonedes;
        PlayerPrefs.SetInt("monedes", monedes);
        uIWorldManager.actualitzaMonedes(monedes);
    }
    // FI VARIABLES PLAYER ////////////////////////

    // EQUIPAMENT /////////////////////////////////
    public void canviaArma(int arm)
    {
        arma = arm;
        PlayerPrefs.SetInt("arma", arma);
        PartidaManager.Instance.getUIPartidaManager().cambiaUiAtacsArma(EquipamentManager.Instance.getAtacs(arma), EquipamentManager.Instance.getUiEquipament(arma));
        
        // Cambiar el model de la arma que porta el player
        if (armaPrincipal.transform.childCount > 0) 
        {
            GameObject.Destroy(armaPrincipal.transform.GetChild(0).gameObject);
        }
        
        GameObject newArmaObjecte = Instantiate(EquipamentManager.Instance.getObjecteEquipament(arma), armaPrincipal.transform);
    }

    public int getArmaEquipada()
    {
        return arma;
    }

    public int getAtacEquipament()
    {
        int atacArma = EquipamentManager.Instance.getAtac(arma);
        return atacArma;
    }

    public int getDefensaEquipament()
    {
        int defensaArma = EquipamentManager.Instance.getDefensa(arma);
        return defensaArma;
    }
}
