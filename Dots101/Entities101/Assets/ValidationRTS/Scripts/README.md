# Scripts 目录结构

- `Components/`：各类 DOTS 组件、Buffer、Blob、常量。
- `Authoring/`：MonoBehaviour Authoring，负责场景配置。
- `Bakers/`：与 Authoring 配对的 Baker。
- `Systems/Core`：输入、Spawn、全局状态系统。
- `Systems/Navigation`：Boids/RTS 行为与路径。
- `Systems/Combat`：战斗、伤害、指令解析。
- `Systems/Lifecycle`：对象池、回收、生命周期处理。
- `Systems/RenderingBridge`：用于 `UNITY_EDITOR` 下同步材质/颜色。
- `RuntimeBridge/`：PGDCommandQueue、ComponentLookup 等桥接逻辑。
