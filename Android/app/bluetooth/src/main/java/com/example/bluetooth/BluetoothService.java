package com.example.bluetooth;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.UUID;

public class BluetoothService{

    private static BluetoothService instance;
    private static final UUID MY_UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");
    private static final String TAG = "BluetoothManager-Plugin";
    public static final int STATE_NONE = 0;
    public static final int STATE_LISTENING = 1;
    public static final int STATE_CONNECTING = 2;
    public static final int STATE_CONNECTED = 3;

    public static final String BT_MESSAGE = "Message";
    private static String gameObject;
    private static String serverObject;

    public static BluetoothService getInstance(){
        if (instance == null){
            instance = new BluetoothService();
        }
        return instance;
    }

    public static void setGameObject(String name){
        gameObject = name;
    }
    public static String getGameObject(){
        return gameObject;
    }
    public static void setServerObject(String name){
        serverObject = name;
    }
    public static String getServerObject(){
        return serverObject;
    }

    public static class BluetoothBroadcastReceiver extends BroadcastReceiver {
        @Override
        public void onReceive(Context context, Intent intent) {
            if (intent.getAction().equals(BluetoothAdapter.STATE_DISCONNECTED)){
                state = STATE_NONE;
                UnityPlayer.UnitySendMessage(serverObject, BT_MESSAGE, "status:disconnected");
            }
            if (intent.getAction().equals(BluetoothAdapter.ACTION_STATE_CHANGED)) {
                UnityPlayer.UnitySendMessage(serverObject, BT_MESSAGE, "status:"+intent.getStringExtra("EXTRA_STATE"));
            }
        }
    }

    private boolean isEnabled;
    private boolean isAvailable;
    private BluetoothAdapter btAdapter;
    private AcceptThread acceptThread;
    private ConnectThread connectThread;
    private ConnectedThread connectedThread;
    private static int state;

    private BluetoothService() {
        btAdapter = BluetoothAdapter.getDefaultAdapter();
        isAvailable = btAdapter != null;
        isEnabled = isAvailable && btAdapter.isEnabled();
        state = STATE_NONE;
        gameObject = "BluetoothObject";
    }

    public static synchronized int getState(){
        return state;
    }

    public List<BluetoothDevice> getBondedDevices() {
        return new ArrayList<>(btAdapter.getBondedDevices());
    }

    public boolean isEnabled() {
        return isEnabled;
    }

    public boolean isAvailable() {
        return isAvailable;
    }

    public boolean enable() {
        return btAdapter.enable();
    }

    public boolean disable() {
        return btAdapter.disable();
    }

    private void connected(BluetoothDevice device, BluetoothSocket socket){
        if (acceptThread != null){
            acceptThread.cancel();
            acceptThread = null;
        }
        if (connectThread != null) {
            connectThread.cancel();
            connectThread = null;
        }
        if (connectedThread != null) {
            connectedThread.cancel();
            connectedThread = null;
        }

        connectedThread = new ConnectedThread(socket);
        connectedThread.start();
    }

    public synchronized void connect(BluetoothDevice device) {
        if (state == STATE_CONNECTING) {
            if (connectThread != null) {
                connectThread.cancel();
                connectThread = null;
            }
        }

        if (connectedThread != null) {
            connectedThread.cancel();
            connectedThread = null;
        }

        connectThread = new ConnectThread(device);
        connectThread.start();
    }

    public synchronized void stop() {
        if (connectThread != null){
            connectThread.cancel();
            connectThread = null;
        }

        if (connectedThread != null) {
            connectedThread.cancel();
            connectedThread = null;
        }

        if (acceptThread != null) {
            acceptThread.cancel();
            acceptThread = null;
        }

        UnityPlayer.UnitySendMessage(serverObject, BT_MESSAGE, "status:server stopped");
        state = STATE_NONE;
    }

    public void start(){

        if (connectThread != null){
            connectThread.cancel();
            connectThread = null;
        }

        if (connectedThread != null){
            connectedThread.cancel();
            connectedThread = null;
        }

        if (acceptThread != null) {
            acceptThread.cancel();
            acceptThread = null;

        }

        Log.i(TAG, "Starting server");
        acceptThread = new AcceptThread();
        acceptThread.start();
    }

