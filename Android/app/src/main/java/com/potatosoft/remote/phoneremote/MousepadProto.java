package com.potatosoft.remote.phoneremote;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;

import com.potatosoft.remote.phoneremote.Utilities.Subscriber;
import com.potatosoft.remote.phoneremote.Utilities.Vector2;
import com.potatosoft.remote.phoneremote.mousepad.MousepadStateHandler;

import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;

public class MousepadProto extends Activity {

    public static final String INTENT_KEY_CONNECTION_HOSTNAME = "com.potatosoft.remote.phoneremote.hostname";
    public static final String INTENT_KEY_CONNECTION_PORT = "com.potatosoft.remote.phoneremote.port";

    private int Port = 37015;
    private String Hostname;

    private DatagramSocket Udp;

    private MousepadStateHandler StateHandler;

    private Button Button_Scrollbar;
    private Button Button_Mousepad;

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

        try {
            Udp = new DatagramSocket();
        } catch (Exception e) {
        }

        //wire up output events
        StateHandler = new MousepadStateHandler();
        StateHandler.OnLeftClick.Subscribe(new Subscriber<Void>() {
            @Override
            public void HandleMessage(Void message) {
                SendMessage(MessageBuilder.LeftClick());
            }
        });

        StateHandler.OnLeftDoubleClick.Subscribe(new Subscriber<Void>() {
            @Override
            public void HandleMessage(Void message) {
                SendMessage(MessageBuilder.LeftDoubleClick());
            }
        });

        StateHandler.OnLeftDown.Subscribe(new Subscriber<Void>() {
            @Override
            public void HandleMessage(Void message) {
                SendMessage(MessageBuilder.LeftPress());
            }
        });

        StateHandler.OnLeftUp.Subscribe(new Subscriber<Void>() {
            @Override
            public void HandleMessage(Void message) {
                SendMessage(MessageBuilder.LeftRelease());
            }
        });

        StateHandler.OnMouseMoved.Subscribe(new Subscriber<Vector2<Integer>>() {
            @Override
            public void HandleMessage(Vector2<Integer> message) {
                SendMessage(MessageBuilder.MouseMove(message.X, message.Y));
            }
        });

        Button_Scrollbar = (Button) findViewById(R.id.button_scrollbar);
        Button_Mousepad = (Button) findViewById(R.id.button_mousepad);

        Button_Mousepad.setOnTouchListener(MousepadTouchListener);
        Button_Scrollbar.setOnTouchListener(ScrollbarTouchListener);
    }


    public void Button_RightClick_OnClick(View view) {
        SendMessage(MessageBuilder.RightClick());
    }

    private View.OnTouchListener ScrollbarTouchListener = new View.OnTouchListener() {

        private float LastY;
        private final int SCROLL_DISTANCE = 50;


        @Override
        public boolean onTouch(View v, MotionEvent event) {

            if (event.getActionIndex() != 0)
                return false;

            switch (event.getAction()) {
                case MotionEvent.ACTION_DOWN: {
                    return true;
                }
                case MotionEvent.ACTION_UP: {
                    return true;
                }
                case MotionEvent.ACTION_MOVE: {
                    MotionEvent.PointerCoords coords = new MotionEvent.PointerCoords();
                    event.getPointerCoords(event.getActionIndex(), coords);

                    if (LastY - coords.y > SCROLL_DISTANCE) {
                        LastY -= SCROLL_DISTANCE;
                        SendMessage(MessageBuilder.Scrollwheel(-1));
                    }

                    if (coords.y - LastY > SCROLL_DISTANCE) {
                        LastY += SCROLL_DISTANCE;
                        SendMessage(MessageBuilder.Scrollwheel(1));
                    }

                    return true;
                }
            }

            return false;
        }
    };

    private View.OnTouchListener MousepadTouchListener = new View.OnTouchListener() {

        @Override
        public boolean onTouch(View v, MotionEvent event) {

            //we don't care about later touches while the 0 index is down (first one)
            if(event.getActionIndex() != 0)
                return false;

            switch(event.getAction()) {
                case MotionEvent.ACTION_DOWN: {
                    MotionEvent.PointerCoords coords = new MotionEvent.PointerCoords();
                    event.getPointerCoords(event.getActionIndex(), coords);


                    StateHandler.InputTouchDown(coords.x, coords.y);
                    break;
                }
                case MotionEvent.ACTION_UP: {
                    MotionEvent.PointerCoords coords = new MotionEvent.PointerCoords();
                    event.getPointerCoords(event.getActionIndex(), coords);

                    StateHandler.InputTouchUp(coords.x, coords.y);
                    break;
                }
                case MotionEvent.ACTION_MOVE: {
                    MotionEvent.PointerCoords coords = new MotionEvent.PointerCoords();
                    event.getPointerCoords(event.getActionIndex(), coords);

                    StateHandler.InputTouchPositionChanged(coords.x, coords.y);
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


   /* private class MousepadPrototype {
        private float LastSent_X;
        private float LastSent_Y;

        private float minimum_movement_to_send = 2;

        public void TouchUp(float x, float y) {
        }

        public void TouchDown(float x, float y) {
            LastSent_X = x;
            LastSent_Y = y;
        }

        public void TouchPositionChanged(float x, float y) {

            if (Math.abs(x - LastSent_X) > minimum_movement_to_send || Math.abs(y - LastSent_Y) > minimum_movement_to_send) {

                int sendX = Math.round(x - LastSent_X);
                int sendY = Math.round(y - LastSent_Y);

                LastSent_X = x;
                LastSent_Y = y;
                SendMessage(MessageBuilder.MouseMove(sendX, sendY));
            }
        }
    }*/
}
