using System.Runtime.CompilerServices;
using UnityEngine;

public class gun : MonoBehaviour
{
    public bool hitscan;
    public GameObject bulletOrTrail;
    public Transform muzzlePoint;
    public Sprite normalSprite;
    public Sprite sideSprite;
    public float muzzleVelocity;
    public float rateOfFire; // in ticks between shots
    public float bullets;
    public float spread;
    public float recoil;
    public LayerMask wallLayer;
    public ParticleSystem muzzleSmoke;

    private GameObject bullet;
    private LineRenderer trail;
    private bool shootInput;
    private float cooldown;

    void Start()
    {
        if (hitscan)
        {
            trail = bulletOrTrail.GetComponent<LineRenderer>();
        }
        else
        {
            bullet = bulletOrTrail;
        }
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
            Shoot();
            cooldown = rateOfFire;
            shootInput = false;
        }

        cooldown -= 1;
    }

    private void Shoot()
    {
        Vector2 direction = new Vector2(transform.up.x, transform.up.y);

        // get a position behind the muzzlepoint
        Vector3 behind = muzzlePoint.position - 0.5f * transform.up;

        if (!hitscan)
        {
            for (int i = 0; i < bullets; i++)
            {
                float angle = Random.Range(-spread, spread);
                direction = Quaternion.Euler(0, 0, angle) * direction;
                GameObject newBullet = Instantiate(bullet, behind, Quaternion.identity);
                newBullet.GetComponent<Rigidbody2D>().velocity = direction * muzzleVelocity;

                Destroy(newBullet, 2f);
            }
        }
        else
        {
            // fire a raycast out of the muzzlepoint that hits everything
            RaycastHit2D hit = Physics2D.Raycast(muzzlePoint.position, transform.up, Mathf.Infinity, wallLayer);
            if (hit.collider != null)
            {
                trail.SetPosition(0, muzzlePoint.position);
                trail.SetPosition(1, hit.point);
            }
            else
            {
                // get a point 50 units in the pointing direction and enable the trail, then set the two trail points accordingly
                Vector3 point = muzzlePoint.position + 50 * transform.up;
                trail.SetPosition(0, muzzlePoint.position);
                trail.SetPosition(1, point);
            }
        }

        // set the velocity value in the parent of the parent to the opposite of the bullet's velocity to simulate recoil
        transform.parent.parent.GetComponent<playerMovement>().velocity = -direction * muzzleVelocity * recoil;
    }
}
