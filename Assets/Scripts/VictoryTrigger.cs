using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    [SerializeField] private string victorySceneName = "VictoryScene";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Movement>() != null)
        {
            Debug.Log("Player reached the victory trigger!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(victorySceneName);
        }
    }
}
