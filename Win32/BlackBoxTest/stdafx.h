// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>

// C RunTime Header Files
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <tchar.h>

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS      // some CString constructors will be explicit

#include <atlbase.h>
#include <atlstr.h>

// TODO: reference additional headers your program requires here

#include <atlwin.h>
#include <atltypes.h>
#include <atlcoll.h>
#include <atlimage.h>
#include <atlfile.h>
#include <cstdarg>
#include <Dbghelp.h>
#include <locale.h>
#include <time.h>
#include <list>
using namespace ATL;

#include <process.h>
#include <WinBase.h>
#include <shellapi.h>




#import "BlackBoxLib.tlb" raw_interfaces_only
using namespace BlackBoxLib;