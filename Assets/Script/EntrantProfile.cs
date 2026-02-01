using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Game/EntrantProfile")]
public class EntrantProfile : ScriptableObject
{

    [TextArea] public string[] Dialogue_Normal; // 캐릭터 대사 
}