# ValidationRTS 场景 TODO

1. 在 Unity 中创建 `Assets/ValidationRTS/Scenes/ValidationRTS.unity`。
2. 将 `WorldGeometry`、`StarterUnits`、`Waypoints` SubScene 加入主场景。
3. 放置 `ValidationRTSBootstrap` Authoring，配置材质、对象池、全局参数。
4. Bake 所有 SubScene，确认控制台无报错。
5. 执行 SourceGenerator build + PGDConverter + PGD 验证，并记录 ChangeLog。
