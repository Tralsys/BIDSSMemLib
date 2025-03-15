# BIDSSMemLib BveEX Extension Plugin

zip 解凍前にブロック解除を行ったうえで、`C:\Users\Public\Documents\BveEx\2.0\Extensions` に、`TR.BIDSSMemLib.BveEX.dll` と`TR.BIDSSMemLib.BveEX.deps` を配置してください。

BIDSSMemLib の ATS プラグイン版や入力デバイスプラグイン版と競合せずに動作しますが、基本的にはどちらも削除したうえでご利用ください。

現時点では、以下の機能が未実装です。

- キー入力機能
- カント高さ (高さではなく角度で入っている)
- 勾配情報

## ライセンスについて

このプラグインは、BveEX、Microsoft 提供の.NET ライブラリ群以外に、以下のライブラリを使用しています。
これらのライブラリには、MIT ライセンス以外のライセンスが適用される場合があります。

- SIPSorcery (BSD 3-Clause License)
  - https://github.com/sipsorcery-org/sipsorcery/blob/master/LICENSE.md

## 更新履歴

- 1.0.0 新規公開
- 1.1.0 BveEX 対応
- 2.0.0-alpha1 BIDS-RTC 対応
