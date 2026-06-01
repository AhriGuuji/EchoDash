using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private Transform topTarget, botTarget;
    [SerializeField] private string inputName = "Move";
    
    private Rigidbody2D _rb;
    private InputAction _input;
    private Vector3 _ogPos;
    
    // Tutorial state
    private bool tutorialMode = true;
    private bool forceUp = false;
    private bool forceDown = false;
    private bool tutorialControlActive = false;
    
    void OnEnable()
    {
        Tutorial.OnRequireUp += HandleRequireUp;
        Tutorial.OnRequireDown += HandleRequireDown;
        Tutorial.OnReleaseUpEarly += HandleReleaseUpEarly;
        Tutorial.OnReleaseDownEarly += HandleReleaseDownEarly;
        Tutorial.OnTutorialComplete += HandleTutorialComplete;
    }
    
    void OnDisable()
    {
        Tutorial.OnRequireUp -= HandleRequireUp;
        Tutorial.OnRequireDown -= HandleRequireDown;
        Tutorial.OnReleaseUpEarly -= HandleReleaseUpEarly;
        Tutorial.OnReleaseDownEarly -= HandleReleaseDownEarly;
        Tutorial.OnTutorialComplete -= HandleTutorialComplete;
    }

    private void Start()
    {
        _input = InputSystem.actions.FindAction(inputName);
        _rb = GetComponent<Rigidbody2D>();
        _ogPos = transform.position;
    }

    private void FixedUpdate()
    {
            // Normal gameplay movement
            float dir = _input.ReadValue<Vector2>().y;

            if (dir > 0.1f)
            {
                _rb.MovePosition(topTarget.position);
            }
            else if (dir < -0.1f)
            {
                _rb.MovePosition(botTarget.position);
            }
            else
            {
                _rb.MovePosition(_ogPos);
            }
            // No input - bat stays where it is
    }
    
    // ============ TUTORIAL EVENT HANDLERS ============
    
    private void HandleRequireUp()
    {
        tutorialControlActive = true;
        forceUp = true;
        forceDown = false;
        Debug.Log("Tutorial: Move UP");
    }
    
    private void HandleRequireDown()
    {
        tutorialControlActive = true;
        forceDown = true;
        forceUp = false;
        Debug.Log("Tutorial: Move DOWN");
    }
    
    private void HandleReleaseUpEarly()
    {
        tutorialControlActive = false;
        forceUp = false;
        Debug.Log("Tutorial: Released UP too early!");
    }
    
    private void HandleReleaseDownEarly()
    {
        tutorialControlActive = false;
        forceDown = false;
        Debug.Log("Tutorial: Released DOWN too early!");
    }
    
    private void HandleTutorialComplete()
    {
        tutorialMode = false;
        tutorialControlActive = false;
        forceUp = false;
        forceDown = false;
        Debug.Log("Tutorial complete! Normal movement enabled.");
    }
    
    // Public method to check if in tutorial mode
    public bool IsInTutorialMode() => tutorialMode;
}