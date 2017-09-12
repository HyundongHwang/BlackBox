$global:G_STORAGE_CONTEXT = $null




<#
.SYNOPSIS
.EXAMPLE
PS> blackbox-module-install-import -MODULE_NAME Azure.Storage
Azure.Storage already installed !!!
import Azure.Storage ...
#>
function blackbox-module-install-import
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $MODULE_NAME
    )

    if((Get-InstalledModule -name $MODULE_NAME).count -eq 0)
    {
        write "install $MODULE_NAME ..."
        Install-Package $MODULE_NAME -Force -AllowClobber
    }
    else 
    {
        write "$MODULE_NAME already installed !!!"
    }

    write "import $MODULE_NAME ..."
    Import-Module $MODULE_NAME -Force
}



<#
.SYNOPSIS
.EXAMPLE
PS> blackbox-init -STORAGE_ACCOUNT_NAME blackboxandroid -STORAGE_ACCOUNT_KEY cZg2h0pMVDjDE...==
#>
function blackbox-init
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $STORAGE_ACCOUNT_NAME,

        [Parameter(Mandatory=$true, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $STORAGE_ACCOUNT_KEY
    )

    $global:G_STORAGE_CONTEXT = New-AzureStorageContext $STORAGE_ACCOUNT_NAME -StorageAccountKey $STORAGE_ACCOUNT_KEY
}




<#
.SYNOPSIS
.EXAMPLE
PS> blackbox-init-check
True
#>
function blackbox-init-check
{
    [CmdletBinding()]
    param(
    )

    if ($global:G_STORAGE_CONTEXT -eq $null) 
    {
        Write-Error "Please call blackbox-init first !!!"
        return $false;
    }
    else 
    {
        return $true;
    }
}



<#
.SYNOPSIS
.EXAMPLE
PS> blackbox-get-table-list

CloudTable                                            Uri                                                                                               
----------                                            ---                                                                                               
log71384680ffe50ddadate170911                         https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170911                         
log71384680ffe50ddadate170912                         https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170912                         
logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912 https://blackboxtest.table.core.windows.net/logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912 

.EXAMPLE
PS> blackbox-get-table-list -FILTER_STR 7138

CloudTable                    Uri
----------                    ---
log71384680ffe50ddadate170911 https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170911
log71384680ffe50ddadate170912 https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170912

.EXAMPLE
PS> blackbox-get-table-list -FILTER_STR 170912

CloudTable                                            Uri
----------                                            ---
log71384680ffe50ddadate170912                         https://blackboxtest.table.core.windows.net/log71384680ffe50ddadate170912
logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912 https://blackboxtest.table.core.windows.net/logDC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79date170912
#>
function blackbox-get-table-list
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$false, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $FILTER_STR
    )



    if (!(blackbox-init-check))
    {
        return        
    }

    Get-AzureStorageTable -Context $global:G_STORAGE_CONTEXT | where CloudTable -like "*$FILTER_STR*"
}




<#
.SYNOPSIS
.EXAMPLE
PS> blackbox-get-log -TABLE_NAME log71384680ffe50ddadate170912 -KEEP_MONITOR

[00000001][INFOR][22:17:59]CALL[MainActivity.java:62]TID[0001] hello
[00000002][DEBUG][22:18:00]CALL[MainActivity.java:67]TID[0001] world
[00000003][INFOR][22:18:10]CALL[BlackBox.java:173]TID[6272] CAPTURESCREEN /storage/emulated/0/Android/data/com.hhd2002.blackboxtest/cache/blackbox-screencaptu
re-170912-101800.png : https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170912/blackbox-screencapture-101800-3f8aed96-1d5f-408e-a53a-
wait ...
wait ...

