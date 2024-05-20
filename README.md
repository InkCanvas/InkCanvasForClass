<div align="center">

<img src="icc.png" width="128">

# InkCanvasForClass

Elegant by Default. Based on `ChangSakura/InkCanvas` .

**這將會是最後一次基於InkCanvas控件的倔強**

[![UPSTREAM](https://img.shields.io/badge/UpStream-ChangSakura%2FInk--Canvas-red.svg "LICENSE")](https://github.com/ChangSakura/Ink-Canvas)
![Gitea Last Commit](https://img.shields.io/gitea/last-commit/kriastans/InkCanvasForClass?gitea_url=https%3A%2F%2Fgitea.bliemhax.com%2F)
[![LICENSE](https://img.shields.io/badge/License-GPL--3.0-red.svg "LICENSE")](https://gitea.bliemhax.com/kriastans/InkCanvasForClass/src/branch/master/LICENSE)
[![交流群](https://img.shields.io/badge/-%E4%BA%A4%E6%B5%81%E7%BE%A4%20825759306-blue?style=flat&logo=TencentQQ)]()
[![Telegram](https://img.shields.io/badge/-Telegram%20@InkCanvasForClass-blue?style=flat&logo=Telegram)](https://t.me/InkCanvasForClass)

![Screenshot-1](./Images/icc1.png)
![Screenshot-2](./Images/icc2.png)

</div>

## 公告
该项目旨在为ICA提供更加舒适和易于使用的UI和用户体验，对于任何墨迹书写方面的Bug如果本人无法修复将不会被修复。本人因为最近忙于开发InkCanvasForClass X导致本项目开发进度最近比较缓慢，请多一份理解，谢谢！

## 前言
使用和分發本軟體前，請您應當且務必知曉相關開源協議，本軟體基於 https://github.com/ChangSakura/Ink-Canvas 修改而成，而ICA又基於 https://github.com/WXRIW/Ink-Canvas 修改而成，增添了包括但不限於隱藏到側邊欄等功能，更改了相關UI和軟體操作邏輯。對於墨跡書寫功能以及ICA獨有功能的相關 issue 提出，應優先查閱 https://github.com/WXRIW/Ink-Canvas/issues 。

[直接下載](https://gitea.bliemhax.com/kriastans/InkCanvasForClass/releases "Latest Releases")
——該安裝包使用 NodeJs+NodeGUI打包，默認配置適配絕大多數紅外觸摸框的設置。

> ⚠️注意：此項目仍在開發中，只會在發佈正式發行版時提供Release。您可以自行使用VS2022編譯打包後自行使用

## 特性
1. Support Active Pen (支持壓感)
2. 工具欄顯示了每個功能的文字描述
3. 添加了調色盤的顏色
4. 添加了熒光筆支持

## 提示
- 對新功能的有效意見和合理建議，開發者會適時回復並進行開發。本軟體並非商業性質軟體，請勿催促開發者，耐心才能讓功能更少 BUG、更加穩定。
- 此軟體僅用於私人使用，請勿商用。更新也不會很快，如果有能力請PR貢獻程式碼而不是在Issue裡面提問題。
- 歡迎您使用InkCanvas家族的其他成員，包括ICC和ICA的創始者IC以及和ICC差不多的ICA。您的大力宣傳能夠幫助我們的軟件被更多的用戶發現。

## FAQ

### 點擊放映後一翻頁就閃退？
考慮是由於`Microsoft Office`未啟用導致的，請自行啟用

### 放映後畫板程序不會切換到PPT模式？
如果你曾經安裝過`WPS`且在卸載後發現此問題則是由於暫時未確定的問題所導致，可以嘗試重新安裝WPS
> “您好，關於您回饋的情況我們已經回饋技術同學進一步分析哈，辛苦您可以留意後續WPS版本更新哈~” --回復自WPS客服

另外，處在保護（只讀）模式的PPT不會被識別

若因安裝了最新版本的 WPS 而導致無法在 WPS 軟體內進入 PPT 模式，可以嘗試卸載 WPS 後，並清除電腦垃圾、註冊表垃圾、刪除電腦上所有帶 "kingsoft" 名稱的文件夾，重新安裝 WPS 2021 後，（以上步驟可能有多餘步驟），經測試在 WPS 內可以正常進入 PPT 模式。

不支援WPS純淨版！

### **安裝後**程序無法正常啟動？
請檢查你的電腦上是否安裝了 `.Net Framework 4.7.2` 或更高版本。若沒有，請前往官網下載  
如果仍無法運行，請檢查你的電腦上是否安裝了 `Microsoft Office`。若沒有，請安裝後重試

## 特別鳴謝

<table>
    <tbody>
        <tr>
            <td align="center" valign="top" width="14.28%"><a href="https://github.com/ChangSakura"><img
                        src="https://avatars.githubusercontent.com/u/90511645?v=4" width="100px;"
                        alt="HelloWRC" /><br /><sub><b>ChangSakura</b></sub></a></td>
            <td align="center" valign="top" width="14.28%"><a href="https://github.com/WXRIW"><img
                        src="https://avatars.githubusercontent.com/u/62491584?v=4" width="100px;"
                        alt="Doctor-yoi" /><br /><sub><b>WXRIW</b></sub></a></td>
            <td align="center" valign="top" width="14.28%"><a href="https://github.com/Alan-CRL"><img
                        src="https://avatars.githubusercontent.com/u/92425617?v=4" width="100px;"
                        alt="姜胤" /><br /><sub><b>Alan-CRL</b></sub></a></td>
        </tr>
    </tbody>
</table>