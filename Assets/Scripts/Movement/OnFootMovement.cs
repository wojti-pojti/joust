using UnityEngine;

/// <summary>
/// Script connected with the option for the fight to persist even after a player gets de-horsed. 
/// It controls/manages movement (and combat) on foot.
/// </summary>
public class OnFootMovement : MonoBehaviour
{
    public int playerIndex;
    [Header("")]
    [SerializeField] float walkSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float armRotationSpeed;

    [Header("Inputs")]
    [SerializeField] KeyCode jumpKey;
    [SerializeField] KeyCode swingKey;

    private PlayerScript player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        
    }
}
