$global:G_STORAGE_CONTEXT = $null




<#
.SYNOPSIS
.EXAMPLE
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
    blackbox-init -STORAGE_ACCOUNT_NAME blackboxandroid -STORAGE_ACCOUNT_KEY cZg2h0pMVDjDEEBuA/nDw0nXGttc7rAv5o977ZXO3IdCHt9UAE+U7X0XA5ZfxBrkJ0NKTymCCXRzvmHUSOEKIw==
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




blackbox-module-install-import -MODULE_NAME Azure.Storage