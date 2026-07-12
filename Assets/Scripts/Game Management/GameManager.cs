using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game state")]
    public bool isFightActive;
    public bool isInCombat;
    bool isAfterMatch = false;
    [SerializeField] int turnsPlayed;

    [Header("Players")]
    public GameObject player1;
    PlayerScript pScript1;
    HorseMovement horse1;
    bool hasPlayer1ArrivedToEndZone;

    public GameObject player2;
    PlayerScript pScript2;
    HorseMovement horse2;
    bool hasPlayer2ArrivedToEndZone;

    [Header("Settings")]
    public float gameConditionsCheckInterval;

    [Header("Arena")]
    [SerializeField] Transform LeftStartPos;
    [SerializeField] Transform RightStartPos;

    [Header("UI")]
    [SerializeField] GameObject menuUI;
    [SerializeField] GameObject gameUI;
    [SerializeField] TMP_Text turnCounter;
    [SerializeField] GameObject messagePanel;
    [SerializeField] TMP_Text message;
    [SerializeField] GameObject controlsPanel;

    #region Singleton
    public static GameManager Instance;
    private void Awake()
    {
        if(Instance == null)
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
        pScript1 = player1.GetComponent<PlayerScript>();
        pScript2 = player2.GetComponent<PlayerScript>();
        PrepareMatch();
        InvokeRepeating("CheckForEndTurnConditions", 0f, gameConditionsCheckInterval);

        menuUI.SetActive(true);
        controlsPanel.SetActive(false);
        gameUI.SetActive(false);
        messagePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            if (isAfterMatch) 
            { 
                isAfterMatch = false;
                PrepareMatch();

                // display menu screen
            }
            else if(!isFightActive && !isInCombat) 
            {
                StartCoroutine(StartMatch());
            }     
        }

        if (Input.GetKeyDown(KeyCode.G) && !isFightActive) // or whatever
        {
            // show or hide controls panel
            controlsPanel.SetActive(!controlsPanel.activeSelf);
            menuUI.SetActive(!menuUI.activeSelf);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (isFightActive)
            {
                StartCoroutine(ForfeitMatch());
            }
            else
            {
                Application.Quit();
            }
        }
    }

    #region Match initialization

    /// <summary>
    /// This function sets up initial values and positions for both players to prepare for a new match.
    /// </summary>
    public void PrepareMatch()
    {
        pScript1.state = PlayerState.IDLE;
        pScript2.state = PlayerState.IDLE;
        if (horse1 == null) horse1 = player1.GetComponentInChildren<HorseMovement>();
        if (horse2 == null) horse2 = player2.GetComponentInChildren<HorseMovement>();

        // starts on the left
        player1.GetComponent<Rigidbody2D>().MovePosition(LeftStartPos.position);
        horse1.Setup(false);

        // starts on the right
        player2.GetComponent<Rigidbody2D>().MovePosition(RightStartPos.position);
        horse2.Setup(true);

        Debug.Log("New match prepared.");
    }

    /// <summary>
    /// This function is called to start a new match.
    /// </summary>
    /// <returns></returns>
    IEnumerator StartMatch()
    {
        Debug.Log("Beginning new match...");
        turnsPlayed = 0;
        hasPlayer1ArrivedToEndZone = false;
        hasPlayer2ArrivedToEndZone = false;
        isFightActive = true;
        CameraController.Instance.dynamic = true;

        pScript1.ResetPlayerState();
        pScript2.ResetPlayerState();

        yield return new WaitForSeconds(1.5f);

        gameUI.SetActive(true);
        menuUI.SetActive(false);
        isInCombat = true;
        pScript1.state = PlayerState.COMBAT;
        pScript2.state = PlayerState.COMBAT;
        Debug.Log("JOUST!");
        StartCoroutine(ShowMessage("JOUST!"));
    }
    #endregion

    #region Change match state
    /// <summary>
    /// This function is called when Escape button is pressed during a match. It returns the game back to menu.
    /// </summary>
    /// <returns></returns>
    IEnumerator ForfeitMatch()
    {
        Debug.Log("The match has been forfeited. Returning to menu.");
        StartCoroutine(ShowMessage("Match forfeited"));

        isInCombat = false;
        yield return new WaitForSeconds(1.5f);
        gameUI.SetActive(false);
        menuUI.SetActive(true);
        isFightActive = false;
        CameraController.Instance.dynamic = false;
    }

    /// <summary>
    /// This function is called when one player is defeated. It ends the game loop, shows results and returns the game to menu.
    /// </summary>
    /// <param name="winnerIndex">The index of the last player standing.</param>
    /// <returns></returns>
    IEnumerator EndMatch(int winnerIndex)
    {
        turnsPlayed++;
        isFightActive = false;
        CameraController.Instance.dynamic = false;

        if (winnerIndex == 0)
        {
            // draw
            StartCoroutine(ShowMessage("Draw!"));
        }

        if (winnerIndex == 1)
        {
            horse1.TurnAround();
            StartCoroutine(ShowMessage("Player 1 wins!"));
        }
        if (winnerIndex == 2)
        {
            horse2.TurnAround();
            StartCoroutine(ShowMessage("Player 2 wins!"));
        }

        yield return new WaitForSeconds(1.5f);

        // display result / some fancy animation

        yield return new WaitForSeconds(1f);

        // display input prompt
        gameUI.SetActive(false);
        menuUI.SetActive(true);
        isAfterMatch = true;
    }

    /// <summary>
    /// Called after each turn to begin a new one.
    /// </summary>
    /// <returns></returns>
    IEnumerator StartNewTurn()
    {
        isInCombat = false;
        pScript1.Charge(false);
        pScript2.Charge(false);

        turnsPlayed++;
        turnCounter.text = turnsPlayed.ToString();
        Debug.Log("Turn " + turnsPlayed.ToString() + " completed.");

        horse1.TurnAround();
        horse2.TurnAround();

        hasPlayer1ArrivedToEndZone = false;
        hasPlayer2ArrivedToEndZone = false;

        // visually turn both players around via scale
        player1.transform.localScale.Set(-1f * player1.transform.localScale.x, player1.transform.localScale.y, player1.transform.localScale.z);
        player2.transform.localScale.Set(-1f * player2.transform.localScale.x, player2.transform.localScale.y, player2.transform.localScale.z);

        yield return new WaitForSeconds(3f);

        Debug.Log("Beginning turn " + (turnsPlayed + 1).ToString());
        isInCombat = true;
        pScript1.Charge(true);
        pScript2.Charge(true);
    }

    /// <summary>
    /// Called periodically to check for conditions that result in the end of turn or match.
    /// </summary>
    void CheckForEndTurnConditions()
    {
        if (!isFightActive || !isInCombat) { return; }

        // both players survived the turn
        if (hasPlayer1ArrivedToEndZone && hasPlayer2ArrivedToEndZone)
        {
            StartCoroutine(StartNewTurn());
            return;
        }
        if (hasPlayer1ArrivedToEndZone && pScript2.state == PlayerState.DEAD)
        {
            StartCoroutine(EndMatch(1));
            return;
        }
        if (hasPlayer2ArrivedToEndZone && pScript1.state == PlayerState.DEAD)
        {
            StartCoroutine(EndMatch(2));
        }
    }
    #endregion

    /// <summary>
    /// Called by each player upon reaching the end zone to pass the information.
    /// </summary>
    /// <param name="playerIndex">The index of the informing player.</param>
    public void InformOfReachingEndZone(int playerIndex)
    {
        if(playerIndex == 1)
        {
            hasPlayer1ArrivedToEndZone = true;
        }
        else if (playerIndex == 2)
        {
            hasPlayer2ArrivedToEndZone = true;
        }
        Debug.Log("Player " + playerIndex + " has arrived at the end zone");
    }

    IEnumerator ShowMessage(string content, float duration = 1.5f)
    {
        messagePanel.SetActive(true);
        message.text = content;
        yield return new WaitForSeconds(duration);
        messagePanel.SetActive(false);
    }
}
