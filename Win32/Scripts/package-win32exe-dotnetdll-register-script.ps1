# [CmdletBinding()]
# param (
#     [parameter(Mandatory = $true)]
#     [ValidateSet("AAA", "BBB")]
#     [string]
#     $OPTION = "AAA",

#     [parameter(Mandatory = $false)]
#     [switch]
#     [bool]
#     $OPTION2 = $false
# )

$ROOT_DIR = (Resolve-Path "$PSScriptRoot\..\..").Path

rm BlackBoxTestDeploy -Recurse -Force
md BlackBoxTestDeploy
cp "$ROOT_DIR\Win32\BlackBoxTest\Debug\*" BlackBoxTestDeploy -Recurse -Force
cp "$ROOT_DIR\Win32\Scripts\register-dotnet-dll.ps1" BlackBoxTestDeploy
cp "$ROOT_DIR\Win32\Scripts\unregister-dotnet-dll.ps1" BlackBoxTestDeploy

cd BlackBoxTestDeploy
rm *.tlog, *.tlh, *.log, *.ilk, *.tlog, *.pch, *.pdb, *.idb, *.obj -Recurse -Force
cd ..

$now_date_time = [System.DateTime]::Now.ToString("yyMMddHHmm")
Compress-Archive BlackBoxTestDeploy "BlackBoxTestDeploy-$now_date_time.zip"