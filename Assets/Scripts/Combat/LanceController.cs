using System.Collections;
using UnityEngine;

public class LanceController : MonoBehaviour
{
    [SerializeField] bool holdButton;
    [SerializeField] bool releasedButton;
    //[SerializeField] float charge;
    [SerializeField] float chargeAccumulationMultiplier;

    [Header("")]
    [SerializeField] KeyCode lowerLanceKeyCode;
    Rigidbody2D rb;
    HingeJoint2D joint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        joint = GetComponent<HingeJoint2D>();
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
            newMotor.motorSpeed = chargeAccumulationMultiplier;
            joint.motor = newMotor;
        }

        if (!holdButton && joint.motor.motorSpeed > 0)
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

        rb.SetRotation(0);

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

            StartCoroutine(ReverseHingeDirection());   
        }
    }

    /// <summary>
    /// Delayed for avoiding setup issues.
    /// </summary>
    /// <returns></returns>
    IEnumerator ReverseHingeDirection()
    {
        yield return new WaitForEndOfFrame();
        JointAngleLimits2D newLimits = new JointAngleLimits2D();
        newLimits.min = -90;
        newLimits.max = 0;
        joint.limits = newLimits;

        chargeAccumulationMultiplier *= -1f;
    }
}
