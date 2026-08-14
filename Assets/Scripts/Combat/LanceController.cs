using System.Collections;
using UnityEngine;

public class LanceController : MonoBehaviour
{
    [HideInInspector] public LanceScript parentLanceScript;

    [SerializeField] private bool holdButton;
    [SerializeField] private bool releasedButton;
    [SerializeField] private float chargeAccumulationMultiplier;
    [SerializeField] private float directionMultiplier;

    [Header("")]
    [SerializeField] private KeyCode lowerLanceKeyCode;
    [Header("")]
    [SerializeField] private HingeJoint2D joint;
    private Vector3 verticalPosition, startVerticalPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        verticalPosition = this.transform.localPosition;
        startVerticalPosition = verticalPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.gameState == GameState.ACTIVE_COMBAT)
        {
            if (Input.GetKeyDown(lowerLanceKeyCode) && !releasedButton)
            {
                holdButton = true;
            }

            if (Input.GetKeyUp(lowerLanceKeyCode) && holdButton)
            {
                holdButton = false;
                releasedButton = true;
            }
        }
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
            lowerLanceKeyCode = KeyCode.W;
        }
        else
        {
            lowerLanceKeyCode = KeyCode.RightArrow;
        }
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
