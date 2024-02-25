using System;
using System.Collections;
using LootLocker.Requests;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI leaderboardScoreText;
    [SerializeField] private TextMeshProUGUI leaderboardNameText;
    
    private int score;
    private const string leaderboardID = "20548";
    private int leaderboardTopCount = 10;

    public void StopGame(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
        GetLeaderboard();
    }

    public void SubmitScore()
    {
        StartCoroutine(SubmitScoreToLeaderBoard());
    }

    private IEnumerator SubmitScoreToLeaderBoard()
    {
        bool? nameSet = null;
        bool? scoreSubmitted = null;

        LootLockerSDKManager.SetPlayerName(inputField.text, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully set the player name.");
                nameSet = true;
            }
            else
            {
                Debug.Log("Was not able to set the name");
                nameSet = false;
            }
        });

        yield return new WaitUntil(() => nameSet.HasValue);

        // if (!nameSet.HasValue) yield break;

        LootLockerSDKManager.SubmitScore("", score, leaderboardID, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully submitted the score to the leaderboard.");
                scoreSubmitted = true;
            }
            else
            {
                Debug.Log("Unsuccessfully submitted the score to the leaderboard.");
                scoreSubmitted = false;
            }
        });

        yield return new WaitUntil(() => scoreSubmitted.HasValue);
        if (!scoreSubmitted.HasValue) yield break;

        GetLeaderboard();
    }

    private void GetLeaderboard()
    {
        LootLockerSDKManager.GetScoreList(leaderboardID, leaderboardTopCount, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully got the score from leaderboard.");
                string leaderboardName = "";
                string leaderboardScore = "";

                LootLockerLeaderboardMember[] members = response.items;
                for (int i = 0; i < members.Length; ++i)
                {
                    var player = members[i].player;

                    if (player == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(members[i].player.name))
                    {
                        leaderboardName += player.name + Environment.NewLine;
                    }
                    else
                    {
                        leaderboardName += player.id + Environment.NewLine;
                    }

                    leaderboardScore += members[i].score + Environment.NewLine;
                }

                leaderboardNameText.SetText(leaderboardName);
                leaderboardScoreText.SetText(leaderboardScore);
            }
            else
            {
                Debug.Log("Unsuccessfully got the score from leaderboard.");
            }
        });
    }

    public void AddXP(int score)
    {
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}