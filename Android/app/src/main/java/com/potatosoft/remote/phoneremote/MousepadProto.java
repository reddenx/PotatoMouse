package com.potatosoft.remote.phoneremote;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;

public class MousepadProto extends Activity {

    public static final String INTENT_KEY_CONNECTION_HOSTNAME = "com.potatosoft.remote.phoneremote.hostname";
    public static final String INTENT_KEY_CONNECTION_PORT = "com.potatosoft.remote.phoneremote.port";

    private int Port = 37015;
    private String Hostname;
    private UdpClient Udp;

    private Button Button_LeftClick;
    private Button Button_RightClick;
    private Button Button_MouseNub;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_mousepad_proto);

        Intent intent = getIntent();
        int intentPort = intent.getIntExtra(INTENT_KEY_CONNECTION_PORT, -1);
        String hostname = intent.getStringExtra(INTENT_KEY_CONNECTION_HOSTNAME);

        if(intentPort == -1 || hostname == null || hostname.length() == 0) {
            //entered without intent information, deal with it
        }

        Button_LeftClick = (Button)findViewById(R.id.button_left_click);
        Button_RightClick = (Button)findViewById(R.id.button_right_click);
        Button_MouseNub = (Button)findViewById(R.id.button_mouse_nub);

        Button_MouseNub.setOnTouchListener(NubClickListener);

        Udp = new UdpClient(Port);
        Udp.Target(Hostname);
    }

    public void Button_LeftClick_OnClick(View view) {
    }

    public void Button_RightClick_OnClick(View view) {
    }

    private View.OnTouchListener NubClickListener = new View.OnTouchListener() {
        @Override
        public boolean onTouch(View v, MotionEvent event) {

            switch(event.getAction()) {
                case MotionEvent.ACTION_BUTTON_PRESS: {
                    break;
                }
                case MotionEvent.ACTION_BUTTON_RELEASE: {
                    break;
                }
            }

            return true; //always consume, this button shouldn't really be interactive
        }
    };
}
