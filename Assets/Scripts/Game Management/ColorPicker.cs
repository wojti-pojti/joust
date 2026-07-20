using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

public class ColorPicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Color output;
    bool holding;
    int r, g, b;
    PointerEventData mostRecentEventData;

    [Header("")]
    [SerializeField] Image colorPaletteImage;
    [SerializeField] TMP_Text rgbText;

    // Update is called once per frame
    void Update()
    {
        if (holding)
        {
            output = PickColor(Camera.main.WorldToScreenPoint(mostRecentEventData.position), colorPaletteImage);

            CustomizationManager.Instance.SetNewColor(output);
            UpdateDisplay();
        }
    }

    private void OnEnable()
    {
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
    }

    /// <summary>
    /// This function is called on mouse button lifted.
    /// </summary>
    /// <param name="eventData">Information about the press.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        holding = false;
        mostRecentEventData = eventData;

        UpdateDisplay();
        CustomizationManager.Instance.SetNewColor(output);
    }

    /// <summary>
    /// This function chooses the color, which is currently under the cursor.
    /// </summary>
    /// <param name="screenPoint">Position vector of the cursor.</param>
    /// <param name="imageToPick">The image containing choosable colors.</param>
    /// <returns>The chosen color.</returns>
    Color PickColor(Vector2 screenPoint, Image imageToPick)
    {
        Vector2 point;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(imageToPick.rectTransform, screenPoint, Camera.main, out point);
        point += imageToPick.rectTransform.sizeDelta / 2;

        Texture2D texture = imageToPick.sprite.texture;
        Vector2Int middlePoint = new Vector2Int((int)((texture.width * point.x) / imageToPick.rectTransform.sizeDelta.x),
           (int)((texture.height * point.y) / imageToPick.rectTransform.sizeDelta.y));

        return texture.GetPixel(middlePoint.x, middlePoint.y);
    }

    /// <summary>
    /// This function updates the RGB text in the color picker panel to reflect the selected color;
    /// </summary>
    void UpdateDisplay()
    {
        r = (int)(output.r * 255);
        g = (int)(output.g * 255);
        b = (int)(output.b * 255);
        rgbText.text = "R: " + r.ToString() + "\tG: " + g.ToString() + "\tB: " + b.ToString();

        // maybe display the position of the color on the image
    }
}
