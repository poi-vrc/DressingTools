---
sidebar_position: 2
---

# コンポーネントを切り替える

## ガイド

このガイドでは、アバターのVRCPhysBoneコンポーネントのオンとオフを切り替えるトグルをメニューに作ります。

### 0. メニューグループとスマートコントロールアイテムを作成する

このガイドでは、既存のメニューグループの準備が必要です。[このガイド](/docs/getting-started/smart-control/menu-basics)に従って、新しいメニューグループを作成してください。

DT Menu Group コンポーネントから直接作成することもできますし、そのメニューグループの下に新しいGameObjectを作成し、DT Smart Control コンポーネントを追加することもできます。

![Add SmartControl](/img/smartcontrol-create-0.PNG)

### 1. メニューアイテムのドライバを設定する

ドライバが `メニューアイテム` で、タイプが `トグル` であることを確認してください。名前とアイコンは好きなようにカスタマイズしてください。

![Setup Driver](/img/smartcontrol-toggle-go-1.PNG)

### 2. コントロールタイプを二値に設定し、VRCPhysBone付きのオブジェクトを追加フィールドにドラッグする

ドロップダウンボックスはトグルするコンポーネントを指定することできます。今の場合は、`VRCPhysBone` に設定すると、GameObject全体じゃなくて、VRCPhysBoneがトグルされます。

フィールドの隣にあるチェックボックスは、このコントロールが有効なときの状態を指定します。今の場合は、オブジェクトはオフになります。

![Toggle Component](/img/smartcontrol-toggle-comp-2.PNG)

### 3. 完了！

これでコンポーネント用トグルの作成は完了です！
