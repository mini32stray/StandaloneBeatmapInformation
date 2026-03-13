# StandaloneBeatmapInformation

プレイ中のマップ情報を表示します。

## 概要
これ↓

![overview1](docs/overview1.jpg)

- プレイ時にマップ情報を表示するパネルを空間上に置けます。
- 曲開始時にインターネットに情報を取りに行きません。
- Diffs(難易度)やCharacteristics(Normal/360°/Lawlessなど)は、プレイ中のものだけではなく、選択可能なものも全部表示されます。
- DiffsやCharacteristicsの表記はゲーム内と同じになります。
- ランクマップの場合は星を表示できます。
- プレイ時のJD値を表示できます。
- `IndependentBsr` と一緒に使うと、リクエスト受付状態、リクエスタの名前を表示できます。
- `CameraPlusMovementScriptBox` と一緒に使うと、カメラスクリプトの作者を表示できます。(v0.2.1から)

  ![overview2](docs/overview2.jpg)


## 動作環境
ゲーム本体の対応バージョンと依存MODは [manifest.json](StandaloneBeatmapInformation/manifest.json) を見てください。

特に以下のMODはよく確認しましょう。
- BS Utils
- SongDetailsCache

## インストール
`StandaloneBeatmapInformation.dll` を `Plugins` フォルダに置くだけ。

