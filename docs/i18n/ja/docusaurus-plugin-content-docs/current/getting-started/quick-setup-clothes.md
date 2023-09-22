---
sidebar_position: 3
---

# クイックセットアップ

DressingTools を使用すると、アバターを簡単かつきれいに着替えることができます。

DressingToolsは、**シングル クリックで簡単な服を着せたり、手動でアニメート**したりするために使うことができますが、DressingToolsのほとんどの使用例は、メニューとアニメーションを自動的に生成するためにキャビネット システムを使うはずです。

:::info
これはキャビネットのアニメーションとメニューを自動生成しません。代わりに[別の方法](setup-clothes-with-cabinet-anim)を使ってセットアップする必要があります。キャビネット・ウィンドウでウェアラブルを`編集`し、`自動セットアップ`を押しても同じ効果が得られます
:::

:::tip
通常のアニメーションの作成方法と同じように、ウェアラブル内のオブジェクトをアニメーション化できます。これらは適用中に自動的に再マップされます。
:::

## ガイド

:::info
このガイドでは、[キャビネットのセットアップガイド](setup-cabinet)に従ってアバター用のキャビネットを作成済みであることを前提としています
:::

はじめにシリーズでは、[Eliya WorkshopのSummer Streat](https://booth.pm/ja/items/4666271) を [Kyubi ClosetのMoe](https://kyubihome.booth.pm/items/4667400) に着せ替えます。

### 1.服をアバターにドラッグししてください

服をアバターにドラッグし、右クリックして `DressingTools -> Auto-setup Wearable (Mappings Only)` を選択ししてください

[![ウェアラブルのセットアップ](/img/setup-simple-2-setup-wearable.PNG)](/img/setup-simple-2-setup-wearable.PNG)

### 2. 完了！

再生モードに入ると、ウェアラブルがボーンに正しくマッピングされているかどうかをテストできます。

[![セットアップ完了](/img/setup-simple-3-done.PNG)](/img/setup-simple-3-done.PNG)

## 背後で起こったこと

### 「Auto-setup wearable (Mappings Only)」の使用

`Auto-setup wearable (Mappings Only)` をクリックすると、DressingTools は自動セットアップを実行してウェアラブル アーマチュアを見つけ、そのボーン マッピングのドライランと生成を試みます。

生成された設定を使用して、ウェアラブル ルート上に `DT Cabinet Wearable` コンポーネントが作成されます。

### 再生モードに入る

再生モードに入るとき、付属の `DT Cabinet` は、Unity が再生モードへの移行を完了する直前に含まれるウェアラブルを適用します。
