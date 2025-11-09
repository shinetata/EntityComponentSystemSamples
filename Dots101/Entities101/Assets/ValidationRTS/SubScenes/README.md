# SubScenes 说明

- 待创建：
  - `WorldGeometry.subscene`：地形、障碍、导航区域。
  - `StarterUnits.subscene`：初始部队、指挥官、资源点。
  - `Waypoints.subscene`：Boids/RTS 用的路径锚点。
- SubScene 中仅放 Authoring 对象，禁止脚本逻辑。
- Bake 成功后运行 PGDConverter，让 PGD 端直接消费输出数据。
