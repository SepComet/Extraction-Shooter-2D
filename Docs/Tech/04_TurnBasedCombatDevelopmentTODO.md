# 回合制战斗开发 TODO

> 状态：进行中（M0 组件已落地，其余里程碑未开始）
>
> 创建日期：2026-08-31
>
> 相关设计：`Docs/GameDesign/04_TurnBasedCombat.md`、`Docs/GameDesign/03_RunExploration.md`、`Docs/GameDesign/06_PrototypeScope.md`
>
> 目标：在不新增 Battle 场景的前提下，实现覆盖在地图上的半透明多对多回合制战斗 UI，以及可独立测试的战斗规则内核。

## 一、完成纪律

- 里程碑是否完成只看标题中的 `[ ]`；第八节的复选框单独追踪 22 条设计验收，不代替里程碑完成状态。
- 代码写完、静态检查通过或 Unity 编译通过，都不能单独把里程碑改为 `[x]`。
- 只有完成该里程碑列出的 Unity 编译、EditMode 测试和 Play Mode 可见验收，并记录证据后，才能改为 `[x]`。
- 每次只实现当前里程碑。本里程碑“明确不做”的内容不能因为顺手而提前并入。
- 生成代码不能手改。Luban 结构或数据需要调整时，修改源表后运行 `数据表/gen_cli.sh`。
- 战斗内核不得直接读写 `SaveData`，不得直接操作地图敌人物体、掉落物或局外结算。
- 本文档当前所有里程碑均为未完成；本文档落地不代表任何代码或运行时验收已经完成。

每个里程碑完成后按以下格式补证据：

```text
代码状态：未开始 / 进行中 / 已完成
Unity 编译：未验证 / 通过（日期、日志或截图）
EditMode：未验证 / 通过（用例数量、结果）
Play Mode：未验证 / 通过（操作步骤、可见结果、截图或录像）
提交或差异：commit / diff 路径
剩余问题：无 / 具体问题
```

## 二、总体进度

| 里程碑 | 可见结果 | 当前状态 |
| --- | --- | --- |
| M0 接口与调试入口 | Main 场景可打开和关闭半透明战斗壳层，地图更新暂停 | 已完成 |
| M1 1v1 攻击闭环 | 玩家攻击、敌人反击并产生胜负 | 未开始 |
| M2 4v4 回合调度 | 多人队伍按实时速度和并列规则行动 | 未开始 |
| M3 技能、目标与效果 | 数值型攻击和技能可合法选目标并结算 HP/MP | 未开始 |
| M4 状态与速度变化 | 眩晕跳过下一次行动，速度变化立即刷新顺序 | 未开始 |
| M5 先制与确定性随机 | 先制首轮和普通次轮正确，同一随机输入可复现 | 未开始 |
| M6 单人逃跑与结束矩阵 | 成功、失败、全逃跑、部分逃跑后阵亡均可见 | 未开始 |
| M7 单局状态回写 | HP/MP、阵亡恢复和战斗状态清理正确回到临时单局状态 | 未开始 |
| M8 探索遭遇接入 | 碰撞进入战斗，胜利移除敌人，逃跑恢复敌人并获得保护 | 未开始 |
| M9 战斗 UI 完成 | 最小信息、输入限制和即时反馈完整 | 未开始 |
| M10 全量回归 | 战斗文档 22 条验收全部留有运行证据 | 未开始 |

### 验证记录

| 里程碑 | 代码 | Unity 编译 | EditMode | Play Mode | 证据 |
| --- | --- | --- | --- | --- | --- |
| M0 | 已完成 | 通过（2026-08-31，dotnet build SepCore.Runtime.csproj 0 错误；Unity 编辑器内 Play Mode 实测编译通过） | 不适用（M0 无 EditMode 用例） | 通过（2026-08-31，见 M0 证据） | 已记录（M0 小节） |
| M1 | 未开始 | 未验证 | 未验证 | 未验证 | 未记录 |
| M2 | 未开始 | 未验证 | 未验证 | 未验证 | 未记录 |
| M3 | 未开始 | 未验证 | 未验证 | 未验证 | 未记录 |
| M4 | 未开始 | 未验证 | 未验证 | 未验证 | 未记录 |
| M5 | 未开始 | 未验证 | 未验证 | 未验证 | 未记录 |
| M6 | 未开始 | 未验证 | 未验证 | 未验证 | 未记录 |
| M7 | 未开始 | 未验证 | 未验证 | 未验证 | 未记录 |
| M8 | 未开始 | 未验证 | 未验证 | 未验证 | 未记录 |
| M9 | 未开始 | 未验证 | 未验证 | 未验证 | 未记录 |
| M10 | 未开始 | 未验证 | 未验证 | 未验证 | 未记录 |

## 三、已确认的范围与约定

以下决定于 2026-08-31 确认，后续实现不得自行改变：

