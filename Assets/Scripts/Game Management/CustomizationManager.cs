using UnityEngine;
using UnityEngine.UI;

public class CustomizationManager : MonoBehaviour
{
    [SerializeField] GameObject customizationPanel;
    [SerializeField] int currentlyConsideredColorField;
    [SerializeField] Color selectedFieldColor;
    [Header("")]
    [SerializeField] Color[] colors = new Color[6];
    [SerializeField] Button[] colorFields = new Button[6];
    Image[] colorFieldDisplays = new Image[6];
    [SerializeField] Image[] colorDisplays = new Image[6];

    [SerializeField] GameObject colorPickerUI;
    [SerializeField] Material player1ColorSwapMaterial;
    [SerializeField] Material player2ColorSwapMaterial;

    Color baseButtonColor;
    ColorPicker picker;

    #region Singleton
    public static CustomizationManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadCustomizationSettigns();

        for (int i = 0; i < colorFieldDisplays.Length; i++) 
        {
            colorFieldDisplays[i] = colorFields[i].GetComponent<Image>();
        }
        baseButtonColor = colorFieldDisplays[0].color;
        colorPickerUI.SetActive(false);
        customizationPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.gameState == GameState.MENU)
        {
            if (Input.GetKeyDown(KeyCode.K)) 
            { 
                if (!customizationPanel.activeSelf) 
                { 
                    Cursor.lockState = CursorLockMode.Confined;
                    UpdateColorsArray();
                    UpdateCustomizationPanel();
                }
                else 
                { 
                    ApplyCustomizationSettings();
                    Cursor.lockState = CursorLockMode.Locked; 
                    colorPickerUI.SetActive(false);
                }
                customizationPanel.SetActive(!customizationPanel.activeSelf);
            }
        }
    }

    /// <summary>
    /// Begins the selection of the new color for the color field of a given index.
    /// </summary>
    /// <param name="index">The index of the color field to modify.</param>
    public void ChooseNewColor(int index)
    {
        currentlyConsideredColorField = index;
        foreach (var display in colorFieldDisplays) 
        {
            display.color = baseButtonColor;
        } 
        colorFieldDisplays[index].color = selectedFieldColor;
        colorPickerUI.SetActive(true);
        if (picker == null) picker = colorPickerUI.GetComponent<ColorPicker>();
        picker.output = colors[index];
        picker.enabled = false;
        picker.enabled = true;
    }

    /// <summary>
    /// Sets the color of the color field that is currently included in the selection process.
    /// </summary>
    /// <param name="color">The new color.</param>
    public void SetNewColor(Color color)
    {
        colors[currentlyConsideredColorField] = color;
        UpdateCustomizationPanel();
    }

    /// <summary>
    /// Assigns a random color to the color field that is currently included in the selection process.
    /// </summary>
    public void RandomizeColor()
    {
        Color newColor = new Color();
        newColor.a = 1;
        newColor.r = Random.Range(0f, 1f);
        newColor.g = Random.Range(0f, 1f);
        newColor.b = Random.Range(0f, 1f);
        SetNewColor(newColor);
    }

    /// <summary>
    /// Assigns random colors to all color fields related to a player.
    /// </summary>
    /// <param name="index">The player index identifying the player.</param>
    public void RandomizeAllPlayerColors(int index)
    {
        int startI = (index == 1 ? 0 : 1);
        for (int i = startI; i < colors.Length; i += 2)
        {
            currentlyConsideredColorField = i;
            RandomizeColor();
        }
    }

    /// <summary>
    /// Updates all color fields' displays to portray the assigned colors.
    /// </summary>
    void UpdateCustomizationPanel()
    {
        for (int i = 0; i < colorDisplays.Length; i++) 
        {
            if (colorDisplays[i] != null) colorDisplays[i].color = colors[i];
        }
    }

    /// <summary>
    /// Updates the array holding all relevant colors based on player materials.
    /// </summary>
    void UpdateColorsArray()
    {
        if (player1ColorSwapMaterial != null)
        {
            colors[0] = player1ColorSwapMaterial.GetColor("_Color1");
            colors[2] = player1ColorSwapMaterial.GetColor("_Color2");
            colors[4] = player1ColorSwapMaterial.GetColor("_MetallicColor");
        }
        if (player2ColorSwapMaterial != null)
        {
            colors[1] = player2ColorSwapMaterial.GetColor("_Color1");
            colors[3] = player2ColorSwapMaterial.GetColor("_Color2");
            colors[5] = player2ColorSwapMaterial.GetColor("_MetallicColor");
        }
    }

    /// <summary>
    /// Applies all saved colors to the colorSwap shaders.
    /// </summary>
    void ApplyCustomizationSettings()
    {
        if (player1ColorSwapMaterial != null) 
        {
            player1ColorSwapMaterial.SetColor("_Color1", colors[0]);
            player1ColorSwapMaterial.SetColor("_Color2", colors[2]);
            player1ColorSwapMaterial.SetColor("_MetallicColor", colors[4]);
        }
        if (player2ColorSwapMaterial != null)
        {
            player2ColorSwapMaterial.SetColor("_Color1", colors[1]);
            player2ColorSwapMaterial.SetColor("_Color2", colors[3]);
            player2ColorSwapMaterial.SetColor("_MetallicColor", colors[5]);
        }

        SaveCustomizationSettigns();
    }

    /// <summary>
    /// Saves all player colors using PlayerPrefs. The format is: (key: "color_index", value: "r g b a").
    /// </summary>
    void SaveCustomizationSettigns()
    {
        string colorString = "";
        string key = "";
        for (int i = 0; i < colors.Length; i++) 
        {
            colorString = colors[i].r + " " + colors[i].g + " " + colors[i].b + " " + colors[i].a;
            key = "color_" + i.ToString();
            PlayerPrefs.SetString(key, colorString);
        }
    }

    /// <summary>
    /// This function loads all player colors using PlayerPrefs, decoding the string encapsulation used when saving. 
    /// This is to keep consistent colors between game sessions.
    /// </summary>
    void LoadCustomizationSettigns()
    {
        string colorString = "";
        string key = "";
        string[] RGBs;
        for (int i = 0; i < colors.Length; i++)
        {
            key = "color_" + i.ToString();
            colorString = PlayerPrefs.GetString(key, colorString);
            RGBs = colorString.Split(' ');
            if (!float.TryParse(RGBs[0], out colors[i].r) ||
                !float.TryParse(RGBs[1], out colors[i].g) ||
                !float.TryParse(RGBs[2], out colors[i].b) ||
                !float.TryParse(RGBs[3], out colors[i].a)) { Debug.Log("loading color " + i + " failed"); }
        }
    }
}
