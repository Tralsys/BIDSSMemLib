#pragma once

#define WIN32_LEAN_AND_MEAN             // Windows ヘッダーからほとんど使用されていない部分を除外する
// Windows ヘッダー ファイル
#include <windows.h>
#define KEY_ARRMAXINDEX 128
struct Hand
{
	int P;
	int B;
	int R;
	int C;
};
struct KeyD
{
	bool Keys[KEY_ARRMAXINDEX];
};
extern "C"
{
	__declspec(dllexport) void Initialize(const char*, const char*);
	__declspec(dllexport) Hand GetHandD();
	__declspec(dllexport) KeyD* GetKeyD();
	__declspec(dllexport) void Dispose();
}