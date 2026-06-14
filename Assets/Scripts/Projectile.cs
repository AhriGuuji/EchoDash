using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private AudioClip highSound, lowSound;
    [SerializeField] private float lifespan;
    [SerializeField] private LayerMask high, low;
    [field: SerializeField] public AudioSource AudioSource { get; set; }
    private float _timer;
    private Vector3 _initPos;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out HighPich _)) 
            AudioSource.PlayOneShot(highSound);
        
        if (collision.gameObject.TryGetComponent(out LowPich _))  
            AudioSource.PlayOneShot(lowSound); 

        Destroy(gameObject);
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= lifespan)
            Destroy(gameObject);
    }
}
