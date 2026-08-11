using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResultCalculator : MonoBehaviour
{
    [SerializeField] private int matchScore;

    [Header("UI")]
    [SerializeField] private SpriteRenderer reactionPanel;
    [SerializeField] private Image[] stars = new Image[4];
    [SerializeField] private TMP_Text scoreText;

    [Header("Sprites")]
    [SerializeField] private Sprite[] reactionImage = new Sprite[5];
    [SerializeField] private Sprite emptyStar;
    [SerializeField] private Sprite fullStar;

    /// <summary>
    /// Sets the correct look for the reaction panel based on the given index.
    /// </summary>
    /// <param name="reactionIndex">Index based on the gained score.</param>
    public void SetupReactionScreen(int reactionIndex)
    {
        if(reactionImage[reactionIndex] != null) reactionPanel.sprite = reactionImage[reactionIndex];
        scoreText.text = "Score: " + matchScore.ToString();

        for (int i = 0; i < 4; i++)
        {
            if ((i + 1) <= reactionIndex)
            {
                stars[i].sprite = fullStar;
            }
            else
            {
                stars[i].sprite = emptyStar;
            }
        }
    }
    /// <summary>
    /// Calculates a score based on turns played, state of the players afterwards, the result of the match and some random factor.
    /// <br>Based on the score, the function picks the index of the correct reaction image, to give feedback on the matches result.</br>
    /// <br>The precise calculations are convoluted and secret.</br>
    /// </summary>
    /// <param name="winnerIndex">The index indicating result of the match.</param>
    /// <param name="turnsPlayed">The amount of turns played in the match. Score increases with turns up until some point, where it starts decreasing rapidly. </param>
    /// <param name="totalTimesJumped">The total amount of evasions during the match.</param>
    /// <param name="shp1">The shield HP of player1. A shield lost during match increases score.</param>
    /// <param name="shp2">The shield HP of player2. A shield lost during match increases score.</param>
    /// <returns>Index of the picked reaction image.</returns>
    public int DetermineReaction(int winnerIndex, int turnsPlayed = 0, int totalTimesJumped = 0, float shp1 = 0, float shp2 = 0)
    {
        int score = 0;
        if (winnerIndex > 0) { score += turnsPlayed; }

        score += (int)(5 * turnsPlayed * (Mathf.Pow(0.6f, 0.5f * turnsPlayed - 3)) - 18);

        if (totalTimesJumped > 0 && totalTimesJumped < 3) { score += 10; }

        if (shp1 < 0) { score = (int)(score * 1.2f); }
        if (shp2 < 0) { score = (int)(score * 1.2f); }

        score += Random.Range(0, 8); // random bonus

        Debug.Log("Result of the match: " + score + " score");
        matchScore = score;

        for (int i = 0; i <= 4; i++)
        {
            if (score < 10) { return i; }
            score -= 10;
        }

        return 4;
    }
}
