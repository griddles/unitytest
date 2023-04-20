using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class gun : MonoBehaviour
{
    public bool hitscan;
    public GameObject bullet;
    public trail trail;
    public Transform muzzlePoint;
    public Sprite normalSprite;
    public Sprite sideSprite;
    public float muzzleVelocity;
    public float rateOfFire; // in ticks between shots
    public float bullets;
    public float spread;
    public float recoil;
    public LayerMask hitLayer;
    public ParticleSystem muzzleSmoke;
    public GameObject coin;
    public float coinForce;

    private bool shootInput;
    private float cooldown;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && cooldown <= 0)
        {
            shootInput = true;
        }

        // if the player right clicks, spawn a coin at the player and give it coinForce towards the mouse
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = mousePos - transform.position;
            direction.Normalize();
            GameObject newCoin = Instantiate(coin, transform.position, Quaternion.identity);
            newCoin.GetComponent<Rigidbody2D>().velocity = coinForce * direction;
        }
    }

    void FixedUpdate()
    {
        Debug.Log(shootInput + " | " + cooldown);
        if (shootInput && cooldown <= 0)
        {
            muzzleSmoke.Play();
            Shoot();
            cooldown = rateOfFire;
            shootInput = false;
        }

        cooldown -= 1;
    }

    private void Shoot()
    {
        Vector2 spread = new Vector2(transform.up.x, transform.up.y);

        // get a position behind the muzzlepoint
        Vector3 behind = muzzlePoint.position - 0.5f * transform.up;

        if (!hitscan)
        {
            for (int i = 0; i < bullets; i++)
            {
                float angle = Random.Range(-this.spread, this.spread);
                spread = Quaternion.Euler(0, 0, angle) * spread;
                GameObject newBullet = Instantiate(bullet, behind, Quaternion.identity);
                newBullet.GetComponent<Rigidbody2D>().velocity = spread * muzzleVelocity;

                Destroy(newBullet, 2f);
            }
        }
        else
        {
            // fire a raycast out of the muzzlepoint
            RaycastHit2D hit = Physics2D.Raycast(muzzlePoint.position, transform.up, Mathf.Infinity, hitLayer);
            if (hit.collider != null)
            {
                AddTrail(muzzlePoint.position, hit.point, 2f);

                if (hit.collider.gameObject.GetComponentInParent<enemy>() != null)
                {
                    Vector2 direction = hit.normal;
                    // apply knockback to the enemy
                    hit.collider.gameObject.GetComponent<enemy>().Knockback(direction, muzzleVelocity / 8);
                }
                // otherwise, if the raycast hit an object with tag "Coin :)", call the coin's ricochet function
                else if (hit.collider.gameObject.CompareTag("Coin :)"))
                {
                    hit.collider.gameObject.GetComponent<coin>().Ricochet();
                }
            }
            else
            {
                // if it doesn't hit anything, just draw a 50 unit line so it still looks good
                Vector3 point = muzzlePoint.position + 50 * transform.up;
                AddTrail(muzzlePoint.position, point, 2f);
            }
        }

        // set the velocity value in the parent of the parent to the opposite of the bullet's velocity to simulate recoil
        transform.parent.parent.GetComponent<playerMovement>().velocity = -spread * muzzleVelocity * recoil;
    }

    private void AddTrail(Vector3 pointA, Vector3 pointB, float duration)
    {
        Debug.Log("a");
        GameObject bulletTrail = Instantiate(bullet);
        Debug.Log("b");
        trail trail = bulletTrail.GetComponent<trail>();
        Debug.Log("c");
        trail.Trail(pointA, pointB, duration);
        Debug.Log("d");
    }
}
