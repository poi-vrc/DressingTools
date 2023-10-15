---
sidebar_position: 2
---

# VRMとの使用

DressingToolsを使って、UniVRMだけのプロジェクトで服を着せることができます。

:::info
この機能は実験的です。統合ビルド、VRMスプリングボーンは未来にサポートされる予定です。
:::

## ガイド

### 1. UniVRMプロジェクトの準備

UniVRMのインストールは[こちら](https://github.com/vrm-c/UniVRM#installation)の指示に従ってください。Unity 2022.3で動作確認済みです。

### 2. DressingToolsをインストール

UniVRM を使用する場合、**VCC** で DressingTools をインストールすることはできません。[インストールガイド](/docs/getting-started/installation#via-unitypackage)の指示に従って、以下の3つの方法でインストールしてください：

- .unitypackage
- zip ファイル
- UPM Git URL

以下のパッケージがUnityパッケージマネージャに表示されます: (liltoonは任意です)

[![UPM](/img/guide-vrm-3-upmgr.PNG)](/img/guide-vrm-3-upmgr.PNG)

### 3. アバターや服など必要なパッケージをインポートする

また、アバターや服を Unity に表示させるために、シェーダー（UTS、poyomi、liltoonなど）をインストールする必要があるかもしれません、UniVRMによってすぐに置き換えられますが。

### 4. インポートされていないスクリプトをすべて削除する

インポートされていないスクリプトは、特にVRChat以外のプラットフォームを扱う場合、非常に頭の痛い問題です。

すべての VRCSDK スクリプト (VRC Avatar Descriptor など) を削除する必要があります。
すべての `VRCPhysBone` と `DynamicBone` を VRM スプリングボーンに変換するか、すべて削除する必要があります。

Hierarchy 検索を利用して、インポートされていないスクリプトをすべて削除することができます：

[![Search](/img/guide-vrm-4-search.PNG)](/img/guide-vrm-4-search.PNG)
[![Delete](/img/guide-vrm-4-delete.PNG)](/img/guide-vrm-4-delete.PNG)

### 5. 着換える

[キャビネットの作成](/docs/getting-started/setup-cabinet)と[服の着せ替え](/docs/getting-started/quick-setup-clothes)は、これまでのガイドに書かれているように、いつも通りに従ってください。

:::info
キャビネットアニメーション モジュールはVRMではサポートされていないから、何も生成されません。
:::

### 6. キャビネットをコピーとして適用する

これであなたのアバターのコピーが作成され、キャビネットはこのコピーに適用されます。次に、このコピーを使ってVRMファイルをエクスポートします。

[![Copy](/img/guide-vrm-6-copy.PNG)](/img/guide-vrm-6-copy.PNG)

### 7. VRMをエクスポート

そのコピーを使ってVRMをエクスポートすれば完了です！

[![Export](/img/guide-vrm-7-export.PNG)](/img/guide-vrm-7-export.PNG)
