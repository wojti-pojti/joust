using UnityEngine;

public class LanceScript : MonoBehaviour
{
    [HideInInspector] public int index;
    [Header("")]
    public int segmentsLeft;
    public float damage;
    [Header("")]
    [SerializeField] private int maxLanceSegments = 4;
    private float segmentLength;
    private float baseLength = 2.75f;
    private float startColliderOffset, startColliderSize;
    private Vector3 startLocalPosition, startScale;
    private Sprite startSprite;
    private Material playerMaterial;
    [SerializeField] private float damageMultiplier;

    // maybe instead, keep track of 3 copies of lance, each shorter and just set active / inactive
    // move lanceScript to the player

    [Header("Lance Breaking")]
    [SerializeField] private GameObject startLance;
    [SerializeField] private LanceController mainLanceController;
    [SerializeField] private GameObject[] brokenLance = new GameObject[3];
   // [SerializeField] private LanceController[] controllers = new LanceController[3];

    [Header("Fragments")]
    [SerializeField] private GameObject[] fragments = new GameObject[4];

    private PlayerScript player;
    
    private void Awake()
    {
        player = this.GetComponent<PlayerScript>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetLance();
    }


    /// <summary>
    /// Called initially to assign the input keycode based on starting side;
    /// </summary>
    /// <param name="side">False for left, True for right.</param>
    public void AssignInputKey(bool side)
    {
        //controllers[0] = brokenLance[0].GetComponent<LanceController>();
        //controllers[1] = brokenLance[1].GetComponent<LanceController>();
        //controllers[2] = brokenLance[2].GetComponent<LanceController>();
        player.activeLanceController = mainLanceController;
        mainLanceController.AssignInputKey(side, this);

        //foreach (LanceController controller in controllers) 
        //{
        //    controller.AssignInputKey(side, this);
        //}
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

        GameObject retiredLance = player.lance;
        //player.activeLanceController = controllers[segmentsLeft - 1];
        player.lance = brokenLance[segmentsLeft - 1];
        player.lance.SetActive(true);
        retiredLance.SetActive(false);

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

        player.lance = startLance;
        player.lance.SetActive(true);
        foreach (GameObject weapon in brokenLance) 
        {
            weapon.SetActive(false);
        }

        this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        //Debug.Log("Player " + player.index + "'s lance has been restored.");
    }
}
