# ExcelVerticalTab インストール手順書

本アドインを Excel に組み込み、垂直タブ機能を利用可能にするための手順です。

## 2026 年時点での対応可否

2026 年 3 月 28 日時点では、対応可能です。

ただし前提があります。

- 開発環境は Windows 上の Visual Studio 2022 または Visual Studio 2026 を推奨します。
- VSTO アドインは引き続き .NET Framework ベースです。
- `dotnet build` 単体ではなく、Visual Studio の Office / VSTO 開発環境が必要です。
- Visual Studio 2026 の ARM64 版では、Microsoft の公式システム要件上 `Office/SharePoint development` は未サポートです。x64/AMD64 の Windows 環境を推奨します。

## 事前準備

インストール前に、以下のコンポーネントが PC に入っていることを確認してください。

1. .NET Framework 4.8.1 ランタイム
2. Visual Studio 2010 Tools for Office Runtime (VSTO Runtime)
3. Microsoft Excel デスクトップ版
4. 開発用途の場合は Visual Studio 2022 または Visual Studio 2026

対応対象として想定している Excel は次の系統です。

- Excel 2013
- Excel 2016
- Excel 2019
- Excel 2021
- Microsoft 365

## 公式リンク

- VSTO Runtime ダウンロード
  https://www.microsoft.com/ja-jp/download/details.aspx?id=105522
- VSTO Runtime / サポート方針
  https://learn.microsoft.com/en-us/visualstudio/vsto/visual-studio-tools-for-office-runtime?view=visualstudio
- Office 開発の開始ガイド
  https://learn.microsoft.com/en-us/visualstudio/vsto/getting-started-office-development-in-visual-studio?view=visualstudio
- Visual Studio 2026 システム要件
  https://learn.microsoft.com/en-us/visualstudio/releases/2026/vs-system-requirements
- Visual Studio の `Microsoft 365 development` ワークロード
  https://learn.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-community?view=visualstudio

## Visual Studio をまだ入れていない場合

まずは Visual Studio から入れてください。個人利用や検証用途なら、通常は `Visual Studio Community` で十分です。

### おすすめの進め方

1. Visual Studio Community のダウンロードページを開く
2. インストーラーをダウンロードして実行する
3. ワークロード選択画面で `Microsoft 365 development` を選ぶ
4. そのままインストールを完了する
5. インストール後に `ExcelVerticalTab.sln` を開く

### ダウンロード先

- Visual Studio Community
  https://visualstudio.microsoft.com/vs/community./

### インストール時に確認したい項目

- `Microsoft 365 development` ワークロード
- NuGet パッケージ マネージャー
- .NET Framework 4.8 系の開発ツール

### 迷ったらどのバージョンを選ぶべきか

- まず安全に進めたいなら `Visual Studio 2022`
- 最新環境で進めたいなら `Visual Studio 2026`

2026 年 3 月 28 日時点では、Visual Studio 2026 でも公式に `Microsoft 365 development` ワークロードは案内されています。ただし ARM64 環境には制約があるため、迷う場合は x64 の Windows + Visual Studio 2022 か 2026 を選ぶのが無難です。

## パターンA: 開発環境でビルドして利用する場合

自分でソースコードからビルドし、そのまま現在の PC で使う場合の手順です。

### 1. Visual Studio の準備

Visual Studio インストーラーで、少なくとも次を確認してください。

- `Microsoft 365 development` ワークロード
- .NET Framework 4.8 系の開発ツール
- NuGet パッケージ マネージャー

### 2. ソリューションを開く

`ExcelVerticalTab.sln` を Visual Studio で開きます。

### 3. NuGet パッケージを復元する

ソリューションを右クリックし、`NuGet パッケージの復元` を実行します。

現在このリポジトリで利用している主なパッケージ:

- `CommunityToolkit.Mvvm`
- `Microsoft.Xaml.Behaviors.Wpf`

### 4. 署名設定を確認する

このプロジェクトは、リポジトリ内の `.pfx` ファイルではなく、Windows の現在のユーザー証明書ストア `CurrentUser\My` にあるコード署名証明書を使う構成です。

