// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "pch.h"
#include <string>
BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

HANDLE MMFCtrlHHandle = NULL;
HANDLE MMFCtrlKHandle = NULL;
Hand* MMFViewHHandle = NULL;
KeyD* MMFViewKHandle = NULL;
const Hand brankhd;
const KeyD brankkd;
void Initialize(const char* MMFCtrlKName, const char* MMFCtrlHName) {
	MMFCtrlHHandle = OpenFileMapping(FILE_MAP_ALL_ACCESS, false, (LPCSTR)MMFCtrlHName);
	if (MMFCtrlHHandle == NULL)
		MMFCtrlHHandle = CreateFileMapping((HANDLE)0xFFFFFFFF, NULL, PAGE_READWRITE, 0, sizeof(Hand), (LPCSTR)MMFCtrlHName);
	MMFCtrlKHandle = OpenFileMapping(FILE_MAP_ALL_ACCESS, false, (LPCSTR)MMFCtrlKName);
	if (MMFCtrlKHandle == NULL)
		MMFCtrlKHandle = CreateFileMapping((HANDLE)0xFFFFFFFF, NULL, PAGE_READWRITE, 0, sizeof(KeyD), (LPCSTR)MMFCtrlKName);

	if (MMFCtrlHHandle != NULL) MMFViewHHandle = (Hand*)MapViewOfFile(MMFCtrlHHandle, SECTION_MAP_READ, 0, 0, 0);
	else
	{
		MessageBox(NULL, (LPCSTR)L"Could not open the Memory Mapped File of HandD.", (LPCSTR)L"BIDSSMemLib Ctrl Reader", 0);
	}
	if (MMFCtrlKHandle != NULL) MMFViewKHandle = (KeyD*)MapViewOfFile(MMFCtrlKHandle, SECTION_MAP_READ, 0, 0, 0);
	else
	{
		MessageBox(NULL, (LPCSTR)L"Could not open the Memory Mapped File of KeyD.", (LPCSTR)L"BIDSSMemLib Ctrl Reader", 0);
	}
}

Hand GetHandD() {
	if (MMFViewHHandle != NULL) return (Hand)(*MMFViewHHandle);
	else return brankhd;
}
/*
KeyD GetKeyD() {
	if (MMFViewKHandle != NULL) return (KeyD)(*MMFViewKHandle);
	else return brankkd;
}
*/

KeyD* GetKeyD() {
	return MMFViewKHandle;
}
void Dispose() {
	UnmapViewOfFile(MMFViewHHandle);
	UnmapViewOfFile(MMFViewKHandle);
	CloseHandle(MMFCtrlHHandle);
	CloseHandle(MMFCtrlKHandle);
}