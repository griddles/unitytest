using System;
using Unity.VisualScripting;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("Assets")]
    public Rigidbody2D rigidbody;
    public RectTransform dashMeterSprite;
    public RectTransform healthMeterSprite;
    public Camera camera;
    public LineRenderer grappleLine;
    public Animator animator;

    [Header("Movement Settings")]
    public float speed;
    public float movementSnapping;

    [Header("Dash Settings")]
    public float dashCooldown; // in physics ticks
    public float dashChargeSpeed; // in units per tick
    public float dashCost;
    public float dashMeter;
    public float dashPower;

    [Header("Grapple Settings")] 
    public float grappleRange;
    public float grappleForce;
    public float grappleDecay;

    [Header("Health Settings")]
    public float maxHealth;
    public float damageMultiplier;

    [Header("Camera Settings")] 
    public float minCamSize;
    public float maxCamSize;

    [Header("Other")]
    public LayerMask wallLayer;

    // input
    private float xInput;
    private float yInput;
    private bool dashInput;
    private bool grappleInput;
    // movement vectors
    private Vector2 movement;
    private Vector2 dash;
    private Vector2 grapple;
    [HideInInspector]
    public Vector2 velocity;
    // dash cooldown
    private float dashDelay;
    private float currentDashMeter;
    // health
    private float currentHealth;
    // grapple settings
    private bool grappling;
    private int facing; // N:0 E:1 S:2 W:3

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        currentDashMeter = dashMeter;
        currentHealth = maxHealth;
        camera = Camera.main;

        // find object with tag CameraController
        Cinemachine.CinemachineVirtualCamera cameraController = GameObject.FindGameObjectWithTag("CameraController").GetComponent<Cinemachine.CinemachineVirtualCamera>();
        cameraController.Follow = transform;

    }

    void Update() // get all input (this runs every frame so it'll always detect input)
    {
        // get input
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");
        dashInput = Input.GetKey(KeyCode.LeftShift);
        // this makes sure that the grapple input is always detected, but still functions as an event rather than a continuous input
        // also keeps the grapple calculations tied to the physics engine
        if (Input.GetKeyDown(KeyCode.E))
        {
            grappleInput = true;
        }
    }

    private void FixedUpdate() // do anything that needs to be consistent between computers (this runs every physics tick so it'll always run at the same speed)
    {
        // add X input to movement
        if (xInput > 0.1 || xInput < -0.1)
        {
            movement.x = xInput * speed;
        }
        // add Y input to movement
        if (yInput > 0.1 || yInput < -0.1)
        {
            movement.y = yInput * speed;
        }

        // if the player is moving slowly and there is no input, stop the player (prevents slow drifting)
        if ((rigidbody.velocity.x < movementSnapping || rigidbody.velocity.x < -movementSnapping) && xInput == 0)
        {
            movement.x = 0;
        }
        if ((rigidbody.velocity.y < movementSnapping || rigidbody.velocity.y < -movementSnapping) && yInput == 0)
        {
            movement.y = 0;
        }

        // handles dashing
        if (dashDelay == 0 && currentDashMeter > dashCost && dashInput)
        {
            dash = new Vector2(xInput * speed * 2, yInput * speed * 2);
            dashDelay = dashCooldown;
            currentDashMeter -= dashCost;
        }

        // handles dash cooldown
        if (dashDelay > 0)
        {
            dashDelay--;
        }

        // handles dash meter
        if (currentDashMeter <= dashMeter)
        {
            currentDashMeter += dashChargeSpeed;
        }
        else
        {
            currentDashMeter = dashMeter;
        }

        // get the direction of the player's mouse and set the facingDirection accordingly
        Vector2 direction = camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        if (Math.Abs(direction.x) > Math.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                facing = 1;
            }
            else
            {
                facing = 3;
            }
        }
        else
        {
            if (direction.y > 0)
            {
                facing = 0;
            }
            else
            {
                facing = 2;
            }
        }

        // shoot a ray from the center of the character through the mouse and set the line renderer's points to the hit point and the player
        if (grappleInput && !grappling)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, wallLayer);
            if (hit.collider != null)
            {
                // make the grappleLine extend a little bit into the object that it hits
                Vector2 grapplePoint = hit.point + -(hit.normal * 0.1f);
                grappleLine.SetPosition(1, grapplePoint);
            }

            grappleInput = false;
            grappling = true;
        }
        else if (grappleInput && grappling)
        {
            grappling = false;
            grappleInput = false;
        }

        // shoot a raycast from the player to the point that the grapple line is pointing to
        RaycastHit2D grappleHit = Physics2D.Raycast(transform.position, grappleLine.GetPosition(1) - transform.position, grappleRange, wallLayer);
        // if the raycast is shorter than the grapple by more than 2 units, reset the grapple line
        if (grappleHit.distance < Vector2.Distance(transform.position, grappleLine.GetPosition(1)) - 2)
        {
            grappling = false;
        }
        if (Vector2.Distance(transform.position, grappleLine.GetPosition(1)) > grappleRange * 1.5f)
        {
            grappling = false;
        }

        // the first point of the ray should always originate from the player
        grappleLine.SetPosition(0, transform.position);

        if (grappling)
        {
            // apply acceleration in the direction of the grapple line
            grapple += ((grappleLine.GetPosition(1) - transform.position).normalized * grappleForce).ConvertTo<Vector2>() * (1 - grapple.magnitude / 100);
        }
        else
        {
            // if we aren't grappling, the second point should also be in the player to hide the line renderer
            grappleLine.SetPosition(1, transform.position);
            // slowly lerp the grapple velocity down to zero
            grapple = Vector2.Lerp(grapple, Vector2.zero, grappleDecay);
            // if the player rigidbody is moving slowly, cancel all grapple velocity
            if (rigidbody.velocity.magnitude < movementSnapping)
            {
                // doing this only when the player isn't grappling allows for grapple catapults, which are unintended but cool, but still prevents movement lock against walls
                grapple = Vector2.zero;
            }
        }

        // applies all the different velocities seperately (allows much more control over physics based movement)
        rigidbody.velocity = velocity + dash + grapple + movement;

        // zoom out the camera based on how fast the player is moving
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, Math.Clamp(minCamSize * 0.9f + rigidbody.velocity.magnitude / 10, minCamSize, maxCamSize), 0.1f);

        // lerps velocities back to 0 (simulates friction and drag)
        dash = Vector2.Lerp(dash, Vector2.zero, dashPower);
        velocity = Vector2.Lerp(velocity, Vector2.zero, 0.3f);

        // handles dash meter sprite
        dashMeterSprite.localRotation = Quaternion.Euler(0, 0, 180 * (currentDashMeter / dashMeter));
        // handles health sprite
        healthMeterSprite.localRotation = Quaternion.Euler(0, 0, 180 - 180 * (currentHealth / maxHealth));
        // handles player sprite
        animator.SetInteger("direction", facing);
        if (rigidbody.velocity.magnitude > movementSnapping)
        {
            // set animation speed relative to player speed, but always greater than 1
            animator.speed = Math.Clamp(rigidbody.velocity.magnitude / 10, 1, Mathf.Infinity);
            animator.SetBool("moving", true);
        }
        else
        {
            animator.SetBool("moving", false);
        }
    }
}
