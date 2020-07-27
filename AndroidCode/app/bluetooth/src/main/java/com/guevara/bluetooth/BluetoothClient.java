package com.guevara.bluetooth;

import android.app.Activity;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.util.Log;

import java.io.IOException;
import java.util.UUID;

public class BluetoothClient extends BluetoothService {

    private ConnectedThread connectedThread;
    private ConnectThread connectThread;

    public BluetoothClient() {
        super();
    }

    public BluetoothClient(Activity activity) {
        super(activity);
    }

    public void send(String message) {
        if (connectedThread != null) {
            connectedThread.write(message.getBytes());
        }
    }

    private void connected(BluetoothSocket socket) {
        connectedThread = new ConnectedThread(socket);
        connectedThread.setOnDisconnectEvent(() -> {
            onStatusListener.OnStatus("client.connection_lost");
            //super.send(getServerObject(), "client.connection_lost");
        });
        connectedThread.start();
    }

    public synchronized void connect(BluetoothDevice device) {
        connect(device, SERIAL_UUID);
    }

    public synchronized void connect(BluetoothDevice device, String uuid) {
        if (connectedThread != null) {
            connectedThread.cancel();
            connectedThread = null;
        }
        if (connectThread != null) {
            connectThread.cancel();
            connectThread = null;
        }
        connectThread = new ConnectThread(device, uuid);
        connectThread.start();
    }

    public boolean connect(String address, String uuid) {
        BluetoothDevice dev = btAdapter.getRemoteDevice(address);
        if (dev == null) return false;
        connect(dev, uuid);
        return true;
    }

    public boolean connect(String address) {
        return connect(address, SERIAL_UUID);
    }

    public void disconnect() {
        connectedThread.cancel();
        onStatusListener.OnStatus("client.disconnected");
        //send(getServerObject(), "client.disconnected");
        setState(STATE_NONE);
    }

    private class ConnectThread extends Thread {
        private BluetoothSocket socket;
        private BluetoothDevice device;

        ConnectThread(BluetoothDevice device, String uuid){
            btAdapter.cancelDiscovery();
            this.device = device;
            try{
                socket = this.device.createRfcommSocketToServiceRecord(UUID.fromString(uuid));
            }catch(IOException e) {
                onStatusListener.OnStatus("client.error.COULD_NOT_CREATE_SOCKET");
                //send(getServerObject(), "client.error.COULD_NOT_CREATE_SOCKET");
                Log.e(TAG, "Socket creation failed");
            }
            setState(STATE_CONNECTING);
        }

        public void run() {

            try {
                socket.connect();
                setState(STATE_CONNECTED);
                onStatusListener.OnStatus("client.connected." + socket.getRemoteDevice().getAddress());
                //send(getServerObject(), "client.connected");
            } catch (IOException e){
                onStatusListener.OnStatus("client.error.COULD_NOT_CONNECT");
                //send(getServerObject(), "client.error.COULD_NOT_CONNECT");
                Log.e(TAG,"Error", e);
                cancel();
            }
            connected(socket);
        }

        void cancel(){
            try{
                socket.close();
            } catch (IOException e){
                Log.e(TAG, "Could not close client socket");
            }
        }
    }
}