この開発 PC では、テスト用コード署名証明書がすでに設定されているため、`Debug` / `Release` ビルドと `Publish` が通る状態です。

別の PC や別ユーザー環境で作業する場合は、次のいずれかを行ってください。

- その環境にテスト用コード署名証明書を作成して設定する
- 正式なコード署名証明書を用意して設定する

署名設定が整っていないと、発行や一部のビルド手順で失敗することがあります。

### 5. ビルドと実行

1. 構成を `Debug` にします。
2. `F5` キー、または Visual Studio のデバッグ開始を実行します。
3. Excel が起動し、アドインが読み込まれることを確認します。

### 6. 確認

次の状態になれば成功です。

- Excel に `Sheet Deck` の task pane や関連 UI が表示される
- 左側に縦型タブ pane が表示される
- シート選択、フィルタ、ドラッグアンドドロップが動作する

## パターンB: 配布用パッケージを作成してインストールする場合

他の PC に配布する場合の流れです。

### 1. 開発者側で発行する

1. Visual Studio で `ExcelVerticalTab` プロジェクトを右クリックします。
2. `発行 (Publish)` を選択します。
3. ウィザードに従って発行先を指定します。
4. 発行完了後、`setup.exe` と `.vsto` マニフェストを含むフォルダが生成されることを確認します。

注意:

- 発行は VSTO / ClickOnce の設定に依存します。
- 別 PC では証明書設定が未整備だと、この手順で止まることがあります。

### 2. 利用者側でインストールする

1. 発行フォルダ一式をインストール先 PC にコピーします。
2. `setup.exe` を実行します。
3. 確認ダイアログが表示されたら、内容を確認してインストールします。
4. インストール完了後、Excel を起動します。

### 3. 動作確認する

次を確認してください。

- Excel 起動時にアドインが読み込まれる
- 左側の垂直タブ pane が表示される
- リボンから表示切り替えができる

## セキュリティに関する注意

VSTO アドインは、環境によってはセキュリティ設定の影響を受けます。

インストール後に読み込まれない場合は、次を確認してください。

### COM アドインとして有効か確認する

1. Excel を開きます。
2. `ファイル` → `オプション` → `アドイン` を開きます。
3. 下部の `管理` を `COM アドイン` に切り替えて `設定` を押します。
4. `ExcelVerticalTab` にチェックが入っているか確認します。

### 信頼できる場所を確認する

必要に応じて、発行フォルダを信頼済みの場所に追加してください。

1. Excel を開きます。
2. `ファイル` → `オプション` → `トラスト センター` → `トラスト センターの設定` を開きます。
3. `信頼できる場所` を開きます。
4. 発行フォルダを追加します。

## アンインストール

1. Windows の `設定`
2. `アプリ`
3. `インストールされているアプリ`
4. `ExcelVerticalTab` を選択してアンインストール

## トラブルシューティング

### `dotnet build` では通らない

このプロジェクトは VSTO の `Microsoft.VisualStudio.Tools.Office.targets` を使うため、Visual Studio の Office 開発環境が必要です。

### Ribbon や pane が出ない

次を順に確認してください。

- Excel の COM アドイン一覧で有効になっているか
- VSTO Runtime が入っているか
- Excel がデスクトップ版か
- Visual Studio で起動した場合、デバッグ時に例外が出ていないか

### パッケージ復元に失敗する

NuGet への接続が必要です。社内ネットワークやプロキシ配下では、`nuget.org` へのアクセス設定を確認してください。

### 発行やインストールで警告が出る

ClickOnce / VSTO の署名設定や証明書が原因のことが多いです。少なくともこの開発 PC では解消済みですが、別 PC では証明書設定を見直してください。

## 補足

このリポジトリは現在、VSTO を前提とした構成です。Microsoft の公式方針でも、VSTO Add-in は引き続き .NET Framework ベースで保守され、クロスプラットフォームの新規拡張には Office Add-ins が推奨されています。