#>
function blackbox-get-log
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $TABLE_NAME,
  
        [Parameter(Mandatory=$false, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [ValidateSet("ERROR","WARNI","INFOR","DEBUG","VERBO")]
        [System.String]
        $LOG_LEVEL,

        [Parameter(Mandatory=$false, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [switch]
        $KEEP_MONITOR = $false
    )



    if (!(blackbox-init-check))
    {
        return        
    }

    $table = Get-AzureStorageTable -Context $global:G_STORAGE_CONTEXT -Name $TABLE_NAME
    $query = New-Object Microsoft.WindowsAzure.Storage.Table.TableQuery
    $selectColums = New-Object System.Collections.Generic.List[string]
    $selectColums.Add("RowKey")
    $selectColums.Add("PartitionKey")
    $selectColums.Add("Timestamp")
    $selectColums.Add("Log")
    $selectColums.Add("LogLevel")
    $query.SelectColumns = $selectColums
    [System.String]$logLevelQuery

    if([System.String]::IsNullOrWhiteSpace($LOG_LEVEL) -eq $false)
    {
        $logLevelQuery = "PartitionKey eq '{0}'" -f $LOG_LEVEL
        $query.FilterString = $logLevelQuery
    }

    Write-Verbose ("query.FilterString : {0}" -f $query.FilterString)
    $entities = $table.CloudTable.ExecuteQuery($query) | sort RowKey
    #$entities | select RowKey, PartitionKey, TimeStamp, @{l="Log"; e = {$_.Properties["Log"].StringValue}} | fl
    $lastRowKey = $null

    $entities | %{ 
        # write ("[{0}][{1}][{2}]{3}" -f $_.RowKey, $_.Properties["LogLevel"].StringValue, $_.TimeStamp.ToLocalTime().ToString("HH:mm:ss"), $_.Properties["Log"].StringValue)
        $rowKey = $_.RowKey
        $rowLogLevel = $_.Properties["LogLevel"].StringValue
        $timeStr = $_.TimeStamp.ToLocalTime().ToString("HH:mm:ss")
        $log = $_.Properties["Log"].StringValue

        write "[$rowKey][$rowLogLevel][$timeStr]$log"
        $lastRowKey = $_.RowKey 
    }

    if (!$KEEP_MONITOR) 
    {
        return
    }



    while($true)
    {
        $nextQuery = "RowKey gt '{0}'" -f $lastRowKey

        if([System.String]::IsNullOrWhiteSpace($logLevelQuery) -eq $false)
        {
            $query.FilterString = "({0}) and ({1})" -f $logLevelQuery, $nextQuery
        }
        else
        {
            $query.FilterString = $nextQuery
        }

        Write-Verbose ("query.FilterString : {0}" -f $query.FilterString)
        $entities = $table.CloudTable.ExecuteQuery($query) | sort RowKey

        $entities | %{ 
            write ("[{0}][{1}][{2}] {3}" -f $_.RowKey, $_.Properties["LogLevel"].StringValue, $_.TimeStamp.ToLocalTime().ToString("HH:mm:ss"), $_.Properties["Log"].StringValue)
            $lastRowKey = $_.RowKey
        }

        [System.Threading.Thread]::Sleep(3000)
        write "wait ..."
    }
}



<#
.SYNOPSIS
.EXAMPLE
PS> blackbox-get-session

RowKey                                   SessionStr                     FilteredStr
------                                   ----------                     -----------
71384680ffe50dda                         h2d2002@naver.com from android
DC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79 h2d2002@naver.com from windows
#>
function blackbox-get-session
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$false, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $FILTER_STR,

        [Parameter(Mandatory=$false, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $FILTER_STR2
    )



    if (!(blackbox-init-check))
    {
        return        
    }

    $table = Get-AzureStorageTable -Context $global:G_STORAGE_CONTEXT -Name "session"
    $query = New-Object Microsoft.WindowsAzure.Storage.Table.TableQuery
    $selectColums = New-Object System.Collections.Generic.List[string]
    $selectColums.Add("RowKey")
    $selectColums.Add("SessionStr")
    $query.SelectColumns = $selectColums

    Write-Verbose "query.FilterString : $($query.FilterString)"
    $entities = $table.CloudTable.ExecuteQuery($query) | sort RowKey



    $entities | % { 

        $rowKey = $_.RowKey
        $sessionStr = $_.Properties["SessionStr"].StringValue

        if (($rowKey -notlike "*$FILTER_STR*") -and 
            ($sessionStr -notlike "*$FILTER_STR*"))
        {
            return $null
        }

        if($FILTER_STR2)
        {
            $filteredStr = $sessionStr -split "\n" | sls $FILTER_STR2
        }

        $obj = New-Object -typename PSObject
        $obj | Add-Member -MemberType NoteProperty -Name RowKey -Value $rowKey
        $obj | Add-Member -MemberType NoteProperty -Name SessionStr -Value $sessionStr
        $obj | Add-Member -MemberType NoteProperty -Name FilteredStr -Value $filteredStr
        return $obj
    }
}



<#
.SYNOPSIS
.EXAMPLE
#>
function blackbox-remove-table
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $TABLE_NAME
    )



    if (!(blackbox-init-check))
    {
        return        
    }

    Remove-AzureStorageTable -Name $TABLE_NAME -Context $global:G_STORAGE_CONTEXT
}




<#
.SYNOPSIS

.EXAMPLE
PS> blackbox-get-screencapture

deviceId                                 dateStr
--------                                 -------
71384680ffe50dda
DC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79

.EXAMPLE
PS> blackbox-get-screencapture -DEVICE_ID 7138

