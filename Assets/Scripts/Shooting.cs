using UnityEngine;
using UnityEngine.InputSystem;

public class Shooting : MonoBehaviour
{
    [SerializeField] private GameObject projectilPrefab;
    [SerializeField] private float speed;
    [SerializeField] private Transform offset;
    [SerializeField] private string inputName;
    [SerializeField] private AudioClip sound;
    [Header("Flash Setts")]
    [SerializeField] private Animator lightAnim;
    [SerializeField] private string lightAnimName = "Flash";

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
        Vector2 spawn = offset.position;

        GameObject projectile = Instantiate(projectilPrefab, spawn, transform.rotation);

        _rb = projectile.GetComponent<Rigidbody2D>();
        if (_rb != null)
        {
            _rb.linearVelocity = transform.right * speed;
        }

        if (sound) AudioSource.PlayClipAtPoint(sound, transform.position, 0.2f);
        if (lightAnim) lightAnim.SetTrigger(lightAnimName);
    }
}
