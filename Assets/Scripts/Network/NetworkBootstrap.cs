using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class NetworkBootstrap : MonoBehaviour
{
    public static NetworkBootstrap Instance { get; private set; }
    
    [Header("Status")]
    public bool isAuthenticated = false;
    public string playerName = "Player";
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    async void Start()
    {
        await InitializeUnityServices();
    }
    
    async Task InitializeUnityServices()
    {
        try
        {
            // Initialize Unity Services
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized");
            
            // Sign in anonymously
            await SignInAnonymously();
            
            Debug.Log("Ready for multiplayer!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize: {e.Message}");
        }
    }
    
    async Task SignInAnonymously()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            isAuthenticated = true;
            
            // Generate random player name
            playerName = "Player_" + Random.Range(1000, 9999);
            
            Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
            Debug.Log($"Player name: {playerName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to sign in: {e.Message}");
        }
    }
}
