using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private Transform topTarget, botTarget;
    [SerializeField] private string inputName = "Move";
    
    private Rigidbody2D _rb;
    private InputAction _input;
    private Vector3 _ogPos;

    private void Start()
    {
        _input = InputSystem.actions.FindAction(inputName);
        _rb = GetComponent<Rigidbody2D>();
        _ogPos = transform.position;
    }

    private void FixedUpdate()
    {
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
    }
}