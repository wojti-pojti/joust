using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;
using System.Linq;

public class ColorPicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Color output;
    private bool holding;
    private int r, g, b;
    private PointerEventData mostRecentEventData;

    [Header("")]
    [SerializeField] private Image colorPaletteImage;
    [SerializeField] private TMP_Text rgbText;
    [SerializeField] private GameObject colorPointer;
    [SerializeField] private float colorPointerTolerance;
    private Image colorPointerImage;

    private Texture2D texture;

    // Update is called once per frame
    void Update()
    {
        if (holding)
        {
            output = PickColor(Camera.main.WorldToScreenPoint(mostRecentEventData.position));

            CustomizationManager.Instance.SetNewColor(output);
            UpdateDisplay();
        }
    }

    private void OnEnable()
    {
        if (!texture) { texture = colorPaletteImage.sprite.texture; }
        if (!colorPointerImage) { colorPointerImage = colorPointer.GetComponent<Image>(); }
        UpdateDisplay();
    }

    /// <summary>
    /// This function is called on mouse button pressed.
    /// </summary>
    /// <param name="eventData">Information about the press.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        holding = true;
        mostRecentEventData = eventData;
        InvokeRepeating("UpdatePointerPosition", 0f, 0.075f);
    }

    /// <summary>
    /// This function is called on mouse button lifted.
    /// </summary>
    /// <param name="eventData">Information about the press.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        mostRecentEventData = eventData;
        CancelInvoke();

        UpdateDisplay();
        UpdatePointerPosition();
        holding = false;
        CustomizationManager.Instance.SetNewColor(output);
    }

    /// <summary>
    /// This function chooses the color, which is currently under the cursor.
    /// </summary>
    /// <param name="screenPoint">Position vector of the cursor.</param>
    /// <returns>The chosen color.</returns>
    Color PickColor(Vector2 screenPoint)
    {
        Vector2 point;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(colorPaletteImage.rectTransform, screenPoint, Camera.main, out point);

        point.x = Mathf.Clamp(point.x, colorPaletteImage.rectTransform.rect.xMin, colorPaletteImage.rectTransform.rect.xMax);
        point.y = Mathf.Clamp(point.y, colorPaletteImage.rectTransform.rect.yMin, colorPaletteImage.rectTransform.rect.yMax);

        point += colorPaletteImage.rectTransform.sizeDelta / 2;


        Vector2Int middlePoint = new Vector2Int((int)((texture.width * point.x) / colorPaletteImage.rectTransform.sizeDelta.x),
           (int)((texture.height * point.y) / colorPaletteImage.rectTransform.sizeDelta.y));

        return texture.GetPixel(middlePoint.x, middlePoint.y);
    }

    /// <summary>
    /// This function updates the RGB text in the color picker panel to reflect the selected color;
    /// </summary>
    public void UpdateDisplay()
    {
        r = (int)(output.r * 255);
        g = (int)(output.g * 255);
        b = (int)(output.b * 255);
        rgbText.text = "R: " + r.ToString() + "  G: " + g.ToString() + "  B: " + b.ToString();

        // display the position of the color on the image
        if (!holding) 
        {
            UpdatePointerPosition();
        }
    }

    void UpdatePointerPosition()
    {
        if (!colorPointer.activeSelf) { colorPointer.SetActive(true); }

        if (holding)
        {
            Vector2 mousePos = colorPaletteImage.rectTransform.InverseTransformPoint(mostRecentEventData.position);
            Vector2 clampedMousePosition = new Vector2(Mathf.Clamp(mousePos.x, colorPaletteImage.rectTransform.rect.xMin, colorPaletteImage.rectTransform.rect.xMax),
                Mathf.Clamp(mousePos.y, colorPaletteImage.rectTransform.rect.yMin, colorPaletteImage.rectTransform.rect.yMax));
            colorPointer.transform.position = colorPaletteImage.rectTransform.TransformPoint(clampedMousePosition);
        }
        else
        {
            Vector2Int point = FindColorInImage();
            if (point == new Vector2Int(-1, -1)) { return; }

            RectTransform paletteRect = colorPaletteImage.rectTransform;
            Vector2 relativePosition = new Vector2((float)point.x / texture.width, (float)point.y / texture.height);
            Vector2 localPoint = new Vector2(relativePosition.x * paletteRect.rect.width,
                relativePosition.y * paletteRect.rect.height) - paletteRect.rect.size / 2;

            Vector2 newPosition = paletteRect.TransformPoint(localPoint);

            colorPointer.transform.position = newPosition;
        }
          
        colorPointerImage.color = ContrastColor(output);
    }

    /// <summary>
    /// This function finds the position of the output color in the color palette image.
    /// </summary>
    /// <returns>The position vector of the pixel matching the given color.</returns>
    Vector2Int FindColorInImage()
    {
        Color[] pixels = texture.GetPixels();
        float[] differences = new float[pixels.Length];

        int width = texture.width;
        int height = texture.height;

        float minDiff = 100;
        int minX = -1;
        int minY = -1;

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color current = pixels[index];

                differences[index] = ColorDifference(output, current);

                if (differences[index] < minDiff)
                {
                    minDiff = differences[index];
                    minX = x;
                    minY = y;

                    if(minDiff < colorPointerTolerance) // stop early
                    {
                        return new Vector2Int(minX, minY);
                    }
                }
                
            }
        }
        return new Vector2Int(minX, minY);
    }

    /// <summary>
    /// Calculates a sum of the differences between the r,g,b components of two given colors.
    /// </summary>
    /// <param name="color1"></param>
    /// <param name="color2"></param>
    /// <returns>The calculated sum.</returns>
    float ColorDifference(Color color1, Color color2)
    {
        return Mathf.Abs(color1.r - color2.r) + Mathf.Abs(color1.g - color2.g) + Mathf.Abs(color1.b - color2.b);
    }

    /// <summary>
    /// This function determines a color meant to contrast the given color.
    /// </summary>
    /// <param name="color"></param>
    /// <returns>The contrasting color.</returns>
    Color ContrastColor(Color color)
    {
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        float newHue = 1 - h;
        float newSaturation = s;
        float newBrightness = (v > 0.5f ? 0.1f : 0.9f);
        return Color.HSVToRGB(newHue, newSaturation, newBrightness);
    }
}
