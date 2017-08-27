package com.potatosoft.remote.phoneremote;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import java.net.InetAddress;

public class configuration extends Activity {

    private Networking Udp;
    private String CurrentEndpoint;

    private Button RefreshButton;
    private Button ConnectButton;
    private TextView ErrorText;
    private EditText HostnameEditText;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_configuration);
        Udp = new Networking(37015);

        RefreshButton = (Button)findViewById(R.id.button2);
        ConnectButton = (Button)findViewById(R.id.button);
        ErrorText = (TextView)findViewById(R.id.textView);
        HostnameEditText = (EditText)findViewById(R.id.editText);

        Udp.Listen();
    }

    public void Button_Refresh_OnClick(View view) {
        String message = Udp.GetMessage();

        if(message.length() <= 0)
        return;

        CurrentEndpoint = message;
        RefreshButton.setText("Copy: " + message);
    }

    public void Button_Connect_OnClick(View view) {

    }
}
