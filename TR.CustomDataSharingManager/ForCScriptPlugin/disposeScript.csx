// disposeScript.csx
// プラグインのリソース解放を要求された際に実行されるスクリプト
// Copyright 2021 Tetsu Otter
// License : The MIT License

// パス指定は 絶対パス OR 実行ファイルからの相対パス
// => スクリプトファイルからの相対パスにしたい
#r "bin\TR.CustomDataSharingManager.dll"

// 定数定義ファイルを読み込みます
#load "constants.csx"

using TR;

if (ObjectHolder.TryGetValue(CustomDataSharing, out object obj) && obj is CustomDataSharingManager cdsManager)
{
	//インスタンスが存在したら解放する
	cdsManager.Dispose();

	//DictionaryからKeyValuePairを削除する (念のため)
	ObjectHolder.Remove(CustomDataSharing);
}