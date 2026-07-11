using UnityEngine;
using UnityEngine.UI;

public enum PlayerState
{
    IDLE,
    COMBAT,
    JUMP, // maybe unnecessary
    SHIELD,
    DEAD
}
public class PlayerScript : MonoBehaviour
{
    [Header("State")]
    public int index;
    public PlayerState state; 

    [SerializeField] float shieldHealthPoints;

    [Header("Setup")]
    [SerializeField] KeyCode shieldKeyCode;
    [SerializeField] float maxShieldHealthPoints;
    [SerializeField] GameObject knight;
    [SerializeField] GameObject shield;
    [SerializeField] Slider shieldHealthBar;
    [SerializeField] GameObject lance;
    [SerializeField] GameObject horse;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(index == 1)
        {
            shieldKeyCode = KeyCode.S;
        }
        else if (index == 2)
        {
            shieldKeyCode = KeyCode.DownArrow;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(shieldKeyCode) && state == PlayerState.COMBAT)
        {
            state = PlayerState.SHIELD;
        }
        if (Input.GetKeyUp(shieldKeyCode) && state == PlayerState.SHIELD)
        {
            state = PlayerState.COMBAT;
        }
    }

    /// <summary>
    /// Resets the player's stats and appearance (the knight in particular).
    /// </summary>
    public void ResetPlayerState()
    {
        state = PlayerState.IDLE;
        shieldHealthPoints = maxShieldHealthPoints;
        shieldHealthBar.maxValue = maxShieldHealthPoints;
        shieldHealthBar.value = shieldHealthPoints;

        // set correct sprite

        // set colors
    }

    #region Shield
    /// <summary>
    /// Called when the other player's hitbox interacts with this hurtbox, provided this player is raising their shield.
    /// </summary>
    /// <param name="damage">Damage dealt to the shield.</param>
    public void DamageShield(float damage)
    {
        shieldHealthPoints -= damage;

        UpdateShieldUI();

        if (shieldHealthPoints <= 0)
        {
            ThrowShieldAway();
        }
    }

    /// <summary>
    /// Possible animation and some physical remainder after the shield is destroyed.
    /// </summary>
    void ThrowShieldAway()
    {
        Debug.Log("Player " + index + " has lost their shield!");
    }

    /// <summary>
    /// Updates UI indicating the shield HP left.
    /// </summary>
    void UpdateShieldUI()
    {
        // healthbar
        shieldHealthBar.value = shieldHealthPoints;

        // make inactive if shield is gone
        if(shieldHealthPoints <= 0 && shieldHealthBar.gameObject.activeSelf)
        {
            shieldHealthBar.gameObject.SetActive(false);
        }
    }
    #endregion
}
