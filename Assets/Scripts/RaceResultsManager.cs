using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

public class RaceResultsManager : MonoBehaviour // Changed from NetworkBehaviour
{
    public static RaceResultsManager Instance { get; private set; }
    
    [Header("UI")]
    public GameObject resultsPanel;
    public TMP_Text resultsText;
    
    private Dictionary<string, float> finishTimes = new Dictionary<string, float>();
    
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
    }
    
    public void ReportFinishTime(string playerName, float time)
    {
        bool isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        
        Debug.Log($"ReportFinishTime called: {playerName} - {time}s, IsMultiplayer: {isMultiplayer}");
        
        if (isMultiplayer)
        {
            // Multiplayer: get NetworkBehaviour component to send RPC
            var networkComp = GetComponent<RaceResultsNetworkSync>();
            if (networkComp != null)
            {
                networkComp.ReportFinishTimeServerRpc(playerName, time);
            }
            else
            {
                Debug.LogError("RaceResultsNetworkSync component missing for multiplayer!");
                AddFinishTime(playerName, time); // Fallback
            }
        }
        else
        {
            // Singleplayer: just show locally
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
        Debug.Log("UpdateResultsUI called");
        
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            Debug.Log("Results panel activated");
        }
        else
        {
            Debug.LogError("Results panel is null!");
        }
        
        if (resultsText == null)
        {
            Debug.LogError("Results text is null!");
            return;
        }
        
        bool isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        
        // Sort by time
        List<KeyValuePair<string, float>> sortedResults = new List<KeyValuePair<string, float>>(finishTimes);
        sortedResults.Sort((a, b) => a.Value.CompareTo(b.Value));
        
        // Build results text
        
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
                resultsText.text += $"Your Time: {timeStr}\n";
            }
            
            position++;
        }
        
        // Show waiting message if multiplayer
        if (isMultiplayer)
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
