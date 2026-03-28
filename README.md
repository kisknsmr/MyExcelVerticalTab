# ExcelVerticalTab

Excel のワークシートタブを、左側のカスタム作業ウィンドウに縦並びで表示する VSTO アドインです。

このプロジェクトでは、WinForms ベースの task pane の中に WPF 製の縦タブ UI をホストしています。現在は主に次の機能を持っています。

- ワークシートの選択とアクティブ化
- シート名の部分一致フィルタ
- ドラッグアンドドロップによるシート並べ替え
- Ribbon からのペイン表示切り替え

## ソリューション構成

- `ExcelVerticalTab/`
  VSTO アドイン本体です。Excel イベント購読、task pane のライフサイクル管理、Ribbon 連携、WorkbookHandler、COM 相互運用を担当します。
- `VerticalTabControl/`
  task pane 内で表示する WPF コントロール群です。
- `VerticalTabControl.UnitTests/`
  `VerticalTabControl` のユニット テストです。UI 部分の退行を小さく検証できます。
- `docs/`
  インストール手順、配布手順、VSTO 背景説明などの補助文書です。

## 現在の実装方針

- task pane は Excel ウィンドウごとに `HWND` で管理しています。
- 現在のウィンドウに紐づく Workbook が変わった場合は、必要に応じて `WorkbookHandler` を差し替えます。
- 存在しなくなった Excel ウィンドウに紐づく pane は自動的に掃除します。
- pane 掃除時に一時的に取得した `Excel.Window` / `Excel.Windows` は明示解放し、Excel プロセスが残り続けるリスクを下げています。

## 必要環境

- Windows
- デスクトップ版 Microsoft Excel
- Office / VSTO 開発ツール入りの Visual Studio
- .NET Framework 4.8.1 Targeting Pack
- NuGet 復元が可能な環境

使用している主なパッケージ:

- `CommunityToolkit.Mvvm`
- `Microsoft.Xaml.Behaviors.Wpf`

## ビルド上の注意

このリポジトリは `dotnet build` だけでは完結しません。

`ExcelVerticalTab` プロジェクトでは次の VSTO ターゲットを読み込みます。

- `Microsoft.VisualStudio.Tools.Office.targets`

そのため、想定する開発環境は Windows 上の Visual Studio + Office 開発ワークロードです。

## 署名に関する注意

VSTO プロジェクトでは現在、次の証明書ファイルを参照しています。

- `PPCustomZoom_TemporaryKey.pfx`

この証明書がローカルに存在しない場合、署名や発行まわりで失敗する可能性があります。ローカル開発では、同等のテスト証明書を用意するか、Visual Studio 側で署名設定を調整してください。

## Visual Studio での開き方

1. `ExcelVerticalTab.sln` を開きます。
2. NuGet パッケージを復元します。
3. Office / VSTO 開発ワークロードが入っていることを確認します。
4. 実行対象の Excel がインストールされていることを確認します。
5. Visual Studio からソリューションをビルドします。

## デバッグ

このプロジェクトは Excel VSTO アドインとして構成されています。Visual Studio 側の環境が正しく整っていれば、デバッグ開始時に Excel が起動し、アドインが読み込まれます。

## 推奨手動スモークテスト

1. ブックを開き、左側に縦タブ pane が表示されることを確認する
2. pane からシートを選び、Excel 側で正しいシートがアクティブになることを確認する
3. シート名の一部でフィルタし、一覧が絞り込まれることを確認する
4. シートを上方向・下方向へドラッグし、Excel 上の順序と UI 上の順序が一致することを確認する
5. 複数の Excel ウィンドウを開き、それぞれの pane 状態が正しく保たれることを確認する
6. `名前を付けて保存` 後も pane が正しく追従することを確認する
7. Excel ウィンドウを閉じたあと、不要な pane が残らないことを確認する

## 現時点の制約

- 実ビルド確認は Visual Studio + VSTO 環境で行う必要があります。
- 証明書や Office 開発環境の完全自動セットアップ手順はまだありません。
- `VerticalTabControl` 側にはユニット テストがありますが、VSTO 本体の最終確認は引き続き手動中心です。

## 補足

調査メモやレビュー補助資料は、必要に応じてローカル作業用の `CODEX/` フォルダに蓄積しています。ただし、セットアップや運用の正本はこの README を基準にしてください。

インストール手順は [INSTALL.md](./docs/INSTALL.md) を参照してください。  
配布手順は [DISTRIBUTION.md](./docs/DISTRIBUTION.md) を参照してください。
VSTO / Publish / 署名の背景説明は [VSTO_EXPLANATION.md](./docs/VSTO_EXPLANATION.md) を参照してください。
