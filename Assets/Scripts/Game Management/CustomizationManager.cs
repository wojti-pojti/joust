using System;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationManager : MonoBehaviour
{
    [SerializeField] GameObject customizationPanel;
    [Header("")]
    [SerializeField] Color[] colors = new Color[6];
    [SerializeField] Button[] colorFields = new Button[6];
    [SerializeField] Image[] colorDisplays = new Image[6];
    [SerializeField] GameObject colorPickerUI;

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
                    UpdateCustomizationPanel();
                }
                else 
                { 
                    Cursor.lockState = CursorLockMode.Locked; 
                    colorPickerUI.SetActive(false);
                }
                customizationPanel.SetActive(!customizationPanel.activeSelf);
            }
        }
    }

    public void ChooseNewColor(int index)
    {
        colorPickerUI.SetActive(true);
    }

    void UpdateCustomizationPanel()
    {
        for (int i = 0; i < colorDisplays.Length; i++) 
        {
            colorDisplays[i].color = colors[i];
        }
    }

    /// <summary>
    /// Applies all saved colors to the colorSwap shaders.
    /// </summary>
    void ApplyCustomizationSettings()
    {

    }
}
