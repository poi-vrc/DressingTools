---
sidebar_position: 1
---

# インストール

DressingToolsは **スタンドアロンのUnity
UPMパッケージ**であり、ゲームやツールのSDKをインストールする必要はありません。生成したキャビネットをこれらのゲームやツールに統合しようとしている場合にのみ必要です。

これは ** VRChat にまったく関係のない**
ワークフローに役立ちます。例えば、**VRM**プロジェクト、**ChilloutVR**、**MMD**、またはVRCSDKをまったくサポートしていないUnityプロジェクトの場合も使うことができる。

### 依存関係

DressingTools は現在、2 つの外部プロジェクト `DressingFramework` および `AvatarLib` に依存しています。

## VRChat用a

DressingTools を VRChat と統合するには、**インストールに VCC を使用することをおすすめです**。これは、DressingTools
が必要とするすべてのライブラリとサポートされている VRCSDK のバージョンを管理することができます。

### VCC (VRChat Creator Companion)

1. このボタンで VPM リポジトリをインストールするか、`https://vpm.chocopoi.com/index.json`
   を使用して手動でリポジトリを追加してください。
   <a
className="button button--success button--lg"
target="_self"
href="vcc://vpm/addRepo?url=https%3A%2F%2Fvpm.chocopoi.com%2Findex.json">
   VCC を使用してインストールする </a>

2. `I Understand, Add Repository`を押してリポジトリを追加してください。

   [![インストールVCC理解したボタン](/img/installation-vcc-repo-understand.PNG)](/img/installation-vcc-repo-understand.PNG)

3. DressingTools パッケージをアバター プロジェクトに追加ししてください。

   [![インストールVCC追加パッケージ](/img/installation-vcc-add-package.PNG)](/img/installation-vcc-add-package.PNG)

   :::caution DressingTools がまだ Beta 版なので、パッケージが表示されない可能性があります。 `Settings -&gt;
   Packages`で`Show Pre-release Packages`を有効にする必要があります。 [![VCC
   プレリリースのインストール](/img/installation-vcc-prerelease.png)](/img/installation-vcc-prerelease.png)
   :::

4. プロジェクトを開けば準備完了です!

   [![インストールVCCティーザー](/img/teaser-1.PNG)](/img/teaser-1.PNG)

## その他のプロジェクト

### UPM Git URL

:::danger VCC で DressingTools をすでにインストールしている場合は、これらの手順を実行しないでください。 :::

:::caution Unity は UPM Git URL
の更新と依存関係を処理しません。正しく動作させるためには、正しいバージョンの依存関係をもう一度確認してください。 :::

1. Unityパッケージマネージャーを起動してください

   [![インストールUPMGitURLパッケージマネージャー](/img/installation-upmgit-open-pkg-mgr.PNG)](/img/installation-upmgit-open-pkg-mgr.PNG)

2. `Add package from git URL...` をしてください

   [![インストールUPMGitAddfromgitURL](/img/installation-upmgit-install-from-git.PNG)](/img/installation-upmgit-install-from-git.PNG)

3. 以下のパッケージを追加し、必要なバージョンがインストールするDressingToolsのバージョンと一致していることを確認してください。`#`
   後を希望のバージョンに変更してください。

   - DressingTools
     - `https://github.com/poi-vrc/DressingTools.git?path=Packages/com.chocopoi.vrc.dressingtools#2.0.0-beta`
   - DressingFramework
     - `https://github.com/poi-vrc/DressingFramework.git#1.0.0-beta`
   - AvatarLib
     - `https://github.com/poi-vrc/AvatarLib.git#1.0.0-beta`

### zip ファイル

:::danger この方法でDressingToolsをインストールすることは推奨されません :::

:::caution コピーする前に古いパッケージを削除してください。依存関係の正しいバージョンをインストールしてください :::

zip ファイルを直接ダウンロードして、`Packages`フォルダに解凍することができます。このようになります：
```
Packages
  |- com.chocopoi.vrc.dressingtools
  |- com.chocopoi.vrc.dressingframework
  |- com.chocopoi.vrc.avatarlib
```

- DressingTools
  - https://github.com/poi-vrc/DressingTools/releases
- DressingFramework
  - https://github.com/poi-vrc/DressingFramework/releases
- AvatarLib
  - https://github.com/poi-vrc/AvatarLib/releases

### .unitypackage

:::danger
現在、`.unitypackage`ファイルを使ったインストールはサポートされていません。将来的にはサポートされるかもしれませんが、DressingToolsのインストール方法としては推奨されていません：
