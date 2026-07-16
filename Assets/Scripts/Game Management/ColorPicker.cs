using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ColorPicker : MonoBehaviour, IPointerClickHandler
{
    public Color output;
    [Header("")]
    [SerializeField] Image colorPaletteImage;
    [SerializeField] TMP_Text rgbText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This function is called on mouse click to update the picked color based on where the cursor lands on the given image.
    /// </summary>
    /// <param name="eventData">Information about the click.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        output = PickColor(Camera.main.WorldToScreenPoint(eventData.position), colorPaletteImage);
        rgbText.text = "R: " + output.r.ToString() + "\tG: " + output.g.ToString() + "\tB: " + output.b.ToString();
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

        Texture2D texture = GetComponent<Image>().sprite.texture;
        Vector2Int middlePoint = new Vector2Int((int)((texture.width * point.x) / imageToPick.rectTransform.sizeDelta.x),
           (int)((texture.height * point.y) / imageToPick.rectTransform.sizeDelta.y));

        return texture.GetPixel(middlePoint.x, middlePoint.y);
    }
}
