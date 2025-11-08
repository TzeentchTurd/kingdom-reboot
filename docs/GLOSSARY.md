# Glossary（术语表）

约定：
- 代码标识统一使用 `snake_case`；以下术语均给出可映射到代码的字段/事件名。

## 基础属性

- `hp` / `hp_max` / `hp_ratio`：生命值、最大生命、生命比例。
- `essence` / `essence_delta`：精粹值及其增量/耗散。
- `faction` / `faction_name`：阵营标识与显示名。

## 状态与出局

- `is_downed` / `downed_time`：倒地状态与开始时间（触发条件：HP=0）。
- Essence 对抗链（V1.7）：用于从倒地到移除的提取对抗。
  - 事件：`extraction_start` → `extraction_interrupted` → `extraction_complete`。
  - 过程：进入倒地后可被提取；对抗可被打断；Essence=0 时完成提取并 `is_removed=true`。
- `is_removed` / `remove_reason`：被移除与原因。
- 相关快照：`snapshot_battle`（战斗）、`snapshot_placement`（部署）。

## 玩法结构

- `board_id` / `tile_index` / `grid_x`/`grid_y`：棋盘与格子定位。
- `population_cap` / `population_current`：人口上限与当前人口。
- `legion_id` / `legion_members`：军团标识与成员集合。
- `bench_slots` / `bench_entries`：长椅容量与条目（Bench 为常驻 UI，非状态）。
- `targeting_rule` / `targets`：选敌规则与结果集合。
- `skills` / `skill_cooldown`：技能集合与冷却。

## 系统组件

- `board_manager`：棋盘管理器。
- `board_data`：棋盘/关卡数据（ScriptableObject 等）。
- `tile_presenter`：格子/地块表现组件（Prefab）。
- `event_bus`：事件总线（示例事件：`unit_spawned` / `unit_downed` / `reward_granted`）。

## 场景与状态机

- `state_boot` / Boot：启动。
- `state_stage_sequencer` / StageSequencer：动态舞台过渡（棋盘 N 飞走 → N+1 飞入）。
- `state_placement` / Placement：部署。
- `state_countdown` / Countdown：倒计时。
- `state_battle_board` / BattleBoard：战斗棋盘（Stage 内部）。
- `state_resolution` / Resolution：战后结算。
- `state_event_board` / EventBoard：事件棋盘（安全棋盘）。
- `state_rewards` / Rewards：奖励与通关。

## AI

- `ai_blackboard`：黑板。
- `ai_bt` / `ai_goap`：行为树 / GOAP。
- `ai_difficulty` / `ai_trace`：AI 难度与调试追踪。

## 世界与派系

- `scene_laboratory` / `lab_context`：实验室场景与上下文。
- `faction_ancestors` / `faction_goblins` / `faction_elves` / `faction_magisters` / `faction_fallen` / `faction_lost`：阵营键名。

## 新增术语（可直接映射字段）

- `chaos_forge`（混沌生成/打造，V23）：用于单位生成的总入口；相关：`chaos_forge_seed`、产物 `unit_blueprint`。
- `essence_struggle`（Essence 对抗链，V1.7）：倒地后的提取对抗过程；事件：`extraction_start` / `extraction_interrupted` / `extraction_complete`。
- `v_kill` / `mutual_protocol`（机制杀/互斩协议）：被提取过程中的反杀机制；用于定义当提取者满足触发条件时的致命反馈规则。

