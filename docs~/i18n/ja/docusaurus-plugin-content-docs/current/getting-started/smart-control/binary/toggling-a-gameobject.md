---
sidebar_position: 1
---

# GameObjectを切り替える

## ガイド

このガイドでは、アバター 萌 のアウターをオン・オフするためのトグルをメニューに作ります。

### 0. メニューグループとスマートコントロールアイテムを作成する

このガイドでは、既存のメニューグループの準備が必要です。[このガイド](/docs/getting-started/smart-control/menu-basics)に従って、新しいメニューグループを作成してください。

DT Menu Group コンポーネントから直接作成することもできますし、そのメニューグループの下に新しいGameObjectを作成し、DT Smart Control コンポーネントを追加することもできます。

![Add SmartControl](/img/smartcontrol-create-0.PNG)

### 1. メニューアイテムのドライバを設定する

ドライバが `メニューアイテム` で、タイプが `トグル` であることを確認してください。名前とアイコンは好きなようにカスタマイズしてください。

![Setup Driver](/img/smartcontrol-toggle-go-1.PNG)

### 2. コントロールタイプを二値に設定し、Outer オブジェクトを追加フィールドにドラッグする

ドロップダウンボックスはトグルするコンポーネントを指定することできます（次のガイドで役に立つ）。今の場合は、`Transform`に設定するとGameObject全体がトグルされます。

フィールドの隣にあるチェックボックスは、このコントロールが有効なときの状態を指定します。今の場合は、オブジェクトはオフになります。

![Toggle GameObject](/img/smartcontrol-toggle-go-2.PNG)

### 3. 完了！

これで 萌の Outer 用トグルの作成は完了です！

![Done](/img/smartcontrol-toggle-go-3.PNG)
