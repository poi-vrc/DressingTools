---
sidebar_position: 3
---

# ドライバー

ドライバとは、このスマートコントロールをどのようなメカニズムで駆動させたいかを意味です。

:::caution
🚧 説明書のこの部分はまだ未完成です。
:::

## アニメーターパラメーター

このSmartControlを制御するために、カスタムアニメーターパラメーターを使用できる汎用ドライバーです。

アニメーターパラメーター名が空のままだと、ビルド時に自動生成されます。

## メニューアイテム

このドライバは、同じGameObject内の既存の `DT Menu Item` コンポーネントを追加または置き換えます。
メニューアイテムは、1つのパラメータでSmartControlを直接制御します。

![Menu Item Driver](/img/smartcontrol-basics-driver-menuitem.PNG)

アニメーターパラメーター名が空のままだと、ビルド時に自動生成されます。

現在、以下をサポートしている：

|             | Button | Toggle | Radial | Two-axis | Four-axis | Sub-menu |
|-------------|--------|--------|--------|----------|-----------|----------|
| Binary      | ✅     | ✅    | ❌     | ❌      | ❌        | ❌      | 
| Motion Time | ❌     | ❌    | ✅     | ❌      | ❌        | ❌      | 

✅: 対応, ❌: 非対応

## VRCPhysBone

*2.4.0 以降*

このドライバは、SmartControlをVRC PhysBoneコンポーネントで制御できるようにします。
他のプレイヤーと相互作用するコントロールを簡単に作成できます。

![VRCPhysBone Driver](/img/smartcontrol-basics-driver-vrcphysbone.png)

アニメーターパラメータープレフィックスが空のままだと、ビルド時に自動生成されます。

PhysBoneの機能の詳細については、[公式VRChatドキュメント](https://creators.vrchat.com/avatars/avatar-dynamics/physbones/) を参照してください。

:::caution
少なくとも1つの要件または1つのソースを選択する必要があります。そうでない場合、SmartControlは無視され、生成されません。
:::

異なる要件とソースの組み合わせに対して、異なるコントロールタイプのアニメーションを生成します：

|         | なし        | 掴んでいる時     | ポーズしている時       | 掴んでいるかポーズしている時 |
|---------|-------------|-------------|-------------|------------------|
| なし    | ❌          | Binary      | Binary      | Binary           |
| 角度    | Motion Time | Motion Time | Motion Time | Motion Time      |
| 伸び    | Motion Time | Motion Time | Motion Time | Motion Time      |
| へこみ  | Motion Time | Motion Time | Motion Time | Motion Time      |

❌: 無視されて生成されません

例えば、アホ毛をつかんだりポーズをとったりせずに、つぶされたときに常にコントロールに影響を与えるようにしたい場合、**要件**を**なし**に、**ソース**を**へこみ**に設定します。

掴んだりポーズをとったりしたときだけ伸びの影響を受けたい場合は、**要件** を **掴んでいるかポーズしている時** に、**ソース** を **伸び** に設定します。
