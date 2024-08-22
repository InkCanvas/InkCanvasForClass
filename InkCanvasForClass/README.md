<div align="center">

<img src="icc.png" width="128">

# InkCanvasForClass

Elegant by Default. Based on `ChangSakura/InkCanvas` .

**这将会是最后一次基于InkCanvas控件的倔强**

[![UPSTREAM](https://img.shields.io/badge/UpStream-InkCanvas%2FInk--Canvas--Artistry-purple.svg "LICENSE")](https://github.com/InkCanvas/Ink-Canvas-Artistry)
![Gitea Last Commit](https://img.shields.io/gitea/last-commit/kriastans/InkCanvasForClass?gitea_url=https%3A%2F%2Fgitea.bliemhax.com%2F)
[![LICENSE](https://img.shields.io/badge/License-GPL--3.0-red.svg "LICENSE")](https://gitea.bliemhax.com/kriastans/InkCanvasForClass/src/branch/master/LICENSE)
[![交流群](https://img.shields.io/badge/-%E4%BA%A4%E6%B5%81%E7%BE%A4%20825759306-blue?style=flat&logo=TencentQQ)]()
[![Telegram](https://img.shields.io/badge/-Telegram%20@InkCanvasForClass-blue?style=flat&logo=Telegram)](https://t.me/InkCanvasForClass)

</div>

## Quickly
- 爱发电：[https://afdian.net/a/dubi906w/](https://afdian.net/a/dubi906w/)<br/>
- 新网站：[https://icc.bliemhax.com/](https://icc.bliemhax.com/)
- QQ群聊：[https://qm.qq.com/q/ptrGkAcqMo/](https://qm.qq.com/q/ptrGkAcqMo/)
- Telegram频道：[https://t.me/InkCanvasForClass/](https://t.me/InkCanvasForClass/)

> 本产品与peppy的osu!以及其周边项目和产品无任何关联，若有侵权，请联系Dev协商解决。

---

- 佛系更新，仅供个人使用，禁止用于商业用途<br/>
- 如有需求请自行修改项目，欢迎您的 Pull Request 和 Issue 提出 <br/>
- **基于 [https://github.com/InkCanvas/Ink-Canvas-Artistry](https://github.com/InkCanvas/Ink-Canvas-Artistry) 开发**

> ⚠️注意：此项目仍在开发中，只会在发布正式发行版时提供Release

## FAQ

### 壹 - ICC对PPT的相容性如何呢？

ICC 可以支持 WPS，但目前无法同时支持 MSOffice 和 WPS。若要启用 WPS 支持，请确保 WPS 是否在 “配置工具” 中开启了 “WPS Office 相容第三方系统和软件” 选项，该项目勾选并应用后，将无法检测到 MS Office 的COM接口。

如果您安装了“赣教通”、“畅言智慧课堂”等应用，可能会安装“畅言备课精灵”，因此会导致遗失64位的 Office COM 组件的注册且目前似乎无法修复（可以切换到新用户正常使用）。但 WPS Office 可以正常使用。

若要将 ICC 配合 WPS 使用，可打开“WPS 演示”后，前往“文件” - “选项” ，取消勾选“单萤幕幻灯片放映时，显示放映工具栏”该项，获得更好的体验。若要将 ICC 配合 MS Office 使用，可以打开 Powerpoint，前往“选项” ，“高级”，取消勾选“显示快捷工具栏”，获得更好的体验。

### 贰 - **安装后**程序无法正常启动？
请检查你的电脑上是否安装了 `.Net Framework 4.7.2` 或更高版本。若没有，请前往官网下载。

如果程序在启动后黑屏闪退，请打开 “事件查看器” 搜索有关 InkCanvasForClass 的错误信息并上报给开发者（可以在 GitHub 上提交 Issue，或者和开发者单独沟通）

> 遇到各种奇葩逗比问题请重启应用程式，如果不行请反馈给Dev解决！

## CLI

- `multiple` 以多实例模式启动
- `-f --fold` 启动后自动收纳到屏幕两侧
- `-r --reset` 启动后重置配置文件到默认状态