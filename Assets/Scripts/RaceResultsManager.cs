using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

public class RaceResultsManager : MonoBehaviour
{
    public static RaceResultsManager Instance { get; private set; }
    
    [Header("UI Panels")]
    public GameObject resultsPanel;
    public GameObject singleplayerUI;  // Assign S-Finish
    public GameObject multiplayerUI;   // Assign M-Finish
    
    [Header("Singleplayer UI")]
    public TMP_Text singleplayerTimeText;  // Assign Text (TMP) under S-Finish
    
    [Header("Multiplayer UI")]
    public TMP_Text multiplayerResultsText;  // You'll need to add a text element under M-Finish
    
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
        
        // Hide both UIs at start
        if (singleplayerUI != null) singleplayerUI.SetActive(false);
        if (multiplayerUI != null) multiplayerUI.SetActive(false);
        
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
        bool isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        
        // Only show results if local player finished OR singleplayer
        if (!isMultiplayer || localPlayerFinished)
        {
            if (resultsPanel != null)
            {
                resultsPanel.SetActive(true);
            }
            
            // Show correct UI panel
            if (singleplayerUI != null) singleplayerUI.SetActive(!isMultiplayer);
            if (multiplayerUI != null) multiplayerUI.SetActive(isMultiplayer);
        }
        
        // Sort results by time (fastest first)
        List<KeyValuePair<string, float>> sortedResults = new List<KeyValuePair<string, float>>(finishTimes);
        sortedResults.Sort((a, b) => a.Value.CompareTo(b.Value));
        
        if (isMultiplayer)
        {
            UpdateMultiplayerUI(sortedResults);
        }
        else
        {
            UpdateSingleplayerUI(sortedResults);
        }
    }
    
    void UpdateSingleplayerUI(List<KeyValuePair<string, float>> results)
    {
        if (singleplayerTimeText == null) return;
        
        if (results.Count > 0)
        {
            float time = results[0].Value;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            int milliseconds = Mathf.FloorToInt((time % 1) * 100);
            
            singleplayerTimeText.text = $"Your Time: {minutes:00}:{seconds:00}.{milliseconds:00}";
        }
    }
    
    void UpdateMultiplayerUI(List<KeyValuePair<string, float>> results)
    {
        if (multiplayerResultsText == null) return;
        
        multiplayerResultsText.text = "";
        
        int expectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        int position = 1;
        
        // Show rankings
        foreach (var result in results)
        {
            float time = result.Value;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            int milliseconds = Mathf.FloorToInt((time % 1) * 100);
            string timeStr = $"{minutes:00}:{seconds:00}.{milliseconds:00}";
            
            string positionStr = GetPositionString(position);
            string playerIndicator = (result.Key == localPlayerName) ? " (You)" : "";
            
            multiplayerResultsText.text += $"{positionStr}  {result.Key}{playerIndicator}  {timeStr}\n";
            position++;
        }
        
        // Show waiting message
        int playersRemaining = expectedPlayers - finishTimes.Count;
        if (playersRemaining > 0)
        {
            multiplayerResultsText.text += $"\nWaiting for {playersRemaining} player(s)...";
        }
        else
        {
            multiplayerResultsText.text += "\nAll players finished!";
        }
    }
    
    string GetPositionString(int position)
    {
        switch (position)
        {
            case 1: return "1st";
            case 2: return "2nd";
            case 3: return "3rd";
            default: return $"{position}th";
        }
    }
}