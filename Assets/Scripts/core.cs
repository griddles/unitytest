using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class core : MonoBehaviour
{
    public int trailLength;
    public LayerMask hitLayer;

    public float damage;
    public float radius;
    public float force;

    public GameObject parent;

    public ParticleSystem explosion;

    private LineRenderer trail;
    private Vector3 lastFrame;

    private bool dead = false;

    void Start()
    {
        trail = GetComponent<LineRenderer>();
        lastFrame = transform.position;
    }

    private void FixedUpdate()
    {
        trail.SetPosition(0, transform.position);

        // if the number of linerenderer points is less than trailLength, add a new one to the end at lastFrame
        if (trail.positionCount < trailLength)
        {
            trail.positionCount++;
            trail.SetPosition(trail.positionCount - 1, lastFrame);
        }
        else
        {
            // if the number of linerenderer points is equal to trailLength, shift all points down one and add a new one to the end at lastFrame
            for (int i = 0; i < trailLength - 1; i++)
            {
                trail.SetPosition(i, trail.GetPosition(i + 1));
            }
            trail.SetPosition(trailLength - 1, lastFrame);
        }

        // check if there's an object in the way of the bullet (collision detection at home) (is actually way better than unity's built in collision detection)
        RaycastHit2D hit = Physics2D.Linecast(lastFrame, transform.position, hitLayer);
        if (hit)
        {
            Collision(hit.collider, hit.point);
        }

        lastFrame = transform.position;
    }

    void Collision(Collider2D collision, Vector3 position)
    {
        // explode
        if (!dead)
        {
            explosion = Instantiate(explosion, position, transform.rotation);
            Destroy(explosion.gameObject, 2f);

            // get a list of all gameobjects within the explosion radius
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);
            // loop through all gameobjects in the list and try to get either a player or enemy component
            foreach (Collider2D collider in colliders)
            {
                playerMovement player = collider.gameObject.GetComponent<playerMovement>();
                enemy enemy = collider.gameObject.GetComponent<enemy>();

                // get the direction from the explosion to the gameobject
                Vector2 direction = collider.transform.position - position;

                // get the distance from the explosion to the gameobject
                float distance = Vector2.Distance(collider.transform.position, position);
                if (distance < 2) // ensures direct impacts deal full damage and knockback
                {
                    distance = 0;
                }

                // if the gameobject has a player or enemy component, deal damage to it
                if (player != null)
                {
                    player.Damage(direction, force * ((radius - distance) / radius), damage * ((radius - distance) / radius) * 10);
                }
                else if (enemy != null)
                {
                    enemy.Damage(direction, force * ((radius - distance) / radius), damage * ((radius - distance) / radius));
                }
            }
        }

        dead = true;
        Destroy(gameObject, 0.02f);
    }
}
