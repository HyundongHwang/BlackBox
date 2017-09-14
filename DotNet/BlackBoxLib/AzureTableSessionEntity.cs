using Microsoft.WindowsAzure.Storage.Table;

namespace BlackBoxLib
{
    public class AzureTableSessionEntity : TableEntity
    {
        public AzureTableSessionEntity()
        {
            this.PartitionKey = "PartitionKey";
        }

        public string SessionStr { get; set; }
    }





























}
