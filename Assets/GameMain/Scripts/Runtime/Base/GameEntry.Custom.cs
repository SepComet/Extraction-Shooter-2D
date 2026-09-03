using SepCore.Battle;
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

    public static RandomComponent Random { get; private set; }

    public static TurnBattleComponent TurnBattle { get; private set; }

    private static void InitCustomComponents()
    {
        BuiltinData = UnityGameFramework.Runtime.GameEntry.GetComponent<BuiltinDataComponent>();
        Luban = UnityGameFramework.Runtime.GameEntry.GetComponent<LubanComponent>();
        Save = UnityGameFramework.Runtime.GameEntry.GetComponent<SaveComponent>();
        Random = UnityGameFramework.Runtime.GameEntry.GetComponent<RandomComponent>();
        TurnBattle = UnityGameFramework.Runtime.GameEntry.GetComponent<TurnBattleComponent>();
    }
}
