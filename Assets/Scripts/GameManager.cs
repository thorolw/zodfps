using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance;
    
    [Header("Level Management")]
    public string nextLevelName;
    public float levelCompleteDelay = 2f;
    
    [Header("UI Elements")]
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject levelCompleteMenu;
    public Text levelCompleteText;
    public Text levelStatsText;
    public Text keycardsFoundText;
    
    [Header("Game Stats")]
    public int enemiesKilled = 0;
    public int secretsFound = 0;
    public float levelTime = 0f;
    public bool levelComplete = false;
    
    // KeyCards and Doors
    private Dictionary<string, bool> keyCards = new Dictionary<string, bool>();
    private float gameStartTime;
    private bool isPaused = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize UI elements if present
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameOverMenu != null) gameOverMenu.SetActive(false);
        if (levelCompleteMenu != null) levelCompleteMenu.SetActive(false);
    }
    
    private void Start()
    {
        ResetLevel();
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        // Update level time if level is not complete
        if (!levelComplete)
        {
            levelTime = Time.time - gameStartTime;
        }
        
        // Handle pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }
    
    public void ResetLevel()
    {
        gameStartTime = Time.time;
        enemiesKilled = 0;
        secretsFound = 0;
        levelComplete = false;
        keyCards.Clear();
    }
    
    public void TogglePauseMenu()
    {
        if (levelComplete) return;
        
        isPaused = !isPaused;
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }
        
        // Freeze/unfreeze time and toggle cursor
        Time.timeScale = isPaused ? 0 : 1;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }
    
    public void EnemyKilled()
    {
        enemiesKilled++;
    }
    
    public void SecretFound()
    {
        secretsFound++;
        Debug.Log("Secret found! Total: " + secretsFound);
        
        // You could trigger UI notification or sound here
    }
    
    public void AddKeyCard(string color)
    {
        keyCards[color] = true;
        Debug.Log(color + " keycard found!");
        
        // Update UI if needed
        UpdateKeyCardUI();
    }
    
    public bool HasKeyCard(string color)
    {
        return keyCards.ContainsKey(color) && keyCards[color];
    }
    
    private void UpdateKeyCardUI()
    {
        // Update keycard UI if available
        if (keycardsFoundText != null)
        {
            string keycardText = "Keycards: ";
            foreach (var keycard in keyCards)
            {
                if (keycard.Value)
                {
                    keycardText += keycard.Key + " ";
                }
            }
            keycardsFoundText.text = keycardText;
        }
    }
    
    public void CompleteLevel()
    {
        if (levelComplete) return;
        
        levelComplete = true;
        
        // Show level complete UI
        if (levelCompleteMenu != null)
        {
            levelCompleteMenu.SetActive(true);
            
            // Update level stats
            if (levelStatsText != null)
            {
                string minutes = Mathf.Floor(levelTime / 60).ToString("00");
                string seconds = (levelTime % 60).ToString("00");
                
                levelStatsText.text = string.Format("Time: {0}:{1}\n" +
                                                   "Enemies Killed: {2}\n" +
                                                   "Secrets Found: {3}/{4}",
                                                   minutes, seconds,
                                                   enemiesKilled,
                                                   secretsFound, 3); // Assuming 3 secrets per level
            }
        }
        
        // Enable cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Load next level after delay
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            StartCoroutine(LoadNextLevelAfterDelay());
        }
    }
    
    private IEnumerator LoadNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(levelCompleteDelay);
        
        LoadLevel(nextLevelName);
    }
    
    public void LoadLevel(string levelName)
    {
        Time.timeScale = 1; // Ensure time scale is reset
        SceneManager.LoadScene(levelName);
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1; // Ensure time scale is reset
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    // Call this when player reaches exit
    public void PlayerReachedExit()
    {
        CompleteLevel();
    }
    
    // Format time (seconds) as MM:SS
    public string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
