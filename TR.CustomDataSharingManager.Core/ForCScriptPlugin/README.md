# TR.ATSPI.CScript で TR.CustomDataSharingManager を使用するためのスクリプト群

## 使用方法
車両に `TR.ATSPI.CScript.dll` (C#スクリプトプラグイン)をインストールしたうえで, `TR.ATSPI.CScript.xml` の `ScriptFileLists` タグ内に `TR.CustomDataSharingManager.xml` へのパスを追加してください.

例として, 次のようなファイル配置であったと想定します.

```
\TR.ATSPI.CScript.dll
\TR.ATSPI.CScript.xml
\TR.CustomDataSharingManager\constants.csx
\TR.CustomDataSharingManager\disposeScript.csx
\TR.CustomDataSharingManager\bin\TR.CustomDataSharingManager.ScriptList.xml
(他は省略)
```

この場合, 他にスクリプトを追加していない状態の `TR.ATSPI.CScript.xml` は次のようになります.

```
<?xml version="1.0" encoding="utf-8"?>
<ScriptPathListClass xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<ScriptFileLists>
		<string>TR.CustomDataSharingManager\TR.CustomDataSharingManager.xml</string>
	</ScriptFileLists>
	<LoadScripts />
	<DisposeScripts />
	<SetVehicleSpecScripts />
	<InitializeScripts />
	<ElapseScripts/>
	<SetPowerScripts />
	<SetBrakeScripts />
	<SetReverserScripts />
	<KeyDownScripts />
	<KeyUpScripts />
	<HornBlowScripts />
	<DoorOpenScripts />
	<DoorCloseScripts />
	<SetSignalScripts />
	<SetBeaconDataScripts />
	<GetPluginVersionScripts />
</ScriptPathListClass>
```

## ライセンス
本ファイル群は, MITライセンスの下で自由に使用することができます.

## スクリプトから CustomDataShareingManager を使用してデータを共有する
(準備中)