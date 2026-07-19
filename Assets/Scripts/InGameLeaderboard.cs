using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class InGameLeaderboard : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;


    private List<Leaderboard.ScoreEntry> leaderboard = new();
    private int playerScore;

    private void Start()
    {
        StartCoroutine(RefreshLeaderboard());
    }

    IEnumerator RefreshLeaderboard()
    {
        while (true)
        {
            yield return StartCoroutine(LoadLeaderboard());

            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator LoadLeaderboard()
    {
        UnityWebRequest www = UnityWebRequest.Get(Leaderboard.SERVER_URL + Leaderboard.LEADERBOARD_ROUTE);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
            yield break;
        }

        string json = www.downloadHandler.text;
        Leaderboard.ScoreEntry[] scores =
            Leaderboard.JsonHelper.FromJson<Leaderboard.ScoreEntry>(json);

        leaderboard = new List<Leaderboard.ScoreEntry>(scores);

        UpdateDisplay();
    }

    public void SetPlayerScore(int score)
    {
        playerScore = score;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        List<Leaderboard.ScoreEntry> display = new(leaderboard);

        string username = UserManager.Instance != null
            ? UserManager.Instance.Username
            : "Anonymous";

        bool alreadyPresent = display.Exists(x =>
            x.username == username &&
            x.score == playerScore);

        if (!alreadyPresent)
        {
            display.Add(new Leaderboard.ScoreEntry
            {
                username = username,
                score = playerScore
            });
        }

        display.Sort((a, b) => b.score.CompareTo(a.score));

        leaderboardText.text = "Leaderboard\n";

        int playerRank = -1;

        bool shownPlayer = false;

        for (int i = 0; i < display.Count; i++)
        {
            bool isPlayer = display[i].username == username && display[i].score == playerScore;

            if (isPlayer)
                playerRank = i + 1;

            if (i < 10)
            {
                if (isPlayer && !shownPlayer)
                {
                    leaderboardText.text +=
                        $"<color=#00FF00>{i + 1}. {display[i].username}: {display[i].score}</color>\n";
                    shownPlayer = true;
                }
                else
                {
                    leaderboardText.text +=
                        $"{i + 1}. {display[i].username}: {display[i].score}\n";
                }
            }
        }

        if (playerRank > 10)
        {
            leaderboardText.text += "...\n";
            leaderboardText.text +=
                $"<color=#00FF00>{playerRank}. {username}: {playerScore}</color>";
        }
    }
}