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

[CmdletBinding()]
param(
    [parameter(Mandatory=$true)]
    [string]
    $NUGET_PUBLISH_KEY
)

$ROOT_DIR = (Resolve-Path "$PSScriptRoot\..\..").Path

rm ..\BlackBoxLib\*.nupkg
.\nuget.exe pack ..\BlackBoxLib\BlackBoxLib.csproj
.\nuget.exe push *.nupkg $NUGET_PUBLISH_KEY -Source https://www.nuget.org/api/v2/package
rm *.nupkg