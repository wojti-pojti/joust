using System.Collections;
using UnityEngine;

public class HorseMovement : MonoBehaviour
{
    [Header("Player-specific")]
    [SerializeField] int playerIndex;
    [SerializeField] KeyCode accelerateKeyCode;
    [SerializeField] KeyCode jumpKeyCode;

    [Header("Turn-specific")]
    public bool side; // F - left, T - right
    [SerializeField] float speed;
    [SerializeField] bool hasJumped;
    [SerializeField] bool isBraking;
    [SerializeField] bool isFleeing;

    [Header("Attributes")]
    [SerializeField] float forceAddedPerInput;
    [SerializeField] float jogForce;
    [SerializeField] float jumpForce;
    [SerializeField] float maxSpeed;

    [Header("")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] PlayerScript player;

    Vector2 movementDirection;
    bool tapConstraint; 
    [HideInInspector] public bool hasPassedTheOpponent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasPassedTheOpponent && player.state == PlayerState.COMBAT)
        {
            if (Input.GetKeyDown(accelerateKeyCode))
            {
                Accelerate();
                tapConstraint = true;
            }
            if (Input.GetKeyUp(accelerateKeyCode))
            {
                tapConstraint = false;
            }

            if (Input.GetKeyDown(jumpKeyCode) && !hasJumped)
            {
                Jump();
            }
        }
    }

    private void FixedUpdate()
    {
        if((tapConstraint || hasPassedTheOpponent) && player.state == PlayerState.COMBAT || isFleeing)
        {
            // while the user holds down accelerate button, small but constant force is applied continuously
            rb.AddForce(movementDirection * jogForce, ForceMode2D.Force);
        }

        if(isBraking)
        {
            // apply counter-force

            if(speed <= 0)
            {
                isBraking = false;

                // indicate the run has ended
                GameManager.Instance.InformOfReachingEndZone(playerIndex);
            }
        }

        speed = rb.linearVelocity.magnitude;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "EndZone")
        {
            Brake();
        }
    }

    /// <summary>
    /// The function called at the beginning of each match to pass the important information.
    /// </summary>
    /// <param name="side">The side of the scene, where that horse begins. False indicates left, True indicates right.</param>
    public void Setup(bool side)
    {
        playerIndex = (side ? 2 : 1);

        // set initial side
        this.side = side;
        if (side) { movementDirection = Vector2.left; }
        else { movementDirection = Vector2.right; }
        isBraking = false;
        isFleeing = false;

        // set correct inputs
        accelerateKeyCode = (side ? KeyCode.LeftArrow : KeyCode.D);
        jumpKeyCode = (side ? KeyCode.UpArrow : KeyCode.E);

        // setup color scheme too
    }

    /// <summary>
    /// Function called everytime the player taps the accelerate button. Adds a significant amount of force to the horse charge.
    /// </summary>
    void Accelerate()
    {
        if(speed >= maxSpeed) { return; }

        rb.AddForce(movementDirection * forceAddedPerInput, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Responsible for the horse jumping upon correct input.
    /// </summary>
    void Jump()
    {
        // relate it to speed somehow

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        hasJumped = true;

        // animation
    }

    /// <summary>
    /// This function begins the process of losing speed once the player reaches an end area of the arena.
    /// </summary>
    void Brake()
    {
        Debug.Log("Player " + playerIndex.ToString() + " begins braking.");
        isBraking = true;

        // animation
    }

    /// <summary>
    /// This function is to be called at the end of each run to adjust the horse for the next turn.
    /// </summary>
    public void TurnAround()
    {
        side = !side;
        if (side) { movementDirection = Vector2.left; }
        else { movementDirection = Vector2.right; }

        // the GameManager adjusts the scale to turn the sprite around

        hasJumped = false;
        hasPassedTheOpponent = false;
    }

    /// <summary>
    /// The public function calling the "Flee" coroutine.
    /// </summary>
    public void RunAway()
    {
        StartCoroutine(Flee());
    }

    /// <summary>
    /// Function that makes the horse run away and leave the camera view. Called upon player's death.
    /// </summary>
    /// <returns></returns>
    IEnumerator Flee()
    {
        isFleeing = true;
        Collider cd = this.GetComponent<Collider>();
        cd.enabled = false;
        float temp = rb.gravityScale;
        rb.gravityScale = 0;

        yield return new WaitForSeconds(8f);
        rb.gravityScale = temp;
        cd.enabled = true;
        isFleeing = false;
    }
}
