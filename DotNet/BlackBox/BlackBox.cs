using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackBoxLib
{
    public static class BlackBox
    {
        public class LevelAndLog
        {
            public string Level { get; set; }
            public string Log { get; set; }
        }

        private const string _SCREEN_CAPTURE_FILE_PREFIX = "blackbox-screencapture";
        private static ConcurrentQueue<string> _logCache4File = new ConcurrentQueue<string>();

        private static ConcurrentQueue<LevelAndLog> _logCache4Azure = new ConcurrentQueue<LevelAndLog>();

        private static string _azureStorageConnectionString;

        public static void Init(string azureStorageConnectionString = null)
        {
            _azureStorageConnectionString = azureStorageConnectionString;

            TaskEx.Run(async () =>
            {
                try
                {
                    var storageAccount = CloudStorageAccount.Parse(_azureStorageConnectionString);
                    var tableClient = storageAccount.CreateCloudTableClient();
                    var table = tableClient.GetTableReference($"log{_GetPcUniqueKey()}date{DateTime.Now.ToString("yyMMdd")}");
                    var res = table.CreateIfNotExists();
                    var query = new TableQuery<AzureTableLogEntity>();
                    var maxRowKey = table.ExecuteQuery(query).Max(item => long.Parse(item.RowKey));

                    if (maxRowKey == null)
                    {
                        AzureTableLogEntity.LastRowKey = 0;
                    }
                    else
                    {
                        AzureTableLogEntity.LastRowKey = maxRowKey;
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }

                while (true)
                {
                    if (_logCache4Azure.Count == 0)
                    {
                        await TaskEx.Delay(TimeSpan.FromSeconds(10));
                        continue;
                    }

                    _WriteLogToAzureTable();
                }
            });



            TaskEx.Run(async () =>
            {
                var storageAccount = CloudStorageAccount.Parse(_azureStorageConnectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("screencapture");
                var res = container.CreateIfNotExists();
                var containerPermissions = container.GetPermissions();
                containerPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(containerPermissions);

                var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var screencaptureDir = Path.Combine(exeDir, _SCREEN_CAPTURE_FILE_PREFIX);

                while (true)
                {
                    var files = Directory.GetFiles(screencaptureDir);

                    foreach (var file in files)
                    {
                        if (!file.Contains(_SCREEN_CAPTURE_FILE_PREFIX))
                            continue;

                        var fileName = Path.GetFileNameWithoutExtension(file);
                        var idx = _SCREEN_CAPTURE_FILE_PREFIX.Length + 1;
                        var dateStr = fileName.Substring(idx, 6);
                        idx = idx + 6 + 1;
                        var timeStr = fileName.Substring(idx, 6);

                        var blobName = $"{_GetPcUniqueKey()}/{dateStr}/{_SCREEN_CAPTURE_FILE_PREFIX}-{timeStr}.png";
                        var blob = container.GetBlockBlobReference(blobName);
                        blob.UploadFromFile(file);
                        blob.Properties.ContentType = "image/png";
                        blob.SetProperties();

                        File.Delete(file);
                        var imgFilePath = file;
                        var imgUrl = blob.Uri.ToString();
                        BlackBox.i($"CAPTURESCREEN {imgFilePath} : {imgUrl}");
                    }

                    Thread.Sleep(3000);
                }
            });



        }

        public static void CaptureScreen()
        {
            //Create a new bitmap.
            var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height,
                PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var g = Graphics.FromImage(bmp);

            // Take the screenshot from the upper left corner to the right bottom corner.
            g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                Screen.PrimaryScreen.Bounds.Y,
                0,
                0,
                Screen.PrimaryScreen.Bounds.Size,
                CopyPixelOperation.SourceCopy);

            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var screencaptureDir = Path.Combine(exeDir, _SCREEN_CAPTURE_FILE_PREFIX);
            var screencaptureFilePath = Path.Combine(screencaptureDir, $"{_SCREEN_CAPTURE_FILE_PREFIX}-{DateTime.Now.ToString("yyMMdd-HHmmss")}.png");

            if (!Directory.Exists(screencaptureDir))
            {
                Directory.CreateDirectory(screencaptureDir);
            }

            // Save the screenshot to the specified path that the user has chosen.
            bmp.Save(screencaptureFilePath, ImageFormat.Png);
        }

        public static void Flush()
        {
            if (_logCache4Azure.Count == 0)
                return;

            _WriteLogToAzureTable();
        }

        public static void e(string format, params object[] args)
        {
            _WriteLogAsync("ERROR", format, args);
        }

        public static void w(string format, params object[] args)
        {
            _WriteLogAsync("WARNI", format, args);
        }

        public static void i(string format, params object[] args)
        {
            _WriteLogAsync("INFOR", format, args);
        }

        public static void d(string format, params object[] args)
        {
            _WriteLogAsync("DEBUG", format, args);
        }

        public static void v(string format, params object[] args)
        {
            _WriteLogAsync("VERBO", format, args);
        }

        public static void http_req(HttpWebRequest request, bool showHeader = true, string body = null, int bodyLimitCount = 100, string logLevel = "DEBUG")
        {
            _WriteLogAsync(logLevel, "");
            var reqTopLog = $"HTTP >>> {request.Method} {request.RequestUri}";
            _WriteLogAsync(logLevel, reqTopLog);

            if (showHeader)
            {
                foreach (var key in request.Headers)
                {
                    var value = request.Headers[key.ToString()].ToString();
                    _WriteLogAsync(logLevel, $"HTTP >>> HEADER : {key} : {value}");
                }
            }

            if (!string.IsNullOrEmpty(body))
            {
                _WriteLogAsync(logLevel, string.Format("HTTP >>> BODY LEN : {0}", body.Length));
                _WriteLogAsync(logLevel, string.Format("HTTP >>> BODY : {0} ...", body.Substring(0, bodyLimitCount)));
            }

            _WriteLogAsync(logLevel, "");
        }

        public static void http_res(HttpWebResponse response, string body = null, int bodyLimitCount = 100, string logLevel = "DEBUG")
        {
            _WriteLogAsync(logLevel, "");
            var resTopLog = string.Format("HTTP <<< {0} {1}", response.Method, response.ResponseUri);
            _WriteLogAsync(logLevel, resTopLog);

            var resStatusLog = string.Format("HTTP <<< STATUS : {0}({1})",
                (int)response.StatusCode,
                response.StatusCode);
            _WriteLogAsync(logLevel, resStatusLog);

            if (!string.IsNullOrEmpty(body))
            {
                _WriteLogAsync(logLevel, string.Format("HTTP <<< BODY LEN : {0}", body.Length));
                _WriteLogAsync(logLevel, string.Format("HTTP <<< BODY : {0} ...", body.Substring(0, bodyLimitCount)));
            }

            _WriteLogAsync(logLevel, "");
        }

        public static void exception(Exception ex, string logLevel = "WARNI")
        {
            _WriteLogAsync(logLevel, "");
            _WriteLogAsync(logLevel, "");
            _WriteLogAsync(logLevel, "");

            _WriteLogAsync(logLevel, string.Format("EXCEPTION !!! : {0}", ex));

            _WriteLogAsync(logLevel, "");
            _WriteLogAsync(logLevel, "");
            _WriteLogAsync(logLevel, "");
        }

        public static void session(string sessionStr)
        {
            var storageAccount = CloudStorageAccount.Parse(_azureStorageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference($"session");
            var res = table.CreateIfNotExists();
            var query = new TableQuery<AzureTableSessionEntity>();
            var item = new AzureTableSessionEntity();
            item.RowKey = _GetPcUniqueKey();
            item.SessionStr = sessionStr;
            var insertOperation = TableOperation.InsertOrReplace(item);
            table.Execute(insertOperation);
        }

        private static void _WriteLogAsync(string logLevel, string format, params object[] args)
        {
            var log = args.Any() ? string.Format(format, args) : format;
            var tid = Thread.CurrentThread.ManagedThreadId;
            log = $"[TID:{tid.ToString("D2")}] {log}";
            var nowDateTimeStr = DateTime.Now.ToString("yy/MM/dd|HH:mm:ss");
            var decoLog = $"[{logLevel}][{nowDateTimeStr}]{log}";

            try
            {
                switch (logLevel)
                {
                    case "ERROR":
                        Trace.TraceError(log);
                        break;
                    case "WARNI":
                        Trace.TraceWarning(log);
                        break;
                    case "INFOR":
                    case "DEBUG":
                    case "VERBO":
                    default:
                        Trace.TraceInformation(log);
                        break;
                }
            }
            catch (Exception)
            {
            }

            _logCache4Azure.Enqueue(new LevelAndLog { Level = logLevel, Log = log, });
        }

        private static void _WriteLogToLocalFile()
        {
            var tmpLogQueue = new Queue<string>();

            while (_logCache4File.Count > 0)
            {
                string lineLog;
                _logCache4File.TryDequeue(out lineLog);
                tmpLogQueue.Enqueue(lineLog);
            }

            var nowDateStr = DateTime.Now.ToString("yyMMdd");
            var exePath = Assembly.GetEntryAssembly().Location;
            var logFilePath = $"{exePath}_{nowDateStr}.log";

            try
            {
                File.AppendAllLines(logFilePath, tmpLogQueue);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        private static void _WriteLogToAzureTable()
        {
            var tmpLogQueue = new Queue<LevelAndLog>();

            while (_logCache4Azure.Count > 0)
            {
                LevelAndLog lineLog = null;
                _logCache4Azure.TryDequeue(out lineLog);
                tmpLogQueue.Enqueue(lineLog);
            }

            try
            {
                var storageAccount = CloudStorageAccount.Parse(_azureStorageConnectionString);
                var tableClient = storageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference($"log{_GetPcUniqueKey()}date{DateTime.Now.ToString("yyMMdd")}");
                var res = table.CreateIfNotExists();

                while (tmpLogQueue.Count > 0)
                {
                    var tmpLog = tmpLogQueue.Dequeue();
                    var log = new AzureTableLogEntity();
                    log.PartitionKey = tmpLog.Level;
                    log.LogLevel = tmpLog.Level;
                    log.Log = tmpLog.Log;
                    var insertOperation = TableOperation.InsertOrReplace(log);
                    table.Execute(insertOperation);
                }
            }
            catch (Exception ex)
            {
                var exStr = ex.ToString();
                Trace.TraceError(exStr);

                while (tmpLogQueue.Count > 0)
                {
                    var tmpLog = tmpLogQueue.Dequeue();
                    _logCache4Azure.Enqueue(tmpLog);
                }

                while (_logCache4Azure.Count > 100)
                {
                    LevelAndLog unused = null;
                    _logCache4Azure.TryDequeue(out unused);
                }

                Trace.TraceError(ex.ToString());
            }
        }





        private static string _pcUniqueKey = "";

        private static string _GetPcUniqueKey()
        {
            if (!string.IsNullOrWhiteSpace(_pcUniqueKey))
                return _pcUniqueKey;



            var processorId = "";
            var baseBoardSerialNumber = "";
            var macAddress = "";
            var hardDiskVolumeSerialNumber = "";



            try
            {
                var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
                var mbsList = mbs.Get();

                foreach (var mo in mbsList)
                {
                    if (mo["ProcessorId"] != null)
                    {
                        processorId = mo["ProcessorId"].ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                BlackBox.exception(ex);
            }



            try
            {
                var scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
                scope.Connect();

                using (var wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions()))
                {
                    var snObj = wmiClass["SerialNumber"];

                    if (snObj != null)
                    {
                        baseBoardSerialNumber = snObj.ToString().Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                BlackBox.exception(ex);
            }



            try
            {
                using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                using (var moc = mc.GetInstances())
                {
                    if (moc != null)
                    {
                        foreach (var mo in moc)
                        {
                            if (mo["MacAddress"] != null && mo["IPEnabled"] != null && (bool)mo["IPEnabled"] == true)
                            {
                                macAddress = mo["MacAddress"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BlackBox.exception(ex);
            }



            try
            {
                var drive = "C";
                var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
                dsk.Get();
                hardDiskVolumeSerialNumber = dsk["VolumeSerialNumber"].ToString();
            }
            catch (Exception ex)
            {
                BlackBox.exception(ex);
            }



            var hwInfoStr = $"processorId {processorId} baseBoardSerialNumber {baseBoardSerialNumber} macAddress {macAddress} hardDiskVolumeSerialNumber {hardDiskVolumeSerialNumber}";
            var hwInfoStrBytes = Encoding.UTF8.GetBytes(hwInfoStr);

            using (var sha = new SHA1Managed())
            {
                var hashBytes = sha.ComputeHash(hwInfoStrBytes);
                _pcUniqueKey = BitConverter.ToString(hashBytes).Replace("-", "");
            }

            return _pcUniqueKey;
        }
    }
}
