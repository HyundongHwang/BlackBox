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

cp *.nuspec ../BlackBoxLib/ -Force -Recurse