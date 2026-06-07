# 排序小助手 🐷

猪猪工作室出品的图像识别自动化工具，用于自动执行屏幕上的重复操作。

## 功能特性

- 🖼️ **图像识别** - 基于模板匹配的屏幕元素识别
- 📋 **步骤管理** - 可视化配置自动化步骤
- ⏰ **定时执行** - 支持定时自动运行任务
- 🎨 **主题切换** - 深色/浅色主题一键切换
- 📊 **运行日志** - 详细的执行记录和错误追踪

## 系统要求

- Windows 10/11
- .NET 8.0 Runtime

## 安装使用

### 方式一：直接运行

1. 下载最新发布版本
2. 解压到任意目录
3. 双击运行 `WechatBot.exe`

### 方式二：从源码构建

```bash
# 克隆仓库
git clone https://github.com/Oldfoollearnc/WechatBot.git

# 进入项目目录
cd WechatBot

# 构建项目
dotnet build

# 运行
dotnet run
```

## 使用说明

### 1. 配置步骤

1. 打开应用，进入「步骤」页面
2. 点击「截取」按钮，框选屏幕上的目标元素
3. 为每个步骤配置对应的屏幕区域
4. 配置完成后，状态会显示为 ✅

### 2. 运行任务

- **开始运行** - 执行全部已配置的步骤
- **测试运行** - 最小化窗口后执行，方便观察
- **停止** - 中断当前正在执行的任务

### 3. 定时执行

1. 进入「设置」页面
2. 勾选「启用定时执行」
3. 设置执行时间和日期
4. 保存设置

## 项目结构

```
WechatBot/
├── App.xaml              # 应用入口和全局样式
├── AppMainWindow.xaml    # 主窗口界面
├── ThemeManager.cs       # 主题管理
├── AutomationEngine.cs   # 自动化引擎
├── ImageRecognition.cs   # 图像识别模块
├── Pages/                # 页面
│   ├── HomePage.xaml     # 首页仪表盘
│   ├── StepsPage.xaml    # 步骤管理
│   ├── LogsPage.xaml     # 运行日志
│   └── SettingsPage.xaml # 设置
└── ViewModels/           # 视图模型
```

## 技术栈

- **框架**: .NET 8.0 WPF
- **UI 库**: HandyControl
- **MVVM**: CommunityToolkit.Mvvm
- **图像识别**: 自定义模板匹配算法

## 许可证

MIT License

## 联系方式

猪猪工作室
