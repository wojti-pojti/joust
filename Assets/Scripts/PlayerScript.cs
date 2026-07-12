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
    LanceScript lScript;
    [SerializeField] GameObject horse;
    HorseMovement hScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lScript = lance.GetComponent<LanceScript>();
        lScript.index = index;
        hScript = horse.GetComponent<HorseMovement>();
        if(index == 1)
        {
            shieldKeyCode = KeyCode.S;
            lance.GetComponent<LanceController>().AssignInputKey(false);
        }
        else if (index == 2)
        {
            shieldKeyCode = KeyCode.DownArrow;
            lance.GetComponent<LanceController>().AssignInputKey(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(shieldKeyCode) && state == PlayerState.COMBAT)
        {
            state = PlayerState.SHIELD;
            lScript.enabled = false;
        }
        if (Input.GetKeyUp(shieldKeyCode) && state == PlayerState.SHIELD)
        {
            state = PlayerState.COMBAT;
            lScript.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("Player "+ index + " hit by "+ collision.gameObject.name+", tag: "+ collision.gameObject.tag);
        LanceScript opponentLance;
        if(collision.gameObject.tag == "Weapon" && collision.gameObject.TryGetComponent<LanceScript>(out opponentLance))
        {
            //Debug.Log("Script found. Lance index: "+ opponentLance.index);

            if(opponentLance.enabled && opponentLance.index != index)
            {
                if (state == PlayerState.SHIELD)
                {
                    Debug.Log("Player " + index + " was struck in the shield.");
                    DamageShield(opponentLance.damage);
                    lScript.BreakSegmentOff();
                }
                else
                {
                    Debug.Log("Player " + index + " was struck dead.");
                    state = PlayerState.DEAD;
                    Die();
                }
            }
        }
    }

    /// <summary>
    /// Resets the player's stats and appearance (the knight in particular).
    /// </summary>
    public void ResetPlayerState()
    {
        state = PlayerState.IDLE;
        shieldHealthPoints = maxShieldHealthPoints;
        lScript.ResetLance();

        if(shieldHealthBar != null)
        {
            shieldHealthBar.maxValue = maxShieldHealthPoints;
            shieldHealthBar.value = shieldHealthPoints;
        }

        // set correct sprite

        // set colors
    }

    /// <summary>
    /// Activates or deactivates the damage calculation of the lance and switches state.
    /// </summary>
    /// <param name="activate">The new state.</param>
    public void Charge(bool activate)
    {
        if (activate) { state = PlayerState.COMBAT; }
        else 
        { 
            state = PlayerState.IDLE;
            lance.GetComponent<LanceController>().RaiseBackToPosition();
        }
    }

    /// <summary>
    /// Passes the speed from the HorseMovement to the LanceScript.
    /// </summary>
    /// <param name="speed">The horse's speed.</param>
    public void UpdateLanceDamage(float speed)
    {
        lScript.UpdateDamage(speed);
    }

    /// <summary>
    /// Visually displays the player's death.
    /// </summary>
    void Die()
    {
        Destroy(lScript);
        // play animation
        // throw knight off of the horse
        this.transform.DetachChildren();
        hScript.RunAway();
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
