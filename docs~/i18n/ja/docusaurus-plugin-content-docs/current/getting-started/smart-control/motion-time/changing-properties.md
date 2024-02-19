---
sidebar_position: 3
---

# ラジアルでプロパティを変更する

このガイドでは、萌の `Big ear` のブレンドシェイプを0から100まで変化させるために、メニューにラジアル (Radial) アイテムを作ります。

### 0. メニューグループとスマートコントロールアイテムを作成する

このガイドでは、既存のメニューグループの準備が必要です。[このガイド](/docs/getting-started/smart-control/menu-basics)に従って、新しいメニューグループを作成してください。

DT Menu Group コンポーネントから直接作成することもできますし、そのメニューグループの下に新しいGameObjectを作成し、DT Smart Control コンポーネントを追加することもできます。

![Add SmartControl](/img/smartcontrol-create-0.PNG)

### 1. メニューアイテムのドライバを設定する

:::caution
現在UIに問題があるため、メニューアイテムタイプを`ラジアル`に変更する前に、まずコントロールタイプを`モーションタイム`に変更する必要があります。
:::

まずコントロールタイプを`モーションタイム`に変更します。

![Control Type](/img/smartcontrol-motiontime-1-ctrltype.PNG)

ドライバが `メニューアイテム` で、タイプが `ラジアル` であることを確認してください。名前とアイコンは好きなようにカスタマイズしてください。

![Menu Item](/img/smartcontrol-motiontime-1-menuitem.PNG)

### 2. プロパティグループを作成する

![Create Property Group](/img/smartcontrol-chg-prop-2.PNG)

### 3. Ear オブジェクトをフィールドにドラッグする

選択タイプが `通常` であることを確認します。Ear オブジェクトを `以下のオブジェクトを含む`の追加フィールド と `プロパティの選択元` にドラッグしてます。

![Pick Object](/img/smartcontrol-chg-prop-3.PNG)

:::info
プロパティグループの詳細については、[こちら](/docs/getting-started/smart-control/property-groups)を参照してください。
:::

### 4. ブレンドシェイププロパティを追加する

`SkinnedMeshRenderer` コンポーネントを見つけ、`ブレンドシェイプ` をクリックします。そして`Big ear`ブレンドシェイプを見つけて追加します。

`ブレンドシェイプ` ボタンをもう一度クリックし、結果を非表示にします。

![Pick Object](/img/smartcontrol-chg-prop-4.PNG)

### 5. ブレンドシェイプの値をマップする

始と終の値は、ラジアルコントロールの Float パラメータ 0.0 から 1.0 に対応します。

![Pick Object](/img/smartcontrol-motiontime-5.PNG)

### 6. 完了！

再生モードに入って、テストしてください！

![Result](/img/smartcontrol-motiontime-done.gif)
