using UnityEngine;

/// <summary>
/// This script can assign a random color scheme to the parent object. 
/// If the object is animated, this script activates the animation with a random delay.
/// </summary>
public class ColorSetup : MonoBehaviour
{
    [SerializeField] private Material originalMaterial;
    [SerializeField] private bool reassignColors = true;
    [Header("")]
    [SerializeField] private bool randomizeColor1;
    [SerializeField] private Color color1;
    [SerializeField] private bool randomizeColor2;
    [SerializeField] private Color color2;
    [Header("")]
    [SerializeField] private bool animated;
    [SerializeField] private float delayThreshhold;
    private float timer, delay;
    private Animator animator;

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

            AssignColors();
        }

        if (animated)
        {
            animator = GetComponent<Animator>();
            delay = Random.Range(0f, delayThreshhold);
            timer = 0f;
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
        float hue1, sat1, bri1, hue2, sat2, bri2;
        Color.RGBToHSV(color1, out hue1, out sat1, out bri1);
        Color.RGBToHSV(color2, out hue2, out sat2, out bri2);

        float sum1 = hue1 + 0.3f * sat1 + 0.3f * bri1;
        float sum2 = hue2 + 0.3f * sat2 + 0.3f * bri2;

        float ratio = (Mathf.Min(sum1, sum2)) / (Mathf.Max(sum1, sum2)); // value from 0 to 1, where 1 is identical

        if(ratio > threshhold)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Delayed assigning of colors, as it appears to fix an issue.
    /// </summary>
    /// <returns></returns>
    void AssignColors()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sharedMaterial = originalMaterial;

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color1", color1);
        mpb.SetColor("_Color2", color2);
        renderer.SetPropertyBlock(mpb);

        if (!animated)
        {
            this.enabled = false;
        }
    }
}
