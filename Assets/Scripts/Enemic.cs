using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemic : MonoBehaviour
{
    // Variables Lluita
    [SerializeField] private string nom = "";
    [SerializeField] private bool boss = false;
    private AtributsPersonatge atributs;
    private bool actiu = false;
    private bool enMoviment = false;
    private Transform mesh = null;
    private AgentScript agent = null;
    private Animator animator = null;

    // Variables atributs (s'utilitzen per iniciar els atributs de Player)
    [SerializeField] private float speed = 2f;
    [SerializeField] private int nivell = 1;
    [SerializeField] private ulong experiencia = 2; // Experiencia que guanya el player en matar el mob (experiencia * nivell)
    [SerializeField] private int monedes = 1;
    [SerializeField] private int vida = 5; // (vida * nivell)
    [SerializeField] private int atacEnemic = 5;
    [SerializeField] private int defensaEnemic = 10;
    [SerializeField] private int mana = 4;
    [SerializeField] private int moviments = 3;
    [SerializeField] private char tipus = 'A'; // 'A' Agressiu, 'P' Passiu
    private float tempsEntreAccions = 2f; // L'enemic esperarà 2 segons entre accions
    private float tempsAccioAnterior = 0f;

    [SerializeField] private string[] atacs;
    private Dictionary<Vector2, List<Vector2>> movimentsValids;
    IEnumerator varDesenvolupaTorn;

    // Start is called before the first frame update
    void Start()
    {
        reiniciarMoviments();
        iniciaAtributs();
        varDesenvolupaTorn = null;
        finalitzaTorn();

        mesh = transform.Find("Mesh");
        agent = transform.GetComponent<AgentScript>();
        animator = mesh.GetComponent<Animator>();
    }

    public void setNivell(int lvl)
    {
        nivell = lvl;
    }

    public void iniciaAtributs()
    {
        atributs = new AtributsPersonatge (nivell, experiencia, vida, mana, moviments);
    }

    public void iniciaLluita()
    {
        tempsAccioAnterior = tempsEntreAccions / 2;
        actiu = false;
        enMoviment = false;
        atributs.reiniciaAtributsLluita();
        reiniciarMoviments();
    }

    public void iniciaTorn()
    {
        actiu = true;
        tempsAccioAnterior = 0;

        varDesenvolupaTorn = desenvolupaTorn();
        StartCoroutine( varDesenvolupaTorn );
    }

    public void finalitzaTorn()
    {
        actiu = false;
        atributs.reiniciaAtributsTorn();
        reiniciarMoviments();

        if (varDesenvolupaTorn != null) StopCoroutine( varDesenvolupaTorn );
    }

    private void reiniciarMoviments()
    {
        movimentsValids = new Dictionary<Vector2, List<Vector2>>();
    }

    private IEnumerator desenvolupaTorn() 
    {
        while (actiu)
        {
            if (!enMoviment)
            {
                tempsAccioAnterior += Time.deltaTime;
                if (tempsAccioAnterior >= tempsEntreAccions)
                {
                    if (!haPogutAtacar())
                    {
                        if (atributs.getMovimentsActual() > 0) // Si queden moviments:
                        { // El moviment pot dependre del tipus enemic (passiu, agresiu)
                            if (movimentsValids.Count < 1 && atributs.getMovimentsActual() > 0) buscaMovimentsPossibles();
                            List<Vector2> posicionsValides = new List<Vector2>(movimentsValids.Keys);

                            if (tipus == 'A' || (tipus == 'P' && quedaManaPerAtacar()))
                            {
                                if (!moveCloseToPlayer(posicionsValides)) 
                                {
                                    PartidaManager.Instance.finalitzarTornActual();
                                }
                            }
                            else if (tipus == 'P' && !quedaManaPerAtacar())
                            {
                                if (!moveFarToPlayer(posicionsValides)) 
                                {
                                    PartidaManager.Instance.finalitzarTornActual();
                                }
                            }
                        }
                        else if (atributs.getMovimentsActual() <= 0)
                        {
                            PartidaManager.Instance.finalitzarTornActual();
                        }
                    }

                    tempsAccioAnterior = 0;
                }
            }

            yield return 0;      // Leave the routine and return here in the next frame
        }

        varDesenvolupaTorn = null;
    }

    public bool getEnMoviment()
    {
        return enMoviment;
    }

    private void buscaMovimentsPossibles()
    {
        movimentsValids = PartidaManager.Instance.buscaMovimentsPersonatge(new Vector2(transform.position.x, transform.position.y), atributs.getMovimentsActual());
    }

    private bool moveCloseToPlayer(List<Vector2> posicionsValides)
    { // moviments minims on pugui atacar o la distancia sigui menor
        Vector2 moveTo = new Vector2(transform.position.x, transform.position.y);
        int moveToPotAtacar = -1;
        if (PartidaManager.Instance.distanceToPlayer(moveTo) > 1)
        {
            Vector2 minDist = moveTo; // Distancia minima al player
            bool minDistIsSet = false;

            Vector2 minVisio = moveTo; // Distancia minima amb linia de visió
            bool minVisioIsSet = false;

            Vector2 maxCel = moveTo; // Maximes celes desplaçades
            bool maxCelIsSet = false;

            foreach (Vector2 pos in posicionsValides)
            {
                int potAtacarAct = potAtacar(pos);
                if (potAtacarAct != -1) // pos és valida
                {
                    if (moveToPotAtacar == -1 || (potAtacarAct < moveToPotAtacar) || (potAtacarAct == moveToPotAtacar && celesDesplacades(pos) < celesDesplacades(moveTo))) 
                    {
                        moveToPotAtacar = potAtacarAct;
                        moveTo = pos;
                    }
                }
                else if (moveToPotAtacar == -1)
                {
                    // Comparem La distancia minima al Player
                    if (PartidaManager.Instance.distanceToPlayer(pos) < PartidaManager.Instance.distanceToPlayer(minDist))
                    {
                        minDist = pos;
                        minDistIsSet = true;
                    }
                    else if (PartidaManager.Instance.distanceToPlayer(pos) == PartidaManager.Instance.distanceToPlayer(minDist) && celesDesplacades(pos) < celesDesplacades(minDist))
                    {
                        minDist = pos;
                        minDistIsSet = true;
                    }

                    // Comparem La distancia minima amb linia de visió al Player
                    if (PartidaManager.Instance.liniaDeVisioToPlayer(pos, "Enemic"))
                    {
                        if (!PartidaManager.Instance.liniaDeVisioToPlayer(minVisio, "Enemic"))
                        {
                            minVisio = pos;
                            minVisioIsSet = true;
                        }
                        else if (PartidaManager.Instance.distanceToPlayer(pos) < PartidaManager.Instance.distanceToPlayer(minVisio))
                        {
                            minVisio = pos;
                            minVisioIsSet = true;
                        }
                        else if (PartidaManager.Instance.distanceToPlayer(pos) == PartidaManager.Instance.distanceToPlayer(minVisio) && celesDesplacades(pos) < celesDesplacades(minVisio))
                        {
                            minVisio = pos;
                            minVisioIsSet = true;
                        }
                    }

                    // Comparem Les maximes celes desplaçades amb distancia menor al Player
                    if (celesDesplacades(pos) > celesDesplacades(maxCel))
                    {
                        maxCel = pos;
                        maxCelIsSet = true;
                    }
                    else if (celesDesplacades(pos) == celesDesplacades(maxCel) && PartidaManager.Instance.distanceToPlayer(pos) < PartidaManager.Instance.distanceToPlayer(maxCel))
                    {
                        maxCel = pos;
                        maxCelIsSet = true;
                    }
                }
            }

            if (moveToPotAtacar == -1)
            {
                if (minDistIsSet && PartidaManager.Instance.distanceToPlayer(minDist) <= 1) moveTo = minDist;
                else if (maxCelIsSet && PartidaManager.Instance.liniaDeVisioToPlayer(maxCel, "Enemic")) moveTo = maxCel;
                else if (minVisioIsSet)
                {
                    if (maxCelIsSet && PartidaManager.Instance.distanceTo(maxCel, minVisio) <= 3) moveTo = maxCel;
                    else if (minDistIsSet && PartidaManager.Instance.distanceTo(minDist, minVisio) <= 3) moveTo = minDist;
                    else if (maxCelIsSet) moveTo = maxCel;
                    else if (minDistIsSet) moveTo = minDist;
                    else moveTo = minVisio;
                }
                else if (maxCelIsSet) moveTo = maxCel;
                else if (minDistIsSet) moveTo = minDist;
            }

            if (moveTo != new Vector2(transform.position.x, transform.position.y))
            {
                int moviments = getCamiTo(moveTo).Count;
                atributs.aplicaMoviment(moviments);
                PartidaManager.Instance.mostraUIWorldObjectByType("Moviments", new Vector3(transform.position.x, transform.position.y, -0.01f), moviments.ToString());
                StartCoroutine( MoveFromTo(getCamiTo(moveTo)) );
                reiniciarMoviments();
                return true;
            }
        }

        return false;
    }

    private bool moveFarToPlayer(List<Vector2> posicionsValides)
    { // moviments minims on es trovi més lluny del player
        Vector2 moveTo = new Vector2(transform.position.x, transform.position.y);
        foreach (Vector2 pos in posicionsValides)
        {
            if (PartidaManager.Instance.distanceToPlayer(pos) > PartidaManager.Instance.distanceToPlayer(moveTo)) moveTo = pos;
            else if (PartidaManager.Instance.distanceToPlayer(pos) == PartidaManager.Instance.distanceToPlayer(moveTo) && celesDesplacades(pos) < celesDesplacades(moveTo)) moveTo = pos;
        }

        if (moveTo != new Vector2(transform.position.x, transform.position.y))
        {
            int moviments = getCamiTo(moveTo).Count;
            atributs.aplicaMoviment(moviments);
            PartidaManager.Instance.mostraUIWorldObjectByType("Moviments", new Vector3(transform.position.x, transform.position.y, -0.01f), moviments.ToString());
            StartCoroutine( MoveFromTo(getCamiTo(moveTo)) );
            reiniciarMoviments();
            return true;
        }
        
        return false;
    }

    public void takeEmputxarTo(Vector2 emputxarTo)
    {
        StartCoroutine( MoveFromTo(emputxarTo) );
        reiniciarMoviments();
    }

    public List<Vector2> getCamiTo(Vector2 pos)
    {
        if (movimentsValids.ContainsKey(pos)) return movimentsValids[pos];
        else return new List<Vector2>();
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

    private void setRotation(Vector2 orientacio)
    {
        if (mesh != null)
        {
            agent.setRotation(orientacio);
        }
    }

    private bool haPogutAtacar()
    {
        bool potAtacar = false;
        int i = 0;
        while (!potAtacar && i < atacs.Length) // atacs ordenats per preferencia
        {
            string atac = atacs[i];

            Vector2 posAtac = new Vector2();
            List<Vector2> areaAtac = new List<Vector2>();
            int valorAtac = 0;

            bool continuar = false;
            if (atributs.getManaActual() >= AtacManager.Instance.getManaAtac(atac) && (AtacManager.Instance.getEstatNecessari(atac) == 0 || atributs.getEstat(AtacManager.Instance.getEstatNecessari(atac)) > 0)) // si queda mana per fer l'atac i tenim l'estat necessari
            {
                char atacTipusSpec = AtacManager.Instance.getAtacTipusSpec(atac);
                // Si atac és P i proporciona un estat pero ja el tenim, no fem l'atac
                continuar = true;
                if (atacTipusSpec == 'P')
                {
                    Dictionary<int, int> habilitatsEspecials = AtacManager.Instance.getHabilitatsEspecials(atac);
                    if (habilitatsEspecials.Count > 0)
                    {
                        continuar = false;
                        foreach (KeyValuePair<int, int> habilitatEsp in habilitatsEspecials)
                        {
                            if (atributs.getEstat(habilitatEsp.Key) == 0)
                            {
                                continuar = true;
                                break;
                            }
                        }
                    }

                    // Si ens proporciona un estat Self pero ja el tenim no ataquem
                    if (continuar)
                    {
                        Dictionary<int, int> habilitatsEspecialsSelf = AtacManager.Instance.getHabilitatsEspecialsSelf(atac);
                        if (habilitatsEspecialsSelf.Count > 0)
                        {
                            foreach (KeyValuePair<int, int> habilitatEspSelf in habilitatsEspecialsSelf)
                            {
                                if (atributs.getEstat(habilitatEspSelf.Key) != 0)
                                {
                                    continuar = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                
                if (continuar)
                {
                    // mirem si pot atacar
                    List<Vector2> posicionsValides = AtacManager.Instance.buscaRangAtacPersonatge(new Vector2(transform.position.x, transform.position.y), atac);
                    
                    foreach(Vector2 pos in posicionsValides)
                    {
                        if (atacTipusSpec == 'A') // Si és un Atac o una habilitat negativa
                        {
                            if (PartidaManager.Instance.isPlayerIn(pos)) // si hi ha player a pos ataquem
                            {
                                // agafem la area d'atac
                                List<Vector2> posicionsArea = AtacManager.Instance.buscaRangAreaAtacPersonatge(new Vector2(pos.x, pos.y), atac);
                                
                                int valorAtacActual = valorRecomptePersonatges(AtacManager.Instance.buscaPersonatgesAreaAtac(posicionsArea));
                                if (!potAtacar || valorAtacActual > valorAtac)// si és l'atac més optim
                                {
                                    posAtac = pos;
                                    areaAtac = posicionsArea;
                                    valorAtac = valorAtacActual;
                                }

                                potAtacar = true;
                            }
                            else if (PartidaManager.Instance.distanceToPlayer(pos) >= AtacManager.Instance.getAreaAtacMin(atac) && PartidaManager.Instance.distanceToPlayer(pos) <= AtacManager.Instance.getAreaAtacMax(atac)) // si player està a distancia de la area d'atac
                            {
                                // mirem si pot atacar
                                List<Vector2> posicionsArea = AtacManager.Instance.buscaRangAreaAtacPersonatge(new Vector2(pos.x, pos.y), atac);
                                
                                if (PartidaManager.Instance.isPlayerIn(posicionsArea)) // podem atacar
                                {
                                    int valorAtacActual = valorRecomptePersonatges(AtacManager.Instance.buscaPersonatgesAreaAtac(posicionsArea));
                                    if (!potAtacar || valorAtacActual > valorAtac) // si és l'atac més optim
                                    {
                                        posAtac = pos;
                                        areaAtac = posicionsArea;
                                        valorAtac = valorAtacActual;
                                    }

                                    potAtacar = true;
                                }
                            }
                        }
                        else if (atacTipusSpec == 'H') // Si és una curació
                        {
                            // Busca l'aliat amb menys vida
                            if (Vector2.Distance(pos, transform.position) < 0.01)
                            {
                                potAtacar = true;
                            }
                            else if (PartidaManager.Instance.isTagIn(pos, transform.tag)) // si hi ha player a pos ataquem
                            {
                                potAtacar = true;
                            }

                            if (potAtacar) 
                            {
                                List<Vector2> posicionsArea = AtacManager.Instance.buscaRangAreaAtacPersonatge(new Vector2(pos.x, pos.y), atac);
                                posAtac = pos;
                                areaAtac = posicionsArea;
                                break; // de moment cura al primer que troba
                            }
                        }
                        else if (atacTipusSpec == 'P') // Si és una habilitat positiva
                        {
                            // Se la tira a sobre o a un aliat
                            if (Vector2.Distance(pos, transform.position) < 0.01)
                            {
                                potAtacar = true;
                            }
                            else if (PartidaManager.Instance.isTagIn(pos, transform.tag)) // si hi ha player a pos ataquem
                            {
                                potAtacar = true;
                            }
                            
                            if (potAtacar) 
                            {
                                List<Vector2> posicionsArea = AtacManager.Instance.buscaRangAreaAtacPersonatge(new Vector2(pos.x, pos.y), atac);
                                posAtac = pos;
                                areaAtac = posicionsArea;
                                break;
                            }
                        }
                    }
                }
            }

            if (potAtacar)
            {
                Vector2 distancia = WorldManager.Instance.distanceCells(transform.position, posAtac);
                // Aplicar rotacio al personatge
                setRotation(distancia);

                int mana = AtacManager.Instance.getManaAtac(atac);
                atributs.aplicaMana(mana);
                PartidaManager.Instance.mostraUIWorldObjectByType("Mana", new Vector3(transform.position.x, transform.position.y, -0.01f), mana.ToString());

                // ataca a la pos !!
                int damage = AtacManager.Instance.getDamageAtac(atac, atributs.getNivell()); // agafem el "damage" base de l'atac
                damage += (damage * atacEnemic / 100); // Sumem el damage de l'atac
                if (damage > 0) 
                {
                    if (animator != null) animator.SetTrigger("Atac Complex");
                    dealDamageTo(damage, areaAtac);
                    PartidaManager.Instance.checkFinish();
                }

                // Cura a la pos
                int heal = AtacManager.Instance.getHealAtac(atac, atributs.getNivell()); // agafem el "heal" base de l'atac
                if (heal > 0) 
                {
                    if (animator != null) animator.SetTrigger("Atac Complex");
                    healTo(heal, areaAtac);
                }

                // Habilitats especials
                Dictionary <int, int> habilitatsEspecials = AtacManager.Instance.getHabilitatsEspecials(atac);
                if (habilitatsEspecials.Count > 0) 
                {
                    if (animator != null) animator.SetTrigger("Atac Simple");
                    aplicaHabilitatsTo(habilitatsEspecials, areaAtac);
                }

                // Habilitats especials Self
                Dictionary <int, int> habilitatsEspecialsSelf = AtacManager.Instance.getHabilitatsEspecialsSelf(atac);
                if (habilitatsEspecialsSelf.Count > 0) 
                {
                    aplicaHabilitatsSelfTo(habilitatsEspecialsSelf);
                }

                mostraEfecteAccio(posAtac, atac);
            }
            else if (continuar) // Si no pot atacar, pero si ens movem pot atacar retornem false per fer moure al personatge
            {
                if (movimentsValids.Count < 1 && atributs.getMovimentsActual() > 0) buscaMovimentsPossibles();
                List<Vector2> posicionsValides = new List<Vector2>(movimentsValids.Keys);
                
                if (moveCanAtack(posicionsValides, atac)) 
                {
                    return false;
                }
            }

            i++;
        }

        return potAtacar;
    }

    private void mostraEfecteAccio(Vector3 posicio, string atac)
    {
        AtacManager.Instance.mostraEfecteAtac(atac, posicio);
    }

    private bool moveCanAtack(List<Vector2> posicionsValides, string atac)
    { // Si es mou podria atacar amb "atac"
        Vector2 moveTo = new Vector2(transform.position.x, transform.position.y);
        foreach (Vector2 pos in posicionsValides)
        {
            if (potAtacar(pos, atac)) return true;
        }
        return false;
    }

    private bool quedaManaPerAtacar()
    {
        foreach (string atac in atacs) // atacs ordenats per preferencia
        {
            if (atributs.getManaActual() >= AtacManager.Instance.getManaAtac(atac)) // si queda mana per fer l'atac
            {
                return true;
            }
        }

        return false;
    }

    private int potAtacar(Vector2 posicio)
    {
        int idAtac = 0;
        foreach (string atac in atacs) // atacs ordenats per preferencia
        {
            if (atributs.getManaActual() >= AtacManager.Instance.getManaAtac(atac) && (AtacManager.Instance.getEstatNecessari(atac) == 0 || atributs.getEstat(AtacManager.Instance.getEstatNecessari(atac)) > 0)) // si queda mana per fer l'atac
            {
                var continuar = true;
                // Si ens proporciona un estat Self pero ja el tenim no ataquem
                if (continuar)
                {
                    Dictionary<int, int> habilitatsEspecialsSelf = AtacManager.Instance.getHabilitatsEspecialsSelf(atac);
                    if (habilitatsEspecialsSelf.Count > 0)
                    {
                        foreach (KeyValuePair<int, int> habilitatEspSelf in habilitatsEspecialsSelf)
                        {
                            if (atributs.getEstat(habilitatEspSelf.Key) != 0)
                            {
                                continuar = false;
                                break;
                            }
                        }
                    }
                }

                if (continuar)
                {
                    // mirem si pot atacar
                    List<Vector2> posicionsValides = AtacManager.Instance.buscaRangAtacPersonatge(new Vector2(posicio.x, posicio.y), atac);
                    
                    foreach(Vector2 pos in posicionsValides)
                    {
                        if (PartidaManager.Instance.isPlayerIn(pos)) // si hi ha player a pos avisem que es pot atacar
                        {
                            return idAtac;
                        }
                        else if (PartidaManager.Instance.distanceToPlayer(pos) >= AtacManager.Instance.getAreaAtacMin(atac) && PartidaManager.Instance.distanceToPlayer(pos) <= AtacManager.Instance.getAreaAtacMax(atac)) // si player està a distancia de la area d'atac
                        {
                            // mirem si pot atacar
                            List<Vector2> posicionsArea = AtacManager.Instance.buscaRangAreaAtacPersonatge(new Vector2(pos.x, pos.y), atac);
                            if (PartidaManager.Instance.isPlayerIn(posicionsArea)) // podem atacar
                            {
                                return idAtac;
                            }
                        }
                    }
                }
            }

            idAtac++;
        }

        return -1;
    }

    private bool potAtacar(Vector2 posicio, string atac)
    {
        if (atributs.getManaActual() >= AtacManager.Instance.getManaAtac(atac) && (AtacManager.Instance.getEstatNecessari(atac) == 0 || atributs.getEstat(AtacManager.Instance.getEstatNecessari(atac)) > 0)) // si queda mana per fer l'atac
        {
            // mirem si pot atacar
            List<Vector2> posicionsValides = AtacManager.Instance.buscaRangAtacPersonatge(new Vector2(posicio.x, posicio.y), atac);
            
            foreach(Vector2 pos in posicionsValides)
            {
                if (PartidaManager.Instance.isPlayerIn(pos)) // si hi ha player a pos avisem que es pot atacar
                {
                    return true;
                }
                else if (PartidaManager.Instance.distanceToPlayer(pos) >= AtacManager.Instance.getAreaAtacMin(atac) && PartidaManager.Instance.distanceToPlayer(pos) <= AtacManager.Instance.getAreaAtacMax(atac)) // si player està a distancia de la area d'atac
                {
                    // mirem si pot atacar
                    List<Vector2> posicionsArea = AtacManager.Instance.buscaRangAreaAtacPersonatge(new Vector2(pos.x, pos.y), atac);
                    if (PartidaManager.Instance.isPlayerIn(posicionsArea)) // podem atacar
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void dealDamageTo(int damage, List<Vector2> posicionsArea)
    {
        foreach (Vector2 posArea in posicionsArea)
        {
            PartidaManager.Instance.dealDamageTo(damage, posArea);
        }
    }

    private void healTo(int heal, List<Vector2> posicionsArea)
    {
        foreach (Vector2 posArea in posicionsArea)
        {
            PartidaManager.Instance.healTo(heal, posArea);
        }
    }

    private void aplicaHabilitatsTo(Dictionary <int, int> habilitatsEspecials, List<Vector2> posicionsArea)
    {
        foreach (Vector2 posArea in posicionsArea)
        {
            PartidaManager.Instance.aplicaHabilitatsTo(habilitatsEspecials, new Vector2(transform.position.x, transform.position.y), posArea);
        }
    }

    private void aplicaHabilitatsSelfTo(Dictionary <int, int> habilitatsEspecialsSelf)
    {
        PartidaManager.Instance.aplicaHabilitatsTo(habilitatsEspecialsSelf, new Vector2(transform.position.x, transform.position.y), new Vector2(transform.position.x, transform.position.y));
    }

    public ulong getExperiencia()
    {
        return atributs.getExperiencia();
    }

    public int getNivell()
    {
        return atributs.getNivell();
    }

    public int takeDamage(int damage)
    {
        // RECALCULAR damage amb el % de defensa
        damage -= (damage * defensaEnemic / 100);
        if (damage < 0) damage = 0;

        int damaged = 0;
        damaged = atributs.takeDamage(damage);
        Debug.Log("Enemic: Vida actual "+atributs.getVidaActual());
        
        if (atributs.getVidaActual() <= 0)
        {
            if (animator != null) animator.SetTrigger("Die");
            else onAnimationFinish("Die");
            Debug.Log("Enemic: MORT");
        }
        else if (animator != null) animator.SetTrigger("Get Hit");

        return damaged;
    }

    public void onAnimationFinish(string descAnimacio)
    {
        if (descAnimacio == "Die") gameObject.SetActive(false);
    }

    public int takeHeal(int heal)
    {
        int healed = 0;
        healed = atributs.takeHeal(heal);
        Debug.Log("Enemic: Vida actual "+atributs.getVidaActual());
        return healed;
    }

    public bool isAlive()
    {
        return atributs.getVidaActual() > 0;
    }

    public int getVidaActual()
    {
        return atributs.getVidaActual();
    }

    public int getVidaTotal()
    {
        return atributs.getVida();
    }

    public int getManaActual()
    {
        return atributs.getManaActual();
    }

    public int getMovimentsActual()
    {
        return atributs.getMovimentsActual();
    }

    public string getNom()
    {
        return nom;
    }

    public bool isBoss()
    {
        return boss;
    }

    public void takeRestaMana(int mana)
    {
        atributs.aplicaMana(mana);
        PartidaManager.Instance.mostraUIWorldObjectByType("Mana", new Vector3(transform.position.x, transform.position.y, -0.01f), mana.ToString());
        Debug.Log("Enemic: Resta mana "+mana);
    }

    public void takeRestaPassos(int moviments)
    {
        atributs.aplicaMoviment(moviments);
        PartidaManager.Instance.mostraUIWorldObjectByType("Moviments", new Vector3(transform.position.x, transform.position.y, -0.01f), moviments.ToString());
        reiniciarMoviments();
        Debug.Log("Enemic: Resta moviments "+moviments);
    }

    public void takeEstat(int idEstat, int tornsEstat)
    {
        atributs.setEstat(idEstat, tornsEstat);
        Debug.Log("Enemic: Nou estat "+idEstat+" durant "+tornsEstat+" torns");
    }

    private int celesDesplacades(Vector2 newPos)
    {
        Vector2 pos = WorldManager.Instance.roundPos(newPos);
        if (movimentsValids.ContainsKey(pos)) return movimentsValids[pos].Count;
        else return 0;
        //Vector2 cellSpacing = WorldManager.Instance.getCellSpacing();
        //return (int) Mathf.Max( Mathf.Abs( newPos.x - transform.position.x ) / cellSpacing.x, Mathf.Abs( newPos.y - transform.position.y ) / cellSpacing.y );
    }

    private int valorRecomptePersonatges(List<GameObject> personatges)
    {
        int valor = 0;
        foreach(GameObject personatge in personatges)
        {
            switch (personatge.tag)
            {
                case "Personatge":
                    valor++;
                break;
                case "Enemic":
                    valor--;
                break;
            }
        }
        return valor;
    }

    public int getMonedes()
    {
        return monedes;
    }
}
