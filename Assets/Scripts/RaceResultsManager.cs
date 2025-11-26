using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

public class RaceResultsManager : NetworkBehaviour
{
    public static RaceResultsManager Instance { get; private set; }
    
    [Header("UI - Drag Here!")]
    public GameObject resultsPanel;    // ← Drag your results panel here
    public TMP_Text resultsText;       // ← Drag your text field here
    
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
        
        if (isMultiplayer)
        {
            // Multiplayer: send to server to sync
            ReportFinishTimeServerRpc(playerName, time);
        }
        else
        {
            // Singleplayer: just show locally
            AddFinishTime(playerName, time);
        }
    }
    
    [Rpc(SendTo.Server)]
    void ReportFinishTimeServerRpc(string playerName, float time)
    {
        AddFinishTimeClientRpc(playerName, time);
    }
    
    [Rpc(SendTo.Everyone)]
    void AddFinishTimeClientRpc(string playerName, float time)
    {
        AddFinishTime(playerName, time);
    }
    
    void AddFinishTime(string playerName, float time)
    {
        if (finishTimes.ContainsKey(playerName)) return;
        
        finishTimes.Add(playerName, time);
        Debug.Log($"Added result: {playerName} - {time}s");
        
        UpdateResultsUI();
    }
    
    void UpdateResultsUI()
    {
        if (resultsPanel != null) resultsPanel.SetActive(true);
        if (resultsText == null) return;
        
        bool isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        
        // Sort by time
        List<KeyValuePair<string, float>> sortedResults = new List<KeyValuePair<string, float>>(finishTimes);
        sortedResults.Sort((a, b) => a.Value.CompareTo(b.Value));
        
        // Build results text
        if (isMultiplayer)
        {
            resultsText.text = "RACE RESULTS\n\n";
        }
        else
        {
            resultsText.text = "RACE COMPLETE!\n\n";
        }
        
        int position = 1;
        foreach (var result in sortedResults)
        {
            int minutes = Mathf.FloorToInt(result.Value / 60);
            int seconds = Mathf.FloorToInt(result.Value % 60);
            string timeStr = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            string medal = "";
            if (isMultiplayer)
            {
                if (position == 1) medal = "🥇 ";
                else if (position == 2) medal = "🥈 ";
                else if (position == 3) medal = "🥉 ";
            }
            
            if (isMultiplayer)
            {
                resultsText.text += $"{medal}{position}. {result.Key} - {timeStr}\n";
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
    }
}
