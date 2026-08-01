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

    Rigidbody2D horseRb;
    Vector2 movementDirection;
    Animator animator;
    SpriteRenderer spriteRenderer;
    bool tapConstraint; 
    [HideInInspector] public bool hasPassedTheOpponent;
    float idleAnimationTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
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
            rb.AddForce(movementDirection * jogForce, ForceMode2D.Force);
            idleAnimationTimer = 0f;
        }
        if(isFleeing)
        {
            horseRb.AddForce(movementDirection * jogForce, ForceMode2D.Force);
            idleAnimationTimer = 0f;
        }

        if (isBraking)
        {
            // apply counter-force
            rb.AddForce(-movementDirection * 0.5f * jogForce, ForceMode2D.Force);

            if(speed <= 0)
            {
                isBraking = false;

                // indicate the run has ended
                GameManager.Instance.InformOfReachingEndZone(playerIndex);
            }
        }

        if (player.state == PlayerState.JUMP && player.transform.position.y < -0.5f && rb.linearVelocity.y < 0)
        {
            // land animation
            animator.SetTrigger("Land");
        }

        if (idleAnimationTimer >= idleTimeToStartAnimation) 
        {
            animator.SetTrigger("IdleStomp");
            idleAnimationTimer = 0;
        }

        idleAnimationTimer += Time.fixedDeltaTime;
        speed = rb.linearVelocity.magnitude;
        animator.SetFloat("Speed", speed);
        player.UpdateLanceDamage(speed);
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
        if (!side) Debug.Log("Player 1's horse collided with " + collision.gameObject.name);
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
        if (this.side != side) { TurnAround(); }

        rb.gravityScale = 1f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        playerIndex = (side ? 2 : 1);
        if (side) { idleAnimationTimer = 0.5f * idleTimeToStartAnimation; }

        // set initial side
        this.side = side;
        if (side) { movementDirection = Vector2.left; }
        else { movementDirection = Vector2.right; }
        isBraking = false;
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
        animator.SetTrigger("Jump");
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
            animator.SetTrigger("TurnAround");
        }

        if (side) { player.ScalePlayerAndEquipment(-1f); }
        else { player.ScalePlayerAndEquipment(1f); }
        //spriteRenderer.flipX = !spriteRenderer.flipX;
        player.ChangeLanceDirection();

        tapConstraint = false;
        isFleeing = false;
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
        horseRb = this.AddComponent<Rigidbody2D>();
        isFleeing = true;
        //Collider cd;
        //if(this.TryGetComponent<Collider>(out cd))
        //{
        //    cd.enabled = false;
        //}
        rb.bodyType = RigidbodyType2D.Static;
        rb.gravityScale = 0;

        yield return new WaitForSeconds(5f);
        //if(cd) cd.enabled = true;
        isFleeing = false;
        Destroy(horseRb);
    }
}
