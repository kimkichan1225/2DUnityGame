using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathSceneManager : MonoBehaviour
{
    // 버튼 클릭 시 호출될 함수
    public void GoToMainMenu()
    {
        // 1. (★수정됨★) 멈췄던 시간을 1f (정상 속도)로 되돌립니다.
        Time.timeScale = 1f;

        

        // 3. (★수정됨★) DontDestroyOnLoadManager의 플래그를 설정합니다.
        //    이제 GameManager 등을 직접 Destroy() 할 필요가 없습니다.
        DontDestroyOnLoadManager.isReturningToMainMenu = true;

        // 4. "Main" 씬을 로드합니다.
        SceneManager.LoadScene("Main");
    }
}
