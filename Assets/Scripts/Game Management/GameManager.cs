using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public enum GameState
{
    MENU,
    MATCH,  // in-game but not in combat
    ACTIVE_COMBAT,
    AFTERMATCH  // after the game has concluded
}
public class GameManager : MonoBehaviour
{
    [Header("Game state")]
    private bool firstPlaythrough = true;
    public GameState gameState;
    [HideInInspector] public static event Action<int> OnEndMatchEvent;
    [HideInInspector] public static event Action<bool> OnEnableGameUIEvent;

    [SerializeField] private int turnsPlayed;
    [HideInInspector] public int totalTimesJumped;

    [Header("Players")]
    public GameObject player1;
    private PlayerScript pScript1;
    private HorseMovement horse1;
    private bool hasPlayer1ArrivedToEndZone;

    public GameObject player2;
    private PlayerScript pScript2;
    private HorseMovement horse2;
    private bool hasPlayer2ArrivedToEndZone;

    [Header("Settings")]
    public bool randomUpgradesBetweenTurns;
    public bool offHorseCombat;
    public float baseDeathChance;
    [Header("")]
    public float gameConditionsCheckInterval;
    [SerializeField] private ResultCalculator calc; // short for calculator btw

    [Header("Arena")]
    [SerializeField] private Transform LeftStartPos;
    [SerializeField] private Transform RightStartPos;

    [Header("UI")]
    [SerializeField] private GameObject menuUI;
    [SerializeField] private Image soundIndicatorImage;
    [SerializeField] private GameObject aftermatchUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private TMP_Text turnCounter;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text message;
    [SerializeField] private TransitionController controlsPanel; 
    [SerializeField] private TransitionController titleCard;
    [SerializeField] private TransitionController menuInputPrompts;
    [SerializeField] private TransitionController blackOutScreen;

    [Header("")]
    [SerializeField] private Sprite soundIcon;
    [SerializeField] private Sprite noSoundIcon;

    #region Singleton + black screen
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

        blackOutScreen.Appear(true, true);
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pScript1 = player1.GetComponent<PlayerScript>();
        pScript2 = player2.GetComponent<PlayerScript>();
        PrepareMatch();
        InvokeRepeating("CheckForEndTurnConditions", 0f, gameConditionsCheckInterval);

        SoundManager.Instance.Setup(player1, player2);
        SoundManager.Instance.PlayLongSound(SoundType.MENU_BG_MUSIC, 0.7f);

        menuUI.SetActive(true);
        aftermatchUI.SetActive(false);
        controlsPanel.Appear(false, true);
        gameUI.SetActive(false);
        messagePanel.SetActive(false);
        blackOutScreen.Appear(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            if (gameState == GameState.AFTERMATCH) 
            {
                gameState = GameState.MENU;
                PrepareMatch();
                CameraController.Instance.ResetCamera();
                SoundManager.Instance.PlaySound(SoundType.INTERACT_SOUND);
                SoundManager.Instance.PlayLongSound(SoundType.MENU_BG_MUSIC, 0.7f);

                // display menu screen
                menuUI.SetActive(true);
                titleCard.Appear(true);
                menuInputPrompts.Appear(true);
                gameUI.SetActive(false);
                messagePanel.SetActive(false);
            }
            else if(gameState == GameState.MENU && !controlsPanel.visible) 
            {
                SoundManager.Instance.PlaySound(SoundType.INTERACT_SOUND);
                StartCoroutine(StartMatch());
            }
            else if(CameraController.Instance.cameraTurningAround == true) // skip animation
            {
                CameraController.Instance.InterruptAftermatchDisplay();
            }
        }

