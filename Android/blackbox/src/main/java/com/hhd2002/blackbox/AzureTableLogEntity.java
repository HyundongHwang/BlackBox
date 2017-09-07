package com.hhd2002.blackbox;

import android.annotation.SuppressLint;

import com.microsoft.azure.storage.table.Ignore;
import com.microsoft.azure.storage.table.TableServiceEntity;

/**
 * Created by hhd on 2017-09-07.
 */

public class AzureTableLogEntity extends TableServiceEntity {
    @Ignore
    public static long getLastRowKey() {
        return _lastRowKey;
    }

    @Ignore
    public static void setLastRowKey(long lastRowKey) {
        AzureTableLogEntity._lastRowKey = lastRowKey;
    }

    private static long _lastRowKey;



    @SuppressLint("DefaultLocale")
    public AzureTableLogEntity() {
        long newRowKey = AzureTableLogEntity.getLastRowKey() + 1;
        this.rowKey = String.format("%08d", newRowKey);
        AzureTableLogEntity.setLastRowKey(newRowKey);
        this.partitionKey = "PartitionKey";
    }

    public String getLogLevel() {
        return LogLevel;
    }

    public void setLogLevel(String logLevel) {
        LogLevel = logLevel;
    }

    public String getLog() {
        return Log;
    }

    public void setLog(String log) {
        Log = log;
    }

    private String LogLevel;
    private String Log;
}