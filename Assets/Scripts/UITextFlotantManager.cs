using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITextFlotantManager : MonoBehaviour
{
    private void onAnimationFinish()
    {
        Object.Destroy(this.transform.parent.gameObject);
    }

    private void onAnimationFinishDisable()
    {
        gameObject.SetActive(false);
    }
}
