using System.Collections;
using UnityEngine;

/// <summary>
/// A script attached to each UI element representing an audience member (spectator of the jousting match).
/// It is supposed to dynamically appear and disappear with Game UI, as well as react to the match ending.
/// </summary>
public class SpectatingAudienceController : MonoBehaviour
{
    [SerializeField] Vector2 startPosition;
    [Header("")]
    [SerializeField] Vector2 leaveVector;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] float fadeInDuration;
    [SerializeField] float fadeOutDuration;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
    }

    #region Adding and removing this instance as listener
    void OnEnable() // subscribe to the event
    {
        GameManager.OnEndMatchEvent += Applause;
    }

    void OnDisable() // unsubscribe to the event
    {
        GameManager.OnEndMatchEvent -= Applause;
    }
    #endregion

    /// <summary>
    /// Simulate the reaction of the audience to the match end. The audience member should vibrate excitedly.
    /// </summary>
    /// <param name="reactionIndex">The index identifying the amount of excitement to portray.</param>
    void Applause(int reactionIndex)
    {
        // determine specifics based on the reaction index


        StartCoroutine(ActExcited(5f, 0.5f, 0.5f));
    }


    // make the following activated via events

    /// <summary>
    /// Called to smoothly move the audience member on-screen.
    /// </summary>
    public void FadeIn()
    {
        StartCoroutine(ShiftPosition(fadeInDuration, startPosition));
    }

    /// <summary>
    /// Called to smoothly move the audience member off-screen.
    /// </summary>
    public void FadeOut()
    {
        StartCoroutine(ShiftPosition(fadeOutDuration, startPosition + leaveVector));
    }

    /// <summary>
    /// Smoothly moves the gameobjects position.
    /// </summary>
    /// <param name="duration">How long it takes in total for the object to complete its motion.</param>
    /// <param name="endPos">The target position of the gameobject.</param>
    /// <returns></returns>
    IEnumerator ShiftPosition(float duration, Vector2 endPos)
    {
        Vector2 startPos = transform.position;
        Vector2 delta = startPos - endPos;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float shift = easeCurve.Evaluate(elapsedTime / duration);

            Vector2 newPosition = startPos + shift * delta;
            transform.position = newPosition;
            yield return null;
        }

        transform.position = endPos;
    }

    /// <summary>
    /// Simulates the member of audience acting excited by jumping repeatedly.
    /// </summary>
    /// <param name="duration">How long the gameobject should vibrate.</param>
    /// <param name="period">The duration of a single vibration.</param>
    /// <param name="amplitude">The difference between the highest and lowest position.</param>
    /// <returns></returns>
    IEnumerator ActExcited(float duration, float period, float amplitude)
    {
        while (duration > 0f) 
        {
            StartCoroutine(ShiftPosition(0.5f * period, (Vector2)transform.position + new Vector2(0, amplitude * 0.5f)));
            yield return new WaitForSeconds(0.5f * period);
            StartCoroutine(ShiftPosition(0.5f * period, (Vector2)transform.position + new Vector2(0, -(amplitude * 0.5f))));
            yield return new WaitForSeconds(0.5f * period);
            duration -= period;
        }
    }
}
