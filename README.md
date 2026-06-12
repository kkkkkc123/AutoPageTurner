# AutoPageTurner

![AutoPageTurner icon](AutoPageTurner/Assets/AppIcon-256.png)

AutoPageTurner 是一个 Windows 后台自动翻页工具。它可以按固定或随机时间间隔，向指定窗口发送 `PageDown`，并可在翻页前向指定坐标发送后台点击消息。

## 功能

- 从当前可见窗口中选择翻页目标
- 固定时间间隔自动翻页
- 每次翻页后重新生成随机间隔
- 向目标窗口内部控件发送后台翻页消息
- 翻页前自动点击指定坐标
- 在指定半径内随机漂移点击位置
- 延时拾取鼠标坐标
- 自动保存并恢复用户设置
- 无需安装 .NET Runtime 的 Windows x64 发布包

## 使用方法

1. 从 [Releases](https://github.com/kkkkkc123/AutoPageTurner/releases) 下载最新的 Windows x64 压缩包。
2. 解压全部文件，不要只复制 `AutoPageTurner.exe`。
3. 运行 `AutoPageTurner.exe`。
4. 在“目标窗口”中选择需要自动翻页的程序。
5. 设置固定间隔，或启用随机翻页并填写最小、最大间隔。
6. 如需自动点击，启用该功能并拾取点击坐标。
7. 点击“开始”。

## 注意事项

- 时间单位均为毫秒。
- 随机模式会在每次翻页完成后重新生成下一次间隔。
- 后台翻页依赖目标程序对 Windows 消息的支持，部分浏览器、游戏或使用特殊渲染框架的软件可能会忽略后台输入。
- 如果目标程序以管理员身份运行，AutoPageTurner 通常也需要以管理员身份运行。
- 自动点击坐标使用屏幕坐标；目标窗口移动后建议重新拾取。

## 从源码构建

环境要求：

- Windows 10/11
- .NET 10 SDK

```powershell
dotnet build .\AutoPageTurner.slnx
```

发布 Windows x64 自包含版本：

```powershell
dotnet publish .\AutoPageTurner\AutoPageTurner.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -o .\publish\win-x64
```

## 项目信息

- 作者：kkkkkc123
- 公司：kkkkkc123
- 许可证：[MIT](LICENSE.txt)
