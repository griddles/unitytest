using NUnit.Framework;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class gun : MonoBehaviour
{
    public bool hitscan;
    public GameObject bullet;
    public Transform muzzlePoint;
    public Sprite normalSprite;
    public Sprite sideSprite;
    public float muzzleVelocity;
    public float damage;
    public float rateOfFire; // in ticks between shots
    public float bullets;
    public float spread;
    public float recoil;
    public LayerMask hitLayer;
    public ParticleSystem muzzleSmoke;
    public GameObject coin;
    public float coinForce;
    public AudioClip shootSound;

    private bool shootInput;
    private float cooldown;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && cooldown <= 0)
        {
            shootInput = true;
        }
    }

    void FixedUpdate()
    {
        if (shootInput && cooldown <= 0)
        {
            muzzleSmoke.Play();
            audioSource.PlayOneShot(shootSound);
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
                newBullet.GetComponent<projectile>().damage = damage;
                newBullet.GetComponent<projectile>().parent = gameObject;
                newBullet.GetComponent<Rigidbody2D>().velocity = spread * muzzleVelocity;
            }
        }
        else
        {
            // fire a raycast out of the muzzlepoint
            RaycastHit2D hit = Physics2D.Raycast(muzzlePoint.position, transform.up, Mathf.Infinity, hitLayer);
            if (hit.collider != null)
            {
                AddTrail(muzzlePoint.position, new Vector3(hit.point.x, hit.point.y, 0), 2);

                if (hit.collider.gameObject.GetComponentInParent<enemy>() != null)
                {
                    Vector2 direction = hit.normal;
                    // apply knockback to the enemy
                    hit.collider.gameObject.GetComponent<enemy>().Damage(-direction, muzzleVelocity / 8, damage);
                }
                // otherwise, if the raycast hit an object with tag "Coin :)", call the coin's ricochet function
                else if (hit.collider.gameObject.CompareTag("Coin :)"))
                {
                    hit.collider.gameObject.GetComponent<coin>().Ricochet(damage * 1.2f);
                }
            }
            else
            {
                // if it doesn't hit anything, just draw a 50 unit line so it still looks good
                Vector3 point = muzzlePoint.position + 50 * transform.up;

                AddTrail(muzzlePoint.position, point, 2);
            }
        }

        // set the velocity value in the parent of the parent to the opposite of the bullet's velocity to simulate recoil
        transform.parent.parent.GetComponent<playerMovement>().velocity = -spread * muzzleVelocity * recoil;
    }

    private void AddTrail(Vector3 pointA, Vector3 pointB, float duration)
    {
        GameObject trail = Instantiate(bullet, Vector3.zero, Quaternion.identity);
        trail bulletTrail = trail.GetComponent<trail>();
        bulletTrail.Trail(pointA, pointB, duration);
    }
}
