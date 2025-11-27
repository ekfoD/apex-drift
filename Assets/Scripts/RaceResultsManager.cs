using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

public class RaceResultsManager : MonoBehaviour
{
    public static RaceResultsManager Instance { get; private set; }
    
    [Header("UI")]
    public GameObject resultsPanel;
    public TMP_Text resultsText;
    
    private Dictionary<string, float> finishTimes = new Dictionary<string, float>();
    private bool localPlayerFinished = false;
    private string localPlayerName = "";
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }
        
        // Get local player name
        bool isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        if (isMultiplayer)
        {
            localPlayerName = NetworkBootstrap.Instance.playerName;
        }
        else
        {
            localPlayerName = "Player";
        }
    }
    
    public void ReportFinishTime(string playerName, float time)
    {
        bool isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        
        Debug.Log($"ReportFinishTime called: {playerName} - {time}s, IsMultiplayer: {isMultiplayer}");
        
        // Mark if this is the local player finishing
        if (playerName == localPlayerName)
        {
            localPlayerFinished = true;
            Debug.Log("LOCAL PLAYER FINISHED!");
        }
        
        if (isMultiplayer)
        {
            var networkComp = GetComponent<RaceResultsNetworkSync>();
            if (networkComp != null)
            {
                networkComp.ReportFinishTimeServerRpc(playerName, time);
            }
            else
            {
                Debug.LogError("RaceResultsNetworkSync component missing!");
                AddFinishTime(playerName, time);
            }
        }
        else
        {
            AddFinishTime(playerName, time);
        }
    }
    
    public void AddFinishTime(string playerName, float time)
    {
        if (finishTimes.ContainsKey(playerName))
        {
            Debug.LogWarning($"Player {playerName} already finished!");
            return;
        }
        
        finishTimes.Add(playerName, time);
        Debug.Log($"Added result: {playerName} - {time}s (Total results: {finishTimes.Count})");
        
        UpdateResultsUI();
    }
    
    void UpdateResultsUI()
    {
        Debug.Log($"UpdateResultsUI called. Local player finished: {localPlayerFinished}");
        
        bool isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        
        // ONLY show results panel if local player has finished OR singleplayer
        if (!isMultiplayer || localPlayerFinished)
        {
            if (resultsPanel != null)
            {
                resultsPanel.SetActive(true);
                Debug.Log("Results panel activated for local player");
            }
            else
            {
                Debug.LogError("Results panel is null!");
            }
        }
        else
        {
            Debug.Log("Other player finished, but not showing results yet (local player still racing)");
        }
        
        if (resultsText == null)
        {
            Debug.LogError("Results text is null!");
            return;
        }
        
        // Build results text (even if not showing yet, so it's ready when we finish)
        
        // Sort by time
        List<KeyValuePair<string, float>> sortedResults = new List<KeyValuePair<string, float>>(finishTimes);
        sortedResults.Sort((a, b) => a.Value.CompareTo(b.Value));
        
        int position = 1;
        foreach (var result in sortedResults)
        {
            int minutes = Mathf.FloorToInt(result.Value / 60);
            int seconds = Mathf.FloorToInt(result.Value % 60);
            string timeStr = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            if (isMultiplayer)
            {
                resultsText.text += $"{position}. {result.Key} - {timeStr}\n";
            }
            else
            {
                resultsText.text += $"{timeStr}\n";
            }
            
            position++;
        }
        
        // Show waiting message if multiplayer and we've finished but others haven't
        if (isMultiplayer && localPlayerFinished)
        {
            int expectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
            if (finishTimes.Count < expectedPlayers)
            {
                resultsText.text += $"\nWaiting for {expectedPlayers - finishTimes.Count} player(s)...";
            }
        }
        
        Debug.Log($"Results text updated: {resultsText.text}");
    }
}