| 项目 | 约定 |
| --- | --- |
| 单局基础 | 本 TODO 包含最小单局临时状态及其接口，不包含完整地图生成、搜索和警惕 AI 的实现 |
| 战斗承载 | 不新增 Battle 场景；在 Main 场景地图上覆盖半透明的顶级 `BattleForm` |
| 地图暂停 | 战斗期间暂停地图角色、敌人 AI、碰撞、搜索和单局计时的更新，战斗 UI 继续响应 |
| 模块边界 | 战斗接收值快照和本局随机接口，返回结果；不直接读写存档、地图敌人或掉落 |
| 眩晕 | `DurationRounds=N` 影响目标未来 N 次行动机会；`Stun(1)` 跳过下一次行动后失效 |
| 状态重复施加 | 同类状态不叠层，保留较长的剩余持续次数 |
| 友方目标 | `SingleAlly` 包含施法者；`Self` 只能选择施法者自己 |
| 阵亡目标 | 阵亡和已逃跑单位不是合法目标；首版没有战斗内复活 |
| 战斗表现 | 首版使用面板式即时结算，不做站位动画、攻击动画、VFX、Timeline 或音效同步 |
| 操作方式 | 首版使用鼠标点击行动和目标 |
| 行动顺序 UI | 显示当前行动者和本轮剩余行动者；速度变化后立即刷新，不预测下一轮 |

## 四、模块所有权

### 4.1 目录与程序集

| 位置 | 所有权 |
| --- | --- |
| `Assets/GameMain/Scripts/Runtime/Battle/` | 战斗接口与 DTO、纯战斗模型、调度、指令校验、效果结算、敌人策略和 `BattleController` |
| `Assets/GameMain/Scripts/Runtime/CustomComponent/Run/` | 最小单局临时状态、探索暂停和战斗协调 |
| `Assets/GameMain/Scripts/Runtime/CustomComponent/Random/` | 本局种子与共享随机源（`RandomComponent`、`IRunRandomSource`、`RunRandomSource`） |
| `Assets/GameMain/Scripts/UI/Battle/` | `BattleForm`、子 Form/View 和手写 `.Logic.cs` 表现逻辑 |
| `Assets/GameMain/Scripts/Base/Event/` | 只有确实需要跨模块广播的 UGF 事件；战斗内部通信不走全局事件总线 |
| `Assets/GameMain/Tests/EditMode/Battle/` | 战斗规则的纯逻辑 EditMode 测试及测试程序集 |

不新增战斗程序集。接口、DTO 和内核均放入现有 `SepCore.Runtime`；`SepCore.Presentation` 已经单向引用 Runtime，Runtime 不得反向引用 Presentation。

### 4.2 责任划分

| 模块 | 负责 | 不负责 |
| --- | --- | --- |
| `RandomComponent` | 本局 seed、共享随机源；地图生成、敌人 AI、逃跑和掉落共用 | 战斗行动规则、地图对象、存档写盘 |
| `TurnBattleComponent` | 临时角色状态、计时、战斗占用、探索暂停标志 | 战斗行动规则、战斗 UI、存档写盘 |
| `RunBattleCoordinator` | 从单局状态构建请求、打开 UI、提交结果、生成地图恢复计划 | 回合排序、效果计算、UI 控件状态 |
| `BattleSession` | 单位、轮次、行动机会、效果、状态、敌人 AI、逃跑和结束判定 | 地图对象、SaveData、UI GameObject |
| `BattleController` | 暴露只读快照、接收玩家指令、推进自动行动、产出行动记录和最终结果 | 直接渲染 UI、直接控制地图 |
| `BattleForm` | 展示快照、收集玩家指令、即时播放行动记录、展示结束结果 | 修改战斗数据、读取存档、决定掉落 |
| 探索适配器 | 碰撞发起遭遇、冻结/恢复地图、敌人移除或复位、掉落、2 秒保护 | 战斗内部规则 |

### 4.3 调用链

```text
队长与地图敌人碰撞
    -> 探索层创建 BattleEncounter
    -> RunBattleCoordinator.TryStartBattle
    -> TurnBattleComponent 预留战斗占用，拒绝重复遭遇
    -> RunBattleCoordinator 构建 BattleStartRequest
    -> 创建并校验 BattleController；失败则释放占用且不暂停地图
    -> 校验成功后提交战斗占用并暂停探索更新
    -> 打开 BattleForm，并调用 BattleController.Start
    -> BattleForm 展示 BattleSnapshot
    -> 玩家提交 BattleCommand / 敌人策略自动提交行动
    -> BattleController 返回 BattleAdvance
    -> BattleForm 即时展示 BattleActionRecord
    -> BattleSession 产生 BattleResult
    -> RunBattleCoordinator.ApplyResult
    -> 更新单局临时角色状态并生成 BattleReturnPlan
    -> 探索层执行敌人、掉落、保护或单局失败处理
    -> 关闭 BattleForm，并在允许继续单局时恢复探索更新
```

## 五、接口契约

本节固定接口语义。实现时允许按 C# 规范调整只读集合或构造函数形式，但不得改变所有权和数据流。

### 5.1 身份规则

| 标识 | 用途 |
| --- | --- |
| `EncounterId` | 地图敌人实例的单局唯一标识；不能使用敌人配置 ID 代替 |
| `BattleUnitId` | 本场战斗单位的唯一运行时标识；重复配置的敌人必须拥有不同 ID |
| `CharacterId` | 玩家角色配置标识；同时用于把结果回写到单局角色状态 |
| `EnemyConfigId` | 敌人种类配置标识；同一敌人队伍内可以重复 |
| `PartyOrder` | 玩家取战备顺序，敌人取 `EnemyPartyConfig.EnemyIds` 顺序；作为同阵营同速度的最终并列规则 |

所有 UI 指令和目标选择都使用 `BattleUnitId`，不能使用可能重复的 `EnemyConfigId`。

### 5.2 遭遇输入

`BattleEncounter` 由探索层创建：

| 字段 | 说明 |
| --- | --- |
| `EncounterId` | 触发碰撞的地图敌人实例 |
| `EnemyPartyConfigId` | 该地图敌人代表的敌人队伍预设 |
| `IsPreemptive` | 碰撞时敌人警惕值未满为 true，否则为 false |

