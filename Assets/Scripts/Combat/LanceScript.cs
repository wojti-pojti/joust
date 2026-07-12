using UnityEngine;

public class LanceScript : MonoBehaviour
{
    [HideInInspector] public int index;
    [Header("")]
    public int segmentsLeft;
    public float damage;
    [Header("")]
    [SerializeField] int maxLanceSegments;
    [SerializeField] float damageMultiplier;

    BoxCollider2D collider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();

        ResetLance();
    }

    ///// <summary>
    ///// Recalculates damage based on speed repeatedly.
    ///// </summary>
    //public void BeginCharge()
    //{
    //    InvokeRepeating("RecalculateDamage", 0f, 0.05f);
    //}

    ///// <summary>
    ///// Cancels repeating recalculations of the damage.
    ///// </summary>
    //public void EndCharge()
    //{
    //    CancelInvoke();
    //}

    /// <summary>
    /// Updates the damage dealt by the lance to the shield, based on the speed of movement.
    /// </summary>
    /// <param name="speed"></param>
    public void UpdateDamage(float speed)
    {
        damage = damageMultiplier * speed * speed;
    }

    /// <summary>
    /// Simulates a segment of the lance braking off in result of damage. 
    /// Leads to the sprite and collider shortening and the broken segment falling off.
    /// </summary>
    public void BreakSegmentOff()
    {
        if(segmentsLeft <= 0) { return; }
        segmentsLeft--;
        // Instantiate();
        // add some force
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
