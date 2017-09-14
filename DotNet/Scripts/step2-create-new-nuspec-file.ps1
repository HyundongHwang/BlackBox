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

Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile nuget.exe
.\nuget.exe spec