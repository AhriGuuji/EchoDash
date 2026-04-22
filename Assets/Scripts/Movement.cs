using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private Transform topTarget, botTarget;
    [SerializeField] private string inputName = "Move";
    private Rigidbody2D _rb;
    private InputAction _input;

    private void Start()
    {
        _input = InputSystem.actions.FindAction(inputName);
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        float dir = _input.ReadValue<Vector2>().y;

        if (dir > 0)
        {
            _rb.MovePosition(topTarget.localPosition);
        }
        else if (dir < 0)
        {
            _rb.MovePosition(botTarget.localPosition);
        }
        else _rb.MovePosition(new (transform.position.x, 0));
    }
}
