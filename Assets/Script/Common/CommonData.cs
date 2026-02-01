using UnityEngine;

public enum DecisionType { Pass, Reject, Discard }
public enum EntrantState { None, Approaching, Waiting, Verifying, Decided, Exiting }
public enum MachineSoundType { Normal, Noisy, Heartbeat }
public enum CaptchaResult { None, Pass, Unknown, Failed }
public enum ShopItemType { None, Item_WashSpray, CoolingFan, AutoStamp, HintBook }

[System.Serializable]
public class IdentificationData
{
    public string Entrant_ID;
    public string Actual_ID;
    public string Entrant_PW;
    public string Actual_PW;
    public float Temperature;
    public MachineSoundType Actual_Sound;
    public CaptchaResult Actual_Captcha;

    // --- [추가] 캡챠 인증에 소요되는 시간 데이터 ---
    public float CaptchaTime;

    public int Actual_HeadIndex;
    public int Document_HeadIndex;
    public int Actual_BodyIndex;
    public int Document_BodyIndex;

    public bool IsHumanMask;
    public bool HasContradiction;
    public bool IsDefective;
    public bool HasStain;

    public bool HasVisualContradiction => (Document_HeadIndex != Actual_HeadIndex) || (Document_BodyIndex != Actual_BodyIndex);
}