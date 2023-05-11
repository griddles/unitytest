using UnityEngine;

public class altPierce : MonoBehaviour
{
    public Transform muzzlePoint;
    public LayerMask wallLayer;
    public LayerMask hitLayer;
    public GameObject trail;

    public float damage;

    private gunManager gunManager;

    void Start()
    {
        gunManager = GetComponentInParent<gunManager>();
    }

    void Update()
    {
        // if right click
        if (Input.GetMouseButtonDown(1) && gunManager.currentPierceCooldown <= 0)
        {
            // raycast from muzzlePoint to relative forward
            RaycastHit2D hit = Physics2D.Raycast(muzzlePoint.position, muzzlePoint.forward, Mathf.Infinity, wallLayer);
            // if it hits a coin with tag "Coin :)", ultraricochet
            if (hit.collider.gameObject.tag == "Coin :)")
            {
                hit.collider.gameObject.GetComponent<coin>().UltraRicochet(damage);
            }
            // create a trail
            GameObject newTrail = Instantiate(trail);
            newTrail.GetComponent<trail>().Trail(muzzlePoint.position, hit.point, 6);
            // cooldown
            gunManager.currentPierceCooldown = gunManager.pierceCooldown;

            // linecast from muzzlePoint to hit.point
            RaycastHit2D[] lineHit = Physics2D.LinecastAll(muzzlePoint.position, hit.point, hitLayer);

            // deal damage to the enemy component of all hit objects
            foreach (RaycastHit2D line in lineHit)
            {
                if (line.collider.gameObject.GetComponent<enemy>() != null)
                {
                    line.collider.gameObject.GetComponent<enemy>().Damage(Vector3.forward, 12, damage);
                }
            }
        }
    }
}
