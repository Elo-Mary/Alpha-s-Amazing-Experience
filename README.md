# Alpha-s-Amazing-Experience

<img title="" src="./Alpha's%20Amazing%20Experience/Assets/item.png" alt="Game Banner" data-align="inline">

> 一款结合了文字输入的解谜游戏。玩家需要通过输入精准的英文指令，在多个场景中搜集物资、组装物品并解开跨场景的复杂谜题。
> 
> 作为一个初学者项目，目前游戏并不成熟！

## 游戏特色 (Features)

* **文字指令驱动交互**：摒弃传统的“哪里亮了点哪里”，采用类似老式 Mud 与现代解谜结合的 `Command Parser` 机制。玩家需要通过思考（输入 `cut`, `tie`, `fix`, `pump` 等指令）配合手中的物品来改变世界。
* **深度的物品合成与转化**：从砍伐灌木获取树枝，到用锤子、胶水和木材修复破损的梯子，再到用空油箱从废弃汽车中抽油，拥有完整的资源逻辑链。
* **跨场景的蝴蝶效应**：在一个场景的改变会永久影响另一个场景（例如：在屋顶避雷针上接好电线，能激活屋外庭院的机关）。
* **动态环境交互**：手持点燃的油灯进入暗室可以永久驱散黑暗并解锁新物品；搬运并架设梯子可以开辟通往屋顶与阁楼的新路径。

## 技术架构 (Technical Architecture)

本项目基于 **Unity 2022.3.60f1c1** 开发，采用高度解耦的面向对象设计与组件化开发模式（Component-based Architecture）。

### 核心系统亮点

1. **世界状态管家 (World State Manager)**
   * 实现了跨场景的全局状态持久化。无论玩家砍倒了一棵树、劈开了一个工具箱，还是修好了一台发电机，系统都能精准记录。
   * **彻底解决了 Unity 经典的“预制体 GUID 冲突”与“场景重载物品重生”问题**。通过自定义的 `SceneObjectID` 和生命周期校验，确保动态生成的掉落物和场景固定物品拥有绝对唯一的标识符。
2. **万能指令解析器 (Command Parser)**
   * 实现了轻量级的自然语言映射（NLP-lite），支持同义词容错（如 `fire`/`ignite` -> `start`，`bind` -> `tie`）。
   * 指令系统通过 `IInteractable` 接口与所有场景实体解耦，扩展新物品完全不需要修改核心逻辑。
3. **复合交互组件 (Complex Interactables)**
   * **多状态机**：如发电机（Generator）同时维护“是否启动”与“是否接线”两个独立存档状态。
   * **智能条件校验**：发电机启动需要精确校验玩家背包中包含 `Lighter`（不消耗）和 `Wood x 3`（消耗），背包管理器（InventoryManager）支持精准的数量统计与定向扣除。
   * **一体两面设计**：如梯子放置点（Placed Ladder），巧妙利用贴图渲染开关和触发器，实现了“空底座”与“可用梯子”的无缝切换及跨场景传送。

## 如何运行 (Getting Started)

### 环境要求

建议在 Windows10/11 上运行，其它系统未测试可行性

### 下载步骤

前往发布页：

下载游戏压缩包 `Alpha's Amazing Advanture.zip`，解压后运行 `Alpha's Amazing Advanture.exe` 即可开始游戏

*目前游戏没做退出与存档功能，需要退出时，使用 ALT + TAB 键切换窗口，再将游戏窗口直接关掉即可*

## 核心操作指南 (Controls)

- **移动(Move)**：`WASD` 移动

- **物品栏 (Inventory)**：行动模式中，使用`1` ~ `8` 切换物品栏位置 / 使用 `←` / `→` 切换物品栏位置

- **思考模式(Thought Mode)**：使用 `CTRL` 进入/退出思考模式

- **物品选择列表(Item List)**：思考模式中，使用对应数字键切换，

- **指令 (Command)**：选中目标物体后，在输入框中输入英文指令。

### 常用指令速查表 (Cheat Sheet)

| **指令 (Command)** | **交互示例 (Example)**          | **示例效果 (Effect)** |
| ---------------- | --------------------------- | ----------------- |
| `pick` / `get`   | `pick` 掉落的木材                | 将物品收入背包           |
| `put`            | `put` 背包里的柴火                | 将背包物品放在地上         |
| `cut`            | 手持 `Axe`，`cut` 工具箱          | 劈开工具箱，掉落电线        |
| `pump`           | 手持 `oilBox`，`pump` 汽车       | 从汽车中抽油，获得满油箱      |
| `fix`            | 手持 `hammer`，`fix` 破梯子       | 消耗木材和胶水，获得完好的梯子   |
| `set`            | 手持 `Ladder`，`set` 阁楼入口      | 架设梯子，开辟传送通道       |
| `start` / `fire` | 手持 `lighter`，`start` 发电机    | 消耗 3 块木材启动发电机     |
| `tie`            | 手持 `electricLine`，`tie` 发电机 | 为机器接通电线           |

游戏中还有大量指令，欢迎自行探索！

*当然，你也可以直接在 [CommandParser.cs](./Alpha's Amazing Experience/Assets/script/CommandParser.cs) 中的 `InitializeDictionary` 中查看* 



## 游戏截图 (Screenshots)

![](asset/game_1.png)

![](asset/game_2.png)



## 许可协议 (License)

本项目采用 MIT License 协议开源。详细信息请参阅 [LICENSE](https://www.google.com/search?q=LICENSE) 文件。

美术素材由 [@little7-c](https://github.com/little7-c) 绘制，仅供学习交流，未经允许严禁商业用途。

## 鸣谢 (Credits)

- **开发**：[@Elo-Mary](https://github.com/Elo-Mary)  [@lingter](https://github.com/lingter)  [@Ropert-hrp](https://github.com/Ropert-hrp)

- **美术资产**：[@little7-c](https://github.com/little7-c)

- **特别感谢**：感谢在架构设计与逻辑实现过程中提供协助的 AI 伙伴 [@Gemini 3.1 Pro](https://gemini.google.com)。
