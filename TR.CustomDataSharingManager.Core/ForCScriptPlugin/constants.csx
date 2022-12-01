// constants.csx
// 各スクリプトで共有する定数を定義します
// 使用するには, #load でパスを指定してください
// Copyright 2021 Tetsu Otter
// License : The MIT License

// パス指定は 絶対パス OR 実行ファイルからの相対パス
// => スクリプトファイルからの相対パスにしたい
#r "System.Collections"
#r "bin\TR.CustomDataSharingManager.dll"
#r "bin\TR.SMemCtrler.dll"

using TR;
using System.Collections.Generic;

const string CustomDataSharing = nameof(CustomDataSharing);

T GetValue<T>(Dictionary<string, object> objectHolder, in string SMemName) where T : new()
{
	if (!objectHolder.TryGetValue(CustomDataSharing, out object obj) || obj is null)
	{
		//キーが存在しなければ新規に作成する
		obj = new CustomDataSharingManager();
		objectHolder[CustomDataSharing] = obj;
	}

	if (obj is CustomDataSharingManager manager)
		return manager.CreateDataSharing<T>(SMemName).Read();
	else
		return default;
}

bool TryGetValue<T>(Dictionary<string, object> objectHolder, in string SMemName, out T dst) where T : new()
{
	if (!objectHolder.TryGetValue(CustomDataSharing, out object obj) || obj is null)
	{
		//キーが存在しなければ新規に作成する
		obj = new CustomDataSharingManager();
		objectHolder[CustomDataSharing] = obj;
	}

	if (obj is CustomDataSharingManager manager)
		return manager.CreateDataSharing<T>(SMemName).TryRead(out dst);
	else
		return default;
}

void SetValue<T>(Dictionary<string, object> objectHolder, in string SMemName, in T value) where T : new()
{
	if (!objectHolder.TryGetValue(CustomDataSharing, out object obj) || obj is null)
	{
		//キーが存在しなければ新規に作成する
		obj = new CustomDataSharingManager();
		objectHolder[CustomDataSharing] = value;
	}

	if (obj is CustomDataSharingManager manager)
		manager.CreateDataSharing<T>(SMemName).Write(value);
}
