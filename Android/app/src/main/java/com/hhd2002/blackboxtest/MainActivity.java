package com.hhd2002.blackboxtest;

import android.os.Bundle;
import android.support.annotation.Nullable;
import android.support.v7.app.AppCompatActivity;
import android.widget.Button;
import android.widget.TextView;

import com.hhd2002.blackbox.BlackBox;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Timer;
import java.util.TimerTask;

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
    @BindView(R.id.btn_screen_capture)
    Button btnScreenCapture;
    @BindView(R.id.tv_current_time)
    TextView tvCurrentTime;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        this.setContentView(R.layout.activity_main);
        ButterKnife.bind(this);

        Timer timer = new Timer();

        timer.schedule(new TimerTask() {
            @Override
            public void run() {
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        Date now = new Date();
                        SimpleDateFormat sdf = new SimpleDateFormat("yyMMdd-hhmmss");
                        String dateTimeStr = sdf.format(now);
                        tvCurrentTime.setText(dateTimeStr);
                    }
                });
            }
        }, 0, 1000);
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
        BlackBox.session("h2d2002@naver.com from android");
    }

    @OnClick(R.id.btn_screen_capture)
    public void onbtnScreenCaptureClicked() {
        BlackBox.captureScreen(this);
    }
}


