using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public enum PlayerState
{
    IDLE,
    COMBAT,
    JUMP, // maybe unnecessary
    SHIELD,
    OFFHORSE,
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
    BoxCollider2D lanceCd;

    [SerializeField] GameObject horse;
    HorseMovement hScript;

    [SerializeField] GameObject PlayerUI;

    // start positions and rotations
    Vector3 knightPos, lancePos, shieldPos, UIPos, horsePos;
    Quaternion knightRot, lanceRot, shieldRot, UIRot, horseRot;

    BoxCollider2D opponentLanceCollider;
    bool madeContact;

    float newShieldPositionY, shieldTargetY, shieldUIToObjectDifference;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lScript = lance.GetComponent<LanceScript>();
        lScript.index = index;
        hScript = horse.GetComponent<HorseMovement>();
        lanceController = lance.GetComponent<LanceController>();
        lanceCd = lance.GetComponent <BoxCollider2D>();
        if (index == 1)
        {
            shieldKeyCode = KeyCode.S;
            lanceController.AssignInputKey(false);
        }
        else if (index == 2)
        {
            shieldKeyCode = KeyCode.DownArrow;
            lanceController.AssignInputKey(true);
        }

        shieldUIToObjectDifference = shieldParent.transform.localPosition.y - shieldHealthBar.transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(shieldKeyCode) && state == PlayerState.COMBAT && shieldHealthPoints > 0)
        {
            state = PlayerState.SHIELD;
            UseShield(true);
            lScript.enabled = false;
        }
        if (Input.GetKeyUp(shieldKeyCode) && state == PlayerState.SHIELD)
        {
            state = PlayerState.COMBAT;
            UseShield(false);
            lScript.enabled = true;
        }
    }

    private void FixedUpdate()
    {
        // raise/lower shield animations
        if(shieldTargetY != shield.transform.localPosition.y)
        {
            shield.transform.localPosition = new Vector3(shield.transform.localPosition.x, newShieldPositionY, shield.transform.localPosition.z);
            shieldHealthBar.transform.localPosition = new Vector3(shieldHealthBar.transform.localPosition.x, newShieldPositionY - shieldUIToObjectDifference, shieldHealthBar.transform.localPosition.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        LanceScript opponentLance;
        if(!madeContact && collision.gameObject.tag == "Weapon" && collision.gameObject.TryGetComponent<LanceScript>(out opponentLance))
        {
            if(opponentLance.enabled && opponentLance.index != index)
            {
                madeContact = true;
                if (state == PlayerState.SHIELD)
                {
                    Debug.Log("Player " + index + " was struck in the shield.");
                    DamageShield(opponentLance.damage);
                    opponentLance.BreakSegmentOff();
                    opponentLanceCollider = opponentLance.gameObject.GetComponent<BoxCollider2D>();
                    opponentLanceCollider.enabled = false;
                }
                else
                {
                    opponentLanceCollider = null;
                    float deathRandomNumber = Random.Range(0, 100);
                    float deathChance = GameManager.Instance.baseDeathChance + opponentLance.damage;
                    if (deathRandomNumber < deathChance || !GameManager.Instance.offHorseCombat)
                    {
                        Debug.Log("Player " + index + " was struck dead.");
                        state = PlayerState.DEAD;
                        Die();
                    }
                    else
                    {
                        Debug.Log("Player " + index + " fell off their horse.");
                        state = PlayerState.OFFHORSE;

                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (opponentLanceCollider != null && opponentLanceCollider.enabled == false)
        {
            opponentLanceCollider.enabled = true;
        }
    }

    /// <summary>
    /// Resets the player's stats and appearance (the knight in particular).
    /// </summary>
    public void ResetPlayerState()
    {
        RepairPlayer();
        state = PlayerState.IDLE;
        shieldHealthPoints = maxShieldHealthPoints;
        if (lScript == null) 
        {
            lScript = lance.GetComponent<LanceScript>();
            lScript.index = index;
        }
        lScript.ResetLance();
        lanceController.RaiseBackToPosition();

        shield.transform.position = shieldParent.transform.position;

        if (!shieldHealthBar.gameObject.activeSelf) shieldHealthBar.gameObject.SetActive(true);
        if (shieldHealthBar != null)
        {
            shieldHealthBar.maxValue = maxShieldHealthPoints;
            shieldHealthBar.value = shieldHealthPoints;
        }

        // set correct sprite

        // set colors

        UpdateShieldUI();
    }

    /// <summary>
    /// Activates or deactivates the damage calculation of the lance and switches state.
    /// </summary>
    /// <param name="activate">The new state.</param>
    public void Charge(bool activate)
    {
        if (activate) 
        { 
            hScript.hasPassedTheOpponent = false;
            state = PlayerState.COMBAT; 
            lanceCd.enabled = true; 
        }
        else 
        { 
            state = PlayerState.IDLE;
            madeContact = false;
            if (!lanceController) lanceController = lance.GetComponent<LanceController>();
            lanceController.RaiseBackToPosition();
        }
    }

    #region Lance Management
    /// <summary>
    /// Passes the speed from the HorseMovement to the LanceScript.
    /// </summary>
    /// <param name="speed">The horse's speed.</param>
    public void UpdateLanceDamage(float speed)
    {
        if (lScript == null) lScript = lance.GetComponent<LanceScript>();
        lScript.UpdateDamage(speed);
    }

    /// <summary>
    /// Passes on the function call to the lance controller. Reverses the direction where the lance is pointed.
    /// </summary>
    public void ChangeLanceDirection()
    {
        if (!lanceController) lanceController = lance.GetComponent<LanceController>();
        lanceController.ReverseHingeDirection();
    }

    /// <summary>
    /// Disables the collider of the lance, after a short delay to allow for draws.
    /// </summary>
    /// <returns></returns>
    IEnumerator DisableLanceCollider()
    {
        yield return new WaitForSeconds(0.1f);
        lanceCd.enabled = false;
    }
    #endregion

    #region Death & Rebirth
    /// <summary>
    /// Visually displays the player's death.
    /// </summary>
    void Die()
    {
        UpdateShieldUI();
        StartCoroutine(DisableLanceCollider());
        // play animation
        // throw knight off of the horse
        this.transform.DetachChildren();
        hScript.RunAway();
    }

    /// <summary>
    /// Records the starting position and rotation for each child gameobject of player.
    /// </summary>
    public void RecordLocalStartTransforms()
    {
        knightPos = knight.transform.localPosition;
        knightRot = knight.transform.localRotation;

        lancePos = lance.transform.localPosition;
        lanceRot = lance.transform.localRotation;

        shieldPos = shieldParent.transform.localPosition;
        shieldRot = shieldParent.transform.localRotation; 

        horsePos = horse.transform.localPosition;
        horseRot = horse.transform.localRotation;

        UIPos = PlayerUI.transform.localPosition;
        UIRot = PlayerUI.transform.localRotation;
    }

    /// <summary>
    /// Re-attaches all children back to the player and resets their relative positions.
    /// </summary>
    void RepairPlayer()
    {
        knight.transform.SetParent(this.transform);
        lance.transform.SetParent(this.transform);
        horse.transform.SetParent(this.transform);
        shieldParent.transform.SetParent(this.transform);
        PlayerUI.transform.SetParent(this.transform);

        knight.transform.SetLocalPositionAndRotation(knightPos, knightRot);
        lance.transform.SetLocalPositionAndRotation(lancePos, lanceRot);
        horse.transform.SetLocalPositionAndRotation(horsePos, horseRot);
        shieldParent.transform.SetLocalPositionAndRotation(shieldPos, shieldRot);
        PlayerUI.transform.SetLocalPositionAndRotation(UIPos, UIRot);
    }
    #endregion

    #region Shield
    /// <summary>
    /// This function is responsible for displaying indicators of using the shield.
    /// </summary>
    /// <param name="raise">True if the player holds the shield active, false if they change their mind.</param>
    void UseShield(bool raise)
    {
        if (raise)
        {
            shieldTargetY = shield.transform.localPosition.y + 0.25f;
        }
        else
        {
            shieldTargetY = 0f;
        }
        newShieldPositionY = Mathf.Lerp(shield.transform.localPosition.y, shieldTargetY, 0.5f);

        // apply shader
    }

    /// <summary>
    /// Called when the other player's hitbox interacts with this hurtbox, provided this player is raising their shield.
    /// </summary>
    /// <param name="damage">Damage dealt to the shield.</param>
    public void DamageShield(float damage)
    {
        shieldHealthPoints -= damage * Mathf.Max(1f, hScript.speed);

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
