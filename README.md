# BIDSSMemLib
BIDS用SharedMemoryを簡単に扱うためのLibrary

Standard Libraryを除いて.NET4.7.2を指定しています。

## BIDSSMemLib Project
BIDSSMemLibシリーズに実装されているすべての機能が含まれています。

一応、このLibraryは.Net Frameworkを使用して開発されているすべてのソフトウェアから参照利用をできますが、通常はBIDSSMemLib.readerを使用してください。

## BIDSSMemLib.Standard Project
.Net Standardや.Net Coreで開発されているソフトウェアから参照利用するためのライブラリです。

## BIDSSMemLib.bve5 Project
BVE5のATSプラグイン専用のプロジェクトです。DllExportを使用し、ATSプラグインとして振舞います。  
生成物にはDllExportのライセンス(MIT License)も発生します。ご注意ください。

一応ATSプラグイン以外の用途でも使用できますが、お勧めはしません。

## BIDSSMemLib.obve Project
openBVEのInput Device Plugin専用のプロジェクトです。OpenBveApiを参照しております。  
openBVE側のInputDevicePlugin周りの実装の都合上、Panel情報とSound情報を取得することができません。これに関しては、そのうち対応します。

SMemLibからイベントなどを取っ払っています。

## BIDSSMemLib.reader Project
ROM専用のLibraryです。Write機能は実装されていません。Displaying Softwareを開発する際は、できるだけこのLibraryを使用するようにしてください。
