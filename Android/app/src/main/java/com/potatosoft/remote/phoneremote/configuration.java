package com.potatosoft.remote.phoneremote;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.InetAddress;
import java.net.MulticastSocket;
import java.net.UnknownHostException;

public class configuration extends Activity {

    private UdpClient Udp;
    private String CurrentEndpoint;

    private Button RefreshButton;
    private Button ConnectButton;
    private TextView ErrorText;
    private EditText HostnameEditText;

    private final int PORT = 37015;
    private final String HOSTNAME = "192.168.10.4";

    @Override
    protected void onCreate(Bundle savedInstanceState) {

        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_configuration) ;
        Udp = new UdpClient(PORT);

        RefreshButton = (Button)findViewById(R.id.button2);
        ConnectButton = (Button)findViewById(R.id.button);
        ErrorText = (TextView)findViewById(R.id.textView);
        HostnameEditText = (EditText)findViewById(R.id.editText);

        Udp.Listen();
        Udp.Target(HOSTNAME);
    }

    public void Button_Refresh_OnClick(View view) {
        String message = Udp.GetMessage();


        try {
            Udp.Send("test");
        }catch(Exception e)
        {
            e.printStackTrace();
        }

        if(message.length() <= 0)
        return;

        CurrentEndpoint = message;
        RefreshButton.setText("Copy: " + message);
    }

    public void Button_Connect_OnClick(View view) {
        Udp.StopListening();

        Intent intent = new Intent(this, MousepadProto.class);
        intent.putExtra(MousepadProto.INTENT_KEY_CONNECTION_HOSTNAME, HOSTNAME);
        intent.putExtra(MousepadProto.INTENT_KEY_CONNECTION_PORT, PORT);

        startActivity(intent);
    }
}
