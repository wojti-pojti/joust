using UnityEngine;

public class LanceScript : MonoBehaviour
{
    [HideInInspector] public int index;
    [Header("")]
    public int segmentsLeft;
    public float damage;
    [Header("")]
    [SerializeField] int maxLanceSegments = 4;
    float segmentLength;
    float baseLength = 2.75f;
    float startColliderOffset, startColliderSize;
    Vector3 startLocalPosition, startScale;
    Sprite startSprite;
    Material playerMaterial;
    [SerializeField] float damageMultiplier;

    [Header("Sprites and transforms")]
    [SerializeField] Sprite[] damagedLances = new Sprite[3];
    [SerializeField] Vector3[] damagedLancePosition = new Vector3[3];
    [SerializeField] Vector3[] damagedLanceScale = new Vector3[3];

    [Header("Fragments")]
    [SerializeField] GameObject[] fragments = new GameObject[4];

    LanceController controller;
    BoxCollider2D collider;
    SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        controller = GetComponent<LanceController>();
        startSprite = spriteRenderer.sprite;
        playerMaterial = spriteRenderer.material;

        startLocalPosition = transform.localPosition;
        startScale = transform.localScale;
        startColliderOffset = collider.offset.y;
        startColliderSize = collider.size.y;

        segmentLength = (float)(collider.size.y - baseLength) / 3;

        ResetLance();
    }

    /// <summary>
    /// Updates the damage dealt by the lance to the shield, based on the speed of movement.
    /// </summary>
    /// <param name="speed">Speed for reference.</param>
    public void UpdateDamage(float speed)
    {
        damage = damageMultiplier * Mathf.Max(1f, speed);
    }

    /// <summary>
    /// Simulates a segment of the lance braking off in result of damage. 
    /// Leads to the sprite and collider shortening and the broken segment falling off.
    /// </summary>
    public void BreakSegmentOff()
    {
        Debug.Log("Lance segment broken off.");
        if(segmentsLeft <= 1) { return; }
        segmentsLeft--;

        collider.size = new Vector2(collider.size.x, ((segmentsLeft - 1) * segmentLength) + baseLength);
        collider.offset = new Vector2(collider.offset.x, collider.offset.y - 0.5f * segmentLength);

        // switch and resize the sprite
        spriteRenderer.sprite = damagedLances[segmentsLeft - 1];

        Vector3 newVerticalPosition = new Vector3(this.transform.localPosition.x, damagedLancePosition[segmentsLeft - 1].y, 0f);
        controller.SetNewVerticalPosition(newVerticalPosition);

        float direction = (this.transform.localScale.x > 0 ? 1f : -1f);
        transform.localScale = new Vector3(damagedLanceScale[segmentsLeft - 1].x * direction, damagedLanceScale[segmentsLeft - 1].y, 1f);

        GameObject flyingSegment = Instantiate(fragments[segmentsLeft], transform.position, transform.rotation);
        flyingSegment.GetComponent<SpriteRenderer>().material = playerMaterial;

        float force = Random.Range(0, 0.5f * damage);
        float randX = Random.Range(0.5f, 1f);
        float randY = Random.Range(0.5f, 1f);
        Vector2 knockback = new Vector2(randX, randY) * force;
        Rigidbody2D rb = flyingSegment.GetComponent<Rigidbody2D>();
        rb.AddForce(knockback, ForceMode2D.Impulse);
        Destroy(flyingSegment, 5f);
    }

    /// <summary>
    /// "Repairs" the lance back to an untouched state.
    /// </summary>
    public void ResetLance()
    {
        segmentsLeft = maxLanceSegments;

        // sprite and collider
        spriteRenderer.sprite = startSprite;
        //this.transform.localScale = startScale;
        this.transform.localPosition = startLocalPosition;

        collider.size = new Vector2(collider.size.x, startColliderSize);
        collider.offset = new Vector2(collider.offset.x, startColliderOffset);

        this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }
}