地图坐标、碰撞器、GameObject 和警惕组件不进入战斗请求，由探索层继续持有。

### 5.3 玩家输入快照

`BattlePlayerInput` 是进入本场战斗时的值快照：

| 字段 | 说明 |
| --- | --- |
| `CharacterId` | 角色配置 ID |
| `PartyOrder` | 当前战备中的顺序 |
| `CurrentHp` / `CurrentMp` | 单局临时状态中的当前值 |
| `MaxHp` / `MaxMp` | 已结算装备加成的最终上限 |
| `Atk` / `Mat` / `Speed` | 已结算装备加成的最终战斗属性 |
| `AttackActionId` | 普通攻击行动配置 ID |
| `SkillActionId` | 该角色首版唯一技能行动配置 ID |

装备计算由单局角色快照构建器完成。`BattleSession` 不读取 `CharacterSave`、装备栏或背包。

### 5.4 战斗启动请求

`BattleStartRequest` 至少包含：

| 字段 | 说明 |
| --- | --- |
| `EncounterId` | 用于将结果关联回地图敌人实例 |
| `Players` | 1 到 4 个 `BattlePlayerInput`，保持战备顺序 |
| `EnemyPartyConfigId` | 用于按队伍预设创建 1 到 4 个敌人运行时单位 |
| `IsPreemptive` | 是否启用第一轮玩家先制 |
| `Random` | 当前单局唯一的 `IRunRandomSource` 实例，来自 `RandomComponent`，不创建战斗私有随机源 |

请求在创建后视为只读。战斗开始失败时不得消耗随机数、修改单局状态或暂停状态计数。

### 5.5 随机数接口

```csharp
public interface IRunRandomSource
{
    int NextInt(int minInclusive, int maxExclusive);
    bool RollPermille(int successPermille);
}
```

- 本局种子由局外（战备界面玩家输入）带入，`RandomComponent.BeginRun(seed)` 初始化共享随机源。
- 同一单局的地图生成、敌人 AI、逃跑和敌人掉落依次消费同一个实例；`RandomComponent` 是唯一持有者。
- 战斗不能根据 EncounterId、回合数或当前时间重新播种。
- `RollPermille(0)` 必须失败，`RollPermille(1000)` 必须成功。
- EditMode 测试使用可注入的序列随机源，不依赖 Unity 全局随机状态。

### 5.6 配置访问接口

`IBattleConfigProvider` 隔离 `BattleSession` 与 `GameEntry.Luban`。运行时适配器读取现有 Luban 表，测试使用内存配置。

```csharp
public interface IBattleConfigProvider
{
    EnemyPartyConfig GetEnemyParty(int id);
    EnemyConfig GetEnemy(int id);
    BattleActionConfig GetAction(int id);
    GlobalConfig GetGlobal();
}
```

配置缺失属于战斗启动失败，不允许用零值单位继续战斗。普通消费路径信任已经导出的合法配表，不在每次行动中重复做全表完整性扫描。

### 5.7 玩家指令

`BattleCommand` 至少包含：

| 字段 | 说明 |
| --- | --- |
| `ActorUnitId` | 当前获得行动机会的玩家单位 |
| `CommandType` | `Attack`、`Skill`、`Item` 或 `Escape`；复用配表 `BattleActionType` 枚举，不单独定义 |
| `ActionConfigId` | 攻击或技能对应的配置 ID；逃跑为 0 |
| `TargetUnitIds` | 按目标类型解析后的唯一运行时目标列表 |

- `Item` 按钮可见但首版禁用，不产生 `BattleCommand`。
- 只有 `BattleSnapshot` 指示的当前玩家单位能提交指令。
- MP 不足时技能按钮禁用；控制器仍需拒绝伪造的非法指令。
- `SingleAlly` 包含自己；`Self` 必须且只能以自己为目标。
- `AllAllies` 包含自己；全体目标由控制器根据当前合法单位展开，UI 不自行拼装。
- 阵亡、逃跑和敌对阵营错误的目标均非法。
- 非法指令不得消耗 MP、行动机会或随机数。

### 5.8 只读战斗快照

`BattleSnapshot` 是 UI 和敌人策略唯一允许读取的战斗状态。每次推进都创建逻辑上的只读快照，不能把可修改的内部列表或 `BattleUnit` 引用暴露给调用方。

| 字段 | 说明 |
| --- | --- |
| `RoundNumber` | 当前轮次，从 1 开始 |
| `IsPreemptiveRound` | 当前是否为先制战斗第一轮 |
| `CurrentActorUnitId` | 当前行动者；战斗完成时为空 |
| `Units` | 全部单位的只读视图，保持稳定显示顺序 |
| `RemainingTurnOrder` | 本轮从当前行动者开始的剩余运行时单位 ID；速度变化后重建 |
| `FlowState` | `AwaitingPlayerCommand` 或 `Completed` |

每个单位视图至少包含 `BattleUnitId`、阵营、配置 ID、`PartyOrder`、当前及上限 HP/MP、ATK、MAT、Speed、是否阵亡、是否逃跑、剩余状态和可用行动 ID。UI 不根据这些字段自行推演下一轮或判定胜负。

### 5.9 战斗推进返回值

`BattleController.Start()` 和 `SubmitCommand()` 返回 `BattleAdvance`：

