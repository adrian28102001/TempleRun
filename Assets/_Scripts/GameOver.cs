using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI leaderboardScoreText;
    [SerializeField] private TextMeshProUGUI leaderboardNameText;

    private int score;
    private int leaderboardTopCount = 5;
    private string submitScoreUrl = "http://localhost:5118/Leaderboard/";
    private string getLeaderboardUrl = "http://localhost:5118/Leaderboard/";

    public void StopGame(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
        StartCoroutine(GetLeaderboard());
    }

    public void SubmitScore()
    {
        StartCoroutine(SubmitScoreToLeaderBoard(inputField.text, score));
    }

    private IEnumerator SubmitScoreToLeaderBoard(string playerName, int playerScore)
    {
        LeaderboardEntry entry = new LeaderboardEntry
        {
            name = playerName,
            score = playerScore
        };

        string json = JsonUtility.ToJson(entry);

        using (UnityWebRequest www = new UnityWebRequest(submitScoreUrl, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error submitting score: " + www.error);
            }
            else
            {
                Debug.Log("Score submitted successfully!");
                StartCoroutine(GetLeaderboard());
            }
        }
    }

    private IEnumerator GetLeaderboard()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(getLeaderboardUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error getting leaderboard: " + www.error);
            }
            else
            {
                // Assuming that the JSON is wrapped as shown above
                LeaderboardEntriesWrapper wrapper = JsonUtility.FromJson<LeaderboardEntriesWrapper>(www.downloadHandler.text);
                if (wrapper != null && wrapper.entries != null)
                {
                    leaderboardNameText.text = "";
                    leaderboardScoreText.text = "";

                    foreach (LeaderboardEntry entry in wrapper.entries)
                    {
                        leaderboardNameText.text += entry.name + Environment.NewLine;
                        leaderboardScoreText.text += entry.score + Environment.NewLine;
                    }

                    Debug.Log("Leaderboard fetched successfully!");
                }
                else
                {
                    Debug.Log("Invalid JSON format for leaderboard.");
                }
            }
        }
    }

    // Wrapper class for deserializing JSON array
    [Serializable]
    private class LeaderboardEntriesWrapper
    {
        public LeaderboardEntry[] entries;
    }

    // Class representing a single leaderboard entry
    [Serializable]
    private class LeaderboardEntry
    {
        public string name;
        public int score;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}