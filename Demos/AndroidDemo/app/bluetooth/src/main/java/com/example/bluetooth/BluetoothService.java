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

public class BluetoothService {

    public static class BluetoothBroadcastReceiver extends BroadcastReceiver {
        @Override
        public void onReceive(Context context, Intent intent) {
            String action = intent.getAction();
            int state = intent.getExtras().getInt(BluetoothAdapter.EXTRA_STATE);
            switch (state) {
                case BluetoothAdapter.STATE_ON:
                    send(serverObject, "bluetooth.on");
                    break;
                case BluetoothAdapter.STATE_OFF:
                    send(serverObject, "bluetooth.off");
                    break;
            }

            if (action != null && action.equals("android.bluetooth.device.action.ACL_CONNECTED")) {
                send(serverObject, "bluetooth.connected");
            }
            if (action != null && action.equals("android.bluetooth.device.action.ACL_DISCONNECTED")) {
                send(serverObject, "bluetooth.disconnected");
            }
        }
    }

    private static BluetoothService instance;
    private static final String SERIAL_UUID = "00001101-0000-1000-8000-00805F9B34FB";
    private static final String TAG = "BluetoothManager-Plugin";
    private static final int STATE_NONE = 0;
    private static final int STATE_LISTENING = 1;
    private static final int STATE_CONNECTING = 2;
    private static final int STATE_CONNECTED = 3;

    private static final String BT_MESSAGE = "Message";
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

    private BluetoothAdapter btAdapter;
    private AcceptThread acceptThread;
    private ConnectThread connectThread;
    private ConnectedThread connectedThread;
    private static int state;
    private static boolean runningUnity = true;

    public static synchronized int getState(){
        return state;
    }

    public static void setUnity(boolean value) {
        runningUnity = value;
    }

    public static boolean isUnity() {
        return runningUnity;
    }

    private static void send(String to, String message) {
        if (runningUnity)
            UnityPlayer.UnitySendMessage(to, BT_MESSAGE, message);
    }

    private BluetoothService() {
        btAdapter = BluetoothAdapter.getDefaultAdapter();
        state = STATE_NONE;
        gameObject = "BluetoothObject";
    }

    public List<BluetoothDevice> getBondedDevices() {
        return new ArrayList<>(btAdapter.getBondedDevices());
    }

    public boolean isEnabled() {
        return btAdapter.isEnabled();
    }

    public boolean isListening() { return state == STATE_LISTENING; }

    public boolean isConnected() { return state == STATE_CONNECTED; }

    public boolean enable() {
        Log.i(TAG, "Bluetooth enabled");
        return btAdapter.enable();
    }

    public boolean disable() {
        return btAdapter.disable();
    }

    public void write(String data) {
        if (state == STATE_CONNECTED && connectedThread != null) {
            connectedThread.write(data.getBytes());
        }
    }

    public synchronized void connect(BluetoothDevice device) {
        connect(device, SERIAL_UUID);
    }

    public synchronized void connect(BluetoothDevice device, String uuid) {
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

        connectThread = new ConnectThread(device, uuid);
        connectThread.start();
    }

    public void startServer() {
        startServer(SERIAL_UUID, "Serial port");
    }

    public void startServer(String name) {
        startServer(name, SERIAL_UUID);
    }

    public synchronized void startServer(String name, String uuid) {

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
        acceptThread = new AcceptThread(name, uuid);
        acceptThread.start();
    }

    public synchronized void stopServer() {
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
        Log.i(TAG, "Server stopped");
        send(serverObject, "server.stopped");
        state = STATE_NONE;
    }

    private void connected(BluetoothDevice device, BluetoothSocket socket) {
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

    private class AcceptThread extends Thread {

        private BluetoothServerSocket serverSocket;

        AcceptThread(String name, String uuidStr) {
            try{
                serverSocket = btAdapter.listenUsingRfcommWithServiceRecord(name, UUID.fromString(uuidStr));
                send(serverObject, "server.listening");
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
                    send(serverObject, "server.failed_connection");
                    Log.e(TAG, "Socket's accept() method failed", ex);
                    break;
                }

                if (socket != null) {
                    synchronized (BluetoothService.this) {
                        switch (state){
                            case STATE_LISTENING:
                            case STATE_CONNECTING:
                                connected(socket.getRemoteDevice(), socket);
                                send(serverObject,  "server.connected."+socket.getRemoteDevice().getAddress());
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

        void cancel(){
            try {
                Log.i(TAG,"Closing server socket");
                serverSocket.close();
            } catch (IOException e) {
                Log.e(TAG, "Could not close server socket", e);
            }
        }

    }

    private static class ConnectedThread extends Thread {
        private final BluetoothSocket socket;
        private final InputStream inputStream;
        private final OutputStream outputStream;

        ConnectedThread(BluetoothSocket socket){
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
            byte[] buffer = new byte[1024];
            int bytes;
            Log.i(TAG, "State: " + state);
            while (socket.isConnected()) {
                try{
                    bytes = inputStream.read(buffer);
                    String message = new String(buffer, 0, bytes);
                    send(gameObject, message);
                    Log.i(TAG, message);
                }catch (IOException e){
                    Log.e(TAG, "Could not read", e);
                    cancel();
                    break;
                }
            }
        }

        void write(byte[] buffer){
            try{
                outputStream.write(buffer);
            }catch (IOException e){
                cancel();
                Log.e(TAG, "Exception during write", e);
            }
        }

        void cancel(){
            try {
                inputStream.close();
                outputStream.close();
                socket.close();
            }catch (IOException e){
                Log.e(TAG, "Could not close", e);
            }
        }
    }

    private class ConnectThread extends Thread {
        private BluetoothSocket socket;
        private BluetoothDevice device;

        ConnectThread(BluetoothDevice device, String uuid){
            this.device = device;
            try{
                socket = this.device.createRfcommSocketToServiceRecord(UUID.fromString(uuid));
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

        void cancel(){
            try{
                Log.i(TAG, "Cancelling connect");
                socket.close();
            } catch (IOException e){
                Log.e(TAG, "Could not close client socket");
            }
        }
    }

}