| 字段 | 说明 |
| --- | --- |
| `Records` | 本次推进产生的有序 `BattleActionRecord` 列表 |
| `Snapshot` | 推进结束后的只读战斗快照 |
| `FlowState` | `AwaitingPlayerCommand` 或 `Completed` |
| `Result` | 仅在 Completed 时存在 |

`BattleActionRecord` 记录行动者、行动、目标，以及每个效果前后的数值或状态变化。首版 UI 立即消费并显示；未来动画系统可以逐条等待播放，不需要修改战斗规则内核。

### 5.10 敌人策略接口

```csharp
public interface IEnemyBattlePolicy
{
    BattleCommand Decide(
        BattleSnapshot snapshot,
        int actorUnitId,
        IRunRandomSource random);
}
```

- `EnemyAiType.Random` 对当前可用行动做等概率选择，再对合法的单体目标做等概率选择。
- MP 不足的行动不进入候选集。
- 全体目标不额外随机目标。
- 当前没有可执行行动时，本次行动机会记为跳过，不能形成无限循环。
- 策略只读取快照并返回指令，不直接修改 `BattleSession`。

### 5.11 战斗结果

`BattleOutcome` 固定为以下四种：

| 结果 | 条件 | 单局是否继续 |
| --- | --- | --- |
| `Victory` | 所有敌人阵亡 | 是 |
| `AllEscaped` | 所有玩家成功逃跑，且没有玩家阵亡 | 是 |
| `PartialEscapeDefeat` | 至少一名玩家逃跑，其余仍在战斗中的玩家全部阵亡 | 是 |
| `TotalDefeat` | 所有玩家阵亡，没有任何玩家成功逃跑 | 否 |

`BattleResult` 至少包含：

| 字段 | 说明 |
| --- | --- |
| `EncounterId` | 关联原地图敌人实例 |
| `Outcome` | 上述四种战斗结果之一 |
| `Players` | 每名参战玩家的原始战后结果 |

每个玩家结果包含 `CharacterId`、`CurrentHp`、`CurrentMp`、`WasDefeated`、`Escaped`。战斗结果保留原始战斗值：阵亡者 HP 为 0；恢复到 1 HP/1 MP 的规则由 `RunBattleCoordinator` 在非全员阵亡结果中统一应用。

### 5.12 探索恢复计划

`RunBattleCoordinator` 根据 `BattleResult` 生成 `BattleReturnPlan`，探索层只执行计划：

`BattleReturnPlan` 至少包含 `EncounterId`、`Outcome`、回写后的玩家临时状态、`RemoveEncounterEnemy`、`ResetEncounterEnemy`、`ShouldRollDrops`、`ProtectionMs`、`ShouldResumeExploration` 和 `EndsRunAsDefeated`。这些布尔值由协调器按下表统一产生，探索层不得再次根据玩家 HP 或逃跑数量重判战斗结果。

| 战斗结果 | 玩家状态 | 触发敌人 | 掉落 | 保护时间 | 单局计时 |
| --- | --- | --- | --- | --- | --- |
| `Victory` | 存活者保留 HP/MP，阵亡者恢复配置的 1/1，清除战斗状态 | 移除 | 使用本局随机源结算 | 无 | 恢复 |
| `AllEscaped` | 逃跑时 HP/MP 保留，清除战斗状态 | 保留并恢复地图行为，下次按完整预设重建 | 无 | 配置的 2 秒 | 恢复 |
| `PartialEscapeDefeat` | 逃跑者保留 HP/MP，阵亡者恢复配置的 1/1，清除状态 | 保留并恢复地图行为，下次按完整预设重建 | 无 | 配置的 2 秒 | 恢复 |
| `TotalDefeat` | 不返回探索 | 保持到单局失败清理 | 无 | 无 | 不恢复，进入失败结算 |

其他地图敌人在战斗期间被冻结；战斗结束后恢复战前的警惕和追击状态，不重新初始化。

探索层通过以下最小接口接收恢复计划：

```csharp
public interface IExplorationBattleBridge
{
    void ApplyBattleReturn(BattleReturnPlan plan);
}
```

碰撞方主动把 `BattleEncounter` 交给 `RunBattleCoordinator.TryStartBattle`；协调器不扫描场景寻找敌人。`ApplyBattleReturn` 必须按 `EncounterId` 精确操作触发敌人，不能按 `EnemyConfigId` 批量处理同类敌人。

### 5.13 探索暂停接口

- `TurnBattleComponent.IsExplorationPaused` 是地图更新的统一门禁。
- 玩家移动、队员跟随、敌人巡逻/警惕/追击、搜索进度、地图碰撞触发和单局计时都必须遵守该门禁。
- 战斗 UI、战斗内核和使用 unscaled time 的 UI 反馈不受该门禁影响。
- 首版不使用切换场景实现暂停。
- 不把 `Time.timeScale = 0` 作为战斗模块的唯一暂停机制，避免 UI 和后续表现链被一同停止。
- 战斗重复进入由 `TurnBattleComponent` 的战斗占用状态拒绝；同一碰撞帧不得创建多场战斗。

## 六、固定规则

### 6.1 下一行动者

每次行动或跳过结束后，重新从本轮尚未获得行动机会且仍在战斗中的单位中选择：

1. 先制战斗第一轮仍有玩家未行动时，只在玩家中选择；玩家全部行动后才允许选择敌人。
2. 当前速度高者优先。
3. 当前速度相同时，玩家优先于敌人。
4. 同阵营同速度时，`PartyOrder` 小者优先。
5. 已经获得过本轮行动机会的单位不会因速度变化再次行动。
6. 候选集为空时开启下一轮；先制限制只适用于第一轮。

