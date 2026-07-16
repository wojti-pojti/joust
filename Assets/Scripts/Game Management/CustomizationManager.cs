using UnityEngine;
using UnityEngine.UI;

public class CustomizationManager : MonoBehaviour
{
    [SerializeField] GameObject customizationPanel;
    [Header("")]
    [SerializeField] Button[] colorFields = new Button[6];
    [SerializeField] Image[] colorDisplays = new Image[6];

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
        customizationPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.gameState == GameState.MENU)
        {
            if (Input.GetKeyDown(KeyCode.K)) 
            { 
                customizationPanel.SetActive(!customizationPanel.activeSelf);
                if (customizationPanel.activeSelf) 
                { 
                    Cursor.lockState = CursorLockMode.Confined;
                    UpdateCustomizationPanel();
                }
                else { Cursor.lockState = CursorLockMode.Locked; }
            }
        }
    }

    void UpdateColorField()
    {

    }

    void UpdateCustomizationPanel()
    {

    }

    void ApplyCustomizationSettings()
    {

    }
}
