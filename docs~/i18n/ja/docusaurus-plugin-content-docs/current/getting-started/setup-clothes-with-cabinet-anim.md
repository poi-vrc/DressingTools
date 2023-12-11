---
sidebar_position: 4
---

# 服とキャビネットアニメーションをセットアップ

DressingTools を使用すると、アバターで複数の服を着ることはとても便利で簡単です。

DressingToolsは、必要なすべてのウェアラブル 着用時のアニメーションと同名のブレンドシェイプ同期（例：Big breast）を下書きします。アバターの切り替え/ブレンドシェイプを追加し、下書きされた内容を確認するだけです！

アニメーションの設定中に**リアルタイム プレビュー**も可能です！

## ガイド

:::info
このガイドでは、[キャビネットのセットアップガイド](setup-cabinet)に従ってアバター用のキャビネットを作成済みであることを前提としています
:::

はじめにシリーズでは、[Eliya WorkshopのSummer Streat](https://booth.pm/ja/items/4666271) を [Kyubi ClosetのMoe](https://kyubihome.booth.pm/items/4667400) に着せ替えます。

### 1.服をアバターにドラッグししてください

服をアバターにドラッグし、右クリックして `DressingTools -> Setup wearable with wizard` を選択してください

[![ウェアラブルのセットアップ](/img/setup-cabanim-1-setup-wearable.PNG)](/img/setup-cabanim-1-setup-wearable.PNG)

### 2. ツールウィンドウが表示されます

自動セットアップが実行されて設定が作成されます。

[![ツール ウィンドウがポップアップ](/img/setup-cabanim-2-tool-window.PNG)](/img/setup-cabanim-2-tool-window.PNG)

### 3. サムネイルを撮る (任意)

サムネイルはメニューを飾り、そのウェアラブルが何かをより分かりやすくします。`新しいサムネイルを撮る`をクリックしてください。シーンビューを動かしてサムネイルに適した場所を選んだら、`撮る`を押してください。

[![サムネイルを撮る](/img/setup-cabanim-3-thumbnail.PNG)](/img/setup-cabanim-3-thumbnail.PNG)

:::caution
シーン ビューがカメラと一緒に動かない場合は、`Gizmos`をオンにして、もう一度試してみてください。

[![Gizmos](/img/setup-cabanim-3-gizmos.PNG)](/img/setup-cabanim-3-gizmos.PNG)
:::

### 4. マッピングを確認する

`マッピング` タブで、アーマチュア ボーン マッピングにエラーが報告されていないか確認してください。

[![マッピング](/img/setup-cabanim-4-mapping.PNG)](/img/setup-cabanim-4-mapping.PNG)

### 5. キャビネットアニメーション

DressingToolsは自動的にウェアラブル切り替えを下書きするので（現在有効なオブジェクトを追加します）、アバターの着用時の切り替えとブレンドシェイプを設定するだけです。

切り替えとブレンドシェイプを編集時、アニメーションを**リアルタイムプレビュー**することができます！

:::caution
`キャビネットアニメーション` がオンになっていることを確認してください。これは自動セットアップで自動的にオンになります。

[![Ensure on](/img/setup-cabanim-5-ensure-cabanim-on.PNG)](/img/setup-cabanim-5-ensure-cabanim-on.PNG)

`自動セットアップ` ボタンを使って、自動セットアップを再度実行することができます。

[![Auto-setup button](/img/setup-cabanim-5-auto-setup-btn.PNG)](/img/setup-cabanim-5-auto-setup-btn.PNG)
:::

#### 5.1 着用時のアバター切り替え

与えられた`提案`を利用し、あなたの服が既存の服と重ならなくなるまでアバターの切り替えを追加してください。オブジェクトフィールドの隣にあるチェックボックスは、そのオブジェクトがオンウェアで有効になるかどうかのステータスです。

また、他の場所にある切り替えを追加するには `+ 追加` を使用してください。`x`は切り替えを削除します。

[![切り替え](/img/setup-cabanim-5-avatar-onwear-toggles.PNG)](/img/setup-cabanim-5-avatar-onwear-toggles.PNG)

#### 5.2 アバターの着用時のブレンドシェイプ

与えられた `提案` を利用して、服とアバターが正常に見えるまでブレンドシェイプの変更を追加してください。 (例：胸/足/ハイヒールのシェイプキーを変更する)

[![ブレンドシェイプ](/img/setup-cabanim-5-avatar-onwear-blendshapes.PNG)](/img/setup-cabanim-5-avatar-onwear-blendshapes.PNG)

#### 5.3 ウェアラブルの着用時の切り替えを確認する

自動生成されたウェアラブルな着用時の切り替えが好みと一致するかどうかを確認してください。最初から有効になっており、ウェアラブル ルートに配置されている場合にのみ自動的に追加されます。

[![ウェアラブル着用時](/img/setup-cabanim-5-wearable-onwear.PNG)](/img/setup-cabanim-5-wearable-onwear.PNG)

:::tip
着用時のプリセットを活用してください。自分のプリセットを保存して読み込めば、次回から早く着こなすことができます！

[![プリセット](/img/setup-cabanim-5-tip-presets.PNG)](/img/setup-cabanim-5-tip-presets.PNG)
:::

### 6. ブレンドシェイプの同期を確認する

:::caution
DressingTools は間違ったブレンドシェイプ同期と一致する可能性がありますので、毎回これを確認する必要があります。
:::

DressingTools は、同じ名前のアバター ブレンドシェイプとウェアラブル ブレンドシェイプを照合し、ここに同期を自動的に追加します。

[![ウェアラブル着用時](/img/setup-cabanim-6-blendshape-sync.PNG)](/img/setup-cabanim-6-blendshape-sync.PNG)

### 7. キャビネットに追加する

:::caution
このボタンを押すまで、設定は保存されません。
:::

`キャビネットに追加`を押すと、ウェアラブルがキャビネット ビューに表示されます。

[![ボタン](/img/setup-cabanim-7-addtocabinet.PNG)](/img/setup-cabanim-7-addtocabinet.PNG)

[![キャビネット ビュー](/img/setup-cabanim-7-cabinetview.PNG)](/img/setup-cabanim-7-cabinetview.PNG)

### 8. 完了！

準備は完了です! 再生モードに入って結果を見てみましょう! BlackStartX の Gesture Manager を使用して、Unity Editor 内でテストできます！

[![](/img/setup-cabanim-8-done.PNG)](/img/setup-cabanim-8-done.PNG)

:::tip
キャビネット エディタからいつでも設定を変更できます。

[![キャビネット](/img/setup-cabanim-8-tip-edit.PNG)](/img/setup-cabanim-8-tip-edit.PNG)
:::

:::tip
モジュールを追加したい場合は、`上級`モードを使用してください！

[![上級](/img/setup-cabanim-8-tip-advanced.PNG)](/img/setup-cabanim-8-tip-advanced.PNG)
:::