阵亡和逃跑单位不再进入候选集。眩晕单位仍会被选为下一行动者，随后跳过这次机会、标记本轮已行动并消耗一次眩晕持续次数。

### 6.2 数值与效果

- 每个 `BattleEffect` 按配置列表顺序结算。
- 目标变化值为 `FlatValue + SourceStat * SourceScalePermille / 1000`。
- 使用整数运算；首版不加入暴击、防御、命中、浮动伤害或元素克制。
- HP 和 MP 始终钳制到 0 与对应上限之间。
- HP 降到 0 时立即标记阵亡，之后不能成为合法目标或行动者。
- MP 不足的行动不可执行，不扣除行动机会。
- 战斗中的属性和状态变化只属于本场战斗；除最终 HP/MP 外不写回探索。

### 6.3 眩晕

- `Stun(1)` 表示跳过目标未来一次行动机会。
- 目标本轮尚未行动时被施加眩晕，会跳过本轮即将到来的行动。
- 目标本轮已经行动时被施加眩晕，会跳过下一轮的行动。
- 重复施加时，剩余次数取当前值与新值中的较大值，不相加。
- 战斗结束时清除全部战斗状态。

### 6.4 逃跑

- 逃跑只允许当前玩家行动者选择。
- 每次尝试使用 `GlobalConfig.EscapeSuccessPermille` 和本局随机源独立判定。
- 成功或失败都消耗当前角色本轮行动机会。
- 成功后角色立即从当前战斗候选集和合法目标中移除，但不视为阵亡。
- 只要仍有玩家留在战斗中，单个角色逃跑不结束战斗。

## 七、开发里程碑

### [x] M0 接口、最小单局状态与战斗壳层

目标：建立不会被后续规则推翻的数据流，并在 Main 场景得到第一个可见结果。

实现范围：

- 建立第五节定义的 DTO、枚举和接口。
- 新增 `RandomComponent`：本局 seed 与共享随机源，种子由局外带入，地图生成、敌人 AI、逃跑和掉落共用。
- 新增 `TurnBattleComponent` 最小状态：临时玩家列表、计时、`IsExplorationPaused` 和战斗占用标志。
- 将 `RandomComponent`、`TurnBattleComponent` 挂入 Launcher 的自定义组件节点，并在 `GameEntry.Custom` 暴露统一访问入口。
- 新增 `RunBattleCoordinator` 空流程，能用固定调试遭遇构建 `BattleStartRequest`。
- 新增半透明顶级 `BattleForm`、`UIFormType` 与 `UIFormConfig`，沿用现有 Form/View/`.Logic.cs` 生成和维护规则。
- 增加仅供 Editor/Development Build 使用的调试入口，不依赖尚未实现的地图敌人。
- 打开战斗 UI 时暂停调试地图计时，关闭调试壳层后恢复。

当前进度（2026-08-31）：

- 已完成：`RandomComponent`（`GameEntry.Random`，Launcher.unity "Random" GameObject）；`TurnBattleComponent`（`GameEntry.TurnBattle`，Launcher.unity "Turn Battle" GameObject）；`IRunRandomSource`、`RunRandomSource`、`RunPlayerState`；第五节全部 DTO/枚举/接口（`Battle/` 目录，命名空间 `SepCore.Battle`）；`RunBattleCoordinator`（`GameEntry.RunBattle`，空流程：预留占用、构建 `BattleStartRequest`、暂停/恢复探索与计时、开/关 `BattleForm`，`EndDebugBattle` 为 M0 专用，M7 由 `ApplyResult` 替代）；`BattleForm.Logic.cs` 壳层逻辑（按钮绑定、道具禁用、占位反馈，不含暂停职责）；`BattleDebuggerWindow` 调试入口（`#if UNITY_EDITOR || DEVELOPMENT_BUILD` 注册于 `ProcedureLaunch`，路径 Battle/Shell，固定遭遇 EncounterId=1/队伍预设 1，缺单局状态时用配表角色与固定种子初始化）；配表 `UIFormType.BattleForm = 103` 与 `UIFormConfig` 行已导出。
- 未完成：`BattleController` 创建与校验（M1 起）；暂停联动已由协调器承担，等待 M0 Play Mode 验收。
- 已知注意点：单局计时暂停来源不止战斗（暂停菜单也会暂停），后续需要多来源组合；`RunRandomSource` 使用 System.Random，跨平台同 seed 序列不保证一致，仅需同构建内复现；`BattleView` 的回合槽与敌人槽为模板 + 运行时实例化（`turnSlotsRoot`/`enemySlotsRoot` + template），数据接入时由 Logic 负责实例化。

验证证据：

```text
代码状态：已完成
Unity 编译：通过（2026-08-31，dotnet build SepCore.Runtime.csproj 0 错误；Unity 编辑器内编译与 Play Mode 实测通过）
EditMode：不适用（M0 无 EditMode 用例）
Play Mode：通过（2026-08-31，Debugger -> Battle/Shell 实测：
  1. Main 场景未切换、未卸载，半透明 BattleForm 覆盖在地图上；
  2. 调试入口 Open 打开战斗壳层，按钮可点击响应；
  3. Run State 读数：打开后 RunElapsedMs 停止增长、IsTimerPaused=true、IsExplorationPaused=true、IsBattleActive=true，Close 后计时恢复增长、标志复位；
  4. 壳层打开时重复点击 Open 被拒绝（TryStartBattle 占用检查 + allowMultiInstance=false），不会打开第二场。
  截图：未提供）
提交或差异：未提交（工作区包含全部 M0 代码、配表导出与 Launcher.unity 挂载）
剩余问题：无
```

