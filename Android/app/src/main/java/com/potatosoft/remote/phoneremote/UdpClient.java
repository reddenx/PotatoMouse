package com.potatosoft.remote.phoneremote;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.util.Vector;

/**
 * Created by seanm_000 on 8/27/2017.
 */

public class UdpClient {

    private int Port;
    private String Hostname;
    private String LastMessage;
    private boolean Disposed;
    private Thread ListenLoopThread;

    public UdpClient(int port) {
        Port = port;
        LastMessage = "";
        Hostname = "";
        Disposed = false;
        ListenLoopThread = null;
    }

    public String GetMessage() {
        return new String(LastMessage);
    }

    public void Listen() {
        if(ListenLoopThread != null)
            return;

        ListenLoopThread = new Thread(new Runnable() {
            public void run(){
                ListenLoop();
            }
        });
        ListenLoopThread.start();
    }

    public void StopListening() {
        if(ListenLoopThread == null)
            return;
        ListenLoopThread.stop(); //the stop is acceptable in this instance
    }

    public void Target(String hostname) {
        Hostname = hostname;
    }

    public void Send(String message) throws IOException {
        if(Hostname == "")
            return;

        byte[] buffer = message.getBytes();

        InetAddress target = InetAddress.getByName(Hostname);
        DatagramPacket packet = new DatagramPacket(buffer, 0, buffer.length, target, Port);
        DatagramSocket udp = new DatagramSocket();

        udp.send(packet);
    }

    private void ListenLoop()
    {
        try {
            DatagramSocket socket = new DatagramSocket(Port);
            socket.setBroadcast(true);
            byte[] buffer = new byte[1024];
            DatagramPacket packet = new DatagramPacket(buffer, buffer.length);

            while (!Disposed) {
                Thread.sleep(500);
                socket.receive(packet);

                this.LastMessage = new String(packet.getData());
            }
        }
        catch (IOException e)
        {
            String test = "WasteTime";
        }
        catch (InterruptedException e) { }
    }
}
