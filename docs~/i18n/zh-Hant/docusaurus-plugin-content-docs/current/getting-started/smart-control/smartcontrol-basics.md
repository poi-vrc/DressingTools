---
sidebar_position: 2
---

# 智能控制基礎知識

建立動畫以開啟和關閉物件或元件，或變更混合形狀、元件、Shader屬性等！

## 控制類型

控制類型決定如何控制物件。 目前只開發了兩種類型的控制。

![Control Type](/img/smartcontrol-basics-ctrltype.PNG)

### 二進位 (Binary)

顧名思義，這種類型的控制只有兩種狀態：關閉（0）和開啟（1），由布林 (Boolean) 參數控制。

**用例：** 開啟和關閉物件和元件，將混合形狀、Shader和屬性變更為特定值等。

<a
    className="button button--success button--lg"
    target="_self"
    href="/docs/category/binary-control">
    開始設定吧！
</a>

### 動態動畫時間 (Motion Time)

這創建一個動畫，其播放時間由浮動 (Float) 參數控制。

**使用案例：** 使用徑向選單項目動態控制混合形狀、Shader和屬性等。

<a
    className="button button--success button--lg"
    target="_self"
    href="/docs/category/motion-time-control">
    開始設定吧！
</a>

## 驅動

驅動是指你希望這個智能控制由甚麽原理去推動。 目前有兩種類型的驅動，將來還會添加更多類型。

### 選單項作為驅動

![Menu Item Driver](/img/smartcontrol-basics-driver-menuitem.PNG)

如果 Animator 參數名稱留空，它將在建置時自動產生。

一些選單項目類型有限制：

|             | Button | Toggle | Radial | Two-axis | Four-axis | Sub-menu |
|-------------|--------|--------|--------|----------|-----------|----------|
| Binary      | ✅     | ✅    | ❌     | ❌      | ❌        | ❌      | 
| Motion Time | ✅*    | ✅*   | ✅     | 🚧      | 🚧        | ❌      | 

✅：支持，❌：不支持 🚧：計畫支持

*此選單項目直接寫入浮點參數 0 或 1, 沒有中間值。

### Animator 參數作為驅動

如果 Animator 參數名稱留空，它將在建置時自動產生。

|             | Float  | Int    | Bool   | Others   |
|-------------|--------|--------|--------|----------|
| Binary      | ❌     | ❌    | ✅     | ❌      |
| Motion Time | ✅     | ✅*   | ❌     | ❌      |

✅：支持，❌：不支持

*Motion Time 與浮點 (Float) 以外的類型一起使用意味著只有值 0 或 1 對控制有效。
