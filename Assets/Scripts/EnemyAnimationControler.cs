using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationControler : MonoBehaviour
{
    Enemic enemic;

    void Start ()
    {
        enemic = transform.parent.GetComponent<Enemic>();
    }

    public void onAnimationDieFinish()
    {
        enemic.onAnimationFinish("Die");
    }
}
