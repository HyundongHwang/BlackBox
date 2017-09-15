#include "stdafx.h"
#include "MainWnd.h"
#include "BlackBoxTestSecureKeys.h"

MainWnd* MainWnd::s_pCurrent = NULL;

MainWnd::MainWnd()
{
}


MainWnd::~MainWnd()
{
}

LRESULT MainWnd::_On_WM_CREATE(UINT nMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
    USES_CONVERSION;

    auto x = 0;
    auto y = 0;
    auto w = 500;
    auto h = 30;
    auto idButtonStart = 1000;
    auto idButton = idButtonStart;

    m_mapButtonLabel.SetAt(idButton++, L"register .NET dll (BlackBoxLib.dll)");
    m_mapButtonLabel.SetAt(idButton++, L"init BlackBox");
    m_mapButtonLabel.SetAt(idButton++, L"hello");
    m_mapButtonLabel.SetAt(idButton++, L"world");
    m_mapButtonLabel.SetAt(idButton++, L"session");
    m_mapButtonLabel.SetAt(idButton++, L"capture screen");

    for (auto i = idButtonStart; i < idButton; i++)
    {
        CWindow wndBtn;
        wndBtn.Create(L"Button", this->m_hWnd, CRect(x, y, x + w, y + h), m_mapButtonLabel[i], WS_CHILD | WS_VISIBLE | BS_LEFT, NULL, i);
        y += h;
    }

    return 0;
}

LRESULT MainWnd::_On_WM_DESTROY(UINT nMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
    ::CoUninitialize();
    ::PostQuitMessage(0);
    return 0;
}

LRESULT MainWnd::_On_WM_SIZE(UINT nMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
    CRect rcClient;
    ::GetClientRect(m_hWnd, &rcClient);
    return 0;
}

LRESULT MainWnd::_On_ID_BTN(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
    auto nBtnId = wID;
    auto strBtnLabel = m_mapButtonLabel[nBtnId];

    if (strBtnLabel == L"register .NET dll (BlackBoxLib.dll)")
    {
        wchar_t wBlackBoxDllPath[MAX_PATH] = { 0, };
        ::GetModuleFileName(NULL, wBlackBoxDllPath, MAX_PATH);
        ::PathRemoveFileSpec(wBlackBoxDllPath);
        ::PathCombine(wBlackBoxDllPath, wBlackBoxDllPath, L"BlackBoxLib.dll");

        HANDLE child = ::ShellExecute(this->m_hWnd, L"runas", L"C:\\windows\\Microsoft.NET\\Framework\\v4.0.30319\\RegAsm.exe", wBlackBoxDllPath, 0, SW_SHOWNORMAL);
        ::WaitForSingleObject(child, INFINITE);
    }
    else if (strBtnLabel == L"init BlackBox")
    {
        // Initialize COM.
        ::CoInitialize(NULL);

        // Create the interface pointer.
        m_pBlackBox.CreateInstance(__uuidof(BlackBoxImpl));

        if (m_pBlackBox == NULL)
        {
            {
                CString strMsg;
                strMsg.Format(L"BlackBox init failed!!!");
                ::MessageBox(NULL, strMsg, NULL, MB_OK);
            }

            return 0;
        }
        else
        {
            {
                CString strMsg;
                strMsg.Format(L"BlackBox init success!!! m_pBlackBox : 0x%x", m_pBlackBox);
                ::MessageBox(NULL, strMsg, NULL, MB_OK);
            }
        }

        auto bstrInit = CString(AZURE_STORAGE_CONNECTION).AllocSysString();
        m_pBlackBox->Init(bstrInit);
        ::SysReleaseString(bstrInit);
    }
    else if (strBtnLabel == L"hello")
    {
        auto bstrMsg = CString(L"hello").AllocSysString();
        m_pBlackBox->d(bstrMsg);
        ::SysReleaseString(bstrMsg);
    }
    else if (strBtnLabel == L"world")
    {
        auto bstrMsg = CString(L"world").AllocSysString();
        m_pBlackBox->i(bstrMsg);
        ::SysReleaseString(bstrMsg);
    }
    else if (strBtnLabel == L"session")
    {
        auto bstrMsg = CString(L"h2d2002@naver.com from win32").AllocSysString();
        m_pBlackBox->session(bstrMsg);
        ::SysReleaseString(bstrMsg);
    }
    else if (strBtnLabel == L"capture screen")
    {
        m_pBlackBox->CaptureScreen();
    }

    return 0;
}