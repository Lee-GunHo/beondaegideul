using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int currentMoney = 1000;
    public int currentStageIndex = 0;
    public List<ShopItemType> itemTypes = new List<ShopItemType>();
    public List<int> itemCounts = new List<int>();
}

public class GlobalDataManager : MonoBehaviour
{
    public static GlobalDataManager Instance { get; private set; }

    public GameData Data = new GameData();
    public Dictionary<ShopItemType, int> inventory = new Dictionary<ShopItemType, int>();

    private string savePath;

    // [호환성 브릿지]
    public int currentMoney
    {
        get { return Data.currentMoney; }
        set { Data.currentMoney = value; }
    }

    public int currentStageIndex
    {
        get { return Data.currentStageIndex; }
        set { Data.currentStageIndex = value; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Path.Combine(Application.persistentDataPath, "SaveFile.json");
        LoadGame();
    }

    // ========================================================================
    // [기능 구현] 외부 호출 함수들
    // ========================================================================

    // ★ [복구됨] EconomyManager가 호출하는 함수!
    public void UpdateMoney(int amount)
    {
        currentMoney += amount; // 돈 더하기/빼기

        // 돈은 마이너스가 될 수 없음 (안전장치)
        if (currentMoney < 0) currentMoney = 0;

        Debug.Log($"[재화 변경] 변동액: {amount}, 현재 잔액: {currentMoney}");

        SaveGame(); // 돈 바뀔 때마다 자동 저장
    }

    public void AddItem(ShopItemType type, int amount = 1)
    {
        if (type == ShopItemType.None) return;

        if (!inventory.ContainsKey(type)) inventory[type] = 0;
        inventory[type] += amount;

        Debug.Log($"[아이템 획득] {type}: {inventory[type]}개");
        SaveGame();
    }

    public bool UseItem(ShopItemType type)
    {
        if (inventory.ContainsKey(type) && inventory[type] > 0)
        {
            inventory[type]--;
            Debug.Log($"[아이템 사용] {type} 남은 개수: {inventory[type]}");
            SaveGame();
            return true;
        }
        return false;
    }

    public void CompleteDay()
    {
        Debug.Log($"[일차 종료] {currentStageIndex + 1}일차 완료 -> 저장됨");
        currentStageIndex++;
        SaveGame();
    }

    public void StartNewGame()
    {
        Data = new GameData();
        Data.currentMoney = 1000;
        Data.currentStageIndex = 0;

        inventory.Clear();
        InitializeInventory();

        SaveGame();
        Debug.Log("새 게임 데이터 생성 완료");
    }

    private void InitializeInventory()
    {
        foreach (ShopItemType type in System.Enum.GetValues(typeof(ShopItemType)))
        {
            if (type != ShopItemType.None && !inventory.ContainsKey(type))
                inventory[type] = 0;
        }
    }

    // ========================================================================
    // [JSON 저장/불러오기]
    // ========================================================================

    public void SaveGame()
    {
        Data.itemTypes.Clear();
        Data.itemCounts.Clear();

        foreach (var pair in inventory)
        {
            Data.itemTypes.Add(pair.Key);
            Data.itemCounts.Add(pair.Value);
        }

        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("저장된 파일이 없어 새 게임으로 시작합니다.");
            StartNewGame();
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            Data = JsonUtility.FromJson<GameData>(json);

            inventory.Clear();
            InitializeInventory();

            for (int i = 0; i < Data.itemTypes.Count; i++)
            {
                ShopItemType type = Data.itemTypes[i];
                int count = Data.itemCounts[i];
                inventory[type] = count;
            }

            Debug.Log($"<color=cyan>[로드 성공]</color> {currentStageIndex + 1}일차 | 소지금: {currentMoney}");
            Debug.Log($"저장 파일 위치: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"세이브 파일 깨짐! 초기화합니다. ({e.Message})");
            StartNewGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}