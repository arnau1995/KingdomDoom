using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    private Transform target = null;
    [SerializeField] private float speed = 2f;

    void Start()
    {
        transform.position = new Vector3(target.transform.position.x - 5, target.transform.position.y + 5, target.transform.position.z);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target != null) 
        {
            Vector3 tagretPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, tagretPosition, speed * Time.deltaTime);
        }
    }

    public void setTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
