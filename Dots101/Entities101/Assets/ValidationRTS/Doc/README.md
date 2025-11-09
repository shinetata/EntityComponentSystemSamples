# ValidationRTS 场景规划概述

- 工程位置：`Assets/ValidationRTS/`
- 目标：在 Entities101 工程中搭建一个 RTS/TPS 混合 Demo，覆盖主流 DOTS API，供 PGDConverter 转换与 PGD Runtime 验证。
- 结构约定：
  - `Scripts/Components`：IComponentData、Buffer、Blob 与常量定义。
  - `Scripts/Authoring`：MonoBehaviour Authoring。
  - `Scripts/Bakers`：与 Authoring 同名 Baker。
  - `Scripts/Systems/**`：按 Core/Navigation/Combat/Lifecycle/RenderingBridge 划分。
  - `Scripts/RuntimeBridge`：PGD 特定桥接层。
- `Scenes`：主场景 `ValidationRTS.unity`。
- `SubScenes`：`WorldGeometry`、`StarterUnits`、`Waypoints` 等纯数据 SubScene。
- `Scripts/Authoring/ValidationRTSBootstrap.cs`：全局配置 Authoring，负责引用 Prefab、材质调色板、对象池参数。
- `Scripts/Bakers/ValidationRTSBootstrapBaker.cs`：将 Authoring 数据写入 `ValidationRTSConfig`、`UnitPoolConfig`、`UnitPrefabReference`、`UnitPalette` 等组件。
- 禁止事项：
  1. 不允许出现 `NativeArray<Entity>`。
  2. 不得引用 `Unity.Entities.Graphics` 或 URP 专属组件，渲染逻辑使用 `MaterialMeshInfo` + `RenderMeshArray`。
  3. 所有 Native 容器统一 `Allocator.TempJob` + 显式 `Dispose()`。
- 渲染资源：推荐在 `Assets/ValidationRTS/Art/Materials/` 下创建若干纯色材质，在 `ValidationRTSBootstrap` 中通过颜色数组引用；SubScene 中的实体可复用同一材质或 Prefab。
- 流程提示：改动完成后需要在 PGD_Tuanjie_Extension 仓库执行 `dotnet build src/Editor/SourceGenerator/PGD.Jobs.SourceGenerator.csproj` → 同步 DLL → Unity 导入 → PGDConverter → PGD 测试 → 记录 `ChangeLog.md`。
