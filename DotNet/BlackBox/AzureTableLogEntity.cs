using Microsoft.WindowsAzure.Storage.Table;

namespace BlackBoxLib
{
    public class AzureTableLogEntity : TableEntity
    {
        [IgnoreProperty]
        public static long LastRowKey { get; set; }

        public AzureTableLogEntity()
        {
            this.RowKey = (AzureTableLogEntity.LastRowKey + 1).ToString("D8");
            AzureTableLogEntity.LastRowKey = AzureTableLogEntity.LastRowKey + 1;
            this.PartitionKey = "PartitionKey";
        }

        public string HwId { get; set; }
        public string HwManufacturer { get; set; }
        public string HwProductName { get; set; }
        public string HwSku { get; set; }
        public string OsCat { get; set; }
        public string OsVersion { get; set; }
        public string OsArchitecture { get; set; }
        public string AppVersion { get; set; }
        public string UserId { get; set; }
        public string UserId2 { get; set; }
        public string UserId3 { get; set; }
        public string UserId4 { get; set; }
        public string UserId5 { get; set; }
        public string LogLevel { get; set; }
        public string Log { get; set; }
    }





























}
