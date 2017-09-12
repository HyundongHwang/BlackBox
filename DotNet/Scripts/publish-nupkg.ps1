[CmdletBinding()]
param(
    [parameter(Mandatory=$true)]
    [string]$NUGET_PUBLISH_KEY
)

.\nuget pack ..\BlackBox\BlackBox.csproj
.\nuget push ..\BlackBox\hhd2002.BlackBox.1.0.0.nupkg $NUGET_PUBLISH_KEY -Source https://www.nuget.org/api/v2/package