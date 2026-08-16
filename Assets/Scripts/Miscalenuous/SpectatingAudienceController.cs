using System.Collections;
using UnityEngine;

/// <summary>
/// A script attached to each UI element representing an audience member (spectator of the jousting match).
/// It is supposed to dynamically appear and disappear with Game UI, as well as react to the match ending.
/// </summary>
public class SpectatingAudienceController : MonoBehaviour
{
    [SerializeField] private bool capableOfApplause;
    [Header("")]
    [SerializeField] private Vector2 visiblePosition;
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;

    private RectTransform rt = null;

    private void Awake()
    {
        if (TryGetComponent<RectTransform>(out rt))
        {
            visiblePosition = rt.anchoredPosition;
            rt.anchoredPosition = hiddenPosition;
        }
        else
        {
            visiblePosition = transform.position;
            transform.position = hiddenPosition;
        }
    }

    #region Adding and removing this instance as listener
    void OnEnable() // subscribe to the event
    {
        GameManager.OnEndMatchEvent += Applause;
        GameManager.OnEnableGameUIEvent += Fade;
        GameManager.OnGameCloseEvent += StopAllProcesses;
    }

    void OnDisable() // unsubscribe to the event
    {
        GameManager.OnEndMatchEvent -= Applause;
        GameManager.OnEnableGameUIEvent -= Fade;
        GameManager.OnGameCloseEvent += StopAllProcesses;
    }
    #endregion

    /// <summary>
    /// Called upon closing the game. Stops all coroutines.
    /// </summary>
    /// <param name="placeholder">No function associated with this boolean.</param>
    void StopAllProcesses(bool placeholder)
    {
        CancelInvoke();
        StopAllCoroutines();
    }

    /// <summary>
    /// Simulate the reaction of the audience to the match end. The audience member should vibrate excitedly.
    /// </summary>
    /// <param name="reactionIndex">The index identifying the amount of excitement to portray.</param>
    void Applause(int reactionIndex)
    {
        if(!capableOfApplause) { return; }

        // determine specifics based on the reaction index
        // reaction index [0; 4]
        float duration = 2.5f, period = 0.3f, amplitude = 0.2f;

        duration += (reactionIndex + 1) * 0.5f;
        if (reactionIndex > 2) { period -= 0.1f; amplitude += 0.1f; }
        if (reactionIndex == 4) { period -= 0.1f; amplitude += 0.1f; }

        float randomDelay = Random.Range(0, 0.5f * period);
        StartCoroutine(ActExcited(duration, period, amplitude, randomDelay));
    }

    /// <summary>
    /// Called to smoothly move the audience member on-screen or off-screen.
    /// </summary>
    /// <param name="showThisGameobject">True if the object is to fade onto the screen, false otherwise.</param>
    void Fade(bool showThisGameobject)
    {
        if(showThisGameobject)
        {
            //Debug.Log(gameObject.name + " shifting position from " + this.gameObject.transform.position + " to " + visiblePosition);
            StartCoroutine(ShiftPosition(fadeInDuration, visiblePosition));
        }
        else
        {
            //Debug.Log(gameObject.name + " shifting position from " + this.gameObject.transform.position + " to " + hiddenPosition);
            StartCoroutine(ShiftPosition(fadeOutDuration, hiddenPosition));
        }
    }

    /// <summary>
    /// Smoothly moves the gameobjects position.
    /// </summary>
    /// <param name="duration">How long it takes in total for the object to complete its motion.</param>
    /// <param name="endPos">The target position of the gameobject.</param>
    /// <returns></returns>
    IEnumerator ShiftPosition(float duration, Vector2 endPos)
    {
        if (rt == null) { TryGetComponent<RectTransform>(out rt); }

        Vector2 startPos = transform.position;
        if (rt != null)
        {
            startPos = rt.anchoredPosition;
        }
        Vector2 delta = endPos - startPos;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float shift = easeCurve.Evaluate(elapsedTime / duration);

            Vector2 newPosition = startPos + shift * delta;
            if (rt != null)
            {
                rt.anchoredPosition = newPosition;
            }
            else
            {
                transform.position = newPosition;
            }
            yield return null;
        }

        if (rt != null)
        {
            rt.anchoredPosition = endPos;
        }
        else
        {
            transform.position = endPos;
        }
    }

    /// <summary>
    /// Simulates the member of audience acting excited by jumping repeatedly.
    /// </summary>
    /// <param name="duration">How long the gameobject should vibrate.</param>
    /// <param name="period">The duration of a single vibration.</param>
    /// <param name="amplitude">The difference between the highest and lowest position.</param>
    /// <param name="delay">The delay before the member starts moving.</param>
    /// <returns></returns>
    IEnumerator ActExcited(float duration, float period, float amplitude, float delay)
    {
        yield return new WaitForSeconds(delay);
        Coroutine currentShiftRoutine = null;
        if (this.gameObject.name == "audienceMember") Debug.Log(this.gameObject.name + " - duration: "+ duration + ", period: " + period + ", amplitude: " + amplitude);
        Vector2 startPos = visiblePosition;
        int cycles = 0;
        while (duration > 0f) 
        {
            if(currentShiftRoutine != null) { StopCoroutine(currentShiftRoutine); }
            currentShiftRoutine = StartCoroutine(ShiftPosition(0.5f * period, visiblePosition + new Vector2(0, rt.rect.size.y * amplitude * 0.5f)));
            yield return new WaitForSeconds(0.5f * period);
            StopCoroutine(currentShiftRoutine);
            currentShiftRoutine = StartCoroutine(ShiftPosition(0.5f * period, visiblePosition + new Vector2(0, rt.rect.size.y * -(amplitude * 0.5f))));
            yield return new WaitForSeconds(0.5f * period);
            duration -= period;
            cycles++;
        }
        if (this.gameObject.name == "audienceMember") Debug.Log(this.gameObject.name + " completed " + cycles + " cycles of movement.");
        ShiftPosition(0.2f, startPos);
    }
}
