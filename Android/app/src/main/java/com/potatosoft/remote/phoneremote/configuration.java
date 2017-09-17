package com.potatosoft.remote.phoneremote;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.net.wifi.WifiManager;
import android.os.AsyncTask;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import java.net.DatagramPacket;
import java.net.DatagramSocket;

public class configuration extends Activity {

    private Button RefreshButton;
    private Button ConnectManualButton;
    private Button ConnectFoundButton;
    private TextView FoundEndpointView;
    private TextView ErrorText;
    private EditText HostnameEditText;

    private final int DEBUG_PORT = 37015;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_configuration) ;

        RefreshButton = (Button)findViewById(R.id.button2);
        ConnectManualButton = (Button)findViewById(R.id.button);
        ConnectFoundButton = (Button)findViewById(R.id.button_connect_found);
        FoundEndpointView = (TextView) findViewById(R.id.text_connection_found);
        ErrorText = (TextView)findViewById(R.id.textView);
        HostnameEditText = (EditText)findViewById(R.id.editText);

        ConnectFoundButton.setEnabled(false);
    }

    public void Button_Refresh_OnClick(View view) {
        ListenForEndpoint listener = new ListenForEndpoint();
        listener.execute();
    }

    public void Button_Connect_Found_OnClick(View view) {
        LaunchMouseScreen(FoundEndpointView.getText().toString(), DEBUG_PORT);
    }

    public void Button_Connect_OnClick(View view) {
        LaunchMouseScreen(HostnameEditText.getText().toString(), DEBUG_PORT);
    }

    private void LaunchMouseScreen(String hostname, int port){
        Intent intent = new Intent(this, MousepadProto.class);
        intent.putExtra(MousepadProto.INTENT_KEY_CONNECTION_HOSTNAME, hostname);
        intent.putExtra(MousepadProto.INTENT_KEY_CONNECTION_PORT, port);

        startActivity(intent);
    }

    private boolean IsListening = false;
    private class ListenForEndpoint extends AsyncTask<Void, Void, String> {
        @Override
        protected void onPreExecute() {
            RefreshButton.setEnabled(false);
        }

        @Override
        protected String doInBackground(Void... params) {
            if(IsListening)
                return null;
            IsListening = true;

            WifiManager wifi = (WifiManager) getSystemService(Context.WIFI_SERVICE);
            WifiManager.MulticastLock multicastLock = wifi.createMulticastLock("phone-remote-broadcast-lock");
            multicastLock.setReferenceCounted(true);
            multicastLock.acquire();

            String message = null;

            try {
                DatagramSocket socket = new DatagramSocket(DEBUG_PORT);
                socket.setBroadcast(true);
                byte[] buffer = new byte[1024];
                DatagramPacket packet = new DatagramPacket(buffer, buffer.length);
                socket.receive(packet);

                byte[] data = packet.getData();
                int length = packet.getLength();
                message = new String(data, 0, length);

                Log.d("udp", "got message " + message);
                socket.close();
            }
            catch(Exception e) {
                Log.d("udp", "failed message get");
            }

            if (multicastLock != null) {
                multicastLock.release();
            }

            IsListening = false;
            return message;
        }

        @Override
        protected void onPostExecute(String message) {
            RefreshButton.setEnabled(true);
            IsListening = false;
            if(message != null) {
                FoundEndpointView.setText(message);
                ConnectFoundButton.setEnabled(true);
            }
        }
    }
}
