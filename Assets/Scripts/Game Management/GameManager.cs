using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game state")]
    bool firstPlaythrough = true;
    public bool isFightActive;
    public bool isInCombat;
    bool isAfterMatch = false;
    [SerializeField] int turnsPlayed;
    [HideInInspector] public int totalTimesJumped;

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
    [SerializeField] GameObject aftermatchUI;
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
        aftermatchUI.SetActive(false);
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
                isFightActive = false;
                isInCombat = false;
                PrepareMatch();
                CameraController.Instance.ResetCamera();

                // display menu screen
                menuUI.SetActive(true);
                controlsPanel.SetActive(false);
                gameUI.SetActive(false);
                messagePanel.SetActive(false);
            }
            else if(!isFightActive && !isInCombat) 
            {
                StartCoroutine(StartMatch());
            }     
        }

        if (Input.GetKeyDown(KeyCode.H) && !isFightActive) // or whatever
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

    private void FixedUpdate()
    {
        if (isInCombat && (turnsPlayed % 2 == 0 && player1.transform.position.x > player2.transform.position.x) ||
            (turnsPlayed % 2 == 1 && player1.transform.position.x < player2.transform.position.x))
        {
            horse1.hasPassedTheOpponent = true;
            horse2.hasPassedTheOpponent = true;
        }

        // ensure that the surviving player gets to their endzone after striking the opponent dead
        if(pScript1.state == PlayerState.DEAD && !hasPlayer2ArrivedToEndZone && !horse2.hasPassedTheOpponent)
        {
            horse2.hasPassedTheOpponent = true;
        }
        if(pScript2.state == PlayerState.DEAD && !hasPlayer1ArrivedToEndZone && !horse1.hasPassedTheOpponent)
        {
            horse1.hasPassedTheOpponent = true;
        }
    }

    #region Match initialization

    /// <summary>
    /// This function sets up initial values and positions for both players to prepare for a new match.
    /// </summary>
    public void PrepareMatch()
    {
        aftermatchUI.SetActive(false);
        pScript1.state = PlayerState.IDLE;
        pScript2.state = PlayerState.IDLE;
        if (horse1 == null) horse1 = player1.GetComponentInChildren<HorseMovement>();
        if (horse2 == null) horse2 = player2.GetComponentInChildren<HorseMovement>();

        if(player1.transform.localScale.x < 0) { horse1.TurnAround(false); }
        // starts on the left
        player1.transform.position = LeftStartPos.position;
        horse1.Setup(false);
        if (firstPlaythrough) { pScript1.RecordLocalStartTransforms(); }

        horse2.TurnAround(false);
        // starts on the right
        player2.transform.position = RightStartPos.position;
        horse2.Setup(true);
        if (firstPlaythrough) { pScript2.RecordLocalStartTransforms(); firstPlaythrough = false; }

        pScript1.ResetPlayerState();
        pScript2.ResetPlayerState();
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
        int reactionIndex = DetermineReaction(winnerIndex);
        CameraController.Instance.DisplayViewersReaction(reactionIndex, 3f);

        yield return new WaitForSeconds(6f);

        // display input prompt
        //gameUI.SetActive(false);
        //menuUI.SetActive(true);
        isAfterMatch = true;
        aftermatchUI.SetActive(true);
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

        yield return new WaitForEndOfFrame();
        horse1.TurnAround(); // turns the whole player around
        horse2.TurnAround();

        hasPlayer1ArrivedToEndZone = false;
        hasPlayer2ArrivedToEndZone = false;

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

        if(pScript1.state == PlayerState.DEAD && pScript2.state == PlayerState.DEAD)
        {
            StartCoroutine(EndMatch(0));
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
            return;
        }

        // both players survived the turn
        if (hasPlayer1ArrivedToEndZone && hasPlayer2ArrivedToEndZone)
        {
            StartCoroutine(StartNewTurn());
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

    /// <summary>
    /// Display a message panel in the center of the screen, conveying a message.
    /// </summary>
    /// <param name="content">The string to be written on the panel.</param>
    /// <param name="duration">How long the message should be visible.</param>
    /// <returns></returns>
    IEnumerator ShowMessage(string content, float duration = 1.5f)
    {
        messagePanel.SetActive(true);
        message.text = content;
        yield return new WaitForSeconds(duration);
        messagePanel.SetActive(false);
    }

    /// <summary>
    /// Calculates a score based on turns played, state of the players afterwards, the result of the match and some random factor.
    /// <br>Based on the score, the function picks the index of the correct reaction image, to give feedback on the matches result.</br>
    /// <br>The precise calculations are convoluted and secret.</br>
    /// </summary>
    /// <param name="winnerIndex">The index indicating result of the match.</param>
    /// <returns>Index of the picked reaction image.</returns>
    int DetermineReaction(int winnerIndex)
    {
        int score = 0;
        if(winnerIndex > 0) { score += turnsPlayed; }

        score += (int)(5 * turnsPlayed * (Mathf.Pow(0.6f, 0.5f*turnsPlayed - 3)) - 18);

        if (totalTimesJumped > 0 && totalTimesJumped < 3) { score += 10; }

        if (pScript1.shieldHealthPoints < 0) { score = (int)(score * 1.2f); }
        if (pScript2.shieldHealthPoints < 0) { score = (int)(score * 1.2f); }

        score += Random.Range(0, 8); // random bonus

        Debug.Log("Result of the match: " + score + " score");

        //if(score > )
        //{

        //}

        return 0;
    }
}
