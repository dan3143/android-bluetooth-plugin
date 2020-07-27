package com.guevara.bluetooth;

import android.app.Activity;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.util.Log;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

public class BluetoothServer extends BluetoothService {

    private int maxConnections;
    private List<ConnectedThread> connections;
    private AcceptThread acceptThread;

    public BluetoothServer() {
        super();
        connections = new ArrayList<>();
        maxConnections = 5;
    }

    public BluetoothServer(Activity activity) {
        super(activity);
        connections = new ArrayList<>();
        maxConnections = 5;
    }

    public void setMaxConnections(int value) {
        if (value > 5) {
            value = 5;
        }
        this.maxConnections = value;
    }

    public void sendAll(String message) {
        connections.forEach(conn -> conn.write(message.getBytes()));
    }

    public void sendTo(String message, String address) {
        connections.stream()
                .filter(connection -> connection.getSocket().getRemoteDevice().getAddress().equals(address))
                .findFirst()
                .get().write(message.getBytes());
    }

    public void start() {
        start("BluetoothServer");
    }
    public void start(String sdpName) {
        start(sdpName, SERIAL_UUID);
    }
    public void start(String sdpName, String uuid) {
        acceptThread = new AcceptThread(sdpName, uuid);
        acceptThread.start();
    }

    public void stop() {
        if (acceptThread != null) {
            acceptThread.cancel();
        }
        setState(STATE_NONE);
        for (ConnectedThread conn : connections) {
            conn.cancel();
        }
        onStatusListener.OnStatus("server.stopped");
        //send(getServerObject(), "server.stopped");
    }

    private class AcceptThread extends Thread {

        private BluetoothServerSocket serverSocket;

        AcceptThread(String name, String uuidStr) {
            btAdapter.cancelDiscovery();
            try{
                serverSocket = btAdapter.listenUsingRfcommWithServiceRecord(name, UUID.fromString(uuidStr));
            } catch (IOException ex){
                onStatusListener.OnStatus("server.error.COULD_NOT_LISTEN");
                //send(getServerObject(), "server.error.COULD_NOT_LISTEN");
                Log.e(TAG, "Could not open socket", ex);
            }
        }

        public void run() {
            onStatusListener.OnStatus("server.listening");
            //send(getServerObject(), "server.listening");
            while (connections.size() < maxConnections) {
                setState(STATE_LISTENING);
                try {
                    BluetoothSocket socket = serverSocket.accept();
                    if (socket != null) {
                        ConnectedThread conn = new ConnectedThread(socket);
                        setState(STATE_CONNECTING);
                        conn.start();
                        connections.add(conn);
                        conn.setOnDisconnectEvent(() -> {
                            onStatusListener.OnStatus("server.disconnected." + socket.getRemoteDevice().getAddress());
                            //send(getServerObject(), "server.disconnected." + socket.getRemoteDevice().getAddress());
                            connections.remove(conn);
                        });
                        onStatusListener.OnStatus("server.connected." + socket.getRemoteDevice().getAddress());
                        //send(getServerObject(),  "server.connected." + socket.getRemoteDevice().getAddress());
                    }
                } catch (IOException ex) {
                    onStatusListener.OnStatus("server.error.COULD_NOT_ACCEPT");
                    //send(getServerObject(), "server.error.COULD_NOT_ACCEPT");
                    Log.e(TAG, "Socket's accept() method failed", ex);
                    break;
                }
            }
            onStatusListener.OnStatus("server.not_listening");
            //send(getServerObject(), "server.not_listening");
            cancel();
        }

        void cancel(){
            try {
                setState(STATE_NONE);
                serverSocket.close();
            } catch (IOException e) {
                Log.e(TAG, "Could not close server socket", e);
            }
        }
    }

}
