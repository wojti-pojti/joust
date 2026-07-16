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
    float baseLength = 0.9229f;
    float startColliderOffset, startColliderSize;
    Vector3 startScale;
    [SerializeField] float damageMultiplier;

    [Header("Fragments")]
    [SerializeField] GameObject[] fragments = new GameObject[4];

    BoxCollider2D collider;
    SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        // get sprites before working on this

        GameObject flyingSegment = Instantiate(fragments[segmentsLeft], transform.position, transform.rotation);
        float force = Random.Range(0, 0.5f * damage);
        float randX = Random.Range(0, 1);
        float randY = Random.Range(0, 1);
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

        collider.size = new Vector2(collider.size.x, startColliderSize);
        collider.offset = new Vector2(collider.offset.x, startColliderOffset);
    }
}
