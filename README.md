# BIDSSMemLib
BIDS用SharedMemoryを簡単に扱うためのLibrary群


## BIDSDataChecker
BIDSのSharedMemoryで共有された情報を表示させます.


## BIDSSMemInputTester
BIDSの入力部分のテストを行うためのコンソールアプリケーションです.


## BIDSSMemLib.bve5 Project
BVE5向けのプラグインとしてのライブラリプロジェクトです。  
生成物にはDllExportのライセンス(MIT License)も発生します。ご注意ください。  
ATSプラグイン, 入力デバイスプラグインともにこのプロジェクトで対応します.
### 機能対応表
実装している機能の一覧です.  実際に利用できるかどうかは確認しておりません.
|機能|対応状況|備考|
|-|-|-|
|車両状態共有エリアの操作|〇||
|操作共有エリアの操作|〇||
|BVE5 ATSPI|〇||
|BVE5 InputDevice|〇||
|openBVE ATSPI|×||
|openBVE InputDevice|×||

なお, 車両状態共有エリアの操作については, 車両状態変化を通知するイベントも付属します.


## BIDSSMemLib.obve Project
openBVE向けのプロジェクトです。OpenBveApiを参照しております。  
openBVE側のInputDevicePlugin周りの実装の都合上、Panel情報とSound情報を取得することができません。これに関しては、保安装置プラグイン機能を実装することにより対応予定です.
### 機能対応表
実装している機能の一覧です.  実際に利用できるかどうかは確認しておりません.
|機能|対応状況|備考|
|-|-|-|
|車両状態共有エリアの操作|〇||
|操作共有エリアの操作|〇||
|BVE5 ATSPI|×||
|BVE5 InputDevice|×||
|openBVE ATSPI|×|実装予定|
|openBVE InputDevice|〇||

なお, 車両状態共有エリアの操作については, 車両状態変化を通知するイベントも付属します.


## BIDSSMemLib.rw Project
BIDSの共有メモリを操作するためのライブラリです.  ソフトウェアを開発する際は, 通常はこのライブラリを使用します.
### 機能対応表
実装している機能の一覧です.  実際に利用できるかどうかは確認しておりません.
|機能|対応状況|備考|
|-|-|-|
|車両状態共有エリアの操作|〇||
|操作共有エリアの操作|〇||
|BVE5 ATSPI|×||
|BVE5 InputDevice|×||
|openBVE ATSPI|×||
|openBVE InputDevice|×||

なお, 車両状態共有エリアの操作については, 車両状態変化を通知するイベントも付属します.
### 依存関係
本ライブラリは, 明示的に以下のライブラリを参照利用しております.
- BIDSSMemLib.structs
- TR.SMemCtrler
- TR.SMemIF


## BIDSSMemLib.structs
BIDSで使用する構造体を定義しています.  機能は実装されておりませんので, 必要に応じて他のライブラリと併せて使用してください.


## TR.SMemCtrler
任意の構造体データ, および任意の構造体配列を管理します.  必要に応じてSMemIFを使用して共有メモリに書き込むほか, 値の更新を通知するイベントも実装しています.
### 依存関係
本ライブラリは, TR.SMemIFを参照利用しています.


## TR.SMemIF
指定の名前を持つ共有メモリを操作し, 情報の読込および書き込みを実行します.  必要に応じて, キャパシティの拡張も行います.


## 削除されたプロジェクト一覧
重複する機能の整理に伴い, 以下のプロジェクトを削除しております.  
なお, こちらの表では, "BIDSSMemLib"という文字列を"BSML"と省略して記載させていただいております.
|プロジェクト名|代替|削除commit|
|-|-|-|
|BIDSSMemLib|各プロジェクト|[commit 0fd148f57e714a940ed0d073815b16ddee3b1e33](/TetsuOtter/BIDSSMemLib/commit/0fd148f57e714a940ed0d073815b16ddee3b1e33)|
|BSML.bve5id|BSML.bve5|[commit 85f21eabbead68b41203d94bf6ccee6a667197c2](/TetsuOtter/BIDSSMemLib/commit/85f21eabbead68b41203d94bf6ccee6a667197c2)|
|BSML.ctrler|BSML.rw|[commit 85f21eabbead68b41203d94bf6ccee6a667197c2](/TetsuOtter/BIDSSMemLib/commit/85f21eabbead68b41203d94bf6ccee6a667197c2)|
|BSML.CtrlIOcs|BSML.rw|[commit 85f21eabbead68b41203d94bf6ccee6a667197c2](/TetsuOtter/BIDSSMemLib/commit/85f21eabbead68b41203d94bf6ccee6a667197c2)|
|BSML.CtrlIOpp|(検討中)|[commit 85f21eabbead68b41203d94bf6ccee6a667197c2](/TetsuOtter/BIDSSMemLib/commit/85f21eabbead68b41203d94bf6ccee6a667197c2)|
|BSML.local|BSML.rw|[commit 85f21eabbead68b41203d94bf6ccee6a667197c2](/TetsuOtter/BIDSSMemLib/commit/85f21eabbead68b41203d94bf6ccee6a667197c2)|
|BSML.reader|BSML.rw|[commit 85f21eabbead68b41203d94bf6ccee6a667197c2](/TetsuOtter/BIDSSMemLib/commit/85f21eabbead68b41203d94bf6ccee6a667197c2)|
|BSML.Standard|BSML.rw|[commit 85f21eabbead68b41203d94bf6ccee6a667197c2](/TetsuOtter/BIDSSMemLib/commit/85f21eabbead68b41203d94bf6ccee6a667197c2)|

