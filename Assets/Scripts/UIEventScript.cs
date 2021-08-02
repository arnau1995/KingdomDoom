using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEventScript : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = transform.GetComponent<Animator>();
    }
    public void setPointerOver(bool isOver)
    {
        animator.SetBool("PointerOver", isOver);
    }
}
