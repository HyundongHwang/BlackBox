package com.hhd2002.blackbox;

import com.microsoft.azure.storage.table.TableServiceEntity;

public class AzureTableSessionEntity extends TableServiceEntity {
    public AzureTableSessionEntity() {
        this.partitionKey = "PartitionKey";
    }

    private String SessionStr;

    public String getSessionStr() {
        return this.SessionStr;
    }

    public void setSessionStr(String sessionStr) {
        this.SessionStr = sessionStr;
    }
}
