package com.hhd2002.blackboxtest;

import android.support.multidex.MultiDexApplication;

import com.hhd2002.blackbox.BlackBox;

public class MainApplication extends MultiDexApplication {

    public static final String _AZURE_STORAGE_CONNECTION_STRING = "DefaultEndpointsProtocol=https;AccountName=blackboxandroid;AccountKey=cZg2h0pMVDjDEEBuA/nDw0nXGttc7rAv5o977ZXO3IdCHt9UAE+U7X0XA5ZfxBrkJ0NKTymCCXRzvmHUSOEKIw==;EndpointSuffix=core.windows.net";

    @Override
    public void onCreate() {
        super.onCreate();

        BlackBox.init(this,
                _AZURE_STORAGE_CONNECTION_STRING);
    }
}
