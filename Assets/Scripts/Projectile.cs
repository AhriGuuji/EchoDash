using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private AudioClip hitSound;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position); 
        }

        Destroy(gameObject);
    }
}
