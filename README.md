<!-- TOC -->

- [BlackBox 소개](#blackbox-소개)
    - [간략기능 소개](#간략기능-소개)
    - [사용하기 좋은 시나리오](#사용하기-좋은-시나리오)
    - [저장소 관리 규칙](#저장소-관리-규칙)
- [BlackBox 실시간 로그, 화면캡쳐 데모영상](#blackbox-실시간-로그-화면캡쳐-데모영상)
- [Azure Storage 계정키 발급받기](#azure-storage-계정키-발급받기)
- [BlackBox for Android](#blackbox-for-android)
    - [jitpack 설정](#jitpack-설정)
    - [초기화](#초기화)
    - [로그, 세션, 스크린캡쳐 기록](#로그-세션-스크린캡쳐-기록)
    - [샘플 apk 다운로드](#샘플-apk-다운로드)
- [BlackBox for .NET](#blackbox-for-net)
    - [nuget 설정](#nuget-설정)
    - [초기화](#초기화-1)
    - [로그, 세션, 스크린캡쳐 기록](#로그-세션-스크린캡쳐-기록-1)
    - [샘플 exe 다운로드](#샘플-exe-다운로드)
- [BlackBox for Win32](#blackbox-for-win32)
    - [.NET dll 시스템 등록](#net-dll-시스템-등록)
    - [tlb 파일 임포트](#tlb-파일-임포트)
    - [초기화](#초기화-2)
    - [로그, 세션, 스크린캡쳐 기록](#로그-세션-스크린캡쳐-기록-2)
    - [샘플 exe 다운로드](#샘플-exe-다운로드-1)
- [BlackBox PowerShell Admin](#blackbox-powershell-admin)
    - [Powershell Gallery 에서 설치](#powershell-gallery-에서-설치)
    - [초기화](#초기화-3)
    - [로그테이블 조회](#로그테이블-조회)
    - [로그 조회](#로그-조회)
    - [로그테이블 삭제](#로그테이블-삭제)
    - [세션 조회](#세션-조회)
    - [스크린캡쳐 조회](#스크린캡쳐-조회)
    - [스크린캡쳐 삭제](#스크린캡쳐-삭제)
- [BlackBox 개발계획 (도와주신다면 대단히 환영합니다.)](#blackbox-개발계획-도와주신다면-대단히-환영합니다)

<!-- /TOC -->

<br>
<br>
<br>

![](https://raw.githubusercontent.com/HyundongHwang/BlackBox/master/blackbox-icon.jpg)

# BlackBox 소개

회사일로 필요해서 만들어서 몇달간 유용하게 사용하고 있던 로그모듈인데 라이브러리 형태로 정리했습니다. <br>
Azure Storage 인스턴스를 한개 만들어서 키등록을 하면 쉽게 안드로이드, .NET, Win32 앱에서 텍스트로깅, 스크린캡쳐를 실시간으로 할 수 있습니다. <br>
부족함이 많으니 PullRequest 환영입니다. <br>
사용중 문의사항 있으면 글남겨 주세요. <br>



## 간략기능 소개
- 로그텍스트, 스크린캡쳐이미지를 모아서 10초에 한번씩 클라우드 저장소로 모으는 기능.
- 클라우드 저장소는 Azure Storage를 사용하고 있어서 그 접근키가 필요함.
- 안드로이드, .NET, Win32 클라이언트에서 사용하기 쉽도록 SDK 형태로 제공함.
    - 안드로이드 : jitpack 에서 gradle 로 제공
    - .NET : nuget 에서 package 로 제공
    - Win32 : nuget 에서 package 로 제공받고, 첨부된 tlb를 임포트해서 COM으로 사용
- PowerShell 로 로그 조회/삭제, 세션 조회, 스크린캡쳐 조회/삭제 를 지원함.
    - PowerShell Gallery 에서 모듈로 제공

## 사용하기 좋은 시나리오
- 클라이언트앱에서 데이타흐름이 복잡한데 그 흐름 안에서 중요한 단서를 찾아야 할때.
    - 이 단서를 찾는 과정이 운영중 수시로 발생하고 실시간으로 파악해야 할때.
- 사내서비스 처럼 개인정보보호보다는 업무자체가 더 중요할때.
    - `그렇다고 해도 중요개인정보의 무분별한 로깅이나 무허가 스크린캡쳐는 안됨.`
- 로그텍스트를 다양한 빅데이타 분석으로 활용해야 할때.
    - 로그텍스트를 NOSQL DB 에서 관리하는 거라 당연히 서버에 텍스트파일로 분석하는 것보다 정규화 되어 있고 접근성도 높음.
- 로그텍스트를 동시에 대량으로 쌓고 있는데 이 때문에 로그서버에 부하가 많이 걸릴때
    - Azure Storage 를 사용하면 이 저장소 사용에 대한 조회/쓰기/삭제 를 REST API로 사용할 수 있어서 자체적으로 로그서버를 만들고 관리하는 부담이 없음.
    - 많은 트래픽 처리도 자기가 만든서버보다는 Azure Storage의 서버가 잘 처리해 주지 않을까?
- 로그텍스트의 용량이 너무 커져서 스토리지 증설이슈가 발생하고 그 때문에 시간과 비용이 낭비될때.
    - Azure Storage 는 클라우드 기반이니 용량은 당연히 무제한이고
    - 한달에 10gb 정도 사용한다고 가정했을때 1000원 이내로 과금될 뿐임.

## 저장소 관리 규칙
- 로그텍스트
    - Azure Storage Table 를 이용해서 NOSQL DB로 관리함.
    - 클라이언트앱별로 매일 한개의 테이블을 생성함.
    - `log{DEVICE-ID}date{yyMMdd}` 로 디바이스당 모든 날짜별로 테이블이 따로 생성됨.
        - 조회/삭제가 디바이스별 날짜별로 지원됨
    
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

- 세션
    - 역시 Azure Storage Table 를 이용해서 NOSQL DB로 관리함.
    - 클라이언트앱의 하드웨어ID와 세션정보를 매칭해줌
        - 로그분석을 해야 할때는 하드웨어ID보다는 서비스ID등 세션정보로 찾게 되기 때문에 필요함.

```powershell
RowKey                                   SessionStr
------                                   ----------
57C7B14B1932357EED39FED1CA8409B11E658973 h2d2002@naver.com from win32
71384680ffe50dda                         h2d2002@naver.com from android
D781889DEC1C6F8B177CEA5E11B0FC220D492A6D h2d2002@naver.com from win32
DC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79 h2d2002@naver.com from win32
```

- 스크린캡쳐
    - Azure Storage Blob 을 이용해서 파일로 관리하고 조회/삭제 지원함.
    - 클라이언트별 날짜별로 폴더 나눠서 관리함.
        - `{DEVICE-ID}/{yyMMdd}/blackbox-screencapture-{HHmmss}-{RANDOM-GUID}.png`
    - random GUID 를 url에 뒤에 붙여서 url예측을 통한 해킹을 어렵게 했음. (그대신 url이 좀 김 --;;)

<br>
<br>
<br>

# BlackBox 실시간 로그, 화면캡쳐 데모영상

- 안드로이드 앱
    - https://www.youtube.com/watch?v=HhdLYEhU-Zc

- .NET 앱
    - https://www.youtube.com/watch?v=VNZTxJAMqn8

- Win32 앱
    - https://www.youtube.com/watch?v=SHLa6vcTgVk

<br>
<br>
<br>

# Azure Storage 계정키 발급받기

- https://hyundonghwang.github.io/2017/09/15/Azure-Storage-Service-%EC%9D%B8%EC%8A%A4%ED%84%B4%EC%8A%A4-%EB%A7%8C%EB%93%A4%EA%B8%B0/
- 테스트를 위해 샘플앱에 포함된 계정키를 원하시는 분은 따로 연락을 주세요~ (카톡아이디 : hhd2002)

<br>
<br>
<br>

# BlackBox for Android

## jitpack 설정
- https://jitpack.io/#HyundongHwang/BlackBox
- 루트의 `/build.gradle` 파일에 `jitpack.io` 저장소 추가

```groovy
allprojects {
    repositories {
        ...
        maven { url 'https://jitpack.io' }
    }
}
```

- 사용대상이 되는 모듈에 종속성 추가
    - 예) `/app/build.gradle`

```groovy
dependencies {
    compile 'com.github.HyundongHwang:BlackBox:1.3.0'
}
```

## 초기화
- Azure Storage 키를 이용해서 초기화 함.

```java
public class MainApplication extends MultiDexApplication {
    @Override
    public void onCreate() {
        super.onCreate();
        BlackBox.init(this, getString("DefaultEndpointsProtocol=https;AccountName=blackboxtest;AccountKey=/gnaCzk...8lr3w==;EndpointSuffix=core.windows.net"));
    }
}
```


## 로그, 세션, 스크린캡쳐 기록

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

## 샘플 apk 다운로드
- https://github.com/HyundongHwang/BlackBox/blob/master/Android/BlackboxTest-170915-0200.apk

<br>
<br>
<br>

# BlackBox for .NET
- https://www.nuget.org/packages/hhd2002.BlackBox

## nuget 설정

```powershell
PM> Install-Package hhd2002.BlackBox -Version 1.3.0
```

## 초기화

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

## 로그, 세션, 스크린캡쳐 기록

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

## 샘플 exe 다운로드

- https://github.com/HyundongHwang/BlackBox/blob/master/DotNet/Scripts/BlackBoxTestDeploy-1709141827.zip

<br>
<br>
<br>

# BlackBox for Win32

## .NET dll 시스템 등록
- .NET dll을 COM interface로 호출할것이라 사용전 시스템 등록이 필요함.
- 보통 설치프로그램에서 이뤄질 작업인데 테스트앱이라 c++ 코드에서 작성했음.

```c++
wchar_t wBlackBoxDllPath[MAX_PATH] = { 0, };
::GetModuleFileName(NULL, wBlackBoxDllPath, MAX_PATH);
::PathRemoveFileSpec(wBlackBoxDllPath);
::PathCombine(wBlackBoxDllPath, wBlackBoxDllPath, L"BlackBoxLib.dll");

HANDLE child = ::ShellExecute(this->m_hWnd, L"runas", L"C:\\windows\\Microsoft.NET\\Framework\\v4.0.30319\\RegAsm.exe", wBlackBoxDllPath, 0, SW_SHOWNORMAL);
::WaitForSingleObject(child, INFINITE);
```

## tlb 파일 임포트
- .NET dll 을 미리 COM export 해 두었고 이를 통해 tlb 파일을 만들수 있고, 임포트하면 컴파일 할때 tlh 파일이 생겨서 c++ 코드에서 강타입으로 사용가능하게 됨.
- 무슨 얘기인지 모르겠다면ㅠㅠ 이미 만들어진 파일을 아래 링크에서 다운로드 받고 #import 구문만 작성하면됨.
    - https://github.com/HyundongHwang/BlackBox/raw/master/Win32/BlackBoxTest/BlackBoxLib.tlb
- `stdafx.h`

```c++
#import "BlackBoxLib.tlb" raw_interfaces_only
using namespace BlackBoxLib;
```

## 초기화

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

## 로그, 세션, 스크린캡쳐 기록

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

## 샘플 exe 다운로드

- https://github.com/HyundongHwang/BlackBox/blob/master/Win32/Scripts/BlackBoxTestDeploy-1709141128.zip

<br>
<br>
<br>

# BlackBox PowerShell Admin

## Powershell Gallery 에서 설치

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

## 초기화

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

## 로그테이블 조회
- 모든 테이블 조회

```powershell
PS> blackbox-get-table-list

CloudTable                                            Uri                                                                                               
----------                                            ---                                                                                               
log71384680ffe50ddadate170911                         https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170911                         
log71384680ffe50ddadate170912                         https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170912                         
logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912 https://blackboxtest.table.core.windows.net/logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912 
```

- 테이블 조회 + 필터링

```powershell
PS> blackbox-get-table-list -FILTER_STR 170912

CloudTable                                            Uri
----------                                            ---
log71384680ffe50ddadate170912                         https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170912
logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912 https://blackboxtest.table.core.windows.net/logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912
```

## 로그 조회

- 실시간 로그보기

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

- 로그를 파일로 저장

```
PS> blackbox-get-log -TABLE_NAME log57C7B14B1932357EED39FED1CA8409B11E658973date170913 | Out-File today-my-pc-log.log
PS> ls .\today-my-pc-log.log
    디렉터리: C:\temp
Mode                LastWriteTime         Length Name
----                -------------         ------ ----
-a----     2017-09-15  오전 11:18           2694 today-my-pc-log.log
```

## 로그테이블 삭제

- 테이블 지정 삭제

```powershell
PS> blackbox-remove-table -TABLE_NAME log57C7B14B1932357EED39FED1CA8409B11E658973date170913

확인
Remove table and all content in it: log57C7B14B1932357EED39FED1CA8409B11E658973date170913
[Y] 예(Y)  [N] 아니요(N)  [S] 일시 중단(S)  [?] 도움말 (기본값은 "Y"): y
```

- n일 이내로만 남기고 모두 삭제

```powershell
PS> blackbox-remove-table -REMAIN_PERIOD_IN_DAYS 1
log57C7B14B1932357EED39FED1CA8409B11E658973date170913
logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170913
Do you really delete these? [y/n]: y
```

## 세션 조회

- 모든 세션 조회

```powershell
PS> blackbox-get-session
RowKey                                   SessionStr                  
------                                   ----------                  
57C7B14B1932357EED39FED1CA8409B11E658973 h2d2002@naver.com from win32
71384680ffe50dda                         h2d2002@naver.com from android
D781889DEC1C6F8B177CEA5E11B0FC220D492A6D h2d2002@naver.com from win32
DC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79 h2d2002@naver.com from win32
```

- 세션 조회 + 필터링

```powershell
PS C:\temp> blackbox-get-session -FILTER_STR "from android"
RowKey           SessionStr                     FilteredStr
------           ----------                     -----------
71384680ffe50dda h2d2002@naver.com from android
```


## 스크린캡쳐 조회

- (디바이스ID + 날짜)로 조회

```powershell
PS C:\temp> blackbox-get-screencapture 71384680ffe50dda 170915

imgUrl
------
https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170915/blackbox-screencapture-014203-c582e7d266e3443894737393660c7e17.png
https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170915/blackbox-screencapture-014211-dd8c1d3d8d3c439db1ff35a0daef0be1.png
https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170915/blackbox-screencapture-014213-fca8b3c6eb404d28a1de175419e4dd6e.png
...
```


## 스크린캡쳐 삭제

- (디바이스 + 날짜)로 삭제

```powershell
PS > blackbox-remove-screencapture -DEVICE_ID 71384680ffe50dda -DATE_STR 170911
71384680ffe50dda/170911/blackbox-screencapture-104248.png
Do you really delete these? [y/n]: y
```

- n일 이내로만 남기고 모두 삭제

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

# BlackBox 개발계획 (도와주신다면 대단히 환영합니다.)

- WEB admin 툴 개발
    - admin 툴이 PowerShell만 있는데 강력하긴 하지만 아무래도 접근성이 떨어짐.
    - 범용적으로 사용할 수 있게 이것도 따로 서버 만들지 않고 Azure Storage Service를 직접연결해서 순수 jquery ajax로 만들예정.

- iOS 용 SDK 개발
    - 하고는 싶지만 정말 누가 도와줘야 됨.
    - 맥북, 아이폰 없음.

<br>
<br>
<br>