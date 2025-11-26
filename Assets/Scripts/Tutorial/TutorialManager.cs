using UnityEngine;
using TMPro; // ← Changed from UnityEngine.UI
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleTutorialManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private GameObject completionPopup;

    [Header("Tutorial Objects")]
    [SerializeField] private GameObject checkpoint;
    [SerializeField] private Transform player;

    [Header("Step Requirements")]
    [SerializeField] private float movementDistance = 5f;
    [SerializeField] private float requiredSpeed = 10f;
    [SerializeField] private float speedHoldTime = 2f;

    [Header("Settings")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private TextMeshProUGUI instructionText; // ← Changed
    private TextMeshProUGUI completionText;  // ← Changed
    private Rigidbody playerRb;
    private int currentStep = 0;
    
    // Step 1: Movement
    private Vector3 startPosition;
    
    // Step 2: Acceleration
    private float timeAtSpeed = 0f;
    
    // Step 3: Checkpoint
    private bool checkpointReached = false;

    void Start()
    {
        Debug.Log("=== TUTORIAL START ===");
        
        // Find UI elements (TMP version)
        if (instructionPanel == null)
        {
            Debug.LogError("❌ Instruction Panel not assigned!");
            return;
        }
        else
        {
            Debug.Log("✓ Instruction Panel found");
        }

        instructionText = instructionPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (instructionText == null)
        {
            Debug.LogError("❌ TextMeshProUGUI not found in Instruction Panel!");
        }
        else
        {
            Debug.Log("✓ Instruction Text (TMP) found");
        }

        if (completionPopup == null)
        {
            Debug.LogError("❌ Completion Popup not assigned!");
        }
        else
        {
            Debug.Log("✓ Completion Popup found");
            completionText = completionPopup.GetComponentInChildren<TextMeshProUGUI>();
            completionPopup.SetActive(false);
        }

        // Find player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerRb = playerObj.GetComponent<Rigidbody>();
                Debug.Log("✓ Player found by tag");
            }
            else
            {
                Debug.LogError("❌ Player not found! Make sure it has 'Player' tag");
                return;
            }
        }
        else
        {
            playerRb = player.GetComponent<Rigidbody>();
            Debug.Log("✓ Player assigned in inspector");
        }

        if (playerRb == null)
        {
            Debug.LogWarning("⚠️ Player has no Rigidbody!");
        }

        startPosition = player.position;
        Debug.Log($"Start position: {startPosition}");

        // Start tutorial
        ShowStep(0);
        Debug.Log("=== TUTORIAL INITIALIZED ===");
    }

    void Update()
    {
        // Check completion conditions for current step
        switch (currentStep)
        {
            case 0: // Step 1: Movement
                CheckMovementStep();
                break;

            case 1: // Step 2: Acceleration
                CheckAccelerationStep();
                break;

            case 2: // Step 3: Checkpoint
                CheckCheckpointStep();
                break;
        }
    }

    void CheckMovementStep()
    {
        float distanceMoved = Vector3.Distance(startPosition, player.position);
        
        if (distanceMoved >= movementDistance)
        {
            Debug.Log("Movement step complete!");
            NextStep();
        }
    }

    void CheckAccelerationStep()
    {
        if (playerRb == null) return;

        float currentSpeed = playerRb.linearVelocity.magnitude;

        if (currentSpeed >= requiredSpeed)
        {
            timeAtSpeed += Time.deltaTime;
            
            if (timeAtSpeed >= speedHoldTime)
            {
                Debug.Log("Acceleration step complete!");
                NextStep();
            }
        }
        else
        {
            timeAtSpeed = 0f;
        }
    }

    void CheckCheckpointStep()
    {
        if (checkpointReached)
        {
            Debug.Log("Checkpoint step complete!");
            NextStep();
        }
    }

    void ShowStep(int step)
    {
        currentStep = step;

        if (instructionText == null)
        {
            Debug.LogError("❌ Cannot show step - instructionText is null!");
            return;
        }

        switch (step)
        {
            case 0:
                instructionText.text = "Use WASD to move your car";
                break;
            
            case 1:
                instructionText.text = "Hold W to accelerate";
                timeAtSpeed = 0f;
                break;
            
            case 2:
                instructionText.text = "Drive through the green checkpoint";
                break;
        }

        Debug.Log($"Tutorial Step {step + 1}: {instructionText.text}");
    }

    void NextStep()
    {
        currentStep++;

        if (currentStep <= 2)
        {
            ShowStep(currentStep);
        }
        else
        {
            CompleteTutorial();
        }
    }

    void CompleteTutorial()
    {
        instructionPanel.SetActive(false);
        completionPopup.SetActive(true);
        
        if (completionText != null)
        {
            completionText.text = "Great job!\nNow go and play some real levels!";
        }

        // Pause game
        Time.timeScale = 0f;

        // Setup button
        Button btn = completionPopup.GetComponentInChildren<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(BackToMenu);
        }

        Debug.Log("Tutorial Complete!");
    }

    void BackToMenu()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(mainMenuScene);
    }

    public void OnCheckpointReached()
    {
        checkpointReached = true;
        Debug.Log("Checkpoint reached!");
    }

    public void SkipTutorial()
    {
        CompleteTutorial();
    }
}
