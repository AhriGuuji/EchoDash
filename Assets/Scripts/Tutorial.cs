using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class Tutorial : MonoBehaviour
{
    [Header("Audio Narration")]
    [SerializeField] private AudioSource narratorAudio;
    [SerializeField] private AudioClip[] narrationClips;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip highPitchSound;
    [SerializeField] private AudioClip lowPitchSound;
    [SerializeField] private AudioClip obstacleHitSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private AudioClip successSound;
    
    [Header("Tutorial Settings")]
    [SerializeField] private float spacebarWaitTime = 5f;
    [SerializeField] private float defaultObstacleSeconds = 3f;
    
    [Header("UI (Optional - for sighted assistants)")]
    [SerializeField] private TextMeshProUGUI narratorText;
    [SerializeField] private TextMeshProUGUI keyHintText;
    [SerializeField] private GameObject gameOverPanel;
    
    // Events for other scripts
    public static event System.Action OnRequireUp;
    public static event System.Action OnRequireDown;
    public static event System.Action OnReleaseUpEarly;
    public static event System.Action OnReleaseDownEarly;
    public static event System.Action<string> OnGameFailed;
    public static event System.Action OnTutorialComplete;
    public static event System.Action<float> OnObstacleDistanceCalculated;
    public static event System.Action<string> OnSoundLaunched;
    
    // State
    private bool tutorialActive = true;
    private bool waitingForSound = false;
    private bool waitingForUp = false;
    private bool waitingForDown = false;
    private bool correctKeyPressed = false;
    private bool keyIsPressed = false;
    private float obstacleTime = 3f;
    private float waitTimer = 0f;
    private string lastSound = "";
    private bool isFrozen = false;
    
    // Input Actions
    private InputAction spaceAction;
    private InputAction aAction;
    private InputAction sAction;
    
    // Audio clip indices
    private enum NarrationType
    {
        Intro, PressSpace, GoodObstacleHigh, GoodObstacleLow,
        WarningUp, WarningDown, WrongReleaseUp, WrongReleaseDown,
        FreezeWait, FailNoUp, FailNoDown, SuccessUp, SuccessDown, TutorialEnd
    }
    
    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        if (narratorAudio == null)
            Debug.LogWarning("Tutorial: No NarratorAudio assigned. Audio narration will be disabled.");
        
        if (narrationClips == null || narrationClips.Length == 0)
            Debug.LogWarning("Tutorial: No narration clips assigned.");
        
        SetupInputActions();
        StartCoroutine(TutorialFlow());
    }
    
    void SetupInputActions()
    {
        // Create input actions
        spaceAction = new InputAction("Space", binding: "<Keyboard>/space");
        aAction = new InputAction("A", binding: "<Keyboard>/a");
        sAction = new InputAction("S", binding: "<Keyboard>/s");
        
        // Enable all actions
        spaceAction.Enable();
        aAction.Enable();
        sAction.Enable();
        
        // Add callbacks
        spaceAction.performed += OnSpacePerformed;
        aAction.performed += OnAPerformed;
        aAction.canceled += OnACanceled;
        sAction.performed += OnSPerformed;
        sAction.canceled += OnSCanceled;
    }
    
    void OnDestroy()
    {
        // Clean up input actions
        spaceAction?.Dispose();
        aAction?.Dispose();
        sAction?.Dispose();
    }
    
    void Update()
    {
        if (!tutorialActive) return;
        if (isFrozen) return;
        
        // Update key press states for holding
        if (waitingForUp)
        {
            bool isAHeld = aAction.IsPressed();
            
            if (isAHeld)
            {
                if (!keyIsPressed)
                {
                    keyIsPressed = true;
                    correctKeyPressed = true;
                    OnRequireUp?.Invoke();
                    PlayNarration(NarrationType.SuccessUp);
                }
            }
            
            if (keyIsPressed && HasObstaclePassed())
            {
                waitingForUp = false;
                keyIsPressed = false;
                correctKeyPressed = true;
            }
        }
        
        // Handle down key (S) holding
        if (waitingForDown)
        {
            bool isSHeld = sAction.IsPressed();
            
            if (isSHeld)
            {
                if (!keyIsPressed)
                {
                    keyIsPressed = true;
                    correctKeyPressed = true;
                    OnRequireDown?.Invoke();
                    PlayNarration(NarrationType.SuccessDown);
                }
            }
            
            if (keyIsPressed && HasObstaclePassed())
            {
                waitingForDown = false;
                keyIsPressed = false;
                correctKeyPressed = true;
            }
        }
        
        // Timer for spacebar wait
        if (waitingForSound && waitTimer > 0)
        {
            waitTimer -= Time.unscaledDeltaTime;
            if (waitTimer <= 0)
            {
                // Update UI before freezing
                if (narratorText != null)
                {
                    narratorText.text = GetNarrationText(NarrationType.FreezeWait);
                    narratorText.ForceMeshUpdate();
                }
                
                PlayNarration(NarrationType.FreezeWait);
                FreezeGame();
            }
        }
    }
    
    void OnSpacePerformed(InputAction.CallbackContext context)
    {
        if (!tutorialActive) return;
        if (isFrozen) return;
        
        if (waitingForSound)
        {
            CancelInvoke();
            LaunchSound();
        }
    }
    
    void OnAPerformed(InputAction.CallbackContext context)
    {
        if (!tutorialActive) return;
        if (isFrozen) return;
        
        if (waitingForUp)
        {
            // First press is handled in Update for holding logic
            // This just ensures we capture the press
        }
    }
    
    void OnACanceled(InputAction.CallbackContext context)
    {
        if (!tutorialActive) return;
        if (isFrozen) return;
        
        if (waitingForUp && keyIsPressed && !HasObstaclePassed())
        {
            OnReleaseUpEarly?.Invoke();
            PlayNarration(NarrationType.WrongReleaseUp);
            FreezeGame();
        }
    }
    
    void OnSPerformed(InputAction.CallbackContext context)
    {
        if (!tutorialActive) return;
        if (isFrozen) return;
        
        if (waitingForDown)
        {
            // First press is handled in Update for holding logic
        }
    }
    
    void OnSCanceled(InputAction.CallbackContext context)
    {
        if (!tutorialActive) return;
        if (isFrozen) return;
        
        if (waitingForDown && keyIsPressed && !HasObstaclePassed())
        {
            OnReleaseDownEarly?.Invoke();
            PlayNarration(NarrationType.WrongReleaseDown);
            FreezeGame();
        }
    }
    
    IEnumerator TutorialFlow()
    {
        // Beginning
        PlayNarration(NarrationType.Intro);
        yield return new WaitForSecondsRealtime(GetClipLength(NarrationType.Intro));
        
        // First sound - HIGH PITCH
        PlayNarration(NarrationType.PressSpace);
        waitingForSound = true;
        waitTimer = spacebarWaitTime;
        
        yield return new WaitUntil(() => !waitingForSound);
        
        PlayHighPitchSound();
        CalculateObstacleDistance();
        PlayNarration(NarrationType.GoodObstacleHigh);
        yield return new WaitForSecondsRealtime(GetClipLength(NarrationType.GoodObstacleHigh));
        
        waitingForUp = true;
        yield return new WaitForSecondsRealtime(1f);
        yield return new WaitUntil(() => !waitingForUp);
        
        if (!correctKeyPressed)
        {
            PlayNarration(NarrationType.FailNoUp);
            PlayFailSound();
            FailGame("You didn't go up in time. The obstacle hit you.");
            yield break;
        }
        
        PlaySuccessSound();
        
        // Second sound - LOW PITCH
        PlayNarration(NarrationType.GoodObstacleLow);
        yield return new WaitForSecondsRealtime(GetClipLength(NarrationType.GoodObstacleLow));
        
        waitingForSound = true;
        waitTimer = spacebarWaitTime;
        yield return new WaitUntil(() => !waitingForSound);
        
        PlayLowPitchSound();
        CalculateObstacleDistance();
        PlayNarration(NarrationType.WarningDown);
        yield return new WaitForSecondsRealtime(GetClipLength(NarrationType.WarningDown));
        
        waitingForDown = true;
        yield return new WaitForSecondsRealtime(1f);
        yield return new WaitUntil(() => !waitingForDown);
        
        if (!correctKeyPressed)
        {
            PlayNarration(NarrationType.FailNoDown);
            PlayFailSound();
            FailGame("You didn't go down in time. You lost.");
            yield break;
        }
        
        PlaySuccessSound();
        
        // Tutorial complete
        PlayNarration(NarrationType.TutorialEnd);
        yield return new WaitForSecondsRealtime(GetClipLength(NarrationType.TutorialEnd));
        
        tutorialActive = false;
        if (narratorText != null) narratorText.gameObject.SetActive(false);
        if (keyHintText != null) keyHintText.text = "A ▲ | S ▼ | Space 🔊";
        
        Time.timeScale = 1f;
        isFrozen = false;
        
        OnTutorialComplete?.Invoke();
    }
    
    void LaunchSound()
    {
        waitingForSound = false;
        waitTimer = 0;
        
        if (lastSound == "" || lastSound == "low")
        {
            lastSound = "high";
            PlayHighPitchSound();
            OnSoundLaunched?.Invoke("high");
        }
        else
        {
            lastSound = "low";
            PlayLowPitchSound();
            OnSoundLaunched?.Invoke("low");
        }
        
        Invoke(nameof(OnSoundCollision), 0.5f);
    }
    
    void OnSoundCollision()
    {
        PlayObstacleHitSound();
    }
    
    void CalculateObstacleDistance()
    {
        OnObstacleDistanceCalculated?.Invoke(defaultObstacleSeconds);
        obstacleTime = defaultObstacleSeconds;
    }
    
    public void ReportObstacleDistance(float distanceInSeconds)
    {
        obstacleTime = distanceInSeconds;
    }
    
    bool HasObstaclePassed()
    {
        return Time.timeSinceLevelLoad > obstacleTime;
    }
    
    void FreezeGame()
    {
        if (isFrozen) return;
        
        isFrozen = true;
        Time.timeScale = 0f;
        
        StartCoroutine(WaitForPlayerUnfreeze());
    }
    
    IEnumerator WaitForPlayerUnfreeze()
    {
        // Wait until player presses Space (works during freeze)
        while (!spaceAction.WasPressedThisFrame())
        {
            yield return new WaitForSecondsRealtime(0.05f);
        }
        
        // Unfreeze
        Time.timeScale = 1f;
        isFrozen = false;
        keyIsPressed = false;
        
        // Handle the state that caused the freeze
        if (waitingForSound)
        {
            // Player pressed Space - continue with the sound
            waitingForSound = false;
            LaunchSound();
        }
    }
    
    void FailGame(string reason)
    {
        tutorialActive = false;
        Time.timeScale = 0f;
        isFrozen = true;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (narratorText != null) narratorText.text = reason;
        
        OnGameFailed?.Invoke(reason);
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        isFrozen = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public bool IsTutorialActive() => tutorialActive;
    public bool IsWaitingForUp() => waitingForUp;
    public bool IsWaitingForDown() => waitingForDown;
    public float GetObstacleTime() => obstacleTime;
    public bool IsFrozen() => isFrozen;
    
    // ============ AUDIO METHODS ============
    
    private void PlayNarration(NarrationType type)
    {
        int index = (int)type;
        
        if (narratorAudio != null && narrationClips != null && index < narrationClips.Length && narrationClips[index] != null)
        {
            narratorAudio.clip = narrationClips[index];
            narratorAudio.Play();
        }
        
        if (narratorText != null)
        {
            narratorText.text = GetNarrationText(type);
        }
    }
    
    private float GetClipLength(NarrationType type)
    {
        int index = (int)type;
        
        if (narrationClips != null && index < narrationClips.Length && narrationClips[index] != null)
        {
            return narrationClips[index].length;
        }
        
        string text = GetNarrationText(type);
        return Mathf.Clamp(text.Length / 15f, 1.5f, 5f);
    }
    
    private string GetNarrationText(NarrationType type)
    {
        return type switch
        {
            NarrationType.Intro => "Let's start a match. To play, you only need the keys: A to go up, S to go down, and Space to launch a sound.",
            NarrationType.PressSpace => "Press Space to launch the sound",
            NarrationType.GoodObstacleHigh => $"Good! Your obstacle is {obstacleTime:F1} seconds away from you. The sound was high-pitched, so you must go UP!",
            NarrationType.GoodObstacleLow => "Good job! Now let's train the low-pitch sound. Get ready. Launch sound with Space.",
            NarrationType.WarningUp => "The sound was high-pitched! Press and hold A to go UP! Don't release until the obstacle passes.",
            NarrationType.WarningDown => $"Obstacle {obstacleTime:F1} seconds away. Low-pitch sound! You must go DOWN. Hold S until it passes.",
            NarrationType.WrongReleaseUp => "Careful! You can't release the key before the obstacle passes, or you'll hit the wall! Keep A held until it passes.",
            NarrationType.WrongReleaseDown => "You can't release the S key early. The bat hits the ground.",
            NarrationType.FreezeWait => "Still waiting for the sound. Press Space when you're ready.",
            NarrationType.FailNoUp => "You didn't go up in time. The obstacle hit you. You lost.",
            NarrationType.FailNoDown => "You didn't go down in time. You lost.",
            NarrationType.SuccessUp => "Good! You're going UP! Keep holding A.",
            NarrationType.SuccessDown => "Good! You're going DOWN! Keep holding S.",
            NarrationType.TutorialEnd => "Now the real game begins!",
            _ => ""
        };
    }
    
    private void PlayHighPitchSound()
    {
        if (highPitchSound != null)
        {
            AudioSource.PlayClipAtPoint(highPitchSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }
    }
    
    private void PlayLowPitchSound()
    {
        if (lowPitchSound != null)
        {
            AudioSource.PlayClipAtPoint(lowPitchSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }
    }
    
    private void PlayObstacleHitSound()
    {
        if (obstacleHitSound != null)
        {
            AudioSource.PlayClipAtPoint(obstacleHitSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }
    }
    
    private void PlayFailSound()
    {
        if (failSound != null)
        {
            AudioSource.PlayClipAtPoint(failSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }
    }
    
    private void PlaySuccessSound()
    {
        if (successSound != null)
        {
            AudioSource.PlayClipAtPoint(successSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }
    }
}