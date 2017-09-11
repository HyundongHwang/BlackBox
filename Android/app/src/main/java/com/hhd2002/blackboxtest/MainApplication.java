package com.hhd2002.blackboxtest;

import android.support.multidex.MultiDexApplication;

import com.hhd2002.blackbox.BlackBox;

public class MainApplication extends MultiDexApplication {

    @Override
    public void onCreate() {
        super.onCreate();

        BlackBox.init(this, getString(R.string.azure_storage_connection));
    }
}
