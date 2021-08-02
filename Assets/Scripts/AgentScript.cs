using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentScript : MonoBehaviour
{
    // SERIALIZED VARIABLES
    [SerializeField] private float speed = 2f;
    private bool loadingPath = false;


    // PRIVATE VARIABLES
    private Vector2 cellSpacing;
    private NavMeshAgent agent;
    private IEnumerator varModificaAgentPath;
    private IEnumerator varMoveFromTo;
    private bool potCanviarTarget;
    private bool esperantCanviarTarget;
    private Vector3 actualTarget;

    private Transform mesh = null;
    [SerializeField] private bool rotacioInvertida = false;
    private float rotacioMesh = 180f;
    private Animator animator = null;

    private float tempsEntreTarget = .2f;
    private float tempsTransTarget = 0;

    // Start is called before the first frame update
    void Start()
    {
        tempsTransTarget = tempsEntreTarget;

        cellSpacing = WorldManager.Instance.getCellSpacing();
        
        potCanviarTarget = true;
        esperantCanviarTarget = false;
        actualTarget = transform.position;
        varModificaAgentPath = null;
        varMoveFromTo = null;

        mesh = transform.Find("Mesh");
        animator = mesh.GetComponent<Animator>();
        rotacioMesh = mesh.localRotation.eulerAngles.y;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        if (tempsTransTarget < tempsEntreTarget)
        {
            tempsTransTarget += Time.deltaTime;
        }
    }

    public void setTarget(Vector2 position)
    {
        if (tempsTransTarget >= tempsEntreTarget)
        {
            tempsTransTarget = 0;
            
            // transformar posicio
            position = WorldManager.Instance.positionToCellSpace(position, transform.tag);
            if (WorldManager.Instance.esPosicioValidaPersonatge(position))
            {
                Vector3 newTarget = WorldManager.Instance.roundPos(new Vector3(position.x, position.y, transform.position.z));
                if (newTarget != actualTarget)
                {
                    actualTarget = newTarget;
                    reiniciaAgentDestination();
                    agent.SetDestination(actualTarget);
                    agent.isStopped = true;
                    
                    if (varModificaAgentPath != null) 
                    {
                        StopCoroutine( varModificaAgentPath );
                        varModificaAgentPath = null;
                    }

                    if (varMoveFromTo == null) 
                    {
                        if (animator != null) animator.SetBool("Run", false);
                    }
                    
                    varModificaAgentPath = modificaAgentPath();
                    StartCoroutine( varModificaAgentPath );
                }
            }
        }
    }

    public string getTag()
    {
        return transform.tag;
    }

    private void reiniciaAgentDestination()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    public Vector3 getTarget()
    {
        return actualTarget;
    }

    private IEnumerator modificaAgentPath() 
    {
        int stopper = 100;
        int iStopper = 0;
        esperantCanviarTarget = true;
        while ((!potCanviarTarget || agent.path.corners.Length < 2) && iStopper < stopper)
        {
            iStopper++;
            yield return 0;
        }
        esperantCanviarTarget = false;

        if (agent.path.corners.Length >= 2)
        {
            loadingPath = true;
            potCanviarTarget = false;
            List<Vector3> cornersList = new List<Vector3>(agent.path.corners);
            cornersList.Add(agent.destination);
            Vector3[] corners = cornersList.ToArray();
            reiniciaAgentDestination();
            
            // per cada parella de corners corner[x], corner[x+1] desplaçar-se de cel·la en cel·la en direcció al corner[x+1]
            //  cal intentar agafar les cel·les que toquin la "linia" entre corners
            //   per saber si toca la "linia", podem mirar la distancia de la cel·la a la linia
            List<Vector3> celes = new List<Vector3>();
            bool errorDiagonal = false; // Si en more en diagonal no podem
            bool errorCostat = false;
            bool errorCami = false;
            for (int i = 0; i < corners.Length-2; i++)
            {
                Vector3 corner1 = WorldManager.Instance.roundPos( WorldManager.Instance.positionToCellSpace(corners[i], transform.tag) ); // Agafem el corner[x] i el discretitzem
                if (i == 0) celes.Add(corner1); // Afegir corner1 al inici

                Vector3 corner2 = WorldManager.Instance.roundPos( WorldManager.Instance.positionToCellSpace(corners[i+1], transform.tag) ); // Agafem el seguent corner i el discretitzem

                // Viatjar de corner1 a corner2 afegint noves cel·les
                Vector3 cornerDesp = corner1;
                errorCami = false;
                while (Vector3.Distance(cornerDesp, corner2) > .8f && !errorCami)
                {
                    Vector2 direccio = new Vector2();
                    Vector2 distancia = WorldManager.Instance.distanceCells(cornerDesp, corner2); // La distancia està en cel·les
                    
                    if (distancia.x >= 0) direccio.x = -cellSpacing.x;
                    else direccio.x = cellSpacing.x;
                    if (distancia.y >= 0) direccio.y = -cellSpacing.y;
                    else direccio.y = cellSpacing.y;

                    // sempre que es pot avançar en diagonal es fa en diagonal
                    // en cas que no es pugui, provar costats
                    Vector2 cornerTmp = cornerDesp;
                    if (!errorDiagonal && (Mathf.Abs(distancia.x) == Mathf.Abs(distancia.y) || errorCostat)) // DIAGONAL
                    {
                        // Diagonal = 1,1; -1,1; 1,-1; -1,-1
                        cornerTmp.x += direccio.x;
                        cornerTmp.y += direccio.y;
                        if (!WorldManager.Instance.esPosicioValidaPersonatge(cornerTmp))
                        {
                            Vector2 cornerXTmp = cornerTmp;
                            cornerXTmp.x += direccio.x;
                            cornerXTmp.y -= direccio.y;

                            Vector2 cornerYTmp = cornerTmp;
                            cornerYTmp.x -= direccio.x;
                            cornerYTmp.y += direccio.y;
                            
                            if (WorldManager.Instance.esPosicioValidaPersonatge(cornerXTmp) && WorldManager.Instance.esPosicioValidaPersonatge(cornerYTmp))
                            {
                                Vector2 distanciaXTmp = WorldManager.Instance.distanceCells(cornerXTmp, corner2);
                                int distanciaX = (int) Mathf.Abs(distanciaXTmp.x) + (int) Mathf.Abs(distanciaXTmp.y);
                                Vector2 distanciaYTmp = WorldManager.Instance.distanceCells(cornerXTmp, corner2);
                                int distanciaY = (int) Mathf.Abs(distanciaYTmp.x) + (int) Mathf.Abs(distanciaYTmp.y);

                                if (distanciaX < distanciaY) cornerTmp = cornerXTmp;
                                else cornerTmp = cornerYTmp;
                            }
                            else if (WorldManager.Instance.esPosicioValidaPersonatge(cornerXTmp))
                            {
                                cornerTmp = cornerXTmp;
                            }
                            else if (WorldManager.Instance.esPosicioValidaPersonatge(cornerYTmp))
                            {
                                cornerTmp = cornerYTmp;
                            }
                        }
                        
                        if (WorldManager.Instance.esPosicioValidaPersonatge(cornerTmp) && !celes.Contains(WorldManager.Instance.roundPos(cornerTmp)))
                        {
                            cornerDesp = WorldManager.Instance.roundPos(cornerTmp);
                            celes.Add(cornerDesp);
                            errorCostat = false;
                            errorDiagonal = false;
                        }
                        else
                        {
                            errorDiagonal = true;
                        }
                    }
                    else // COSTAT
                    {
                        // Costat = 1,0; -1,0; 0,1; 0,-1

                        if (Mathf.Abs(distancia.x) > Mathf.Abs(distancia.y)) // Ens movem en X
                        {
                            Vector2 cornerXTmp = cornerTmp;
                            cornerXTmp.x += direccio.x;
                            if (!WorldManager.Instance.esPosicioValidaPersonatge(cornerXTmp)) // Si X no és correcte, provem amb Y
                            {
                                cornerXTmp = cornerTmp;
                                cornerXTmp.y += direccio.y;
                                if (!WorldManager.Instance.esPosicioValidaPersonatge(cornerXTmp))
                                {
                                    cornerXTmp = cornerTmp;
                                    cornerXTmp.y -= direccio.y;
                                }
                            }

                            cornerTmp = cornerXTmp;
                        }
                        else // Ens movem en Y
                        {
                            Vector2 cornerYTmp = cornerTmp;
                            cornerYTmp.y += direccio.y;
                            if (!WorldManager.Instance.esPosicioValidaPersonatge(cornerYTmp)) // Si Y no és correcte, provem amb X
                            {
                                cornerYTmp = cornerTmp;
                                cornerYTmp.x += direccio.x;
                                if (!WorldManager.Instance.esPosicioValidaPersonatge(cornerYTmp))
                                {
                                    cornerYTmp = cornerTmp;
                                    cornerYTmp.x -= direccio.x;
                                }
                            }

                            cornerTmp = cornerYTmp;
                        }

                        if (WorldManager.Instance.esPosicioValidaPersonatge(cornerTmp) && !celes.Contains(WorldManager.Instance.roundPos(cornerTmp)))
                        {
                            cornerDesp = WorldManager.Instance.roundPos(cornerTmp);
                            celes.Add(cornerDesp);
                            errorCostat = false;
                            errorDiagonal = false;
                        }
                        else
                        {
                            errorCostat = true;
                        }
                    }

                    if (errorDiagonal && errorCostat)
                    {
                        celes = new List<Vector3>();
                        errorCami = true;
                    }
                }
                
                // Afegir corner2 al final
                if (!celes.Contains(corner2)) celes.Add(corner2);
                if (errorCami) 
                {
                    varModificaAgentPath = null;
                    yield break;
                }
            }

            if (celes.Count > 0) celes.Remove(celes[0]);

            if (varMoveFromTo != null) 
            {
                StopCoroutine( varMoveFromTo );
                varMoveFromTo = null;
                if (animator != null) animator.SetBool("Run", false);
            }

            varModificaAgentPath = null;
            varMoveFromTo = MoveFromTo(celes);
            StartCoroutine( varMoveFromTo );
        }
        else 
        {
            reiniciaAgentDestination();
            setTarget(transform.position);
        }

        loadingPath = false;
        if (varMoveFromTo == null) 
        {
            if (animator != null) animator.SetBool("Run", false);
        }
    }

    private IEnumerator MoveFromTo(List<Vector3> bPoints) 
    {
        if (animator != null) animator.SetBool("Run", true);

        Vector2 a = transform.position;
        float z = transform.position.z;
        potCanviarTarget = false;
        foreach (Vector3 b in bPoints)
        {
            Vector2 distancia = WorldManager.Instance.distanceCells(a, b);

            // Aplicar rotacio al personatge
            setRotation(distancia);

            // Per asegurar un bon funcionament pel moviment pel mon, cal fer comprovacions per no repetir un mateix path.
            if ((int) Mathf.Abs(distancia.x) + (int) Mathf.Abs(distancia.y) > 2 || Mathf.Abs(Vector2.Distance(bPoints[bPoints.Count-1], transform.position)) < .5f) break;
            else if (Vector3.Distance(a,b) <= .5f) break;

            float step = (speed / (a - (Vector2) b).magnitude) * Time.fixedDeltaTime;
            float t = 0;
            while (t <= 1.0f) 
            {
                t += step;                                  // Goes from 0 to 1, incrementing by step each time
                transform.position = WorldManager.Instance.roundPos(Vector3.Lerp(a, b, t)); // Move transform closer to b
                transform.position.Set(transform.position.x, transform.position.y, z);
                yield return new WaitForFixedUpdate();      // Leave the routine and return here in the next frame
            }
            a = b;
            transform.position = WorldManager.Instance.roundPos(new Vector3(b.x, b.y, z));
            if (esperantCanviarTarget) break;
        }

        actualTarget = transform.position;
        potCanviarTarget = true;
        varMoveFromTo = null;

        if (animator != null && !loadingPath) animator.SetBool("Run", false);
    }

    public void setRotation(Vector2 orientacio)
    {
        if (mesh != null)
        {
            float rotacio = WorldManager.Instance.getRotacioAccioPersonatge(orientacio);
            rotacio = Mathf.Round(rotacio - rotacioMesh);
            rotacio = Mathf.Round(rotacio / 45) * 45;

            if (rotacioInvertida) rotacio += 180;

            mesh.Rotate(0, rotacio, 0, Space.Self);
            rotacioMesh += rotacio;
            rotacioMesh = Mathf.Round(rotacioMesh / 45) * 45;
        }
    }
}