using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    [Header("UI References")]
    [SerializeField] private LeaderboardsMenu leaderboardMenu; // assign in inspector

    [Header("Settings")]
    [SerializeField] private string leaderboardId = "Players_Score"; // must match Unity Dashboard

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    /// <summary>
    /// Submit a score to Unity Leaderboards for the current player.
    /// </summary>
    public async Task SubmitScoreToLeaderboard(int score)
    {
        try
        {
            var entry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            Debug.Log($"Submitted score {score} for player {entry.PlayerName} (Rank {entry.Rank})");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to submit score: {e.Message}");
        }
    }

    /// <summary>
    /// Open the leaderboard UI panel.
    /// </summary>
    public void ShowLeaderboard()
    {
        if (leaderboardMenu != null)
        {
            leaderboardMenu.Open();
        }
        else
        {
            Debug.LogWarning("LeaderboardMenu reference missing!");
        }
    }

    /// <summary>
    /// Get the winner based on Unity Leaderboards data.
    /// </summary>
    public async Task<(ulong clientId, string playerName, int score)> GetWinnerAsync()
    {
        try
        {
            var scores = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, new GetScoresOptions { Limit = 1 });
            if (scores.Results.Count > 0)
            {
                var top = scores.Results[0];
                return (0, top.PlayerName, (int)top.Score); // clientId not tracked by Unity Leaderboards
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch winner: {e.Message}");
        }

        // fallback: local player
        return (NetworkManager.Singleton.LocalClientId, AuthenticationService.Instance.PlayerName, MoneyManager.Instance.GetMoney());
    }

    /// <summary>
    /// Local helper to get winner without async (used by GameTimer).
    /// </summary>
    public (ulong clientId, string playerName, int score) GetWinner()
    {
        // fallback only: use local player
        return (NetworkManager.Singleton.LocalClientId, AuthenticationService.Instance.PlayerName, MoneyManager.Instance.GetMoney());
    }

    /// <summary>
    /// Submit all connected players' scores (optional, if you want multiplayer sync).
    /// </summary>
    public async Task SubmitAllPlayersScores(Dictionary<ulong, int> playerScores)
    {
        foreach (var kvp in playerScores)
        {
            int score = kvp.Value;
            await SubmitScoreToLeaderboard(score);
        }
    }
}