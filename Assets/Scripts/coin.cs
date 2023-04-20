using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coin : MonoBehaviour
{
    private Rigidbody rigidbody;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        Destroy(gameObject, 4f);
    }

    void Update()
    {
        rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, Vector3.zero, 0.2f);
    }

    public void Ricochet()
    {
        Debug.Log("hit");
    }
}
