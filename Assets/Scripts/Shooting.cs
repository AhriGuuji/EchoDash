using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Shooting : MonoBehaviour
{
    [SerializeField] private GameObject projectilprefab;
    [SerializeField] private float speed;
    [SerializeField] private Vector2 offset = new Vector2(1f, 0f);
    [SerializeField] private string inputName;
    [SerializeField] private AudioClip sound;

    private Rigidbody2D _rb;
    private InputAction _input;

    void Awake()
    {
        _input = InputSystem.actions.FindAction(inputName);
    }

    void Update()
    {
        if (_input != null && _input.triggered)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Vector2 spawn = (Vector2)transform.position + offset;

        GameObject projectile = Instantiate(projectilprefab, spawn, transform.rotation);

        _rb = projectile.GetComponent<Rigidbody2D>();
        if (_rb != null)
        {
            _rb.linearVelocity = transform.right * speed;
        }

        AudioSource.PlayClipAtPoint(sound, transform.position);
    }
}
