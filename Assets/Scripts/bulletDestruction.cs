using UnityEngine;

public class bulletDestruction : MonoBehaviour
{
    public int trailLength;
    public LayerMask hitLayer;

    public float damage;

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

        // check if there's an object in the way of the bullet (collision detection at home)
        RaycastHit2D hit = Physics2D.Linecast(lastFrame, transform.position, hitLayer);
        if (hit.collider != null && !dead)
        {
            Collision(hit.collider);
        }

        lastFrame = transform.position;
    }

    private void Collision(Collider2D collision)
    {
        // check if the object's parent has an enemy script on it
        if (collision.gameObject.GetComponentInParent<enemy>() != null)
        {
            // get the current magnitude and direction of the bullet's velocity
            float magnitude = GetComponent<Rigidbody2D>().velocity.magnitude;
            Vector2 direction = GetComponent<Rigidbody2D>().velocity.normalized;
            // apply knockback to the enemy
            collision.gameObject.GetComponent<enemy>().Damage(direction, magnitude / 8, damage);
        }
        dead = true;
        Destroy(gameObject, 0.02f);
    }
}
