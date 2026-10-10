# 守桥人之约：赵州桥3D课程游戏

《动画设计与制作》个人课程作业。使用Unity制作赵州桥主题的3D交互游戏，结合构件收集、知识问答和桥体修复。

作者：葉 聖颢 / Shenghao Ye。项目为个人课程作业，代码开发使用了AI辅助，场景使用了第三方模型及其他素材。

## 原课程作品画面

![原课程演示中的场景总览](docs/images/scene-overview.jpg)

画面来自原答辩演示视频，保留编辑器界面、原字幕和任务栏，不是本次重新运行仓库所得。

- [构件收集与知识弹窗](docs/images/component-knowledge.jpg)
- [知识问答](docs/images/knowledge-quiz.jpg)
- [桥体修复](docs/images/bridge-repair.jpg)
- [13秒修复演示片段](docs/media/bridge-repair-excerpt.mp4)：截取原视频01:58–02:11，重新编码并移除音频，保留原字幕。

开发过程见 [DEVELOPMENT_NOTES.md](docs/DEVELOPMENT_NOTES.md)，素材来源见 [MEDIA_SOURCES.md](docs/MEDIA_SOURCES.md)。

## 制作过程中完成的工作

原开发记录记载了场景搭建、桥模型从C4D到FBX的转换与导入、任务和UI配置，以及摄像机选择和中文字体显示问题的排查。模型本身来自外部资源。

## 当前仓库是什么

这是脚本与说明展示仓库，**不是可直接启动的完整Unity工程**。

保留了原作业的四个自定义C#脚本和对应 .meta 文件。原场景、第三方模型、贴图、字体、音频以及Unity生成目录没有放入公开整理副本。

## 原作业的玩法

第一人称探索并与守桥人对话，收集三个构件，阅读知识弹窗，回答三道问题，再点击高亮的桥体部位完成修复。

## 脚本分工

- NoClipCamera：移动、视角、碰撞与贴地检查，以及交互时的控制锁定。
- BridgePartCollection：触发检测、构件收集、知识弹窗与进度。
- StoryController：对话、任务衔接、答题、修复进度与音效。
- ClickRepairDetector：修复模式中的射线点击检测。

NoClipCamera沿用了原文件名，实际代码包含碰撞检测。

## 怎样查看与使用

原编辑器版本为 Unity 2021.3.45f2c1。脚本使用Unity UI和TextMesh Pro。可以阅读代码，也可以导入自己的兼容工程后，按照 [SCENE_SETUP.md](docs/SCENE_SETUP.md) 配置场景。

仅把四个脚本导入空工程，不能复现原来的游戏。需要自行设置场景对象、UI、碰撞体、图层和Inspector引用。

## 素材与验证边界

原开发文档已经记录部分素材来源：场景为Unity Asset Store中Pure Poly的 Free Low Poly Nature Forest，赵州桥模型、老者模型和音频来自爱给网。来源名称、截图中的作者及编号已补入 [素材说明](docs/THIRD_PARTY_ASSETS.md)。这些原始素材的公开再分发权限尚未确认，因此未复制进仓库。

整理时核对了脚本和任务逻辑，保留了 .meta；中文注释由原GB18030编码转换为UTF-8，未修改脚本逻辑。另查看了原课程演示视频中的收集、答题和修复过程，并提供了截图与短片段。本展示仓库未进行Unity编译或重新游玩验证；原视频也不能证明所有问题已修复。

尚未指定项目整体的开源许可证；排除的第三方素材不因本仓库而获得使用或分发授权。
