#pragma once

#define WIN32_LEAN_AND_MEAN             // Windows ヘッダーからほとんど使用されていない部分を除外する
// Windows ヘッダー ファイル
#include <windows.h>
#define KEY_ARRMAXINDEX 128
struct Hands
{
	int B;
	int P;
	int R;
	int S;
	double BPos;
	double PPos;
};
struct KeyD
{
	bool Keys[KEY_ARRMAXINDEX];
};
extern "C"
{
	__declspec(dllexport) void Initialize(const char*, const char*);
	__declspec(dllexport) Hands GetHandD();
	__declspec(dllexport) KeyD* GetKeyD();
	__declspec(dllexport) void Dispose();
}