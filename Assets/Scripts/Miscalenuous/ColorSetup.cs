using System.Collections;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This script can assign a random color scheme to the parent object. 
/// If the object is animated, this script activates the animation with a random delay.
/// </summary>
public class ColorSetup : MonoBehaviour
{
    [SerializeField] Material originalMaterial;
    [SerializeField] bool reassignColors = true;
    [Header("")]
    [SerializeField] bool randomizeColor1;
    [SerializeField] Color color1;
    [SerializeField] bool randomizeColor2;
    [SerializeField] Color color2;
    [Header("")]
    [SerializeField] bool animated;
    float timer, delay;
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (reassignColors)
        {
            if (randomizeColor1 || randomizeColor2) 
            {
                do
                {
                    if (randomizeColor1)
                    {
                        color1 = RandomColor();
                    }
                    if (randomizeColor2)
                    {
                        color2 = RandomColor();
                    }
                } while (AreColorsTooSimilar(color1, color2, 0.8f));
            }

            StartCoroutine(AssignColors());
        }

        if (animated)
        {
            animator = GetComponent<Animator>();
            delay = Random.Range(0f, 1.49f);
            timer = 0f;
        }
        else 
        {
            this.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (animated) 
        { 
            if(timer < delay)
            {
                timer += Time.fixedDeltaTime;
            }
            else
            {
                animator.SetTrigger("Activate");
                this.enabled = false;
            }
        }
    }

    /// <summary>
    /// Generates a random color. The saturation and brightness are predetermined to stay near maximum to keep the colors vibrant.
    /// </summary>
    /// <returns>The randomly generated color.</returns>
    Color RandomColor()
    {
        Color newColor = new Color();
        float hue = Random.Range(0f, 1f);
        float saturation = Random.Range(0.85f, 1f);
        float brightness = Random.Range(0.85f, 1f);
        newColor = Color.HSVToRGB(hue, saturation, brightness);
        return newColor;
    }

    /// <summary>
    /// This function compares to colors based on their rgb values. It calculates a ratio and compares it to the gives threshhold
    /// </summary>
    /// <param name="color1">The first color.</param>
    /// <param name="color2">The second color.</param>
    /// <param name="threshhold">How similar the colors have to be. Ranges from 0 to 1.</param>
    /// <returns></returns>
    bool AreColorsTooSimilar(Color color1, Color color2, float threshhold)
    {
        float sum1 = color1.r + color1.g + color1.b;
        float sum2 = color2.r + color2.g + color2.b;

        float ratio = (Mathf.Min(sum1, sum2)) / (Mathf.Max(sum1, sum2)); // value from 0 to 1, where 1 is identical

        if(ratio > threshhold)
        {
            //Debug.Log("Repeating color randomization.\t" + this.gameObject.name + " " + ratio);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Delayed assigning of colors, as it appears to fix an issue.
    /// </summary>
    /// <returns></returns>
    IEnumerator AssignColors()
    {
        yield return new WaitForEndOfFrame();
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sharedMaterial = originalMaterial;

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color1", color1);
        mpb.SetColor("_Color2", color2);
        renderer.SetPropertyBlock(mpb);
    }
}
