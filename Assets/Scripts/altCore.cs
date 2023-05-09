using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class altCore : MonoBehaviour
{
    public GameObject core;
    public float coreForce;

    private gunManager gunManager;

    private void Start()
    {
        gunManager = gameObject.GetComponentInParent<gunManager>();
    }

    void Update()
    {
        // if the player right clicks, spawn a core at the player and give it coreForce towards the mouse
        if (Input.GetMouseButtonDown(1) && gunManager.currentCoreCooldown <= 0)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = mousePos - transform.position;
            GameObject newCore = Instantiate(core, transform.position, Quaternion.identity);
            // get the player
            GameObject player = GameObject.FindWithTag("Player");
            newCore.GetComponent<Rigidbody2D>().velocity = (coreForce * new Vector2(direction.x, direction.y)) + player.GetComponent<Rigidbody2D>().velocity;

            gunManager.currentCoreCooldown = gunManager.coreCooldown;
        }
    }
}
