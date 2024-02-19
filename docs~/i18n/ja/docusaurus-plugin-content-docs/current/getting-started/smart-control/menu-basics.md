---
sidebar_position: 1
---

# メニューの基本

DressingToolsは、カスタムメニューアイテムを処理し、インストールするためのメニューシステムがあります。

## ガイド

スマートコントロールを使い始める前に、メニューの基本を学んでおこう。

### 1. アバターの中にGameObjectを作る

まず、アバターの中に新しい GameObject を作ります。好きな名前に変更してください。

![Add GameObject](/img/menu-basics-1.PNG)

### 2. DT Menu Groupコンポーネントを追加する

新しく作成した GameObject に DT Menu Group コンポーネントを追加します。この GameObject をメニューアイテムのルートとして、その子はグループ化されます。

![Add Component](/img/menu-basics-2-1.PNG)
![Component](/img/menu-basics-2-2.PNG)

### 3. DT Menu Installコンポーネントを追加する

新しく作成した GameObject に DT Menu Install コンポーネントを追加します。このコンポーネントは、DT Menu Group コンポーネントのメニューアイテムをアバターのルートメニューに追加します。

インストール先を変更するには、インストールパスを指定することできます。(例：`Menu1/Menu2/Menu3`はMenu1、Menu2を経て、Menu3にメニューアイテムをインストールします)

空白の場合は、ルートメニューに直接追加されます。

![Menu Install](/img/menu-basics-3.PNG)

### 4. メニューアイテムの追加

DT Menu Group のボタンを使ってメニューアイテムを追加します。

![Add Menu Items](/img/menu-basics-4.PNG)

:::info
パラメータのドロップダウンリストには、アバターアニメーターのレイヤーから見つかったパラメータが表示されます。
パラメータの1つを選択すると、選択したパラメータが **必要に応じて、ネットワーク同期と保存など自動的に設定されます**。

この動作は、**DT Animator Parameters** コンポーネントを使用して設定を上書きすることで変更できます。

![Animator Parameters](/img/menu-basics-4-animparam.PNG)
:::

### 5. スマートコントロールアイテムの追加

メニューグループコンポーネントから直接スマートコントロールアイテムを追加することもできます。

![Smart Control](/img/menu-basics-5.PNG)

### 6. 完了！

再生モードに入ると、アバターのルートメニューに新しいメニューアイテムが追加されているのが見えます！

![Play Mode](/img/menu-basics-6.PNG)
