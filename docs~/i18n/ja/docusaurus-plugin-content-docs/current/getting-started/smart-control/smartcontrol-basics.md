---
sidebar_position: 2
---

# スマートコントロールの基本

オブジェクトやコンポーネントのオンオフを切り替えたり、ブレンドシェイプ、コンポーネント、シェーダーのプロパティを変更したりするアニメーションを作成することできます！

## コントロールタイプ

コントロールタイプは、オブジェクトをどのようにコントロールするかを決定することできます。現在、2種類のコントロールが実装されています。

![Control Type](/img/smartcontrol-basics-ctrltype.PNG)

### 二値 (Binary)

このタイプのコントロールには、オフ（0）とオン（1）の2つの状態しかなく、ブール (Bool) 値のパラメータでコントロールします。

**使用例:** オブジェクトやコンポーネントのオンとオフの切り替え、ブレンドシェイプ、シェーダー、プロパティの特定の値への変更など。

<a
    className="button button--success button--lg"
    target="_self"
    href="/docs/category/binary-control">
    始めましょう!
</a>

### モーションタイム (Motion Time)

再生時間を Float パラメータでコントロールするアニメーションを作成します。

**使用例:** ブレンドシェイプ、シェーダ、プロパティをラジアル (Radial) アイテムで動的に制御するなど。

<a
    className="button button--success button--lg"
    target="_self"
    href="/docs/category/motion-time-control">
    始めましょう!
</a>

## ドライバー

次の部分を参照してください。
