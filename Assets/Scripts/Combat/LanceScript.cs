using UnityEngine;

public class LanceScript : MonoBehaviour
{
    [HideInInspector] public int index;
    [Header("")]
    public int segmentsLeft;
    public float damage;
    [Header("")]
    [SerializeField] int maxLanceSegments;
    float segmentLength;
    Vector3 startScale;
    [SerializeField] float damageMultiplier;

    BoxCollider2D collider;
    SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        segmentLength = startScale.y / (float)maxLanceSegments;

        ResetLance();
    }

    /// <summary>
    /// Updates the damage dealt by the lance to the shield, based on the speed of movement.
    /// </summary>
    /// <param name="speed"></param>
    public void UpdateDamage(float speed)
    {
        damage = damageMultiplier * speed;
    }

    /// <summary>
    /// Simulates a segment of the lance braking off in result of damage. 
    /// Leads to the sprite and collider shortening and the broken segment falling off.
    /// </summary>
    public void BreakSegmentOff()
    {
        if(segmentsLeft <= 0) { return; }
        segmentsLeft--;

        // get sprites before working on this

        // GameObject flyingSegment = Instantiate();
        float force = Random.Range(0, 0.5f * damage);
        float randX = Random.Range(0, 1);
        float randY = Random.Range(0, 1);
        Vector2 knockback = new Vector2(randX, randY) * force;
        // Rigidbody2D rb = flyingSegment.GetComponent<Rigidbody2D>();
        // rb.AddForce(knockback, ForceMode2D.Impulse);
    }

    /// <summary>
    /// "Repairs" the lance back to an untouched state.
    /// </summary>
    public void ResetLance()
    {
        segmentsLeft = maxLanceSegments;

        // sprite and collider
    }
}
