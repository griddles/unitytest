using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class coin : MonoBehaviour
{
    public LayerMask coinLOSCheck;
    public LayerMask ricochetCheck;
    public LayerMask enemyLOSCheck;
    public GameObject bullet;

    public bool dead = false;

    private playerMovement player;
    private trail bulletTrail;

    void Start()
    {
        GameObject trail = Instantiate(bullet);
        bulletTrail = trail.GetComponent<trail>();

        player = GameObject.Find("Player").GetComponent<playerMovement>();
    }

    void Update()
    {
        // if velocity magnitude is ever less than 0.5, kill the coin
        if (GetComponent<Rigidbody2D>().velocity.magnitude < 2f)
        {
            dead = true;
            Destroy(gameObject);
        }
    }

    public void Ricochet(float damage)
    {
        if (dead)
        {
            return;
        }

        // find all other objects with tag "Coin :)"
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin :)");
        // remove this coin from the list
        List<GameObject> coinsList = new List<GameObject>(coins);
        coinsList.Remove(gameObject);
        // check if each coin is dead, if so remove it from the list
        for (int i = 0; i < coins.Length; i++)
        {
            if (coins[i].GetComponent<coin>().dead)
            {
                coinsList.Remove(coins[i]);
            }
        }
        coins = coinsList.ToArray();
        // sort the list by distance to each coin, closest first
        System.Array.Sort(coins, delegate (GameObject a, GameObject b)
        {
            return Vector3.Distance(a.transform.position, transform.position).CompareTo(Vector3.Distance(b.transform.position, transform.position));
        });
        // go through each coin and find the first one that is in LOS
        foreach (GameObject coin in coins) 
        {
            // use a raycast to see if it's in LOS
            RaycastHit2D hit = Physics2D.Raycast(transform.position, coin.transform.position - transform.position, Mathf.Infinity, coinLOSCheck);
            // if the raycast hits an object with tag "Coin :)", set the linerenderer positions to this coin and the other one
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.CompareTag("Coin :)"))
                {
                    dead = true;
                    bulletTrail.Trail(transform.position, coin.transform.position, 2f);
                    coin.GetComponent<coin>().Ricochet(damage * 1.2f);
                    Destroy(gameObject);
                    return;
                }
            }
        }

        // run the same check, but for enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // sort the list by distance to each enemy, closest first
        System.Array.Sort(enemies, delegate (GameObject a, GameObject b)
        {
            return Vector3.Distance(a.transform.position, transform.position).CompareTo(Vector3.Distance(b.transform.position, transform.position));
        });
        // go through each enemy and find the first one that is in LOS
        foreach (GameObject enemy in enemies)
        {
            // use a raycast to see if it's in LOS
            RaycastHit2D hit = Physics2D.Raycast(transform.position, enemy.transform.position - transform.position, Mathf.Infinity, enemyLOSCheck);
            // if the raycast hits an object with tag "Enemy", set the linerenderer positions to this coin and the other one
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.CompareTag("Enemy"))
                {
                    dead = true;
                    player.TimeStop(0.075f);
                    bulletTrail.Trail(transform.position, enemy.transform.position, 2f);
                    // damage the enemy
                    Vector2 direction = hit.normal;
                    hit.collider.gameObject.GetComponent<enemy>().Damage(-direction, damage, damage);
                    // destroy the coin
                    Destroy(gameObject);
                    return;
                }
            }
        }

        if (!dead)
        {
            dead = true;
            // if there aren't any coins in LOS, pick a random angle and fire a raycast in that direction
            float angle = Random.Range(-180, 180);
            Vector2 spread = Quaternion.Euler(0, 0, angle) * transform.up;
            RaycastHit2D hit2 = Physics2D.Raycast(transform.position, spread, Mathf.Infinity, ricochetCheck);
            if (hit2.collider != null)
            {
                bulletTrail.Trail(transform.position, hit2.point, 2f);
            } // if no collision, set the second position to 100 units away
            else
            {
                bulletTrail.Trail(transform.position, transform.position + (Vector3)spread * 50, 2f);
            }
            Destroy(gameObject);
            return;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
