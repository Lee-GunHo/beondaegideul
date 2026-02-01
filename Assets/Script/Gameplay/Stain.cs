using UnityEngine;

public class Stain : MonoBehaviour
{
    // 외부(ToolManager)에서 이 함수를 부르면 로봇 몸체의 얼룩이 사라집니다.
    public void Clean()
    {
        Debug.Log("로봇의 얼룩을 제거했습니다.");

        // (이펙트/사운드 재생 위치)

        Destroy(gameObject); // 얼룩 오브젝트 삭제
    }
}