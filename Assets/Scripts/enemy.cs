using NUnit.Framework.Internal.Commands;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class enemy : MonoBehaviour
{
    public bool debug; // logs damage

    public int awareness; // interval in ticks between each position check
    public float sightRange;
    public float allyRange;

    public float speed;

    public LayerMask sightLayer;

    public ParticleSystem blood;
    public ParticleSystem death;

    public float maxHealth;

    public float contactDamage;

    public bool ranged;
    public GameObject projectile;
    public float projectileSpeed;
    public float projectileCooldown;
    public float burstCount;
    public float burstSpread;
    public float burstInterval;
    public float animationTime; // in seconds

    private int nearbyAllies;

    private int time;
    private int lastCheck;

    private playerMovement player;
    private Vector3 playerPos;
    private Vector3 currentMovement;
    private Vector3 knockback;

    private Rigidbody2D rigidbody;
    private Animator animator;

    private float currentProjectileCooldown;

    private float health;

    private int direction;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<playerMovement>();
        rigidbody = GetComponent<Rigidbody2D>();
        health = maxHealth;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (time - lastCheck > awareness)
        {
            lastCheck = time;
            currentMovement = Tick();
        }

        rigidbody.velocity = currentMovement + knockback;
        knockback = Vector3.Lerp(knockback, Vector3.zero, 0.1f);

        time += 1;
    }

    private Vector3 Tick() // enemy update script
    {
        currentProjectileCooldown -= 1;

        bool playerSeen = false;

        // if the player is in LOS and view distance, get their current position
        if (player != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position, Mathf.Infinity, sightLayer);
            // check to see if the raycast actually hit
            if (hit.collider != null)
            {
                Debug.Log(hit.collider.name);
                if (hit.collider.tag == "Player")
                {
                    playerPos = hit.transform.position;
                    playerSeen = true;
                    // get the direction in degrees to the player
                    float rawDirection = Mathf.Atan2(playerPos.y - transform.position.y, playerPos.x - transform.position.x) * Mathf.Rad2Deg;
                    // up = 0, right = 1, down = 2, left = 3
                    // -45 to 45 = right, -45 to -135 = down, 135 to -135 = left, 135 to 45 = up
                    if (rawDirection > -45 && rawDirection < 45)
                    {
                        direction = 1;
                    }
                    else if (rawDirection < -45 && rawDirection > -135)
                    {
                        direction = 2;
                    }
                    else if (rawDirection > 135 || rawDirection < -135)
                    {
                        direction = 3;
                    }
                    else if (rawDirection > 45 && rawDirection < 135)
                    {
                        direction = 0;
                    }
                    

                    animator.SetInteger("direction", direction);
                }
            }

            // look for nearby allies
            nearbyAllies = 0;

            GameObject[] allies = GameObject.FindGameObjectsWithTag("Enemy");

            // loop through enemies and check the distance against allyRange
            foreach (GameObject ally in allies)
            {
                // if the enemy is within range, add to nearbyAllies
                if (Vector3.Distance(transform.position, ally.transform.position) < allyRange)
                {
                    nearbyAllies += 1;
                }
            }

            Vector3 movement;

            // decide between moving and shooting, if the enemy is ranged
            int shootChance = Random.Range(0, 1);

            if (shootChance == 0 && ranged && playerSeen && currentProjectileCooldown <= 0) 
            {
                // shoot at the player
                StartCoroutine(ShootBurst());

                //GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.identity);
                //newProjectile.GetComponent<projectile>().parent = gameObject;
                //Vector3 direction = playerPos - transform.position;
                //direction.Normalize();
                //newProjectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;

                currentProjectileCooldown = projectileCooldown;

                movement = Vector3.zero;
            }
            else
            {
                // if the player hasnt been seen, only have a 25% chance of moving

                int movementChance = Random.Range(0, 101);

                if (movementChance > 25 || playerSeen)
                {
                    // get the raw direction of the player
                    Vector2 direction = playerPos - transform.position;
                    // normalize the direction
                    direction.Normalize();
                    if (nearbyAllies == 1) // (1 means that only this enemy is nearby)
                    {
                        movement = direction * speed;
                    }
                    else if (nearbyAllies <= 5)
                    {
                        int chance = Random.Range(0, 101);
                        if (chance < 15 * (nearbyAllies - 1))
                        {
                            int directionChance = Random.Range(1, 3);
                            if (directionChance == 1)
                            {
                                Vector2 rightVector = new Vector2(direction.y, -direction.x);
                                direction += rightVector;
                            }
                            else
                            {
                                Vector2 leftVector = new Vector2(-direction.y, direction.x);
                                direction += leftVector;
                            }
                        }
                        // move the enemy in the direction of the player
                        movement = direction * speed;
                    }
                    else
                    {
                        // get a random number between 0 and 100
                        int chance = Random.Range(0, 101);
                        // if the number is less than 50, move diagonally left or right
                        if (chance < 60)
                        {
                            int directionChance = Random.Range(1, 3);
                            if (directionChance == 1)
                            {
                                // add a left vector to the direction
                                direction += Vector2.left;
                            }
                            else
                            {
                                // add a right vector to the direction
                                direction += Vector2.right;
                            }
                        }
                        // move the enemy in the direction of the player
                        movement = direction * speed;
                    }
                }
                else
                {
                    movement = Vector3.zero;
                }
            }

            return movement;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public void Damage(Vector3 direction, float force, float value)
    {
        if (debug)
        {
            Debug.Log(value);
        }

        health -= value;

        knockback = direction * force;

        if (value > 0) // this function is used for knocking back enemies with no damage, so only spawn blood if the enemy is taking damage
        {
            // spawn blood particles in the direction of the knockback
            ParticleSystem bloodParticles = Instantiate(blood, new Vector3(transform.position.x, transform.position.y, 0.8f), Quaternion.identity);
            // get an angle from the direction vector
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // rotate the particles to face the direction
            bloodParticles.transform.rotation = Quaternion.Euler(angle, -90, 180);
            Destroy(bloodParticles.gameObject, 7);
        }

        if (health < 1)
        {
            ParticleSystem deathParticles = Instantiate(death, new Vector3(transform.position.x, transform.position.y, 0.8f), Quaternion.Euler(90, -90, 180));
            Destroy(deathParticles.gameObject, 7);
            // find the player object
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // find the distance to the player
                float distance = Vector3.Distance(transform.position, player.transform.position);
                // if the player is within 5 units, heal them by value times 30
                if (distance < 5)
                {
                    player.GetComponent<playerMovement>().Heal(value * 30);
                }
            }

            Destroy(gameObject);
        }
    }

    IEnumerator ShootBurst()
    {
        animator.SetTrigger("attack");
        yield return new WaitForSeconds(animationTime);

        // if there is a burst interval, shoot projectiles in rapid succession in a straight line
        if (burstInterval > 0)
        {
            // loop through the burst size
            for (int i = 0; i < burstCount; i++)
            {
                // shoot a projectile
                GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.identity);
                newProjectile.GetComponent<projectile>().parent = gameObject;
                Vector3 direction = playerPos - transform.position;
                direction.Normalize();
                newProjectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;
                // wait for the cooldown
                yield return new WaitForSeconds(burstInterval);
            }
        }
        // otherwise, shoot burstCount projectiles simultaneously
        else
        {
            for (int i = 0; i < burstCount; i++)
            {
                // shoot a projectile
                GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.identity);
                newProjectile.GetComponent<projectile>().parent = gameObject;
                Vector3 direction = playerPos - transform.position;
                direction.Normalize();
                // rotate the direction by the burstSpread
                direction = Quaternion.Euler(0, 0, burstSpread * i - (burstSpread * (burstCount - 1) / 2)) * direction;
                newProjectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;
            }
        }
    }
}