明确不做：回合调度、攻击、敌人 AI、胜负和真实碰撞接入。

Play Mode 验收：

- Main 场景不切换、不卸载。
- 调试入口能打开覆盖地图的半透明战斗 UI。
- 战斗 UI 打开时地图计时或可观察的地图更新停止，UI 按钮仍可响应。
- 重复触发调试入口不会打开第二场战斗。

### [ ] M1 1v1 攻击闭环

目标：完成第一场能亲手打完的战斗。

实现范围：

- 创建 1 名玩家和 1 名敌人的 `BattleSession`。
- 支持轮次、当前行动者和每轮一次行动机会。
- 支持普通攻击、简单 HP 伤害、MP/HP 钳制和阵亡。
- 实现 `EnemyAiType.Random` 的免费攻击选择和合法目标选择。
- 实现 `Victory` 与 `TotalDefeat`。
- UI 显示双方 HP、当前行动者和攻击按钮。

明确不做：多人队伍、技能、状态、先制和逃跑。

Play Mode 验收：

- 玩家点击攻击后敌人 HP 立即变化。
- 未结束时敌人能自动反击，玩家 HP 立即变化。
- 任一方 HP 归零后战斗停止接受指令并显示对应结果。
- 行动记录的顺序和最终数值与 UI 一致。

### [ ] M2 1 至 4 人多对多与实时回合顺序

目标：完成设计要求的多对多调度骨架。

实现范围：

- 从 `BattleStartRequest.Players` 和 `EnemyPartyConfig` 创建最多 4v4 的单位。
- 为重复敌人配置生成不同 `BattleUnitId`。
- 实现当前速度、阵营优先和 `PartyOrder` 并列规则。
- 每次行动后重新选择本轮下一行动者。
- 阵亡单位在轮到前死亡时不再行动。
- UI 显示当前行动者和本轮剩余顺序。

明确不做：速度修改效果、眩晕、技能和先制。

Play Mode 验收：

- 调试场景可分别启动 1v1、2v2 和 4v4。
- 同速度时玩家先于敌人，同阵营按队伍顺序行动。
- 每个仍在战斗中的单位每轮最多行动一次。
- 两个相同 EnemyConfig 的敌人可以分别选择、受伤和死亡。

### [ ] M3 技能、目标选择、MP 与通用效果

目标：让当前配表中的数值型玩家和敌人行动可用。

实现范围：

- 支持 `Attack` 和 `Skill` 行动配置。
- 本阶段只结算数值型 `BattleEffect`；状态型效果由 M4 接入。
- 支持 `Self`、`SingleAlly`、`AllAllies`、`SingleEnemy` 和 `AllEnemies`。
- 实现 MP 校验和成功执行后的 MP 扣除。
- 实现 HP 治疗、单体伤害和全体伤害。
- UI 根据行动配置进入或跳过目标选择，并禁用 MP 不足的技能。
- 控制器拒绝错误行动者、错误阵营、错误目标数量、阵亡目标和逃跑目标。

明确不做：角色 4 的眩晕效果、速度修改、逃跑和道具效果。

Play Mode 验收：

- 角色 1 至 3 的单体伤害、单体治疗和全体伤害技能能够完整结算。
- `SingleAlly` 可以选择施法者自己。
- 全体技能一次结算全部合法目标。
- MP 不足时技能不可点击，伪造非法指令也不会消耗行动机会。
- 治疗不会超过 MaxHP，伤害不会使 HP 低于 0。

### [ ] M4 眩晕、无法行动与实时速度变化

目标：补齐会改变行动机会或下一行动者的战斗内变化。

实现范围：

- 实现 `BattleStatusType.Stun` 及已确认的持续次数语义。
- 实现同类状态刷新而非叠层。
- 实现对 Speed 的简单数值修改。
- 速度改变后立即重算本轮剩余顺序。
- 为 EditMode 和开发调试场景提供可复现的速度变化用例；不要求修改四名正式角色的技能定位。

明确不做：复杂状态、每轮伤害、状态图标动画和永久属性写回。

Play Mode 验收：

- 角色 4 的眩晕技能能使尚未行动的目标跳过本次行动。
- 已经行动的目标被眩晕后，会跳过下一轮的行动。
- 重复眩晕不会叠加总次数，只保留较长持续次数。
- 调试行动改变未行动单位速度后，本轮剩余顺序立即更新。
- 已在本轮行动过的单位不会因速度提高再次行动。

### [ ] M5 先制首轮与确定性随机

目标：接通普通遭遇和主动接敌的规则差异，并证明战斗随机来自单局。

实现范围：

- 实现 `IsPreemptive`。
- 第一轮所有尚未行动的玩家先于所有敌人，玩家内部仍按当前速度和队伍顺序选择。
- 第二轮恢复双方共同按速度选择。
- 敌人行动和目标选择只消费注入的本局随机源。
- 增加固定随机序列的 EditMode 回归用例。

明确不做：真实警惕碰撞接入和敌人掉落。

Play Mode 验收：

- 同一队伍可分别以普通和先制模式启动。
- 先制第一轮中，即使敌人速度更高，也要等所有玩家行动后再行动。
- 第二轮高速敌人恢复正常优先级。
- 相同初始状态与固定随机序列产生相同敌人行动和目标记录。

