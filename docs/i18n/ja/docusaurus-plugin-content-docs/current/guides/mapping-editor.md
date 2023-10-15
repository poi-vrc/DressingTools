---
sidebar_position: 1
---

# マッピングエディター

マッピングエディターは、手動で上書きとマッピングを設定することで、DressingToolsを使用して **互換性がない** ウェアラブルに着せることができます。

DressingToolsはすべての種類のウェアラブルを着せることができるはずですが、例外があるかもしれません。このマッピングエディタはボーンマッピングをプレビューし、必要に応じて修正することができます。

:::info
また、このような問題を報告したり、機能要望を出したりして、回避策があるかどうかを確認することをお勧めします。
:::

[![Mapping editor manual](/img/mapping-editor-manual.PNG)](/img/mapping-editor-manual.PNG)

### マッピングエディタを起動する

[![Mapping editor start](/img/mapping-editor-start.png)](/img/mapping-editor-start.png)

### マッピングモード

[![Mapping editor modes](/img/mapping-editor-modes.PNG)](/img/mapping-editor-modes.PNG)

#### 自動（デフォルト）

すべてがドレッサーから自動生成される。オーバーライドや手動マッピングはありません。

#### 上書き・オーバーライド

ドレッサーはマッピングを生成し、あなたはそれらのマッピングのオーバーライドを追加することができます。ボーンマッピングを修正するのに便利です。

#### 手動

:::caution
一部のボーンを削除/再配置したユーザーのアバターとの非互換性の問題が発生する可能性があります。注意して使用してください。
:::

すべてのマッピングは手動で定義されます。

### マッピングを追加する

マッピングは、`+` ボタンを使って追加することができ、目的のウェアラブルボーンを右側のオブジェクトフィールドにドラッグします。

:::info
同じアバターボーンへの複数のマッピングがサポートされています。 `+` ボタンを押すだけで、複数のマッピングを追加できます。
:::

[![Mapping editor add](/img/mapping-editor-add-mapping.PNG)](/img/mapping-editor-add-mapping.PNG)

現在、5種類のマッピングがある：

[![Mapping editor mapping types](/img/mapping-editor-mapping-types.PNG)](/img/mapping-editor-mapping-types.PNG)

- **何もしない**： 文字通り何もしない
  - 使用例: 生成されたマッピングを上書きする
- **アバターボーンに移動する**： ウェアラブルボーンをアバターボーンに移動します。
  - 使用例: 通常のウェアラブルボーン、アクセサリー
- **アバター ボーンへの ParentConstraint を追加する**: 移動せずに、ウェアラブルボーンからターゲットアバターボーンに ParentConstraint を作成します。
  - 使用例: ダイナミクスを持つボーン (例: 胸のボーン)
- **トランスフォーム無視を追加する**： ウェアラブルボーンは、アバターダイナミクスの無視トランスフォームリストに追加されます
  - 使用例： **Questアバターで**、**ParentConstraints が使用できない状況で**、ダイナミクスを持つボーン (例: 胸のボーン)
  - 欠点： すべてのウェアラブルと**100%互換性があるわけではありません**。
- **アバターのダイナミクス データをコピーする**： アバターのダイナミクス データは、ウェアラブルボーンにコピーされます
  - 使用例： ダイナミクスを持つボーン (例: 胸のボーン)、グラブの目的で2つのダイナミクスコンポーネントを分離する🤔

### マッピングを削除する

`x` ボタンを使ってマッピングを削除することができます。

[![Mapping editor remove](/img/mapping-editor-remove-mapping.PNG)](/img/mapping-editor-remove-mapping.PNG)
