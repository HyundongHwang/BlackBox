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

. "C:\windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe" "$ROOT_DIR\DotNet\BlackBoxLib\bin\Debug\BlackBoxLib.dll" /tlb:"$ROOT_DIR\DotNet\BlackBoxLib\bin\Debug\BlackBoxLib.tlb" /codebase

cp "$ROOT_DIR\DotNet\BlackBoxLib\bin\Debug\BlackBoxLib.tlb" "$ROOT_DIR\Win32\BlackBoxTest"