### [ ] M6 单角色逃跑与结束结果矩阵

目标：完成所有战斗结束分支，但暂不操作地图。

实现范围：

- 增加逃跑按钮和 `Escape` 指令。
- 使用 `GlobalConfig.EscapeSuccessPermille` 与本局随机源判定。
- 成功或失败均消耗当前行动机会。
- 成功逃跑单位立即离开候选集和合法目标集合。
- 实现 `AllEscaped` 与 `PartialEscapeDefeat`。
- 固定四种 `BattleOutcome` 的互斥判定和结果数据。

明确不做：2 秒地图保护、地图敌人恢复和 HP/MP 回写。

Play Mode 验收：

- 逃跑失败后角色仍在战斗中，随后敌人正常行动。
- 单人逃跑成功不会在仍有队友战斗时结束战斗。
- 所有人成功逃跑得到 `AllEscaped`。
- 至少一人逃跑、其余人阵亡得到 `PartialEscapeDefeat`，而不是 `TotalDefeat`。
- 没有人逃跑且全部阵亡得到 `TotalDefeat`。

### [ ] M7 单局临时状态回写与战斗关闭

目标：让调试战斗能正确返回同一局临时状态。

实现范围：

- `RunBattleCoordinator` 消费 `BattleResult`，不得由 UI 自行回写。
- 胜利时存活者保留 HP/MP，阵亡者按全局配置恢复到 1/1。
- 全逃跑时所有角色保留逃跑时 HP/MP。
- 部分逃跑失败时逃跑者保留值，阵亡者恢复到 1/1。
- 清除所有战斗状态，不把 Speed 修改或眩晕带回探索。
- `TotalDefeat` 进入单局失败入口，不恢复探索更新。
- 非全员阵亡时关闭 `BattleForm` 并恢复探索更新。

明确不做：真实地图敌人、掉落和保护碰撞门禁。

Play Mode 验收：

- 连续打开两场调试战斗时，第二场使用第一场返回的玩家 HP/MP。
- 第一场阵亡但非全员阵亡的角色，第二场以 1 HP/1 MP 参战。
- 上一场眩晕或速度修改不会进入下一场。
- 全员阵亡后不能返回可操作地图状态。
- 本阶段全过程不修改 `GameEntry.Save.Data` 或写入 `save.json`。

### [ ] M8 真实探索遭遇、敌人恢复、掉落与保护

目标：通过既定接口接入探索层，形成碰撞到返回地图的完整垂直流程。

前置依赖：探索层必须提供唯一 `EncounterId`、敌人队伍预设 ID、警惕是否已满、地图暂停门禁和敌人实例生命周期接口。

实现范围：

- 队长碰撞调用探索适配器创建 `BattleEncounter`。
- 搜索被战斗中断时保留搜索进度和已掉落物。
- 战斗期间冻结所有地图更新，其他敌人保持战前警惕和追击状态。
- `Victory` 移除触发敌人，并通过掉落服务使用本局随机源结算掉落。
- `AllEscaped` 和 `PartialEscapeDefeat` 保留触发敌人，恢复地图行为；下次进入战斗重新按完整队伍预设创建敌方。
- 逃跑类结果开启 `GlobalConfig.EscapeProtectionMs` 保护。
- 保护期间禁止警惕增长、追击和战斗触发；结束后仍重叠可立即再次触发。

明确不做：地图生成、敌人巡逻算法、警惕值算法、物品背包实现和掉落表设计。

Play Mode 验收：

- 警惕未满碰撞进入先制战斗，警惕已满碰撞进入普通战斗。
- 附近其他地图敌人不会加入战斗，战斗后状态保持不变。
- 胜利后只有触发敌人消失并可能产生掉落。
- 逃跑后触发敌人仍在；再次碰撞面对完整初始敌人队伍。
- 2 秒保护内不能立刻重入战斗，保护结束仍重叠时可以重入。

### [ ] M9 战斗 UI、输入限制与最小表现完成

目标：达到设计文档要求的最小可读、可操作战斗界面。

实现范围：

- 我方所有角色显示名称、HP、MP、阵亡和逃跑状态。
- 所有敌人显示名称、HP 条和不可选状态。
- 显示当前行动者和本轮剩余行动顺序。
- 行动菜单只在等待当前玩家指令时出现。
- 攻击、技能、逃跑可用；道具占位可见但禁用。
- 单体目标提供明确选中态；取消目标选择返回行动菜单且不消耗行动。
- 战斗期间不能打开共享背包、保险箱或调整保险箱内容。
- UI 文案走现有本地化/格式化文本流程，不在逻辑代码中硬编码。
- 保持半透明覆盖效果，让玩家能够确认仍在原地图场景。

明确不做：角色站位、骨骼或序列帧动画、VFX、Timeline、镜头演出和音效同步。

Play Mode 验收：

- 1 至 4 名玩家和敌人的最长名称、HP/MP 数值不会溢出或遮挡。
- 速度变化后顺序显示与下一实际行动者一致。
- 非当前玩家不能操作，敌人自动行动期间不能提交玩家指令。
- 背包和保险箱入口在战斗中不可用。
- 取消目标、MP 不足、阵亡目标和逃跑目标均不会误消耗行动。

### [ ] M10 全量回归与原型完成

目标：关闭战斗模块全部设计验收，清理调试入口和遗留临时代码。

实现范围：

