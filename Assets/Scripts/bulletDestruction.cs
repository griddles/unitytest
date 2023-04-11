using UnityEngine;

public class bulletDestruction : MonoBehaviour
{
    public int trailLength;
    public LayerMask layer;

    private LineRenderer trail;
    private Vector3 lastFrame;

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

        lastFrame = transform.position;
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        // check if the collision is in the layermask
        if (layer == (layer | (1 << collision.gameObject.layer)))
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll; 
            Destroy(gameObject, 0.03f); // allows the trail to fade out a little
        }
    }
}