deviceId         dateStr
--------         -------
71384680ffe50dda 170911
71384680ffe50dda 170912

.EXAMPLE
PS> blackbox-get-screencapture -DEVICE_ID 71384680ffe50dda

deviceId         dateStr
--------         -------
71384680ffe50dda 170911
71384680ffe50dda 170912

.EXAMPLE
PS> blackbox-get-screencapture -DATE_STR 170912

deviceId                                 dateStr
--------                                 -------
71384680ffe50dda                         170912
DC6E48F4A8E2E15A245DB4CA6DEFDC4902714C79 170912

.EXAMPLE
PS> blackbox-get-screencapture -DEVICE_ID 71384680ffe50dda -DATE_STR 170911

imgUrl
------
https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170911/blackbox-screencapture-104117.png
https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170911/blackbox-screencapture-104248.png

#>
function blackbox-get-screencapture
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$false, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $DEVICE_ID,

        [Parameter(Mandatory=$false, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $DATE_STR
    )



    if (!(blackbox-init-check))
    {
        return
    }

    $blobList = $null
    $prefix = $null

    if ($DEVICE_ID.Length -ne 0) 
    {
        $prefix = $DEVICE_ID

        if ($DATE_STR.Length -ne 0) 
        {
            $prefix = "$DEVICE_ID/$DATE_STR"
        }
    }

    $blobList = Get-AzureStorageBlob -Context $global:G_STORAGE_CONTEXT -Container screencapture -Prefix $prefix

    if (($DEVICE_ID.Length -ne 0) -and ($DATE_STR.Length -ne 0))
    {
        $blobList | 
        foreach {
            $imgUrl = $_.ICloudBlob.Uri.ToString()
            $obj = New-Object psobject
            $obj | Add-Member NoteProperty imgUrl $imgUrl
            return $obj
        }
    }
    else 
    {
        $blobList |
        foreach {
            $start = 0
            $end = $_.name.IndexOf('/', $start)
            $deviceId = $_.name.Substring(0, $end - $start)
            $start = $end + 1
            $end = $_.name.IndexOf('/', $start)
            $dateStr = $_.name.Substring($start, $end - $start)

            if ($DEVICE_ID.Length -ne 0)
            {
                $obj = New-Object psobject
                $obj | Add-Member NoteProperty deviceId $deviceId
                $obj | Add-Member NoteProperty dateStr $dateStr
                return $obj
            }
            elseif ($DATE_STR.Length -ne 0) 
            {
                if ($dateStr -eq $DATE_STR) 
                {
                    $obj = New-Object psobject
                    $obj | Add-Member NoteProperty deviceId $deviceId
                    $obj | Add-Member NoteProperty dateStr $dateStr
                    return $obj
                }
            }
            else 
            {
                $obj = New-Object psobject
                $obj | Add-Member NoteProperty deviceId $deviceId
                return $obj
            }
        } |
        select -Unique -Property deviceId, dateStr
        
    }
}





<#
.SYNOPSIS

.EXAMPLE
PS> blackbox-get-screencapture -DEVICE_ID 71384680ffe50dda -DATE_STR 170911

imgUrl
------
https://blackboxtest.blob.core.windows.net/screencapture/71384680ffe50dda/170911/blackbox-screencapture-104248.png

PS > blackbox-remove-screencapture -DEVICE_ID 71384680ffe50dda -DATE_STR 170911
71384680ffe50dda/170911/blackbox-screencapture-104248.png
Do you really delete these? [y/n]: y
#>
function blackbox-remove-screencapture
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $DEVICE_ID,

        [Parameter(Mandatory=$false, ValueFromPipeline=$true, ValueFromPipelinebyPropertyName=$true)]
        [System.String]
        $DATE_STR
    )



    if (!(blackbox-init-check))
    {
        return
    }

    $blobList = $null
    $prefix = $null

    if ($DEVICE_ID.Length -ne 0) 
    {
        $prefix = $DEVICE_ID

        if ($DATE_STR.Length -ne 0) 
        {
            $prefix = "$DEVICE_ID/$DATE_STR"
        }
    }

    $blobList = Get-AzureStorageBlob -Context $global:G_STORAGE_CONTEXT -Container screencapture -Prefix $prefix
    write $blobList.Name
    $res = Read-Host "Do you really delete these? [y/n]"
    
    if ($res -ne "y")
    {
        return
    }

    $blobList | 
    foreach { 
        Remove-AzureStorageBlob -Context $global:G_STORAGE_CONTEXT -Blob $_.Name -Container screencapture
    }
}



blackbox-module-install-import -MODULE_NAME Azure.Storage