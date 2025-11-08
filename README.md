 # Kingdom Reboot — 概要与索引

 - 线性棋盘流：Boot → StageSequencer → … → Rewards
 - 动态舞台：StageSequencer 管理棋盘 N 飞走 / N+1 飞入
 - 战斗棋盘：BattleBoard 在 Stage 内加载与卸载
 - 事件棋盘：EventBoard 作为战间安全插曲
 - 部署与倒计时：Placement → Countdown → 战斗开始
 - Essence 对抗链：HP=0 → Downed → 对抗 → Essence=0 → Removed
 - 对抗事件：extraction_start / extraction_interrupted / extraction_complete
 - V23 混沌生成：模板 + 颜色数值 + 职业技能 + 随机词条
 - AI V4.0：小哥布林（非理性） vs 大哥布林（理性）
 - Bench 常驻：UI 常驻，承接原 Hub 职责（非状态）

 ## 索引（文档与锚点）

 - World Bible（V4.0 黑色幽默实验手册）
   - docs/WORLDBIBLE.md#版本语义（传承但更直白）
   - docs/WORLDBIBLE.md#世界运行的冷笑话（可以直接进-ui台词）

 - Framework（核心框架规范）
   - docs/FRAMEWORK.md#玩法循环与场景
   - docs/FRAMEWORK.md#混沌生成（v23）
   - docs/FRAMEWORK.md#状态与出局
   - docs/FRAMEWORK.md#ai-框架

 - Flow Spec（线性棋盘流，无 Hub）
   - docs/FLOW_SPEC.md#状态机骨架
   - docs/FLOW_SPEC.md#stagesequencer
   - docs/FLOW_SPEC.md#battleboard
   - docs/FLOW_SPEC.md#eventboard

 - Glossary（术语表）
   - docs/GLOSSARY.md#新增术语（可直接映射字段）
   - docs/GLOSSARY.md#场景与状态机

 ## 差异清单（与旧版对比）

 - 移除 Lobby / MatchSetup（无 Hub 的线性棋盘流）。
 - 删除 Roles（坦克/输出/控制/支援）；单位差异改为“V23 混沌生成”。
 - Extraction 改为“V1.7 Essence 对抗链”（extraction_start/interrupted/complete）。
 - 新增 StageSequencer / BattleBoard / EventBoard。
 - AI 改为 V4.0 阶级：小哥布林（非理性） vs 大哥布林（理性）。
