using UnityEngine;

public class SuccessSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent(out Movement _))
            audioSource.PlayOneShot(clip);
    }
}
