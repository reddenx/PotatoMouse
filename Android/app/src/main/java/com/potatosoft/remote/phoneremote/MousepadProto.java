package com.potatosoft.remote.phoneremote;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;

import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;

public class MousepadProto extends Activity {

    public static final String INTENT_KEY_CONNECTION_HOSTNAME = "com.potatosoft.remote.phoneremote.hostname";
    public static final String INTENT_KEY_CONNECTION_PORT = "com.potatosoft.remote.phoneremote.port";

    private int Port = 37015;
    private String Hostname;

    private Button Button_LeftClick;
    private Button Button_RightClick;
    private Button Button_MouseNub;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_mousepad_proto);

        Intent intent = getIntent();
        Port = intent.getIntExtra(INTENT_KEY_CONNECTION_PORT, -1);
        Hostname = intent.getStringExtra(INTENT_KEY_CONNECTION_HOSTNAME);

        if(Port == -1 || Hostname == null || Hostname.length() == 0) {
            //entered without intent information, deal with it
            finish();
        }

        Button_LeftClick = (Button)findViewById(R.id.button_left_click);
        Button_RightClick = (Button)findViewById(R.id.button_right_click);
        Button_MouseNub = (Button)findViewById(R.id.button_mouse_nub);

        Button_MouseNub.setOnTouchListener(NubClickListener);
    }

    public void Button_LeftClick_OnClick(View view) {
    }

    public void Button_RightClick_OnClick(View view) {
    }

    private View.OnTouchListener NubClickListener = new View.OnTouchListener() {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            switch(event.getAction()) {
                case MotionEvent.ACTION_DOWN: {
                    Log.d("udp-click", "down");
                    SendMessage("mouse_down");
                    break;
                }
                case MotionEvent.ACTION_UP: {
                    Log.d("udp-click", "up");
                    SendMessage("mouse_up");
                    break;
                }
                case MotionEvent.ACTION_MOVE: {
                    Log.d("udp-click", "move");
                    SendMessage("mouse_move");
                    break;
                }
            }

            return true; //always consume, this button shouldn't really be interactive
        }
    };

    private void SendMessage(String message) {
        try {
            byte[] buffer = message.getBytes();

            Log.d("udp-send", "sending to" + Hostname + ":" + Port + " message: " + message);

            InetAddress target = InetAddress.getByName(Hostname);
            DatagramPacket packet = new DatagramPacket(buffer, 0, buffer.length, target, Port);
            DatagramSocket udp = new DatagramSocket();

            udp.send(packet);
        }
        catch (Exception e) {
            Log.e("udp", "udp error " + e.getMessage());
        }
    }
}