- 完成第八节的全部验收映射。
- 覆盖 EditMode 规则测试和实际探索 Play Mode 流程。
- 调试入口只在 Editor/Development Build 可用，正式构建不可见。
- 清理临时日志、硬编码测试数据和无用调试按钮。
- 检查生成文件、资源收集、UIForm 配表和 Prefab 引用。
- 更新本文档进度、证据和剩余问题。

Play Mode 验收：

- 从地图碰撞进入战斗、完成任一结果并返回或进入失败结算的流程可重复执行。
- 普通、先制、胜利、全逃跑、部分逃跑失败和全员阵亡均留有证据。
- 连续多场战斗没有重复事件订阅、重复 UI、随机源重置或地图暂停泄漏。
- Unity Console 没有由战斗流程产生的新异常或错误。

## 八、设计验收映射

以下 22 项对应 `Docs/GameDesign/04_TurnBasedCombat.md`。只有对应里程碑完成运行验收后才能勾选。

- [ ] 多个玩家和多个敌人可以同时参战。对应 M2。
- [ ] 玩家与敌人上限均为 4，并按战备和敌人队伍预设创建。对应 M2。
- [ ] 普通轮次每个单位只行动一次，每次行动后按当前速度决定下一位。对应 M2、M4。
- [ ] 同速玩家优先，速度变化立即影响尚未行动单位。对应 M2、M4。
- [ ] 同阵营同速按队伍顺序行动。对应 M2。
- [ ] 死亡、眩晕或无法行动单位跳过本轮。对应 M2、M4。
- [ ] 轮到玩家角色时才显示并处理该角色的行动选择。对应 M1、M9。
- [ ] 先制第一轮所有玩家排在所有敌人之前。对应 M5。
- [ ] 先制第二轮恢复普通速度规则。对应 M5。
- [ ] 玩家可选择攻击、技能和逃跑，道具保留但不可用。对应 M3、M6、M9。
- [ ] 战斗中不能打开共享背包或保险箱。对应 M9。
- [ ] 敌人随机选择可用行动和合法目标。对应 M1、M5。
- [ ] 敌人全灭时胜利，玩家全灭时战斗和单局失败。对应 M1、M6、M7。
- [ ] 单个角色可逃跑；只要有人逃跑，其他人阵亡不导致单局失败。对应 M6。
- [ ] 逃跑使用配置成功率，失败后继续留在战斗。对应 M6。
- [ ] 逃跑成功或失败均消耗本轮行动。对应 M6。
- [ ] 逃跑返回战前位置并获得 2 秒保护。对应 M7、M8。
- [ ] 逃跑者保留 HP/MP，部分失败中的阵亡者恢复 1/1。对应 M7。
- [ ] 任何非全员阵亡结果中的阵亡角色恢复 1/1。对应 M7。
- [ ] 逃跑后地图敌人恢复，重战按完整预设创建。对应 M8。
- [ ] 保护期间不警惕、不追击、不触发战斗，结束仍重叠时可重入。对应 M8。
- [ ] 战斗 UI 显示我方 HP/MP、行动顺序和敌人 HP。对应 M9。

## 九、最低测试矩阵

### EditMode

| 类别 | 必测情况 |
| --- | --- |
| 单位创建 | 1v1、4v4、重复 EnemyConfig、不合法队伍人数 |
| 普通排序 | 速度不同、跨阵营同速、同阵营同速、每轮只行动一次 |
| 动态排序 | 未行动单位加速/减速、已行动单位加速、死亡后移出候选集 |
| 先制 | 第一轮玩家全先行、第二轮恢复普通规则 |
| 目标 | Self、SingleAlly 含自己、全体目标、阵营错误、阵亡和逃跑目标 |
| MP 与效果 | MP 不足、伤害、治疗、全体效果、HP/MP 钳制、顺序效果 |
| 眩晕 | 行动前施加、行动后施加、重复施加、跳过后失效 |
| 敌人 AI | 过滤不可用行动、合法目标、无行动时跳过、固定随机序列 |
| 逃跑 | 0/1000 边界、成功/失败消耗行动、四种结束结果 |
| 回写 | 胜利阵亡恢复、全逃跑保留、部分失败恢复、全灭不恢复探索 |

### Play Mode

| 流程 | 必测情况 |
| --- | --- |
| UI 生命周期 | 打开、关闭、重复进入、非当前行动者不可输入 |
| 地图暂停 | 玩家、敌人、搜索、计时停止；战斗 UI 仍响应 |
| 遭遇 | 普通碰撞、先制碰撞、附近敌人不参战 |
| 返回 | 胜利移除敌人、逃跑保留敌人、完整队伍重建 |
| 保护 | 保护期不重入，结束仍重叠可重入 |
| 连战 | HP/MP 延续、状态清除、共享随机序列不重置 |
| 失败 | 全员阵亡进入单局失败，不短暂恢复地图操作 |

## 十、原型完成条件

只有同时满足以下条件，本文档状态才能改为“完成”：

- M0 至 M10 全部标记为 `[x]`，且每个里程碑都有独立的 Unity/Play Mode 证据。
- 第八节 22 条设计验收全部通过。
- EditMode 最低测试矩阵全部通过。
- 战斗过程中没有直接写 `SaveData`、切换 Battle 场景或重建单局随机源。
- 胜利、逃跑和失败对地图敌人、玩家临时状态、计时和保护期的处理符合本文档。
- 正式构建中没有开发调试入口和测试数据。
- 静态检查、Unity 编译、EditMode 和 Play Mode 结果分别记录，不用静态检查代替运行验收。
