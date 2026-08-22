using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// This script controls the movement of the lance, including taking inputs.
/// </summary>
public class LanceController : MonoBehaviour
{
    [HideInInspector] public LanceScript parentLanceScript;

    [SerializeField] private bool holdButton;
    [SerializeField] private bool releasedButton;
    [SerializeField] private float chargeAccumulationMultiplier;
    [SerializeField] private float directionMultiplier;

    [Header("")]
    private Controls controls;
    [SerializeField] private string controlScheme;

    [Header("")]
    public HingeJoint2D joint;
    private Vector3 verticalPosition, startVerticalPosition;

    private void Awake()
    {
        controls = new Controls();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        verticalPosition = this.transform.localPosition;
        startVerticalPosition = verticalPosition;
    }

    private void FixedUpdate()
    {
        if (holdButton && joint.motor.motorSpeed == 0) 
        {
            JointMotor2D newMotor = joint.motor;
            newMotor.motorSpeed = chargeAccumulationMultiplier * directionMultiplier;
            joint.motor = newMotor;
        }

        if (!holdButton && joint.motor.motorSpeed != 0)
        {
            JointMotor2D newMotor = joint.motor;
            newMotor.motorSpeed = 0;
            joint.motor = newMotor;
        }
    }

    #region Input Actions
    void LanceKeyDownAction()
    {
        if (GameManager.Instance.gameState != GameState.ACTIVE_COMBAT) { return; }
        if (!releasedButton)
        {
            holdButton = true;
        }
    }

    void LanceKeyUpAction()
    {
        if (GameManager.Instance.gameState != GameState.ACTIVE_COMBAT) { return; }
        if (holdButton)
        {
            holdButton = false;
            releasedButton = true;
        }
    }
    #endregion

    #region Adding and removing this instance as listener
    void OnEnable() // subscribe to the event
    {
        controls.Match.Enable();
    }

    void OnDisable() // unsubscribe to the event
    {
        controls.Match.Disable();
    }
    #endregion

    #region Controls
    /// <summary>
    /// Assigns a new control scheme to the lance.
    /// </summary>
    /// <param name="newControlScheme"></param>
    public void AssignControlScheme(string newControlScheme)
    {
        controlScheme = newControlScheme;
        controls.bindingMask = InputBinding.MaskByGroup(controlScheme);
    }

    /// <summary>
    /// Adds the newly detected gamepad to the player of given index.
    /// </summary>
    /// <param name="gamepad"></param>
    public void AssignGamepad(Gamepad gamepad)
    {
        controls.devices = new ReadOnlyArray<InputDevice>(new InputDevice[] { gamepad });
    }

    /// <summary>
    /// Removes the gamepad from the player of given index.
    /// </summary>
    /// <param name="gamepad"></param>
    public void RemoveGamepad(Gamepad gamepad)
    {
        controls.devices = new ReadOnlyArray<InputDevice>(new InputDevice[] { Keyboard.current });
    }
    #endregion

    /// <summary>
    /// Setter function for the vertical position vector. Adjustments may be needed when lance segments are broken off.
    /// </summary>
    /// <param name="newVerticalPosition"></param>
    public void SetNewVerticalPosition(Vector3 newVerticalPosition)
    {
        this.verticalPosition = newVerticalPosition;
    }

    /// <summary>
    /// Returns the lance to its initial, vertical position.
    /// </summary>
    /// <param name="reset">True if the lance is in its undamaged form, otherwise false.</param>
    public void RaiseBackToPosition(bool reset = true)
    {
        JointMotor2D newMotor = joint.motor;
        newMotor.motorSpeed = 0;
        joint.motor = newMotor;

        this.transform.rotation = new Quaternion(0, 0, 0, 0);
        if (reset) { this.transform.localPosition = startVerticalPosition; }
        else { this.transform.localPosition = verticalPosition; }

        holdButton = false;
        releasedButton = false;
    }

    /// <summary>
    /// Called initially to assign the input keycode based on starting side;
    /// </summary>
    /// <param name="side">False for left, True for right.</param>
    /// <param name="caller">The parent lance script.</param>
    public void AssignInputKey(bool side, LanceScript caller)
    {
        parentLanceScript = caller;
        if (!side)
        {
            controlScheme = "KeyboardP1";
        }
        else
        {
            controlScheme = "KeyboardP2";
        }
        controls.bindingMask = InputBinding.MaskByGroup(controlScheme);
        controls.Match.Lance.started += ctx => LanceKeyDownAction();
        controls.Match.Lance.canceled += ctx => LanceKeyUpAction();
    }

    /// <summary>
    /// Reverses the direction of the lance hinge joint.
    /// </summary>
    /// <returns></returns>
    public void ReverseHingeDirection()
    {
        directionMultiplier = -1f * directionMultiplier;

        JointAngleLimits2D newLimits = new JointAngleLimits2D();
        if (directionMultiplier > 0)
        {
            newLimits.min = 0;
            newLimits.max = 110;
        }
        else if (directionMultiplier < 0)
        {
            newLimits.min = -110;
            newLimits.max = 0;
        }
        joint.limits = newLimits;
    }
}
