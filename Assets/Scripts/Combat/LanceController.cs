using System.Collections;
using UnityEngine;

public class LanceController : MonoBehaviour
{
    [SerializeField] bool holdButton;
    [SerializeField] bool releasedButton;
    //[SerializeField] float charge;
    [SerializeField] float chargeAccumulationMultiplier;
    [SerializeField] float directionMultiplier;

    [Header("")]
    [SerializeField] KeyCode lowerLanceKeyCode;
    Rigidbody2D rb;
    HingeJoint2D joint;
    Vector3 verticalPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        joint = GetComponent<HingeJoint2D>();
        verticalPosition = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.isInCombat)
        {
            if (Input.GetKeyDown(lowerLanceKeyCode) && !releasedButton)
            {
                holdButton = true;
            }

            if (Input.GetKeyUp(lowerLanceKeyCode))
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
    /// Returns the lance to its initial, vertical position.
    /// </summary>
    public void RaiseBackToPosition()
    {
        JointMotor2D newMotor = joint.motor;
        newMotor.motorSpeed = 0;
        joint.motor = newMotor;

        this.transform.rotation = new Quaternion(0, 0, 0, 0);
        this.transform.localPosition = verticalPosition;

        holdButton = false;
        releasedButton = false;
    }

    /// <summary>
    /// Called initially to assign the input keycode based on starting side;
    /// </summary>
    /// <param name="side">False for left, True for right.</param>
    public void AssignInputKey(bool side)
    {
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
      // Debug.Log(directionMultiplier + " - New limits assigned: min:" + newLimits.min.ToString() + ", max: " + newLimits.max.ToString());
    }
}
