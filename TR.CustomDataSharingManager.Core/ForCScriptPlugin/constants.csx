// constants.csx
// 各スクリプトで共有する定数を定義します
// 使用するには, #load でパスを指定してください
// Copyright 2021 Tetsu Otter
// License : The MIT License

// パス指定は 絶対パス OR 実行ファイルからの相対パス
// => スクリプトファイルからの相対パスにしたい
#r "System.Collections"
// #r "TR.CustomDataSharingManager.Core.dll"
#r "TR.BIDSSMemLib.Variable.dll"

using TR;

T GetValue<T>(in string SMemName) where T : new()
{
    return DataSharingManager.CreateDataSharing<T>(SMemName).Read();
}

bool TryGetValue<T>(in string SMemName, out T dst) where T : new()
{
    return DataSharingManager.CreateDataSharing<T>(SMemName).TryRead(out dst);
}

void SetValue<T>(in string SMemName, in T value) where T : new()
{
    DataSharingManager.CreateDataSharing<T>(SMemName).Write(in value);
}
