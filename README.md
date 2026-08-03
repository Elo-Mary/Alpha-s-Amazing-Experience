# Alpha's Amazing Experience

<p align="left">
  <img src="./Alpha's%20Amazing%20Experience/Assets/item.png" alt="Game Banner" width="50%" />
</p>

> 一款结合文字输入的解谜游戏。玩家需要通过输入精准的英文指令，在多个场景中搜集物资、组装物品并解开跨场景的复杂谜题。
> 
> 作为一个初学者项目，目前游戏并不成熟！

## 目录

- [游戏特色](#游戏特色-features)
- [技术架构](#技术架构-technical-architecture)
- [如何运行](#如何运行-getting-started)
- [核心操作指南](#核心操作指南-controls)
- [游戏截图](#游戏截图-screenshots)
- [许可协议](#许可协议-license)
- [鸣谢](#鸣谢-credits)

## 游戏特色 (Features)

- **文字指令驱动交互**：摒弃传统的“哪里亮了点哪里”，采用类似老式 Mud 与现代解谜结合的 `Command Parser` 机制。玩家需要通过思考（输入 `cut`、`tie`、`fix`、`pump` 等指令）配合手中的物品来改变世界。
- **深度的物品合成与转化**：从砍伐灌木获取树枝，到用锤子、胶水和木材修复破损的梯子，再到用空油箱从废弃汽车中抽油，拥有完整的资源逻辑链。
- **跨场景的蝴蝶效应**：在一个场景的改变会永久影响另一个场景（例如：在屋顶避雷针上接好电线，能用于激活屋外庭院的发电机）。
- **动态环境交互**：手持点燃的油灯进入暗室可以永久驱散黑暗并解锁新物品；搬运并架设梯子可以开辟通往屋顶与阁楼的新路径。

## 技术架构 (Technical Architecture)

本项目基于 **Unity 2022.3.60f1c1** 开发，采用高度解耦的面向对象设计与组件化开发模式（Component-based Architecture）。

### 项目结构

```
Assets/script/
├── CoreInitializer.cs        # 核心系统初始化入口
├── GameManager.cs            # 游戏全局生命周期管理
├── WorldStateManager.cs      # 跨场景全局状态持久化
├── SceneObjectID.cs          # 物品唯一标识符（解决 GUID 冲突）
├── GlobalStateListener.cs    # 全局状态变更监听
├── CommandParser.cs          # 指令解析器（NLP-lite 同义词映射）
├── InventoryManager.cs       # 背包与物品数量管理
├── IInteractable.cs          # 交互实体统一接口
├── InteractableScanner.cs    # 场景交互物扫描
├── PlayerController.cs       # 玩家移动与控制
├── CameraController.cs       # 相机跟随
├── InputManager.cs           # 行动 / 思考模式输入切换
├── UIManager.cs              # 全局 UI 管理
├── InventoryUI.cs            # 物品栏 UI
├── InteractableListUI.cs     # 交互物选择列表 UI
├── LevelFlowManager.cs       # 关卡流程推进
├── SceneConfig.cs            # 场景配置
├── itemScript/               # 通用可拾取 / 交互物品
└── Level1_itemScript/        # 第一关场景专属交互物
    ├── InteractableGenerator.cs      # 发电机（多状态机）
    ├── InteractablePlacedLadder.cs   # 梯子放置点（一体两面）
    ├── InteractableBrokenLadder.cs   # 破损梯子（修复）
    ├── InteractableCar.cs            # 废弃汽车（抽油）
    ├── InteractableToolbox.cs        # 工具箱（劈开）
    ├── InteractableShrub.cs          # 灌木（砍伐）
    ├── DarkRoomController.cs         # 暗室状态控制
    └── ...
```

### 核心系统亮点

1. **世界状态管家 (World State Manager)**
   - 实现了跨场景的全局状态持久化。无论玩家砍倒了一棵树、劈开了一个工具箱，还是修好了一台发电机，系统都能精准记录。
   - **彻底解决了 Unity 经典的“预制体 GUID 冲突”与“场景重载物品重生”问题**。通过自定义的 `SceneObjectID` 和生命周期校验，确保动态生成的掉落物和场景固定物品拥有绝对唯一的标识符。
2. **万能指令解析器 (Command Parser)**
   - 实现了轻量级的自然语言映射（NLP-lite），支持同义词容错（如 `fire` / `ignite` → `start`，`bind` → `tie`）。
   - 指令系统通过 `IInteractable` 接口与所有场景实体解耦，扩展新物品完全不需要修改核心逻辑。
3. **复合交互组件 (Complex Interactables)**
   - **多状态机**：如发电机（Generator）同时维护“是否启动”与“是否接线”两个独立存档状态。
   - **智能条件校验**：发电机启动需要精确校验玩家背包中包含 `Lighter`（不消耗）和 `Wood x 3`（消耗），背包管理器（InventoryManager）支持精准的数量统计与定向扣除。
   - **一体两面设计**：如梯子放置点（Placed Ladder），巧妙利用贴图渲染开关和触发器，实现了“空底座”与“可用梯子”的无缝切换及跨场景传送。

## 如何运行 (Getting Started)

### 环境要求

- **操作系统**：建议 Windows 10 / 11，其它系统未测试。
- **玩家**：直接下载编译好的可执行文件，无需安装 Unity。
- **开发者**：使用 Unity 2022.3.60f1c1 打开项目源码，详见[从源码构建](#从源码构建)。

### 下载与运行

1. 前往 [Releases 发布页](https://github.com/Elo-Mary/Alpha-s-Amazing-Experience/releases)。
2. 下载游戏压缩包 `Alpha's Amazing Advanture.zip`。
3. 解压后运行 `Alpha's Amazing Advanture.exe` 即可开始游戏。

> 目前游戏暂未实现退出与存档功能。需要退出时，使用 `ALT + TAB` 切换窗口，再将游戏窗口直接关闭即可。

### 从源码构建

```bash
git clone https://github.com/Elo-Mary/Alpha-s-Amazing-Experience.git
```

1. 安装 [Unity Hub](https://unity.com/download)，并通过其安装 **Unity 2022.3.60f1c1**。
2. 在 Unity Hub 中点击 `Add` → 选择仓库内的 `Alpha's Amazing Experience` 文件夹。
3. 打开项目后，在 `Assets/Scenes` 中双击打开起始场景（如 `Prologue_outside.unity`），点击播放按钮 ▶ 即可运行。

## 核心操作指南 (Controls)

| 操作                  | 按键                          |
| ------------------- | --------------------------- |
| 移动 (Move)           | `W` `A` `S` `D`             |
| 物品栏切换 (Inventory)   | 行动模式下 `1` ~ `8`，或 `←` / `→` |
| 思考模式 (Thought Mode) | `CTRL` 进入 / 退出              |
| 物品选择 (Item List)    | 思考模式下按对应数字键选择目标             |
| 指令输入 (Command)      | 选中目标后，在输入框输入英文指令            |

### 常用指令速查表 (Cheat Sheet)

| 指令 (Command)     | 交互示例 (Example)              | 示例效果 (Effect)   |
| ---------------- | --------------------------- | --------------- |
| `pick` / `get`   | `pick` 掉落的木材                | 将物品收入背包         |
| `put`            | `put` 背包里的柴火                | 将背包物品放在地上       |
| `cut`            | 手持 `Axe`，`cut` 工具箱          | 劈开工具箱，掉落电线      |
| `pump`           | 手持 `oilBox`，`pump` 汽车       | 从汽车中抽油，获得满油箱    |
| `fix`            | 手持 `hammer`，`fix` 破梯子       | 消耗木材和胶水，获得完好的梯子 |
| `set`            | 手持 `Ladder`，`set` 阁楼入口      | 架设梯子，开辟传送通道     |
| `start` / `fire` | 手持 `lighter`，`start` 发电机    | 消耗 3 块木材启动发电机   |
| `tie`            | 手持 `electricLine`，`tie` 发电机 | 为机器接通电线         |

游戏中还有大量指令，欢迎自行探索！

> 当然，你也可以直接在 [CommandParser.cs](./Alpha's%20Amazing%20Experience/Assets/script/CommandParser.cs) 的 `InitializeDictionary` 方法中查看完整的指令映射。

## 游戏截图 (Screenshots)

![游戏截图 1](asset/game_1.png)

![游戏截图 2](asset/game_2.png)

## 许可协议 (License)

本项目采用 **MIT License** 协议开源。详细信息请参阅 [LICENSE](./LICENSE) 文件。

美术素材由 [@little7-c](https://github.com/little7-c) · [@yanlandai](https://github.com/yanlandai) 绘制，仅供学习交流，未经允许严禁商业用途。

## 鸣谢 (Credits)

- **开发**：[@Elo-Mary](https://github.com/Elo-Mary) · [@lingter](https://github.com/lingter) · [@Ropert-hrp](https://github.com/Ropert-hrp)
- **美术与音乐资产**：[@little7-c](https://github.com/little7-c) · [@yanlandai](https://github.com/yanlandai)
- **特别感谢**：感谢在架构设计与逻辑实现过程中提供协助的 AI 伙伴 [@Gemini 3.1 Pro](https://gemini.google.com)，由 [@GLM5.2](https://open.bigmodel.cn/) 协助文档重构与润色。
