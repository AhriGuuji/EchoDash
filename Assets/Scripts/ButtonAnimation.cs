using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image sprite;
    [SerializeField] private string animationName;
    [SerializeField] private string defaultAnimationName;
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    public void OnSelect(BaseEventData eventData)
    {
        sprite.enabled = true;
        anim.Play(animationName);
        audioSource.PlayOneShot(clip);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        sprite.enabled = false;
        anim.Play(defaultAnimationName);
    }
}