    private class AcceptThread extends Thread {

        private BluetoothServerSocket serverSocket;

        public AcceptThread() {
            try{
                serverSocket = btAdapter.listenUsingRfcommWithServiceRecord("CONTROLLER", MY_UUID);
                UnityPlayer.UnitySendMessage(serverObject, BT_MESSAGE, "status:listening");
                state = STATE_LISTENING;
            } catch (IOException ex){
                Log.e(TAG, "Could not open socket", ex);
            }
        }

        public void run() {
            BluetoothSocket socket = null;
            while (state != STATE_CONNECTED) {
                try {
                    socket = serverSocket.accept();
                } catch (IOException ex) {
                    Log.e(TAG, "Socket's accept() method failed", ex);
                    break;
                }

                if (socket != null) {
                    synchronized (BluetoothService.this) {
                        switch (state){
                            case STATE_LISTENING:
                            case STATE_CONNECTING:
                                connected(socket.getRemoteDevice(), socket);
                                UnityPlayer.UnitySendMessage(serverObject, BT_MESSAGE, "connected:"+socket.getRemoteDevice());
                                break;
                            case STATE_NONE:
                            case STATE_CONNECTED:
                                cancel();
                                break;
                        }
                    }
                }
            }
        }

        public void cancel(){
            try {
                Log.i(TAG,"Closing server socket");
                serverSocket.close();
            } catch (IOException e) {
                Log.e(TAG, "Could not close server socket", e);
            }
        }

    }

    private class ConnectedThread extends Thread {
        private final BluetoothSocket socket;
        private final InputStream inputStream;
        private final OutputStream outputStream;

        public ConnectedThread(BluetoothSocket socket){
            this.socket = socket;
            InputStream tmpI = null;
            OutputStream tmpO = null;

            try {
                tmpI = socket.getInputStream();
                tmpO = socket.getOutputStream();
            }catch (IOException e) {
                Log.e(TAG, "Could not get streams", e);
            }

            this.inputStream = tmpI;
            this.outputStream = tmpO;
            state = STATE_CONNECTED;
        }

        public void run() {
            byte buffer[] = new byte[1024];
            int bytes;
            Log.i(TAG, "State: " + state);
            while (socket.isConnected()) {
                try{
                    bytes = inputStream.read(buffer);
                    String message = new String(buffer, 0, bytes);
                    UnityPlayer.UnitySendMessage(gameObject, BT_MESSAGE, message);
                    Log.i(TAG, message);
                }catch (IOException e){
                    Log.e(TAG, "Could not read", e);
                    cancel();
                    break;
                }
            }
        }

        public void write(byte[] buffer){
            try{
                outputStream.write(buffer);
            }catch (IOException e){
                cancel();
                Log.e(TAG, "Exception during write", e);
            }
        }

        public void cancel(){
            try {
                Log.i(TAG, "Cancelling connected");
                inputStream.close();
                outputStream.close();
                socket.close();
                state = STATE_NONE;
            }catch (IOException e){
                Log.e(TAG, "Could not close", e);
            }
        }
    }

    private class ConnectThread extends Thread {
        private BluetoothSocket socket;
        private BluetoothDevice device;

        public ConnectThread(BluetoothDevice device){
            this.device = device;
            BluetoothSocket tmp = null;
            try{
                Log.i(TAG, "UUIDs: " + Arrays.toString(device.getUuids()));
                socket = this.device.createRfcommSocketToServiceRecord(device.getUuids()[3].getUuid());
            }catch(IOException e) {
                Log.e(TAG, "Socket creation failed");
            }
            state = STATE_CONNECTING;
        }

        public void run() {
            btAdapter.cancelDiscovery();
            try {
                socket.connect();
            } catch (IOException e){
                cancel();
            }
            synchronized (BluetoothService.this){
                connectThread = null;
            }
            connected(device, socket);
        }

        public void cancel(){
            try{
                Log.i(TAG, "Cancelling connect");
                socket.close();
            } catch (IOException e){
                Log.e(TAG, "Could not close client socket");
            }
        }
    }
}
