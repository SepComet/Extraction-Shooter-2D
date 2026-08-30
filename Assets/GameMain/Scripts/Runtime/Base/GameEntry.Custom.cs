using SepCore.CustomComponent;
using UnityEngine;

/// <summary>
/// 游戏入口。
/// </summary>
public partial class GameEntry : MonoBehaviour
{
    public static BuiltinDataComponent BuiltinData { get; private set; }

    public static LubanComponent Luban { get; private set; }

    public static SaveComponent Save { get; private set; }

    private static void InitCustomComponents()
    {
        BuiltinData = UnityGameFramework.Runtime.GameEntry.GetComponent<BuiltinDataComponent>();
        Luban = UnityGameFramework.Runtime.GameEntry.GetComponent<LubanComponent>();
        Save = UnityGameFramework.Runtime.GameEntry.GetComponent<SaveComponent>();
    }
}
