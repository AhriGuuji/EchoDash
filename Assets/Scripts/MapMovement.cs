using UnityEngine;

public class MapMovement : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = -2f;

    private void Update()
    {
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);
    }
}
