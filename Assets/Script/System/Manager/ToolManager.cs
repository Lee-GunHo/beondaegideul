using UnityEngine;
using UnityEngine.UI; // 버튼 사용을 위해 필요
using TMPro;          // 텍스트(TMP) 사용을 위해 필요

public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance;

    [Header("UI 연결")]
    public Button sprayButton;       // 인스펙터에서 넣을 버튼
    public TMP_Text sprayCountText;  // 인스펙터에서 넣을 개수 텍스트

    [Header("Scanner 감별칩")]
    public Button scannerButton;     // 스캐너 사용 버튼
    public TMP_Text scannerResultText;
    public GameObject scannerResultPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        UpdateUI();

        // 버튼 클릭 시 실행할 함수 연결
        if (sprayButton != null)
        {
            sprayButton.onClick.RemoveAllListeners();
            sprayButton.onClick.AddListener(TryUseSpray);
        }

        // 스캐너 버튼 연결
        if (scannerButton != null)
        {
            scannerButton.onClick.RemoveAllListeners();
            scannerButton.onClick.AddListener(TryUseScanner);
        }

        if (scannerResultPanel != null) scannerResultPanel.SetActive(false);
        if (scannerResultText != null) scannerResultText.text = "";
    }

    // 버튼을 누르면 호출되는 함수
    public void TryUseSpray()
    {
        if (GlobalDataManager.Instance == null) return;

        // 아이템 개수 확인 로직 (그대로 유지)
        int currentCount = 0;
        if (GlobalDataManager.Instance.inventory.ContainsKey(ShopItemType.Item_WashSpray))
        {
            currentCount = GlobalDataManager.Instance.inventory[ShopItemType.Item_WashSpray];
        }

        if (currentCount <= 0)
        {
            Debug.Log("스프레이 아이템이 부족합니다!");
            return;
        }

        // [수정됨] 화면(로봇 몸체)에 있는 'Stain' 컴포넌트를 찾습니다.
        Stain targetStain = FindObjectOfType<Stain>();

        if (targetStain != null)
        {
            targetStain.Clean(); // 얼룩 지우기
            GlobalDataManager.Instance.UseItem(ShopItemType.Item_WashSpray); // 아이템 소모

            UpdateUI(); // UI 갱신
        }
        else
        {
            Debug.Log("지울 얼룩이 없습니다.");
        }
    }

    public void TryUseScanner()
    {
        // 1. 매니저들이 살아있는지 확인
        if (GlobalDataManager.Instance == null || EntrantManager.Instance == null) return;

        // 2. 아이템 개수 확인 (Scanner 아이템 타입이 있다고 가정)
        // 만약 아직 Enum에 Scanner가 없다면, 일단 개수 체크 로직은 주석 처리하고 테스트하세요.
        /*
        int currentCount = 0;
        if (GlobalDataManager.Instance.inventory.ContainsKey(ShopItemType.Item_Scanner))
        {
            currentCount = GlobalDataManager.Instance.inventory[ShopItemType.Item_Scanner];
        }

        if (currentCount <= 0)
        {
            Debug.Log("스캐너 아이템이 부족합니다!");
            return;
        }
        */

        // 3. 현재 입국자 데이터 가져오기
        IdentificationData data = EntrantManager.Instance.currentData;

        // 4. 인간인지 판별 (EntrantManager의 로직과 동일하게)
        // 인간인 경우: IsHumanMask가 true이거나 심장박동 소리가 나는 경우
        bool isHuman = data.IsHumanMask || (data.Actual_Sound == MachineSoundType.Heartbeat);

        // 5. 결과 출력 (코루틴으로 2초간 보여주기)
        StartCoroutine(ShowScannerResult(isHuman));

        // 6. 아이템 소모 (Enum 추가 후 주석 해제)
        // GlobalDataManager.Instance.UseItem(ShopItemType.Item_Scanner);
        // UpdateUI();
    }

    // 결과를 2초 동안 보여주고 끄는 코루틴
    private System.Collections.IEnumerator ShowScannerResult(bool isHuman)
    {
        if (scannerResultPanel != null) scannerResultPanel.SetActive(true);

        if (scannerResultText != null)
        {
            if (isHuman)
            {
                scannerResultText.text = "<color=red>인간입니다.</color>";
                // (필요하면 여기서 '삐- 경고' 효과음 재생)
            }
            else
            {
                scannerResultText.text = "<color=green>로봇입니다.</color>";
                // (필요하면 여기서 '딩동' 효과음 재생)
            }
        }

        // 2초 대기
        yield return new WaitForSeconds(2.0f);

        // 다시 끄기
        if (scannerResultPanel != null) scannerResultPanel.SetActive(false);
        if (scannerResultText != null) scannerResultText.text = "";
    }

    // 남은 개수 표시 갱신
    public void UpdateUI()
    {
        if (sprayCountText == null || GlobalDataManager.Instance == null) return;

        int count = 0;
        if (GlobalDataManager.Instance.inventory.ContainsKey(ShopItemType.Item_WashSpray))
        {
            count = GlobalDataManager.Instance.inventory[ShopItemType.Item_WashSpray];
        }

        sprayCountText.text = count.ToString();
    }
}