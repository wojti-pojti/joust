using System.Collections;
using Unity.VisualScripting;
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

    public float shieldHealthPoints;

    [Header("Setup")]
    [SerializeField] KeyCode shieldKeyCode;
    [SerializeField] float maxShieldHealthPoints;
    [SerializeField] GameObject knight;
    [SerializeField] GameObject shieldParent;
    [SerializeField] GameObject shield;
    [SerializeField] Slider shieldHealthBar;
    [SerializeField] GameObject lance;
    LanceScript lScript;
    LanceController lanceController;
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
        if (Input.GetKeyDown(shieldKeyCode) && state == PlayerState.COMBAT && shieldHealthPoints > 0)
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
                    opponentLance.BreakSegmentOff();
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

        shield.transform.position = shieldParent.transform.position;

        if (!shieldHealthBar.gameObject.activeSelf) shieldHealthBar.gameObject.SetActive(true);
        if (shieldHealthBar != null)
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
            if (!lanceController) lanceController = lance.GetComponent<LanceController>();
            lanceController.RaiseBackToPosition();
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
        UpdateShieldUI();
        Destroy(lScript);
        // play animation
        // throw knight off of the horse
        this.transform.DetachChildren();
       // hScript.RunAway();
    }

    /// <summary>
    /// Passes on the function call to the lance controller. Reverses the direction where the lance is pointed.
    /// </summary>
    public void ChangeLanceDirection()
    {
        if (!lanceController) lanceController = lance.GetComponent<LanceController>();
        lanceController.ReverseHingeDirection();
    }

    #region Shield
    /// <summary>
    /// Called when the other player's hitbox interacts with this hurtbox, provided this player is raising their shield.
    /// </summary>
    /// <param name="damage">Damage dealt to the shield.</param>
    public void DamageShield(float damage)
    {
        shieldHealthPoints -= damage * hScript.speed;

        UpdateShieldUI();

        if (shieldHealthPoints <= 0)
        {
            StartCoroutine(ThrowShieldAway());
        }
    }

    /// <summary>
    /// Possible animation and some physical remainder after the shield is destroyed.
    /// </summary>
    IEnumerator ThrowShieldAway()
    {
        Debug.Log("Player " + index + " has lost their shield!");
        Rigidbody2D shieldRb = shield.AddComponent<Rigidbody2D>();

        // detach and throw away
        shieldParent.transform.DetachChildren();
        float knockback = Mathf.Abs(shieldHealthPoints) / 10f;
        Vector2 direction = (hScript.side ? new Vector2(1, 1) : new Vector2(-1, 1));
        shieldRb.AddForce(direction * knockback, ForceMode2D.Impulse);

        yield return new WaitForSeconds(6f);

        // stop falling
        Destroy(shieldRb);
        shield.transform.SetParent(shieldParent.transform, true);
    }

    /// <summary>
    /// Updates UI indicating the shield HP left.
    /// </summary>
    void UpdateShieldUI()
    {
        // healthbar
        shieldHealthBar.value = shieldHealthPoints;

        // make inactive if shield is gone
        if((shieldHealthPoints <= 0 || state == PlayerState.DEAD) && shieldHealthBar.gameObject.activeSelf)
        {
            shieldHealthBar.gameObject.SetActive(false);
        }
    }
    #endregion
}