        if(gameState == GameState.MENU)
        {
            if (Input.GetKeyDown(KeyCode.H) && !CustomizationManager.Instance.inCustomization) // or whatever
            {
                SoundManager.Instance.PlaySound(SoundType.INTERACT_SOUND);
                // show or hide controls panel
                controlsPanel.Appear(!controlsPanel.visible);
            }

            if (controlsPanel.visible && Input.GetKeyDown(KeyCode.K)) // customization panel instead
            {
                controlsPanel.Appear(false);
            }

            if (Input.GetKeyDown(KeyCode.M)) // mute sound or unmute
            {
                SoundManager.Instance.PlaySound(SoundType.INTERACT_SOUND);
                if (SoundManager.Instance.GetVolume() > 0)
                {
                    SoundManager.Instance.SetVolume(0f);
                    soundIndicatorImage.sprite = noSoundIcon;
                }
                else
                {
                    SoundManager.Instance.SetVolume(1f);
                    soundIndicatorImage.sprite = soundIcon;
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if ((gameState == GameState.MATCH || gameState == GameState.ACTIVE_COMBAT) 
                && pScript1.state != PlayerState.DEAD && pScript2.state != PlayerState.DEAD)
            {
                StartCoroutine(ForfeitMatch());
            }
            else
            {
                PlayerPrefs.SetInt("SessionID", PlayerPrefs.GetInt("SessionID") + 1);
                StartCoroutine(ShowMessage("Quitting tournament...", 0f));
                Application.Quit();
            }
        }
    }

    private void FixedUpdate()
    {
        if (gameState == GameState.ACTIVE_COMBAT)
        {
            if ((horse1.side == false && player1.transform.position.x > player2.transform.position.x) ||
            (horse1.side == true && player1.transform.position.x < player2.transform.position.x))
            {
                horse1.hasPassedTheOpponent = true;
                horse2.hasPassedTheOpponent = true;
            }

            // ensure that the surviving player gets to their endzone after striking the opponent dead
            if (pScript1.state == PlayerState.DEAD && !hasPlayer2ArrivedToEndZone && !horse2.hasPassedTheOpponent)
            {
                horse2.hasPassedTheOpponent = true;
            }
            if (pScript2.state == PlayerState.DEAD && !hasPlayer1ArrivedToEndZone && !horse1.hasPassedTheOpponent)
            {
                horse1.hasPassedTheOpponent = true;
            }
        }
    }

    #region Match initialization

    /// <summary>
    /// This function sets up initial values and positions for both players to prepare for a new match.
    /// </summary>
    void PrepareMatch()
    {
        aftermatchUI.SetActive(false);
        if (horse1 == null) horse1 = player1.GetComponentInChildren<HorseMovement>();
        if (horse2 == null) horse2 = player2.GetComponentInChildren<HorseMovement>();

        if(player1.transform.localScale.x < 0) { horse1.TurnAround(false); }
        // starts on the left
        player1.transform.position = LeftStartPos.position;
        horse1.Setup(false);
        pScript1.AdjustSpriteRendererLayers(true);
        if (firstPlaythrough) { pScript1.RecordLocalStartTransforms(); }

        horse2.TurnAround(false);
        // starts on the right
        player2.transform.position = RightStartPos.position;
        horse2.Setup(true);
        pScript2.AdjustSpriteRendererLayers(false);
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
        turnCounter.text = turnsPlayed.ToString();
        hasPlayer1ArrivedToEndZone = false;
        hasPlayer2ArrivedToEndZone = false;
        gameState = GameState.MATCH;

        SoundManager.Instance.InterruptPlayingSound();
        SoundManager.Instance.PlaySound(SoundType.APPLAUSE, 0.7f);
        titleCard.Appear(false);
        menuInputPrompts.Appear(false);

        yield return new WaitForSeconds(1.5f);

        SoundManager.Instance.PlayLongSound(SoundType.MATCH_BG_MUSIC, 0.7f);

        gameUI.SetActive(true);
        OnEnableGameUIEvent?.Invoke(true);
        menuUI.SetActive(false);
        gameState = GameState.ACTIVE_COMBAT;
        pScript1.state = PlayerState.COMBAT;
        pScript2.state = PlayerState.COMBAT;
        CameraController.Instance.ResetCamera();
        Debug.Log("JOUST!");
        StartCoroutine(ShowMessage("JOUST!"));
        pScript1.Charge(true);
        pScript2.Charge(true);
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

        SoundManager.Instance.InterruptPlayingSound();
        gameState = GameState.MATCH;
        OnEnableGameUIEvent?.Invoke(false);
        blackOutScreen.Appear(true);
        yield return new WaitForSeconds(1.2f);
        blackOutScreen.Appear(false);
        PrepareMatch();
        gameUI.SetActive(false);
        menuUI.SetActive(true);
        titleCard.Appear(true);
        menuInputPrompts.Appear(true);
        gameState = GameState.MENU;
    }

    /// <summary>
    /// This function is called when one player is defeated. It ends the game loop, shows results and returns the game to menu.
    /// </summary>
    /// <param name="winnerIndex">The index of the last player standing.</param>
    /// <returns></returns>
    IEnumerator EndMatch(int winnerIndex)
    {
        turnsPlayed++;
        gameState = GameState.MATCH;

        int reactionIndex = calc.DetermineReaction(winnerIndex, turnsPlayed, totalTimesJumped, pScript1.shieldHealthPoints, pScript2.shieldHealthPoints);
        Debug.Log("Chosen reaction: " + reactionIndex);
        OnEndMatchEvent?.Invoke(reactionIndex);
        SoundManager.Instance.InterruptPlayingSound();
        SoundManager.Instance.PlaySound(SoundType.APPLAUSE, 0.7f);
        yield return new WaitForSeconds(1.5f);

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

        OnEnableGameUIEvent?.Invoke(false);
        yield return new WaitForSeconds(4.5f + 0.5f * reactionIndex);

        gameUI.SetActive(false);
        // display result / some fancy animation
        calc.SetupReactionScreen(reactionIndex);
        CameraController.Instance.DisplayViewersReaction(6.5f, 5f);

        yield return new WaitForSeconds(19f);

        // display input prompt
        gameState = GameState.AFTERMATCH;
        aftermatchUI.SetActive(true);
    }

    /// <summary>
    /// Called to bypass the waiting time of the EndMatch function.
    /// </summary>
    public void InterruptEndMatchScreen()
    {
        StopAllCoroutines();
        gameUI.SetActive(false);
        gameState = GameState.AFTERMATCH;
        aftermatchUI.SetActive(true);
    }

    /// <summary>
    /// Called after each turn to begin a new one.
    /// </summary>
    /// <returns></returns>
    IEnumerator StartNewTurn()
    {
        gameState = GameState.MATCH;
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

        SoundManager.Instance.PlaySound(SoundType.GASP, 0.7f);

        if(randomUpgradesBetweenTurns)
        {
            int upgradeIndex = UnityEngine.Random.Range(0, 6);
            ModifierScript.Instance.ApplyModifier(upgradeIndex);
        }

        yield return new WaitForSeconds(3f);

        Debug.Log("Beginning turn " + (turnsPlayed + 1).ToString());
        gameState = GameState.ACTIVE_COMBAT;
        pScript1.Charge(true);
        pScript2.Charge(true);
    }

    /// <summary>
    /// Called periodically to check for conditions that result in the end of turn or match.
    /// </summary>
    void CheckForEndTurnConditions()
    {
        if (gameState != GameState.ACTIVE_COMBAT) { return; }

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
        if(duration > 0)
        {
            yield return new WaitForSeconds(duration);
            messagePanel.SetActive(false);
        }
    }
}
