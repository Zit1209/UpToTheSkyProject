using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryTrigger : MonoBehaviour
{
    [SerializeField] private string victorySceneName = "Victory Scene";

    // Dùng khi Player chạm vào trigger (ví dụ: cổng đích, vật phẩm chiến thắng)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(victorySceneName);
        }
    }
}