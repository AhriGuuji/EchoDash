using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float lifespan;
    private float _timer;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position); 

        Destroy(gameObject);
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= lifespan)
            Destroy(gameObject);
    }
}
