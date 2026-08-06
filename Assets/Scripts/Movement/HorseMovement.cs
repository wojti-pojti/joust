using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class HorseMovement : MonoBehaviour
{
    [Header("Player-specific")]
    [SerializeField] int playerIndex;
    [SerializeField] KeyCode accelerateKeyCode;
    [SerializeField] KeyCode jumpKeyCode;

    [Header("Turn-specific")]
    public bool side; // F - left, T - right
    public float totalAppliedForce;
    public float speed;
    [SerializeField] bool hasJumped;
    [SerializeField] bool isBraking;
    [SerializeField] bool isFleeing;

    [Header("Attributes")]
    [SerializeField] float forceAddedPerInput;
    [SerializeField] float jogForce;
    [SerializeField] float jumpForce;
    [SerializeField] float maxSpeed;
    [SerializeField] float idleTimeToStartAnimation;

    [Header("")]
    [SerializeField] Rigidbody2D rb; // Rigidbody2D of the player
    [SerializeField] PlayerScript player;
    [SerializeField] Animator animator;

    int speedFloat = Animator.StringToHash("Speed");
    int idleStompTrigger = Animator.StringToHash("IdleStomp");
    int landTrigger = Animator.StringToHash("Land");
    int jumpTrigger = Animator.StringToHash("Jump");
    int turnAroundTrigger = Animator.StringToHash("TurnAround");
    int isBrakingBool = Animator.StringToHash("IsBraking");

    Rigidbody2D horseRb;
    Vector2 movementDirection;
    SpriteRenderer spriteRenderer;
    bool tapConstraint; 
    [HideInInspector] public bool hasPassedTheOpponent;
    float idleAnimationTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasPassedTheOpponent && (player.state == PlayerState.COMBAT || player.state == PlayerState.SHIELD))
        {
            if (Input.GetKeyDown(accelerateKeyCode))
            {
                Accelerate();
                idleAnimationTimer = 0f;
                tapConstraint = true;
            }
            if (Input.GetKeyUp(accelerateKeyCode))
            {
                tapConstraint = false;
            }

            if (Input.GetKeyDown(jumpKeyCode) && !hasJumped)
            {
                Jump();
                idleAnimationTimer = 0f;
            }
        }
    }

    private void FixedUpdate()
    {
        if((tapConstraint || hasPassedTheOpponent) && (player.state == PlayerState.COMBAT || player.state == PlayerState.SHIELD))
        {
            // while the user holds down accelerate button, small but constant force is applied continuously
            Vector2 movement = movementDirection * jogForce;
            if (hasPassedTheOpponent) { movement *= 1.5f; }
            rb.AddForce(movement, ForceMode2D.Force);
            idleAnimationTimer = 0f;
        }

        if(isFleeing)
        {
            if (horseRb) horseRb.AddForce(movementDirection * jogForce * 2.25f, ForceMode2D.Force);
            idleAnimationTimer = 0f;
        }
        else if (isBraking)
        {
            // apply counter-force
            rb.AddForce(-movementDirection * 0.5f * jogForce, ForceMode2D.Force);

            if(speed <= 0)
            {
                isBraking = false;
                animator.SetBool(isBrakingBool, false);

                // indicate the run has ended
                GameManager.Instance.InformOfReachingEndZone(playerIndex);
            }
        }

        if (player.state == PlayerState.JUMP && player.transform.position.y < -0.5f && rb.linearVelocity.y < 0)
        {
            // land animation
            animator.SetTrigger(landTrigger);
        }

        if (idleAnimationTimer >= idleTimeToStartAnimation) 
        {
            animator.SetTrigger(idleStompTrigger);
            idleAnimationTimer = 0;
        }

        idleAnimationTimer += Time.fixedDeltaTime;
        speed = rb.linearVelocity.magnitude;
        animator.SetFloat(speedFloat, speed);
        player.UpdateLanceDamage(speed);

        //if(!isFleeing) totalAppliedForce = rb.totalForce.magnitude;
        //else { totalAppliedForce = horseRb.totalForce.magnitude; }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground" && player.state == PlayerState.JUMP)
        {
            player.state = PlayerState.COMBAT;
            GameManager.Instance.totalTimesJumped++;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (!side) Debug.Log("Player 1's horse collided with " + collision.gameObject.name);
        if ((!side && collision.gameObject.tag == "RightEndZone") ||
            (side && collision.gameObject.tag == "LeftEndZone"))
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
        if (this.side != side) { TurnAround(false); }

        rb.gravityScale = 1f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        playerIndex = (side ? 2 : 1);
        if (side) { idleAnimationTimer = 0.5f * idleTimeToStartAnimation; }

        // set initial side
        this.side = side;
        if (side) { movementDirection = Vector2.left; }
        else { movementDirection = Vector2.right; }
        isBraking = false;
        animator.SetBool(isBrakingBool, false);
        isFleeing = false;
        hasPassedTheOpponent = false;
        tapConstraint = false;

        // set correct inputs
        accelerateKeyCode = (side ? KeyCode.LeftArrow : KeyCode.D);
        jumpKeyCode = (side ? KeyCode.UpArrow : KeyCode.E);
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
        rb.AddForce(Vector2.up * (jumpForce + speed * 0.1f), ForceMode2D.Impulse);
        hasJumped = true;

        player.state = PlayerState.JUMP;

        // animation
        animator.SetTrigger(jumpTrigger);
    }

    /// <summary>
    /// This function begins the process of losing speed once the player reaches an end area of the arena.
    /// </summary>
    void Brake()
    {
        Debug.Log("Player " + playerIndex.ToString() + " begins braking.");
        isBraking = true;
        tapConstraint = false;

        // animation
        animator.SetBool(isBrakingBool, true);
    }

    /// <summary>
    /// This function is to be called at the end of each run to adjust the horse for the next turn.
    /// </summary>
    /// <param name="includeAnimation">Whether the turnaround should be instantenuous or include animation.</param>
    public void TurnAround(bool includeAnimation = true)
    {
        side = !side;
        if (side) { movementDirection = Vector2.left; }
        else { movementDirection = Vector2.right; }

        if(includeAnimation)
        {
            // some animation
            animator.SetTrigger(turnAroundTrigger);
            player.TurnPlayerAround();
        }

        StartCoroutine(TurnEverythingAround(includeAnimation ? 0.75f : 0f));

        tapConstraint = false;
        isFleeing = false;
        hasJumped = false;
        hasPassedTheOpponent = false;
    }

    /// <summary>
    /// A function holding all different function calls responsible for rotating the player character around. Can be delayed via argument.
    /// </summary>
    /// <param name="delay">The delay before the player is rotated</param>
    /// <returns></returns>
    IEnumerator TurnEverythingAround(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (side) { player.ScalePlayerAndEquipment(-1f); }
        else { player.ScalePlayerAndEquipment(1f); }
        player.ChangeLanceDirection();
        player.AdjustSpriteRendererLayers(!side);
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
        horseRb = this.AddComponent<Rigidbody2D>();
        horseRb.gravityScale = 0f;
        Collider cd;
        if(this.TryGetComponent<Collider>(out cd))
        {
            cd.enabled = false;
        }
        rb.bodyType = RigidbodyType2D.Static;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;

        rb.AddForce(movementDirection * forceAddedPerInput, ForceMode2D.Impulse);
        isFleeing = true;

        yield return new WaitForSeconds(5f);
        if(cd) cd.enabled = true;
        Destroy(horseRb);
    }
}
