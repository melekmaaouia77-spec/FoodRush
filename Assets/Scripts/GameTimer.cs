/*using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class GameTimer : NetworkBehaviour
{
    public static GameTimer Instance;

    [Header("Timer Settings")]
    [SerializeField] private float gameDuration = 300f; // 5 minutes in seconds
    [SerializeField] private TMP_Text timerText;

    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private bool freezePlayersOnEnd = true;

    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>();
    private NetworkVariable<bool> gameActive = new NetworkVariable<bool>(true);

    private bool hasGameEnded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            timeRemaining.Value = gameDuration;
            gameActive.Value = true;
        }

        timeRemaining.OnValueChanged += OnTimeChanged;
        gameActive.OnValueChanged += OnGameActiveChanged;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!IsServer || !gameActive.Value) return;

        if (timeRemaining.Value > 0)
        {
            timeRemaining.Value -= Time.deltaTime;

            if (timeRemaining.Value <= 0)
            {
                timeRemaining.Value = 0;
                EndGame();
            }
        }
    }

    private void OnTimeChanged(float oldTime, float newTime)
    {
        UpdateTimerDisplay();
    }

    private void OnGameActiveChanged(bool oldValue, bool newValue)
    {
        if (!newValue && !hasGameEnded)
        {
            hasGameEnded = true;
            StartCoroutine(ShowGameOverSequence());
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining.Value / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining.Value % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Change color when time is running out
        if (timeRemaining.Value <= 30f)
        {
            timerText.color = Color.red;
        }
        else if (timeRemaining.Value <= 60f)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.white;
        }
    }

    private void EndGame()
    {
        if (!IsServer) return;

        gameActive.Value = false;
        Debug.Log("Game Over! Time's up!");

        // Trigger game over on all clients
        TriggerGameOverClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerGameOverClientRpc()
    {
        if (hasGameEnded) return;
        hasGameEnded = true;

        StartCoroutine(ShowGameOverSequence());
    }

    private IEnumerator ShowGameOverSequence()
    {
        Debug.Log("Starting Game Over Sequence");

        // Freeze all players if enabled
        if (freezePlayersOnEnd)
        {
            FreezeAllPlayers();
        }

        // Wait a moment for dramatic effect
        yield return new WaitForSeconds(1f);

        // Get final scores and determine winner
        if (LeaderboardManager.Instance != null)
        {
            var winner = LeaderboardManager.Instance.GetWinner();

            // Show game over panel with winner
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (winnerText != null)
                {
                    bool isLocalPlayerWinner = winner.clientId == NetworkManager.Singleton.LocalClientId;

                    if (isLocalPlayerWinner)
                    {
                        winnerText.text = $"🎉 YOU WIN! 🎉\nScore: ${winner.score}";
                        winnerText.color = Color.green;
                    }
                    else
                    {
                        winnerText.text = $"Winner: {winner.playerName}\nScore: ${winner.score}\n\nYour Score: ${MoneyManager.Instance.GetMoney()}";
                        winnerText.color = Color.white;
                    }
                }
            }

            // Show the leaderboard
            LeaderboardManager.Instance.ShowLeaderboard();

            // Submit scores to Unity Leaderboards (if you're using it)
            yield return StartCoroutine(SubmitScoresToLeaderboard());
        }
    }

    private void FreezeAllPlayers()
    {
        // Find all player instances and freeze them
        players[] allPlayers = FindObjectsOfType<players>();

        foreach (players player in allPlayers)
        {
            if (player != null)
            {
                player.FreezePlayer();
            }
        }

        Debug.Log($"Froze {allPlayers.Length} players");
    }

    private IEnumerator SubmitScoresToLeaderboard()
    {
        // Get the local player's final score
        int finalScore = MoneyManager.Instance.GetMoney();

        Debug.Log($"Submitting score to Unity Leaderboard: {finalScore}");

        if (LeaderboardManager.Instance != null)
        {
            // Submit to Unity Leaderboards
            var submitTask = LeaderboardManager.Instance.SubmitScoreToLeaderboard(finalScore);

            // Wait for submission to complete
            while (!submitTask.IsCompleted)
            {
                yield return null;
            }

            if (submitTask.IsFaulted)
            {
                Debug.LogError("Failed to submit score to leaderboard");
            }
            else
            {
                Debug.Log("Score submitted successfully!");
            }
        }
    }

    // Public methods to control the timer
    public void StartTimer()
    {
        if (IsServer)
        {
            timeRemaining.Value = gameDuration;
            gameActive.Value = true;
            hasGameEnded = false;
        }
    }

    public void PauseTimer()
    {
        if (IsServer)
        {
            gameActive.Value = false;
        }
    }

    public void ResumeTimer()
    {
        if (IsServer && !hasGameEnded)
        {
            gameActive.Value = true;
        }
    }

    public void AddTime(float seconds)
    {
        if (IsServer)
        {
            timeRemaining.Value += seconds;
        }
    }

    public float GetTimeRemaining()
    {
        return timeRemaining.Value;
    }

    public bool IsGameActive()
    {
        return gameActive.Value;
    }
}
*/