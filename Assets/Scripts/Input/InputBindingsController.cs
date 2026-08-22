using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// This class is used to control which input scheme or device is currently used by each player.
/// </summary>
public class InputBindingsController : MonoBehaviour
{
    public int currentPlayer1Input;
    public int currentPlayer2Input;
    [Header("")]
    [HideInInspector] public Gamepad player1Gamepad {  get; private set; }
    [HideInInspector] public Gamepad player2Gamepad { get; private set; }
    [Header("UI")]
    [SerializeField] private TMP_Dropdown player1Dropdown;
    [SerializeField] private TMP_Dropdown player2Dropdown;
    [SerializeField] private TMP_Text player1ConnectionText;
    [SerializeField] private TMP_Text player2ConnectionText;
    [SerializeField] private GameObject[] player1BindingDisplays = new GameObject[3];
    [SerializeField] private GameObject[] player2BindingDisplays = new GameObject[3];

    #region Singleton
    public static InputBindingsController Instance;
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

        player1Dropdown.ClearOptions();
        player2Dropdown.ClearOptions();

        List<string> inputOptions = new List<string> { "WASD", "Arrows", "Gamepad" };
        player1Dropdown.AddOptions(inputOptions);
        player2Dropdown.AddOptions(inputOptions);

        player1Dropdown.value = 0;
        currentPlayer1Input = 0;
        player1Dropdown.RefreshShownValue();
        player2Dropdown.value = 1;
        currentPlayer2Input = 1;
        player2Dropdown.RefreshShownValue();
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateControlSchemeDisplays();
    }

    private void OnEnable()
    {
        player1Dropdown.onValueChanged.AddListener(OnPlayer1DropdownChanged);
        player2Dropdown.onValueChanged.AddListener(OnPlayer2DropdownChanged);
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        player1Dropdown.onValueChanged.RemoveListener(OnPlayer1DropdownChanged);
        player2Dropdown.onValueChanged.RemoveListener(OnPlayer2DropdownChanged);
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    /// <summary>
    /// Called specifically to react to when a gamepad is connected.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="change"></param>
    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if(device.GetType() != typeof(Gamepad)) { return; }

        Gamepad gamepad = (Gamepad)device;

        if (change == InputDeviceChange.Added) 
        {
            GameManager.Instance.DisplayMessage("Gamepad connected", 1f);
            if (currentPlayer1Input == 2 && player1Gamepad != null)
            {
                player1Gamepad = gamepad;
                GameManager.Instance.AssignGamepadToPlayer(1, gamepad);
            }
            else if (currentPlayer2Input == 2 && player2Gamepad != null)
            {
                player2Gamepad = gamepad;
                GameManager.Instance.AssignGamepadToPlayer(2, gamepad);
            }
            else
            {
                Debug.Log("New gamepad connected, but no player has Gamepad control scheme assigned.");
            }
        }

        if (change == InputDeviceChange.Removed)
        {
            if (gamepad == player1Gamepad) 
            { 
                player1Gamepad = null;
                GameManager.Instance.RemoveGamepadFromPlayer(1, gamepad);
            }
            if (gamepad == player2Gamepad) 
            {
                player2Gamepad = null;
                GameManager.Instance.RemoveGamepadFromPlayer(2, gamepad);
            }
        }

        UpdateControlSchemeDisplays();
    }

    #region Dropdown
    /// <summary>
    /// This function handles the player specific changes upon a binding dropdown field being modified.
    /// </summary>
    /// <param name="index">Input option index.</param>
    void OnPlayer1DropdownChanged(int index)
    {
        currentPlayer1Input = index;

        if (currentPlayer2Input == currentPlayer1Input && currentPlayer1Input < 2) // excluding gamepad
        {
            currentPlayer2Input = (index == 0 ? 1 : 0);
            player2Dropdown.value = currentPlayer2Input;
        }
        OnDropdownChanged(1, player1Dropdown.value);
    }

    /// <summary>
    /// This function handles the player specific changes upon a binding dropdown field being modified.
    /// </summary>
    /// <param name="index">Input option index.</param>
    void OnPlayer2DropdownChanged(int index)
    {
        currentPlayer2Input = index;

        if (currentPlayer2Input == currentPlayer1Input && currentPlayer2Input < 2) // excluding gamepad
        {
            currentPlayer1Input = (index == 0 ? 1 : 0);
            player1Dropdown.value = currentPlayer1Input;
        }
        OnDropdownChanged(2, player2Dropdown.value);
    }

    /// <summary>
    /// This function is called whenever a dropdown field is modified, assigning the right control scheme to the right player.
    /// </summary>
    /// <param name="playerIndex">Index of the considered player.</param>
    /// <param name="index">Index of the input option.</param>
    void OnDropdownChanged(int playerIndex, int index)
    {
        switch (index)
        {
            case 0: // WASD
                GameManager.Instance.AssignControlSchemeToPlayer(playerIndex, "KeyboardP1");
                break;

            case 1: // arrows
                GameManager.Instance.AssignControlSchemeToPlayer(playerIndex, "KeyboardP2");
                break;

            case 2: // gamepad
                GameManager.Instance.AssignControlSchemeToPlayer(playerIndex, "Gamepad");
                break;

            default:
                Debug.LogWarning("There is no corresponding input scheme. Something went wrong.");
                break;
        }

        UpdateControlSchemeDisplays();
    }
    #endregion

    /// <summary>
    /// Updates the control displays based on assigned control schemes.
    /// </summary>
    void UpdateControlSchemeDisplays()
    {
        player1Dropdown.RefreshShownValue();
        player2Dropdown.RefreshShownValue();

        foreach (GameObject display in player1BindingDisplays)
        {
            display.SetActive(false);
        }
        foreach (GameObject display in player2BindingDisplays)
        {
            display.SetActive(false);
        }
        player1BindingDisplays[currentPlayer1Input].SetActive(true);
        player2BindingDisplays[currentPlayer2Input].SetActive(true);

        // indicating the gamepad connection state
        if(currentPlayer1Input == 2)
        {
            if (player1Gamepad != null)
            {
                player1ConnectionText.text = "connected";
                player1ConnectionText.color = Color.white;
            }
            else
            {
                player1ConnectionText.text = "not connected";
                player1ConnectionText.color = Color.red;
            }
        }
        if (currentPlayer2Input == 2)
        {
            if (player2Gamepad != null)
            {
                player2ConnectionText.text = "connected";
                player2ConnectionText.color = Color.white;
            }
            else
            {
                player2ConnectionText.text = "not connected";
                player2ConnectionText.color = Color.red;
            }
        }
    }
}
