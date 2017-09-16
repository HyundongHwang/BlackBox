<!-- TOC -->

- [BlackBox Intro](#blackbox-intro)
    - [Feature Introduction](#feature-introduction)
    - [Good Scenario](#good-scenario)
    - [storage naming rules](#storage-naming-rules)
- [BlackBox realtime log, screencapture demos](#blackbox-realtime-log-screencapture-demos)
- [Azure Storage Access Key Acquisition](#azure-storage-access-key-acquisition)
- [BlackBox for Android](#blackbox-for-android)
    - [jitpack setting](#jitpack-setting)
    - [Initialize](#initialize)
    - [Write log, session, screen capture](#write-log-session-screen-capture)
    - [Download sample apk](#download-sample-apk)
- [BlackBox for .NET](#blackbox-for-net)
    - [nuget setting](#nuget-setting)
    - [initialize](#initialize)
    - [Write log, session, screen capture](#write-log-session-screen-capture-1)
    - [Download sample exe](#download-sample-exe)
- [BlackBox for Win32](#blackbox-for-win32)
    - [Registering .NET dll system](#registering-net-dll-system)
    - [import tlb file](#import-tlb-file)
    - [Initialize](#initialize-1)
    - [Write log, session, screen capture](#write-log-session-screen-capture-2)
    - [Download sample exe](#download-sample-exe-1)
- [BlackBox PowerShell Admin](#blackbox-powershell-admin)
    - [Install from Powershell Gallery](#install-from-powershell-gallery)
    - [Initialize](#initialize-2)
    - [Search the log table](#search-the-log-table)
    - [Log Search](#log-search)
    - [Delete log table](#delete-log-table)
    - [Session Search](#session-search)
    - [Screen Capture Search](#screen-capture-search)
    - [Delete screen capture](#delete-screen-capture)
- [BlackBox development plan (the person who will help is very welcome)](#blackbox-development-plan-the-person-who-will-help-is-very-welcome)

<!-- /TOC -->

<br>
<br>
<br>

![](https://raw.githubusercontent.com/HyundongHwang/BlackBox/master/blackbox-icon.jpg)

[한국어로 보기](/README-ko.md)

# BlackBox Intro
It was a log module that I used to make it for my company and was useful for several months. <br>
By creating a single Azure Storage instance and registering your key, you can easily do text logging and screen capture in real time on Android, .NET, Win32 apps. <br>
PullRequest is welcome because there are many deficiencies. <br>
If you have any questions, please leave a message. <br>


## Feature Introduction
- Collect log text, screen capture images, and collect them into the cloud storage every 10 seconds.
- Using Azure Storage with CloudStorage and so it needs its AccessKey.
- Provided in SDK form for easy use on Android, .NET, Win32 client.
     - Android: Available as jitpack grade
     - .NET: provided by package in nuget
     - Win32: nuget provides package, import attached tlb and use it as COM
- PowerShell supports log search/delete, session search, screen capture search/delete.
     - Provided as a module in PowerShell Gallery

## Good Scenario
- When data flow is complicated in Client App, you have to find important clues in it.
    - When the process of finding this clue occurs occasionally during operation and needs to be grasped in real time.
- When business itself is more important than personal information protection like in-house service.
    - `(Note !!!) Logging or screen capture of personal information without explicit user consent is illegal.`
- When to use log text for various big data analysis.
    - It manages log text in NOSQL DB. Naturally, it is more normalized and accessibility than analyzing text file on server.
- I am building a large number of log texts at the same time,
    - With Azure Storage, you can use the REST API to query, write, and delete the use of this repository, so there is no need to create and manage your own log server.
    - Will not Azure Storage's servers handle much traffic?
- When the capacity of the log text is too large to cause storage expansion issues, which wastes time and money.
    - Azure Storage is cloud-based, so the capacity is of course unlimited
    - Assuming you use about 10gb per month, you will be charged less than 1000 won.
    
## storage naming rules
- log text
    - It is managed by NOSQL DB using Azure Storage Table.
    - Generate one table per client app.
    - `log {DEVICE-ID} date {yyMMdd}` creates a table for every date per device.
        - search/delete supported by device by date

```powershell
CloudTable                                           

----------                                           
log57C7B14B1932357EED39FED1CA8409B11E658973date170913
log57C7B14B1932357EED39FED1CA8409B11E658973date170914
log71384680ffe50ddadate170915                        
logD781889DEC1C6F8B177CEA5E11B0FC220D492A6Ddate170914
logD781889DEC1C6F8B177CEA5E11B0FC220D492A6Ddate170915
logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170913
```

- session
    - Also managed by NOSQL DB using Azure Storage Table.
    - It matches client app's hardware ID and session information
        - When log analysis is needed, the session function is also needed because it is searched by session information such as service ID rather than hardware ID.

```powershell
RowKey                                   SessionStr
------                                   ----------
57C7B14B1932357EED39FED1CA8409B11E658973 h2d2002@naver.com from win32
71384680ffe50dda                         h2d2002@naver.com from android
D781889DEC1C6F8B177CEA5E11B0FC220D492A6D h2d2002@naver.com from win32
DC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79 h2d2002@naver.com from win32
```

- Screen capture
    - Azure Storage Blob allows you to manage and search/delete files.
    - It manages folders by client by date.
        - `{DEVICE-ID} / {yyMMdd} / blackbox-screencapture- {HHmmss} - {RANDOM-GUID} .png`
    - Random GUID was appended to the url. And it makes that it is difficult to hack through url prediction. (Instead the url is a little bit long :( )

<br>
<br>
<br>

# BlackBox realtime log, screencapture demos

- Android App
    - https://www.youtube.com/watch?v=HhdLYEhU-Zc

- .NET App
    - https://www.youtube.com/watch?v=VNZTxJAMqn8

- Win32 App
    - https://www.youtube.com/watch?v=SHLa6vcTgVk

<br>
<br>
<br>

# Azure Storage Access Key Acquisition

- https://hyundonghwang.github.io/2017/09/15/Azure-Storage-Service-%EC%9D%B8%EC%8A%A4%ED%84%B4%EC%8A%A4-%EB%A7%8C%EB%93%A4%EA%B8%B0/
- If you want to use the account key included in the sample app for testing, please contact me separately ~ (hhd2002@kakaotalk or h2d2002@facebook)

<br>
<br>
<br>

# BlackBox for Android

## jitpack setting
- https://jitpack.io/#HyundongHwang/BlackBox
- Add `jitpack.io` repository to the root's `/build.gradle` file

```groovy
allprojects {
    repositories {
        ...
        maven { url 'https://jitpack.io' }
    }
}
```

- Add dependencies to the modules you want to use
    - ex) `/app/build.gradle`

```groovy
dependencies {
    compile 'com.github.HyundongHwang:BlackBox:1.3.2'
}
```

## Initialize
- Initialized using Azure Storage Access Key.

```java
public class MainApplication extends MultiDexApplication {
    @Override
    public void onCreate() {
        super.onCreate();
        BlackBox.init(this, getString("DefaultEndpointsProtocol=https;AccountName=blackboxtest;AccountKey=/gnaCzk...8lr3w==;EndpointSuffix=core.windows.net"));
    }
}
```


## Write log, session, screen capture

```java
public class MainActivity extends AppCompatActivity {
...
    @OnClick(R.id.btn_hello)
    public void onBtnHelloClicked() {
        BlackBox.i("hello");
    }

    @OnClick(R.id.btn_world)
    public void onBtnWorldClicked() {
        BlackBox.d("world");
    }

    @OnClick(R.id.btn_save_session)
    public void onbtnSaveSessionClicked() {
        BlackBox.session("h2d2002@naver.com from android");
    }

    @OnClick(R.id.btn_screen_capture)
    public void onbtnScreenCaptureClicked() {
        BlackBox.captureScreen(this);
    }
}
```

## Download sample apk
- https://github.com/HyundongHwang/BlackBox/blob/master/Android/BlackboxTest-170915-0200.apk

<br>
<br>
<br>

# BlackBox for .NET
- https://www.nuget.org/packages/hhd2002.BlackBox

## nuget setting

```powershell
PM> Install-Package hhd2002.BlackBox -Version 1.3.0
```

## initialize

```c#
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        BlackBoxLib.BlackBox.Init("DefaultEndpointsProtocol=https;AccountName=blackboxtest;AccountKey=/gnaCzkiwEzwq3S9JuC...Cm8lr3w==;EndpointSuffix=core.windows.net");
    }
}
```

## Write log, session, screen capture

```c#
public partial class MainWindow : Window
{
    private void BtnHello_Click(object sender, RoutedEventArgs e)
    {
        BlackBox.d("hello");
    }

    private void BtnWorld_Click(object sender, RoutedEventArgs e)
    {
        BlackBox.i("world");
    }

    private void BtnSession_Click(object sender, RoutedEventArgs e)
    {
        BlackBox.session("h2d2002@naver.com from .NET");
    }

    private void BtnCaptureScreen_Click(object sender, RoutedEventArgs e)
    {
        BlackBox.CaptureScreen();
    }
}
```

## Download sample exe

- https://github.com/HyundongHwang/BlackBox/blob/master/DotNet/Scripts/BlackBoxTestDeploy-1709141827.zip

<br>
<br>
<br>

# BlackBox for Win32

## Registering .NET dll system
- .NET dll will be called as COM interface.
- This is usually done in the installer, but in this case it's a test, so I wrote it in c ++ code.

```c++
wchar_t wBlackBoxDllPath[MAX_PATH] = { 0, };
::GetModuleFileName(NULL, wBlackBoxDllPath, MAX_PATH);
::PathRemoveFileSpec(wBlackBoxDllPath);
::PathCombine(wBlackBoxDllPath, wBlackBoxDllPath, L"BlackBoxLib.dll");

HANDLE child = ::ShellExecute(this->m_hWnd, L"runas", L"C:\\windows\\Microsoft.NET\\Framework\\v4.0.30319\\RegAsm.exe", wBlackBoxDllPath, 0, SW_SHOWNORMAL);
::WaitForSingleObject(child, INFINITE);
```

## import tlb file
- .NET DLL is COM exported in advance, and you can create tlb file. When you import, tlh file is generated when compiling, so it can be used as strong type in c ++ code.
- If you do not understand me, download the already created file from the link below and just write #import syntax.
    - https://github.com/HyundongHwang/BlackBox/raw/master/Win32/BlackBoxTest/BlackBoxLib.tlb
- `stdafx.h`

```c++
#import "BlackBoxLib.tlb" raw_interfaces_only
using namespace BlackBoxLib;
```

## Initialize

```c++
IBlackBoxPtr m_pBlackBox;

::CoInitialize(NULL);
m_pBlackBox.CreateInstance(__uuidof(BlackBoxImpl));

if (m_pBlackBox == NULL)
    return 0;

auto bstrInit = CString(L"DefaultEndpointsProtocol=https;AccountName=blackboxtest;AccountKey=/gnaCzkiwEzwq3S9JuC...Cm8lr3w==;EndpointSuffix=core.windows.net").AllocSysString();
m_pBlackBox->Init(bstrInit);
::SysReleaseString(bstrInit);
```

## Write log, session, screen capture

```c++
if (strBtnLabel == L"hello")
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
```

## Download sample exe

- https://github.com/HyundongHwang/BlackBox/blob/master/Win32/Scripts/BlackBoxTestDeploy-1709141128.zip

<br>
<br>
<br>

# BlackBox PowerShell Admin

## Install from Powershell Gallery

- https://www.powershellgallery.com/packages/blackbox

```powershell
PS> Install-Module -Name blackbox

PS> gcm blackbox*
CommandType     Name                                               Version    Source
-----------     ----                                               -------    ------
Function        blackbox-get-log                                   1.3.2      blackbox
Function        blackbox-get-screencapture                         1.3.2      blackbox
Function        blackbox-get-session                               1.3.2      blackbox
Function        blackbox-get-table-list                            1.3.2      blackbox
Function        blackbox-init                                      1.3.2      blackbox
Function        blackbox-init-check                                1.3.2      blackbox
Function        blackbox-module-install-import                     1.3.2      blackbox
Function        blackbox-remove-screencapture                      1.3.2      blackbox
Function        blackbox-remove-table                              1.3.2      blackbox
```

## Initialize

```powershell
PS> $STORAGE_ACCOUNT_NAME = "blackboxtest"
>> $STORAGE_ACCOUNT_KEY = "/gnaCzkiw...ced97OMqCm8lr3w=="
>>
>> blackbox-init -STORAGE_ACCOUNT_NAME $STORAGE_ACCOUNT_NAME -STORAGE_ACCOUNT_KEY $STORAGE_ACCOUNT_KEY

StorageAccountName : blackboxtest
BlobEndPoint       : https://blackboxtest.blob.core.windows.net/
TableEndPoint      : https://blackboxtest.table.core.windows.net/
QueueEndPoint      : https://blackboxtest.queue.core.windows.net/
FileEndPoint       : https://blackboxtest.file.core.windows.net/
Context            : Microsoft.WindowsAzure.Commands.Storage.AzureStorageContext
Name               :
StorageAccount     : BlobEndpoint=https://blackboxtest.blob.core.windows.net/;QueueEndpoint=https://blackboxtest.queue.core.windows.net/;TableEndpoint=https:
                     //blackboxtest.table.core.windows.net/;FileEndpoint=https://blackboxtest.file.core.windows.net/;AccountName=blackboxtest;AccountKey=[key
                      hidden]
EndPointSuffix     : core.windows.net/
ConnectionString   : BlobEndpoint=https://blackboxtest.blob.core.windows.net/;QueueEndpoint=https://blackboxtest.queue.core.windows.net/;TableEndpoint=https:
                     //blackboxtest.table.core.windows.net/;FileEndpoint=https://blackboxtest.file.core.windows.net/;AccountName=blackboxtest;AccountKey=/gna
                     ...ced97OMqCm8lr3w==
ExtendedProperties : {}
```

## Search the log table
- Search all tables

```powershell
PS> blackbox-get-table-list

CloudTable                                            Uri                                                                                               
----------                                            ---                                                                                               
log71384680ffe50ddadate170911                         https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170911                         
log71384680ffe50ddadate170912                         https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170912                         
logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912 https://blackboxtest.table.core.windows.net/logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912 
```

- Table Search + Filtering

```powershell
PS> blackbox-get-table-list -FILTER_STR 170912

CloudTable                                            Uri
----------                                            ---
log71384680ffe50ddadate170912                         https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170912
logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912 https://blackboxtest.table.core.windows.net/logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912
```

## Log Search

- Real-time log viewing

```powershell
PS> blackbox-get-log -TABLE_NAME log71384680ffe50ddadate170912 -KEEP_MONITOR
[00000001][INFOR][22:17:59]CALL[MainActivity.java:62]TID[0001] hello
[00000002][DEBUG][22:18:00]CALL[MainActivity.java:67]TID[0001] world
[00000003][INFOR][22:18:10]CALL[BlackBox.java:173]TID[6272] CAPTURESCREEN 
/storage/emulated/0/Android/data/com.hhd2002.blackboxtest/cache/blackbox-screencaptu
re-170912-101800.png
https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170912/blackbox-screencapture-101800-3f8aed96-1d5f-408e-a53a-
wait ...
wait ...
```

- Save log to file

```
PS> blackbox-get-log -TABLE_NAME log57C7B14B1932357EED39FED1CA8409B11E658973date170913 | Out-File today-my-pc-log.log
PS> ls .\today-my-pc-log.log
    디렉터리: C:\temp
Mode                LastWriteTime         Length Name
----                -------------         ------ ----
-a----     2017-09-15  오전 11:18           2694 today-my-pc-log.log
```

## Delete log table

- Delete table assignment

```powershell
PS> blackbox-remove-table -TABLE_NAME log57C7B14B1932357EED39FED1CA8409B11E658973date170913

확인
Remove table and all content in it: log57C7B14B1932357EED39FED1CA8409B11E658973date170913
[Y] 예(Y)  [N] 아니요(N)  [S] 일시 중단(S)  [?] 도움말 (기본값은 "Y"): y
```

- Delete all logs except those within n days

```powershell
PS> blackbox-remove-table -REMAIN_PERIOD_IN_DAYS 1
log57C7B14B1932357EED39FED1CA8409B11E658973date170913
logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170913
Do you really delete these? [y/n]: y
```

## Session Search

- Search all sessions

```powershell
PS> blackbox-get-session
RowKey                                   SessionStr                  
------                                   ----------                  
57C7B14B1932357EED39FED1CA8409B11E658973 h2d2002@naver.com from win32
71384680ffe50dda                         h2d2002@naver.com from android
D781889DEC1C6F8B177CEA5E11B0FC220D492A6D h2d2002@naver.com from win32
DC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79 h2d2002@naver.com from win32
```

- Session search + filtering

```powershell
PS C:\temp> blackbox-get-session -FILTER_STR "from android"
RowKey           SessionStr                     FilteredStr
------           ----------                     -----------
71384680ffe50dda h2d2002@naver.com from android
```


## Screen Capture Search

- Search by (device ID + date)

```powershell
PS C:\temp> blackbox-get-screencapture 71384680ffe50dda 170915

imgUrl
------
https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170915/blackbox-screencapture-014203-c582e7d266e3443894737393660c7e17.png
https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170915/blackbox-screencapture-014211-dd8c1d3d8d3c439db1ff35a0daef0be1.png
https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170915/blackbox-screencapture-014213-fca8b3c6eb404d28a1de175419e4dd6e.png
...
```


## Delete screen capture

- Delete with (Device + Date)

```powershell
PS > blackbox-remove-screencapture -DEVICE_ID 71384680ffe50dda -DATE_STR 170911
71384680ffe50dda/170911/blackbox-screencapture-104248.png
Do you really delete these? [y/n]: y
```

- Delete all logs except those within n days

```powershell
PS C:\project\blackbox\PowerShell> blackbox-remove-screencapture -REMAIN_PERIOD_IN_DAYS 2
71384680ffe50dda/170912/blackbox-screencapture-101800-3f8aed96-1d5f-408e-a53a-698b7013acd8.png
71384680ffe50dda/170912/blackbox-screencapture-102016-84700d2cacdc4d349da878bb72435d84.png
71384680ffe50dda/170912/blackbox-screencapture-102031-5e683477e04f4186991cabbe9995b4ff.png
71384680ffe50dda/170912/blackbox-screencapture-102051-4d70ecbd62e047fb94c7553a189e437f.png
71384680ffe50dda/170912/blackbox-screencapture-102113-0556455a18114fb8887011eff8ac1ba1.png
71384680ffe50dda/170912/blackbox-screencapture-102128-745f0036ca8949f59072201cf70f0e70.png
71384680ffe50dda/170912/blackbox-screencapture-102139-f31beca46c524875a226a7e32d83625f.png
71384680ffe50dda/170912/blackbox-screencapture-105155-6222746a488f46ccb218042c04339540.png
DC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79/170912/blackbox-screencapture-171930.png
DC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79/170912/blackbox-screencapture-225315-d72ef25849b143bbbe2fc2231a2c9516.png
Do you really delete these? [y/n]: y
```

<br>
<br>
<br>

# BlackBox development plan (the person who will help is very welcome)

- WEB admin tool development
    - The admin tool only has PowerShell, which is powerful, but it is inaccessible.
    - I would like to make it a pure jquery ajax app that connects Azure Storage Service directly without creating a separate server so that I can use it for general purpose.

- Develop SDK for iOS
    - I want to do it, but I really need someone to help.
    - I do not even have a MacBook and an iPhone.

<br>
<br>
<br>