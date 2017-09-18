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

    private MousepadPrototype Joystick;
    private DatagramSocket Udp;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_mousepad_proto);

        Intent intent = getIntent();
        Port = intent.getIntExtra(INTENT_KEY_CONNECTION_PORT, -1);
        Hostname = intent.getStringExtra(INTENT_KEY_CONNECTION_HOSTNAME);

        if (Port == -1 || Hostname == null || Hostname.length() == 0) {
            //entered without intent information, deal with it
            finish();
        }

        Joystick = new MousepadPrototype();
        try {
            Udp = new DatagramSocket();
        } catch (Exception e) {
        }

        Button_LeftClick = (Button) findViewById(R.id.button_left_click);
        Button_RightClick = (Button) findViewById(R.id.button_right_click);
        Button_MouseNub = (Button) findViewById(R.id.button_mouse_nub);

        Button_MouseNub.setOnTouchListener(NubClickListener);
    }

    public void Button_LeftClick_OnClick(View view) {
        SendMessage("[click_left::]");
    }

    public void Button_RightClick_OnClick(View view) {
        SendMessage("[click_right::]");
    }

    private View.OnTouchListener NubClickListener = new View.OnTouchListener() {

        @Override
        public boolean onTouch(View v, MotionEvent event) {

            //we don't care about later touches while the 0 index is down (first one)
            if(event.getActionIndex() != 0)
                return false;

            switch(event.getAction()) {
                case MotionEvent.ACTION_DOWN: {
                    MotionEvent.PointerCoords coords = new MotionEvent.PointerCoords();
                    event.getPointerCoords(event.getActionIndex(), coords);

                    Joystick.TouchDown(coords.x, coords.y);
                    break;
                }
                case MotionEvent.ACTION_UP: {
                    MotionEvent.PointerCoords coords = new MotionEvent.PointerCoords();
                    event.getPointerCoords(event.getActionIndex(), coords);

                    Joystick.TouchUp(coords.x, coords.y);
                    break;
                }
                case MotionEvent.ACTION_MOVE: {
                    MotionEvent.PointerCoords coords = new MotionEvent.PointerCoords();
                    event.getPointerCoords(event.getActionIndex(), coords);

                    Joystick.TouchPositionChanged(coords.x, coords.y);
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

            Udp.send(packet);
        }
        catch (Exception e) {
            Log.e("udp", "udp error " + e.getMessage());
        }
    }


    private class MousepadPrototype {
        private float LastSent_X;
        private float LastSent_Y;

        private float minimum_movement_to_send = 5;

        public void TouchUp(float x, float y) {
            SendMessage("[up:" + x + "," + y + ":]");
        }

        public void TouchDown(float x, float y) {
            SendMessage("[down:" + x + "," + y + ":]");
        }

        public void TouchPositionChanged(float x, float y)
        {
            if(Math.abs(x - LastSent_X) > minimum_movement_to_send || Math.abs(y - LastSent_Y) > minimum_movement_to_send) {
                LastSent_X = x;
                LastSent_Y = y;
                SendMessage("[move:" + x + "," + y + ":]");
            }
        }
    }
}
