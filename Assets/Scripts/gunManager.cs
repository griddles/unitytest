using System;
using System.Collections.Generic;
using UnityEngine;

public class gunManager : MonoBehaviour
{
    public Camera camera;
    public List<KeyCode> inputKeys;
    public List<GameObject> guns;

    private GameObject currentGun;

    void Start()
    {
        currentGun = guns[0];
        currentGun = Instantiate(currentGun, transform.position, transform.rotation, transform);
        camera = Camera.main;
    }

    void Update()
    {
        // brute forcing yeah
        foreach (KeyCode keyCode in inputKeys)
        {
            if (Input.GetKeyDown(keyCode))
            {
                SwitchGun(guns[inputKeys.IndexOf(keyCode)]);
            }
        }
    }

    void FixedUpdate()
    {
        // rotate to point at the mouse
        Vector3 mousePos = Input.mousePosition;
        mousePos = camera.ScreenToWorldPoint(mousePos);
        Vector2 direction = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
        transform.up = direction;

        // handle Z coordinates so that the gun overlaps with other sprites properly
        if (transform.up.y > 0)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0.5f);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -0.5f);
        }

        // change the gun sprite depending on which direction it's pointing
        if (Math.Abs(direction.x) > Math.Abs(direction.y))
        {
            currentGun.GetComponent<SpriteRenderer>().sprite = currentGun.GetComponent<gun>().sideSprite;
            if (direction.x > 0)
            {
                // facing right, sprite needs to be mirrored
                currentGun.GetComponent<SpriteRenderer>().flipX = true;
            }
            else
            {
                // facing left, no mirroring required
                currentGun.GetComponent<SpriteRenderer>().flipX = false;
            }
        }
        else
        {
            // the gun is facing up or down, both directions work with the same sprite flipping
            currentGun.GetComponent<SpriteRenderer>().sprite = currentGun.GetComponent<gun>().normalSprite;
            currentGun.GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    private void SwitchGun(GameObject newGun)
    {
        // delete the current gun and instantiate the new gun
        Destroy(currentGun);
        currentGun = Instantiate(newGun, transform.position, transform.rotation, transform);
    }

}
