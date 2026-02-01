using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    public List<IdentificationData> tutorialEntrants = new List<IdentificationData>();
    private int currentTutorialIndex = 0;

    private void Awake() { Instance = this; InitializeTutorialData(); }

    private void InitializeTutorialData()
    {
        tutorialEntrants.Clear();
        // 1.정상 2.ID위조 3.고온 4.인간 5.캡챠재시도
        tutorialEntrants.Add(CreateData(0, 0, "KB-2541-FL", 32.5f, MachineSoundType.Normal, false, false));
        var data2 = CreateData(1, 1, "ZT-4052-MP", 33.0f, MachineSoundType.Normal, false, true);
        data2.Actual_ID = "KB-9999-XX"; tutorialEntrants.Add(data2);
        tutorialEntrants.Add(CreateData(2, 2, "VN-1938-QR", 41.5f, MachineSoundType.Normal, true, false));
        var data4 = CreateData(0, 1, "SN-0000-ZZ", 36.5f, MachineSoundType.Heartbeat, false, false);
        data4.IsHumanMask = true; tutorialEntrants.Add(data4);
        var data5 = CreateData(3, 3, "CP-7788-RZ", 31.0f, MachineSoundType.Normal, false, false);
        data5.Actual_Captcha = CaptchaResult.Unknown; tutorialEntrants.Add(data5);
    }

    private IdentificationData CreateData(int h, int b, string id, float t, MachineSoundType s, bool d, bool c)
    {
        IdentificationData data = new IdentificationData { Actual_HeadIndex = h, Document_HeadIndex = h, Actual_BodyIndex = b, Document_BodyIndex = b, Actual_ID = id, Entrant_ID = id, Temperature = t, Actual_Sound = s, IsDefective = d, HasContradiction = c, Actual_Captcha = CaptchaResult.Pass };
        return data;
    }

    public IdentificationData GetNextTutorialData()
    {
        if (currentTutorialIndex < tutorialEntrants.Count) return tutorialEntrants[currentTutorialIndex++];
        return null;
    }
    public bool IsTutorialFinished() => currentTutorialIndex >= tutorialEntrants.Count;
}