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
    [Header("Knight")]
    [SerializeField] GameObject knight;
    [SerializeField] SpriteRenderer knightSpriteRenderer;

    [Header("Shield")]
    [SerializeField] GameObject shieldParent;
    [SerializeField] GameObject shield;
    [SerializeField] Slider shieldHealthBar;

    [Header("Lance")]
    [SerializeField] GameObject lance;
    [SerializeField] LanceScript lScript;
    [SerializeField] LanceController lanceController;
    [SerializeField] BoxCollider2D lanceCd;

    [Header("Horse")]
    [SerializeField] GameObject horse;
    [SerializeField] HorseMovement hScript;

    [Header("Other")]
    [SerializeField] GameObject PlayerUI;
    [SerializeField] Material playerMaterial;
    [SerializeField] Animator animator;

    int deathTrigger = Animator.StringToHash("Die");
    int resetTrigger = Animator.StringToHash("Reset");

    // start positions and rotations
    Vector3 knightPos, lancePos, shieldPos, UIPos, horsePos;
    Quaternion knightRot, lanceRot, shieldRot, UIRot, horseRot;

    [SerializeField] BoxCollider2D collider;
    BoxCollider2D opponentLanceCollider;
    bool madeContact;
    SpriteRenderer[] renderers;

    float newShieldPositionY, shieldTargetY, shieldUIToObjectDifference;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        if(shieldTargetY != shield.transform.localPosition.y && shieldHealthPoints > 0 && state != PlayerState.DEAD)
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

                    ThrowOffTheHorse(opponentLance.damage);
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

    #region Player state
    /// <summary>
    /// Resets the player's stats and appearance (the knight in particular).
    /// </summary>
    public void ResetPlayerState()
    {
        RepairPlayer();
        hScript.Setup(index == 1 ? false : true);
        state = PlayerState.IDLE;
        shieldHealthPoints = maxShieldHealthPoints;
        if (lScript == null) 
        {
            lScript = lance.GetComponent<LanceScript>();
            lScript.index = index;
        }
        lScript.ResetLance();
        lanceController.RaiseBackToPosition();
        lance.GetComponent<HingeJoint2D>().enabled = true;

        shield.transform.position = shieldParent.transform.position;

        if (!shieldHealthBar.gameObject.activeSelf) shieldHealthBar.gameObject.SetActive(true);
        if (shieldHealthBar != null)
        {
            shieldHealthBar.maxValue = maxShieldHealthPoints;
            shieldHealthBar.value = shieldHealthPoints;
        }

        knight.GetComponent<BoxCollider2D>().enabled = false;
        knight.GetComponent<Rigidbody2D>().simulated = false;

        // set correct sprite
        animator.SetTrigger(resetTrigger);

        UpdateShieldUI();
    }

    /// <summary>
    /// This function scales the player and all associated gameobjects by a given factor.
    /// </summary>
    /// <param name="scale">The new value of the x-component of player's local scale.</param>
    public void ScalePlayerAndEquipment(float scale)
    {
        Vector3 lScale = lance.transform.localScale;
        Vector3 sScale = shieldParent.transform.localScale;
        Vector3 hScale = horse.transform.localScale;
        gameObject.transform.localScale = new Vector3(scale, 1, 1);

        lance.transform.localScale = new Vector3(-1f * lScale.x, lScale.y, lScale.z);
        shieldParent.transform.localScale = new Vector3(-1f * sScale.x, sScale.y, sScale.z);
        //horse.transform.localScale = new Vector3(-1 * hScale.x, hScale.y, hScale.z);
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

    /// <summary>
    /// Ensures that the player appears on the right side of the fence.
    /// </summary>
    /// <param name="toTheFront">If true, the player will appear over the fence.</param>
    public void AdjustSpriteRendererLayers(bool toTheFront)
    {
        if (renderers == null) { renderers = GetComponentsInChildren<SpriteRenderer>(); }
        foreach (SpriteRenderer renderer in renderers) 
        {
            if (renderer.sortingOrder < 10)
            {
                if(toTheFront)
                {
                    renderer.sortingOrder += 20;
                    PlayerUI.GetComponent<Canvas>().sortingOrder += 20;
                }
                else
                {
                    renderer.sortingOrder += 10;
                    PlayerUI.GetComponent<Canvas>().sortingOrder += 10;
                }
                continue;
            }

            if (toTheFront && renderer.sortingOrder > 10 && renderer.sortingOrder < 20)
            {
                renderer.sortingOrder += 10;
                PlayerUI.GetComponent<Canvas>().sortingOrder += 10;
            }
            else if (!toTheFront && renderer.sortingOrder > 20)
            {
                renderer.sortingOrder -= 10;
                PlayerUI.GetComponent<Canvas>().sortingOrder -= 10;
            }
        }
    }

    public void TurnPlayerAround()
    {
        StartCoroutine(HidePlayerTemporarily(0.4f, 0.4f));
    }
    IEnumerator HidePlayerTemporarily(float duration, float offset)
    {
        yield return new WaitForSeconds(offset);
        knight.SetActive(false);
        lance.SetActive(false);
        shieldParent.SetActive(false);
        yield return new WaitForSeconds(duration);
        knight.SetActive(true);
        lance.SetActive(true);
        shieldParent.SetActive(true);
    }
    #endregion

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
    /// Possible animation and some physical remainder after the shield is destroyed.
    /// </summary>
    IEnumerator ThrowLanceAway()
    {
        yield return new WaitForFixedUpdate();
        Rigidbody2D lanceRb = lance.GetComponent<Rigidbody2D>();
        lance.GetComponent<HingeJoint2D>().enabled = false;

        // detach and throw away
        float knockback = 3;
        Vector2 direction = (hScript.side ? new Vector2(1, 1) : new Vector2(-1, 1));
        lanceRb.AddForce(direction * knockback, ForceMode2D.Impulse);
        lanceRb.gravityScale = 1f;

        yield return new WaitForSeconds(4f);

        // stop falling
        lanceRb.bodyType = RigidbodyType2D.Static;
        lanceRb.gravityScale = 0f;
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
    /// This function is responsible for simulating the knight falling off of the horse.
    /// </summary>
    void ThrowOffTheHorse(float forceMultiplier)
    {
        BoxCollider2D cd = knight.GetComponent<BoxCollider2D>();
        Rigidbody2D rb = knight.GetComponent<Rigidbody2D>();

        rb.simulated = true;
        cd.enabled = true;
        Vector2 direction = (hScript.side ? new Vector2(1, 1) : new Vector2(-1, 1));
        direction *= Mathf.Max(2.5f, forceMultiplier);
        rb.AddForce(direction, ForceMode2D.Impulse);

        if (shieldHealthPoints > 0) StartCoroutine(ThrowShieldAway(direction * 1.4f));
        StartCoroutine(ThrowLanceAway());

        // animation
        animator.SetTrigger(deathTrigger);
    }

    /// <summary>
    /// Visually displays the player's death.
    /// </summary>
    void Die()
    {
        UpdateShieldUI();
        StartCoroutine(DisableLanceCollider());
        // play animation
        animator.SetTrigger("Die");
        // throw knight off of the horse
        collider.enabled = false;
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
        //knight.transform.SetParent(this.transform);
        //lance.transform.SetParent(this.transform);
        //horse.transform.SetParent(this.transform);
        //shieldParent.transform.SetParent(this.transform);
        //PlayerUI.transform.SetParent(this.transform);

        knight.transform.SetLocalPositionAndRotation(knightPos, knightRot);
        lance.transform.SetLocalPositionAndRotation(lancePos, lanceRot);
        horse.transform.SetLocalPositionAndRotation(horsePos, horseRot);
        shieldParent.transform.SetLocalPositionAndRotation(shieldPos, shieldRot);
        PlayerUI.transform.SetLocalPositionAndRotation(UIPos, UIRot);
        collider.enabled = true;
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

            // apply shader
            StartCoroutine(HighlightPlayer(0.25f));
        }
        else
        {
            shieldTargetY = 0f;
        }
        newShieldPositionY = Mathf.Lerp(shield.transform.localPosition.y, shieldTargetY, 0.5f);
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
    /// Simulates the shield being discarded and leaves some physical remainder after the shield is thrown away.
    /// </summary>
    /// <param name="predeterminedVector">The direction of the thrown shield, if it is determined elsewhere.</param>
    IEnumerator ThrowShieldAway(Vector2 predeterminedVector = new Vector2())
    {
        Debug.Log("Player " + index + " has lost their shield!");
        Rigidbody2D shieldRb = shield.AddComponent<Rigidbody2D>();
        shieldRb.gravityScale = 1f;

        // detach and throw away
        shieldParent.transform.DetachChildren();
        shield.transform.position = shieldParent.transform.position;
        if(predeterminedVector != new Vector2(0, 0))
        {
            shieldRb.AddForce(predeterminedVector, ForceMode2D.Impulse);
        }
        else
        {
            float knockback = Mathf.Abs(shieldHealthPoints) / 10f;
            Vector2 direction = (hScript.side ? new Vector2(1, 1) : new Vector2(-1, 1));
            shieldRb.AddForce(direction * knockback, ForceMode2D.Impulse);
        }

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

    /// <summary>
    /// Activates the shader to display a highlight effect on the player shader.
    /// </summary>
    /// <param name="duration">Duration for which the sprite will appear white / illuminated.</param>
    /// <returns></returns>
    IEnumerator HighlightPlayer(float duration)
    {
        playerMaterial.SetInt("_Highlight", 1);
        yield return new WaitForSeconds(duration);
        playerMaterial.SetInt("_Highlight", 0);
    }
}
