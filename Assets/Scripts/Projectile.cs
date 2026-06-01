using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private AudioClip highSound, lowSound;
    [SerializeField] private float lifespan;
    [SerializeField] private LayerMask high, low;
    private float _timer;
    private Vector3 _initPos;

    private void Awake()
    {
        _initPos = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out HighPich _)) 
        { AudioSource.PlayClipAtPoint(highSound, _initPos); }
        
        if (collision.gameObject.TryGetComponent(out LowPich _))  AudioSource.PlayClipAtPoint(lowSound, _initPos); 

        Destroy(gameObject);
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= lifespan)
            Destroy(gameObject);
    }
}
