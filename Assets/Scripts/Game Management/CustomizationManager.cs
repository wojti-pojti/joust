using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CustomizationManager : MonoBehaviour
{
    [SerializeField] private TransitionController customizationPanel;
    [SerializeField] private int currentlyConsideredColorField;
    [SerializeField] private Color selectedFieldColor;
    [Header("")]
    [SerializeField] private Color[] colors = new Color[6];
    [SerializeField] private Button[] colorFields = new Button[6];
    private Image[] colorFieldDisplays = new Image[6];
    [SerializeField] private Image[] colorDisplays = new Image[6];

    [SerializeField] private GameObject colorPickerUI;
    [Header("Affected materials")]
    public Material player1ColorSwapMaterial;
    public Material player2ColorSwapMaterial;
    public Material player1StaticMaterial;
    public Material player2StaticMaterial;

    private Color baseButtonColor;
    private ColorPicker picker;

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
        int numberOfSessions = PlayerPrefs.GetInt("SessionID");
        if (numberOfSessions > 0)
        {
            LoadCustomizationSettigns();
        }
        else
        {
            AssignStartingColors();
        }

        for (int i = 0; i < colorFieldDisplays.Length; i++) 
        {
            colorFieldDisplays[i] = colorFields[i].GetComponent<Image>();
        }
        baseButtonColor = colorFieldDisplays[0].color;
        colorPickerUI.SetActive(false);
        customizationPanel.Appear(false, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.gameState == GameState.MENU)
        {
            if (Input.GetKeyDown(KeyCode.K)) 
            {
                SoundManager.Instance.PlaySound(SoundType.INTERACT_SOUND);
                if (!customizationPanel.visible) 
                { 
                    Cursor.lockState = CursorLockMode.Confined;
                    UpdateColorsArray();
                    UpdateCustomizationPanel();
                }
                else 
                {
                    colorFieldDisplays[currentlyConsideredColorField].color = baseButtonColor;
                    ApplyCustomizationSettings();
                    Cursor.lockState = CursorLockMode.Locked; 
                    colorPickerUI.SetActive(false);
                }
                customizationPanel.Appear(!customizationPanel.visible);
            }
        }
    }

    /// <summary>
    /// Begins the selection of the new color for the color field of a given index.
    /// </summary>
    /// <param name="index">The index of the color field to modify.</param>
    public void ChooseNewColor(int index)
    {
        colorFieldDisplays[currentlyConsideredColorField].color = baseButtonColor;
        currentlyConsideredColorField = index;
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

        if (colorPickerUI.activeSelf) { picker.UpdateDisplay(); }
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
        if (player1StaticMaterial != null)
        {
            player1StaticMaterial.SetColor("_Color1", colors[0]);
            player1StaticMaterial.SetColor("_Color2", colors[2]);
            player1StaticMaterial.SetColor("_MetallicColor", colors[4]);
        }
        if (player2ColorSwapMaterial != null)
        {
            player2ColorSwapMaterial.SetColor("_Color1", colors[1]);
            player2ColorSwapMaterial.SetColor("_Color2", colors[3]);
            player2ColorSwapMaterial.SetColor("_MetallicColor", colors[5]);
        }
        if (player2StaticMaterial != null)
        {
            player2StaticMaterial.SetColor("_Color1", colors[1]);
            player2StaticMaterial.SetColor("_Color2", colors[3]);
            player2StaticMaterial.SetColor("_MetallicColor", colors[5]);
        }

        SaveCustomizationSettigns();
    }

    /// <summary>
    /// This function assigns player colors if it is the first game session.
    /// </summary>
    void AssignStartingColors()
    {
        colors[0] = new Color(0.16f, 0.84f, 0.16f, 1f);
        colors[1] = new Color(0.03f, 0.04f, 0.03f, 1f);
        colors[2] = new Color(0.92f, 0.94f, 0.06f, 1f);
        colors[3] = new Color(0.98f, 0.54f, 0.02f, 1f);
        colors[4] = new Color(0.83f, 0.53f, 0.17f, 1f);
        colors[5] = new Color(0.4f, 0.4f, 0.4f, 1f);

        SaveCustomizationSettigns();
        ApplyCustomizationSettings();
    }

    #region Loading and saving
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
            // parse each number into the color
            if (!float.TryParse(RGBs[0], out colors[i].r) ||
                !float.TryParse(RGBs[1], out colors[i].g) ||
                !float.TryParse(RGBs[2], out colors[i].b) ||
                !float.TryParse(RGBs[3], out colors[i].a)) { Debug.Log("loading color " + i + " failed"); }
        }

        ApplyCustomizationSettings();
    }
    #endregion
}
