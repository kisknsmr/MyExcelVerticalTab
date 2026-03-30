# ExcelVerticalTab (Sheet Deck) 🚀

[![Excel](https://img.shields.io/badge/Microsoft%20Excel-217346?style=for-the-badge&logo=microsoft-excel&logoColor=white)](https://office.com/)
[![.NET](https://img.shields.io/badge/.NET%20Framework%204.8.1-512BD4?style=for-the-badge&logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg?style=for-the-badge)](LICENSE)

> **もう、大量のシートタブに悩まされない。**  
> ExcelVerticalTab は、Excel の左側に垂直なシート管理パネルを追加する VSTO アドインです。

---

## ✨ 主な機能

- **📋 垂直タブリスト**: 大量（数十〜数百枚）のシートがあるブックでも、縦並びならスクロールでサクサク探せます。
- **🔍 リアルタイムフィルタ**: シート名の一部を入力するだけで、目的のシートを瞬時に抽出。もうタブの左右ボタンを連打する必要はありません。
- **🖱️ ドラッグ＆ドロップ**: 直感的な操作でシートの並べ替えが可能。Excel 本体のタブ順序と完全同期。
- **🪟 マルチウィンドウ対応**: 複数の Excel ウィンドウを開いていても、それぞれのブックに最適なパネルを表示。
- **🛠️ Sheet Deck リボン**: 専用のリボンタブから、ワンクリックでパネルの表示/非表示を切り替え。

---

## 📸 スクリーンショット (イメージ)

| 垂直タブ表示 | シート検索・フィルタ |
| :--- | :--- |
| ![Vertical Tab Pane](https://via.placeholder.com/300x450?text=Vertical+Tab+UI) | ![Filtering](https://via.placeholder.com/300x450?text=Search+Filter+UI) |
> *※ 上記はプレースホルダです。実際の動作画面はインストールしてお確かめください。*

---

## 🚀 クイックスタート (インストール)

開発者でない方も、配布済みのインストーラーを使用してすぐに利用可能です。

1. **前提条件**: [VSTO Runtime](https://www.microsoft.com/ja-jp/download/details.aspx?id=105522) がインストールされていることを確認してください。
2. **インストール**: `setup.exe` を実行し、画面の指示に従ってください。
3. **起動**: Excel を開くと、左側に 「Sheet Deck」 パネルが表示されます。

> 💡 **配布について**: リリース済みの EXE / インストーラーによる配布も検証済みです。詳細は [DISTRIBUTION.md](./docs/DISTRIBUTION.md) をご覧ください。

---

## 🛠️ 開発者向け情報

本プロジェクトは、WinForms の Task Pane 内で WPF コントロールをホストする構成を採用しています。

### アーキテクチャ & 品質
- **ExcelVerticalTab**: VSTO アドイン本体。COM インターフェース、リボン連携、ライフサイクル管理。
- **VerticalTabControl**: WPF 製の高機能なタブ操作 UI。MVVM パターン（CommunityToolkit.Mvvm）を採用。
- **Unit Tests**: `VerticalTabControl.UnitTests` プロジェクトにより、UI ロジックの品質を担保しています。
- **WorkbookHandler**: ブックやウィンドウの切り替えに追従するためのロジック。

### 技術スタック
- **Language**: C#
- **Framework**: .NET Framework 4.8.1 (VSTO)
- **UI**: WPF (Modern Look & Feel)
- **Toolkit**: CommunityToolkit.Mvvm, Microsoft.Xaml.Behaviors.Wpf

### 開発環境のセットアップとビルド
Visual Studio を使用してデバッグや開発を行う際の手順です。

1. **前提条件**:
   - **Visual Studio 2022/2026** (`Microsoft 365 development` ワークロードが必須)
   - **.NET Framework 4.8.1 Targeting Pack**
   - **Desktop版 Microsoft Excel**
2. **ビルドの注意**:
   - VSTO 固有のターゲットファイルを読み込むため、**`dotnet build` コマンド単体ではビルドできません。** 必ず Visual Studio からビルドしてください。
3. **手順**:
   - `ExcelVerticalTab.sln` を開き、NuGet パッケージを復元します。
   - 構成を `Debug` にして `F5` キーで実行すると、Excel が起動しアドインがロードされます。
   - 署名エラーが出る場合は、プロジェクトプロパティの「署名」タブからテスト用証明書を更新してください。

---

## ❓ トラブルシューティング

### インストールしたのにパネルが表示されない場合
1. Excel の `ファイル` > `オプション` > `アドイン` を開きます。
2. 下部の `管理:` を `COM アドイン` に変更して `設定` をクリックします。
3. `ExcelVerticalTab` にチェックが入っているか確認してください。

より詳細なトラブル解決方法は [INSTALL.md](./docs/INSTALL.md) を参照してください。

---

## 📜 ライセンス

このプロジェクトは [Apache License 2.0](LICENSE) の下で公開されています。
詳細は [NOTICE](NOTICE) ファイルもあわせてご確認ください。

---

## 📄 ドキュメント

- [インストール詳細手順 (INSTALL.md)](./docs/INSTALL.md)
- [配布ガイド (DISTRIBUTION.md)](./docs/DISTRIBUTION.md)
- [VSTO 技術背景 (VSTO_EXPLANATION.md)](./docs/VSTO_EXPLANATION.md)

---
*Created with ❤️ by the ExcelVerticalTab Team*
