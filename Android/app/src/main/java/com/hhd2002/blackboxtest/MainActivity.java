package com.hhd2002.blackboxtest;

import android.os.AsyncTask;
import android.os.Bundle;
import android.support.annotation.Nullable;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.widget.Button;

import com.hhd2002.blackbox.BlackBox;

import butterknife.BindView;
import butterknife.ButterKnife;
import butterknife.OnClick;

/**
 * Created by hhd on 2017-09-07.
 */

public class MainActivity extends AppCompatActivity {
    @BindView(R.id.btn_hello)
    Button btnHello;
    @BindView(R.id.btn_world)
    Button btnWorld;
    @BindView(R.id.btn_save_session)
    Button btnSaveSession;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        this.setContentView(R.layout.activity_main);
        ButterKnife.bind(this);
    }

    @OnClick(R.id.btn_hello)
    public void onBtnHelloClicked() {
        BlackBox.i("hello");
    }

    @OnClick(R.id.btn_world)
    public void onBtnWorldClicked() {
        BlackBox.d("world");
    }

    @OnClick(R.id.btn_save_session)
    public void onbtnSaveSessionClicked() {
        BlackBox.session("h2d2002@naver.com");
    }
}


