// BlackBoxTest.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "BlackBoxTest.h"
#include "MainWnd.h"

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_ LPWSTR    lpCmdLine,
    _In_ int       nCmdShow)
{
    MainWnd mainWnd;
    int width = 1000;
    int height = 800;
    int cx = ::GetSystemMetrics(SM_CXSCREEN);
    int cy = GetSystemMetrics(SM_CYSCREEN);
    int left = (cx - width) / 2;
    int top = (cy - height) / 2;
    mainWnd.Create(NULL, CRect(left, top, left + width, top + height), L"BlackBoxTest", WS_VISIBLE | WS_OVERLAPPEDWINDOW | WS_CLIPSIBLINGS | WS_CLIPCHILDREN);

    MSG msg = { 0, };

    while (::GetMessage(&msg, NULL, 0, 0))
    {
        ::TranslateMessage(&msg);
        ::DispatchMessage(&msg);
    }

    return (int)msg.wParam;
}