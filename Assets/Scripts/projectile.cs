using UnityEngine;
using UnityEngine.Rendering;

public class projectile : MonoBehaviour
{
    public int trailLength;
    public LayerMask hitLayer;

    public float damage;
    public float lifetime;

    public GameObject parent;

    private LineRenderer trail;
    private Vector3 lastFrame;

    private bool dead = false;

    void Start()
    {
        trail = GetComponent<LineRenderer>();
        lastFrame = transform.position;
        Destroy(gameObject, lifetime);
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
        if (hit.collider != null && !dead)
        {
            Collision(hit.collider);
        }

        lastFrame = transform.position;
    }

    private void Collision(Collider2D collision)
    {
        if (parent != null)
        {
            if (collision.gameObject.tag == parent.tag)
            {
                return;
            }
        }
        // check if the object's parent has an enemy script on it
        if (collision.gameObject.GetComponentInParent<enemy>() != null)
        {

            // get the current magnitude and direction of the bullet's velocity
            float magnitude = GetComponent<Rigidbody2D>().velocity.magnitude;
            Vector2 direction = GetComponent<Rigidbody2D>().velocity.normalized;
            // apply knockback to the enemy
            collision.gameObject.GetComponent<enemy>().Damage(direction, magnitude / 8, damage);
        }
        // check if the object has a playermovement script, and damage the player
        else if (collision.gameObject.GetComponent<playerMovement>() != null)
        {
            // get the direction that the bullet is travelling in
            Vector2 direction = GetComponent<Rigidbody2D>().velocity.normalized;
            collision.gameObject.GetComponent<playerMovement>().Damage(direction, 2, damage * 10);
        }

        dead = true;
        Destroy(gameObject, 0.02f);
    }
}
