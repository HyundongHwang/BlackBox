using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlackBoxLib
{
    public static class BlackBox
    {
        public class LevelAndLog
        {
            public string Level { get; set; }
            public string Log { get; set; }
        }

        public static List<LogOutputDirections> _outputDirectionList { get; set; }

        private static ConcurrentQueue<string> _logCache4File = new ConcurrentQueue<string>();

        private static ConcurrentQueue<LevelAndLog> _logCache4Azure = new ConcurrentQueue<LevelAndLog>();

        private static string _azureStorageConnectionString;

        public static void Init(List<LogOutputDirections> outputDirectionList, string azureStorageConnectionString = null)
        {
            _outputDirectionList = outputDirectionList;

            if (_outputDirectionList.Contains(LogOutputDirections.LocalFile))
            {
                TaskEx.Run(async () =>
                {
                    // 로그로테이션 테스트 준비용 로그파일 무한생성 ps 스크립트
                    // 00..99 | %{ cp Coconut.exe.log ("Coconut.exe_1702{0:D2}.log" -f $_) }

                    var exePath = Assembly.GetEntryAssembly().Location;
                    var exeDir = System.IO.Path.GetDirectoryName(exePath);
                    var exeFileName = System.IO.Path.GetFileName(exePath);
                    var logFileCount = Directory.GetFiles(exeDir, $"{exeFileName}_*.log").Count();

                    if (logFileCount > 60)
                    {
                        var delLogFileList = Directory.GetFiles(exeDir, $"{exeFileName}_*.log").OrderBy(item => item).Take(logFileCount - 30);

                        foreach (var delLogFile in delLogFileList)
                        {
                            File.Delete(delLogFile);
                        }
                    }

                    while (true)
                    {
                        if (_logCache4File.Count == 0)
                        {
                            await TaskEx.Delay(TimeSpan.FromSeconds(10));
                            continue;
                        }

                        _WriteLogToLocalFile();
                    }
                });
            }



            if (_outputDirectionList.Contains(LogOutputDirections.LocalSqliteDb))
            {
            }




            if (_outputDirectionList.Contains(LogOutputDirections.AzureTable))
            {
                TaskEx.Run(async () =>
                {
                    _azureStorageConnectionString = azureStorageConnectionString;

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
            }




        }

        public static void Flush()
        {
            if (_outputDirectionList.Contains(LogOutputDirections.LocalFile))
            {
                if (_logCache4File.Count == 0)
                    return;

                _WriteLogToLocalFile();
            }


            if (_outputDirectionList.Contains(LogOutputDirections.AzureTable))
            {
                if (_logCache4Azure.Count == 0)
                    return;

                _WriteLogToAzureTable();
            }
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

            if (_outputDirectionList.Contains(LogOutputDirections.VsConsole))
            {
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
            }

            if (_outputDirectionList.Contains(LogOutputDirections.LocalFile))
            {
                var oneline = decoLog.Split("\r\n".ToArray()).FirstOrDefault();
                var onelineunder100 = oneline.Substring(0, oneline.Length < 100 ? oneline.Length : 100);
                _logCache4File.Enqueue(onelineunder100);
            }

            if (_outputDirectionList.Contains(LogOutputDirections.LocalSqliteDb))
            {
            }

            if (_outputDirectionList.Contains(LogOutputDirections.AzureTable))
            {
                _logCache4Azure.Enqueue(new LevelAndLog { Level = logLevel, Log = log, });
            }
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
