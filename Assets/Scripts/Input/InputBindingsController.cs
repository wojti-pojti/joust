using NUnit.Framework;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputBindingsController : MonoBehaviour
{
    [SerializeField] int currentPlayer1Input;
    [SerializeField] int currentPlayer2Input;
    [Header("UI")]
    [SerializeField] private TMP_Dropdown player1Dropdown;
    [SerializeField] private TMP_Dropdown player2Dropdown;
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
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        UpdateControlSchemeDisplays();
    }

    #region Dropdown
    private void OnEnable()
    {
        player1Dropdown.onValueChanged.AddListener(OnPlayer1DropdownChanged);
        player2Dropdown.onValueChanged.AddListener(OnPlayer2DropdownChanged);
    }

    private void OnDisable()
    {
        player1Dropdown.onValueChanged.RemoveListener(OnPlayer1DropdownChanged);
        player2Dropdown.onValueChanged.RemoveListener(OnPlayer2DropdownChanged);
    }

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
    }
}
