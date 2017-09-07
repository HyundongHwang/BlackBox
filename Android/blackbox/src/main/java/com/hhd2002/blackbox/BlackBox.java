package com.hhd2002.blackbox;

import android.annotation.SuppressLint;
import android.content.Context;
import android.os.AsyncTask;
import android.provider.Settings;
import android.util.Log;

import com.microsoft.azure.storage.CloudStorageAccount;
import com.microsoft.azure.storage.table.CloudTable;
import com.microsoft.azure.storage.table.CloudTableClient;
import com.microsoft.azure.storage.table.TableEntity;
import com.microsoft.azure.storage.table.TableOperation;
import com.microsoft.azure.storage.table.TableQuery;
import com.microsoft.azure.storage.table.TableServiceEntity;

import java.text.SimpleDateFormat;
import java.util.ArrayDeque;
import java.util.Date;
import java.util.concurrent.CopyOnWriteArrayList;

public class BlackBox {

    public static class LevelAndLog {
        public String level;
        public String log;
    }

    private static Context _context = null;
    private static CopyOnWriteArrayList<LevelAndLog> _logCache4Azure = new CopyOnWriteArrayList<LevelAndLog>();
    private static String _azureStorageConnectionString;

    @SuppressLint("StaticFieldLeak")
    public static void init(Context context, String azureStorageConnectionString) {
        _context = context;
        _azureStorageConnectionString = azureStorageConnectionString;

        if (_isStringNullOrEmpty(_azureStorageConnectionString))
            return;


        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... voids) {
                try {
                    CloudStorageAccount storageAccount = CloudStorageAccount.parse(_azureStorageConnectionString);
                    CloudTableClient tableClient = storageAccount.createCloudTableClient();
                    String hwid = _getHwId(_context);
                    Date now = new Date();
                    SimpleDateFormat sdf = new SimpleDateFormat("yyMMdd");
                    String dateStr = sdf.format(now);
                    CloudTable table = tableClient.getTableReference(String.format("log%sdate%s", hwid, dateStr));
                    boolean res = table.createIfNotExists();
                    TableQuery<AzureTableLogEntity> query = TableQuery.from(AzureTableLogEntity.class);
                    long maxRowKey = 0;

                    for (AzureTableLogEntity logEntity : table.execute(query)) {
                        long rowKey = Long.parseLong(logEntity.getRowKey());

                        if (rowKey > maxRowKey) {
                            maxRowKey = rowKey;
                        }
                    }

                    AzureTableLogEntity.setLastRowKey(maxRowKey);

                    while (true) {

                        if (_logCache4Azure.size() == 0) {
                            Thread.sleep(10 * 1000);
                            continue;
                        }

                        _writeLogToAzureTable();
                    }

                } catch (Exception e) {
                    e.printStackTrace();
                }

                return null;
            }

            private void _writeLogToAzureTable() {
                ArrayDeque<LevelAndLog> tmpLogQueue = new ArrayDeque<>();

                while (_logCache4Azure.size() > 0) {
                    LevelAndLog lineLog = _logCache4Azure.get(0);
                    _logCache4Azure.remove(0);
                    tmpLogQueue.push(lineLog);

                    try {
                        CloudStorageAccount storageAccount = CloudStorageAccount.parse(_azureStorageConnectionString);
                        CloudTableClient tableClient = storageAccount.createCloudTableClient();
                        String hwid = _getHwId(_context);
                        Date now = new Date();
                        SimpleDateFormat sdf = new SimpleDateFormat("yyMMdd");
                        String dateStr = sdf.format(now);
                        CloudTable table = tableClient.getTableReference(String.format("log%sdate%s", hwid, dateStr));
                        boolean res = table.createIfNotExists();

                        while (tmpLogQueue.size() > 0) {
                            LevelAndLog tmpLog = tmpLogQueue.pop();
                            AzureTableLogEntity log = new AzureTableLogEntity();
                            log.setPartitionKey(tmpLog.level);
                            log.setLogLevel(tmpLog.level);
                            log.setLog(tmpLog.log);
                            TableOperation insertOperation = TableOperation.insertOrReplace(log);
                            table.execute(insertOperation);
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }

                }
            }
        }.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
    }

    @SuppressLint("HardwareIds")
    public static void d(String format, Object... args) {
        _writeLogAsync("DEBUG", format, args);
    }

    @SuppressLint("DefaultLocale")
    private static void _writeLogAsync(String logLevel, String format, Object[] args) {
        String log = args.length > 0 ? String.format(format, args) : format;
        long tid = Thread.currentThread().getId();
        StackTraceElement[] stList = Thread.currentThread().getStackTrace();
        String tag = String.format("%s:%d", stList[4].getFileName(), stList[4].getLineNumber());
        Date now = new Date();
        SimpleDateFormat sdf = new SimpleDateFormat("yy/MM/dd|HH:mm:ss");
        String nowDateTimeStr = sdf.format(now);
        log = String.format("CALL[%s]TID[%04d] %s", tag, tid, log);
        String decoLog = String.format("[%s][%s]%s", logLevel, nowDateTimeStr, log);

        switch (logLevel) {
            case "ERROR":
                Log.e(tag, log);
                break;
            case "WARNI":
                Log.w(tag, log);
                break;
            case "INFOR":
                Log.i(tag, log);
                break;
            case "DEBUG":
                Log.d(tag, log);
                break;
            case "VERBO":
                Log.v(tag, log);
                break;
            default:
                Log.v(tag, log);
                break;
        }

        LevelAndLog laLog = new LevelAndLog();
        laLog.level = logLevel;
        laLog.log = log;
        _logCache4Azure.add(laLog);
    }

    public static void e(String format, Object... args) {
        _writeLogAsync("ERROR", format, args);
    }

    public static void i(String format, Object... args) {
        _writeLogAsync("INFOR", format, args);
    }

    public static void v(String format, Object... args) {
        _writeLogAsync("VERBO", format, args);
    }

    public static void w(String format, Object... args) {
        _writeLogAsync("WARNI", format, args);
    }

    @SuppressLint("StaticFieldLeak")
    public static void session(String sessionStr) {
        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... voids) {
                try {
                    CloudStorageAccount storageAccount = CloudStorageAccount.parse(_azureStorageConnectionString);
                    CloudTableClient tableClient = storageAccount.createCloudTableClient();
                    CloudTable table = tableClient.getTableReference("session");
                    boolean res = table.createIfNotExists();
                    TableQuery<AzureTableSessionEntity> query = new TableQuery<>();
                    AzureTableSessionEntity item = new AzureTableSessionEntity();
                    item.setRowKey(_getHwId(_context));
                    item.setSessionStr(sessionStr);
                    TableOperation insertOperation = TableOperation.insertOrReplace(item);
                    table.execute(insertOperation);
                } catch (Exception e) {
                    e.printStackTrace();
                }

                return null;
            }
        }.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
    }








    private static String _getHwId(Context context) {

        if (context == null) {
            return "contextisnull";
        }



        @SuppressLint("HardwareIds")
        String hwid = Settings.Secure.getString(
                context.getContentResolver(),
                Settings.Secure.ANDROID_ID);

        return hwid;
    }

    private static boolean _isStringNullOrEmpty(String str) {
        boolean result = (str == null || str.equals(""));
        return result;
    }
}



