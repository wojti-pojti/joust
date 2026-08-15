using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This script is meant to control different acts of appearing and disappearing of UI or physical gameobjects.
/// </summary>
public class TransitionController : MonoBehaviour
{
    [Header("General")]
    public bool visible;
    public bool inTransition;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Shift")]
    [SerializeField] private bool transitionViaShift;
    [SerializeField] private Vector2 visiblePosition;
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private float shiftInDuration;
    [SerializeField] private float shiftOutDuration;

    [Header("Fade")]
    [SerializeField] private bool transitionViaFade;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private float fadeTimeStep;

    int childrenQuantity;
    SpriteRenderer exactSpriteRenderer;
    Image exactImage;
    TMP_Text exactText;
    SpriteRenderer[] associatedSpriteRenderers;
    Image[] associatedImages;
    TMP_Text[] associatedTexts;

    private RectTransform rt = null;

    private void Awake()
    {
        inTransition = false;
        if(transitionViaShift) TryGetComponent<RectTransform>(out rt);
        if(transitionViaFade)
        {
            childrenQuantity = this.transform.childCount;
            this.gameObject.TryGetComponent<SpriteRenderer>(out exactSpriteRenderer);
            this.gameObject.TryGetComponent<Image>(out exactImage);
            this.gameObject.TryGetComponent<TMP_Text>(out exactText);

            if (childrenQuantity > 0)
            {
                associatedSpriteRenderers = this.gameObject.GetComponentsInChildren<SpriteRenderer>();
                associatedImages = this.gameObject.GetComponentsInChildren<Image>();
                associatedTexts = this.gameObject.GetComponentsInChildren<TMP_Text>();
            }
        }
    }

    /// <summary>
    /// Called to smoothly move the object on-screen or off-screen.
    /// </summary>
    /// <param name="showThisGameobject">True if the object is to fade onto the screen, false otherwise.</param>
    /// <param name="instantenuously">True if exclude the assigned transition animation.</param>
    public void Appear(bool showThisGameobject, bool instantenuously = false)
    {
        if(inTransition && !instantenuously) // already during a transition, ignore input
        {
            Debug.LogWarning(this.gameObject.name + " cannot transition as another transition is in progress.");
            return;
        }

        if(showThisGameobject == visible)
        {
            Debug.LogWarning(this.gameObject.name + " already is " + (visible ? "" : "not") + " visible.");
            return;
        }

        if (instantenuously)
        {
            AppearInstantenuously(showThisGameobject);
            return;
        }

        if (transitionViaFade && transitionViaShift)
        {
            Debug.LogError(this.gameObject.name + "\tCannot perform transition as more than one type is selected.");
            AppearInstantenuously(showThisGameobject);
            return;
        }

        inTransition = true;
        if (showThisGameobject)
        {
            if (transitionViaShift) StartCoroutine(ShiftPosition(shiftInDuration, visiblePosition));
            if (transitionViaFade) StartCoroutine(FadeGameObject(fadeInDuration, true));
        }
        else
        {
            if (transitionViaShift) StartCoroutine(ShiftPosition(shiftOutDuration, hiddenPosition));
            if (transitionViaFade) StartCoroutine(FadeGameObject(fadeOutDuration, false));
        }
    }

    /// <summary>
    /// Makes the object appear or disappear without a smooth traansition.
    /// </summary>
    /// <param name="show">True to make it visible, false to make it hidden.</param>
    void AppearInstantenuously(bool show)
    {
        if (show) 
        {
            if (transitionViaFade)
            {
                AdjustOpacity(1f);
            }
            if (transitionViaShift) 
            {
                if (rt == null) transform.position = visiblePosition;
                else rt.anchoredPosition = visiblePosition;
            }
        }
        else
        {
            if (transitionViaFade)
            {
                AdjustOpacity(0f);
            }
            if (transitionViaShift)
            {
                if (rt == null) transform.position = hiddenPosition;
                else rt.anchoredPosition = hiddenPosition;
            }
        }
        visible = show;
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
        visible = !visible;
        inTransition = false;
    }

    /// <summary>
    /// Gradually changes opacity of the gameobject and all its' children to make it appear or disappear.
    /// </summary>
    /// <param name="duration">Duration of the animation.</param>
    /// <param name="show">The final effect of the transition.</param>
    /// <returns></returns>
    IEnumerator FadeGameObject(float duration, bool show)
    {
        yield return null;
        if (fadeTimeStep <= 0) { fadeTimeStep = 0.1f; }
        float startOpacity = (show ? 0f : 1f);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += fadeTimeStep;
            float change = easeCurve.Evaluate(elapsedTime / duration);
            if (!show) change *= -1f;
            AdjustOpacity(startOpacity + change);

            yield return new WaitForSeconds(fadeTimeStep);
        }
        
        visible = show;
        inTransition = false;
    }

    /// <summary>
    /// Sets the opacity of this object and all its children to the given value.
    /// </summary>
    /// <param name="newOpacity">The new opacity.</param>
    void AdjustOpacity(float newOpacity)
    {
        if(exactSpriteRenderer)
        {
            Color newColor = exactSpriteRenderer.color;
            newColor.a = newOpacity;
            exactSpriteRenderer.color = newColor;
        }
        if (exactImage)
        {
            Color newColor = exactImage.color;
            newColor.a = newOpacity;
            exactImage.color = newColor;
        }
        if (exactText)
        {
            Color newColor = exactText.color;
            newColor.a = newOpacity;
            exactText.color = newColor;
        }

        if (childrenQuantity > 0)
        {
            foreach (SpriteRenderer visual in associatedSpriteRenderers)
            {
                Color newColor = visual.color;
                newColor.a = newOpacity;
                visual.color = newColor;
            }
            foreach (Image visual in associatedImages)
            {
                Color newColor = visual.color;
                newColor.a = newOpacity;
                visual.color = newColor;
            }
            foreach (TMP_Text visual in associatedTexts)
            {
                Color newColor = visual.color;
                newColor.a = newOpacity;
                visual.color = newColor;
            }
        }
    }
}
