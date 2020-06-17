package com.example.bluetooth;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

public class BluetoothService {

    private class AcceptThread extends Thread {

        private BluetoothServerSocket serverSocket;

        AcceptThread(String name, String uuidStr) {
            btAdapter.cancelDiscovery();
            try{
                Log.i(TAG, "Listening...");
                serverSocket = btAdapter.listenUsingRfcommWithServiceRecord(name, UUID.fromString(uuidStr));
                send(serverObject, "server.listening");
                state = STATE_LISTENING;
            } catch (IOException ex){
                send(serverObject, "server.error.COULD_NOT_LISTEN");
                Log.e(TAG, "Could not open socket", ex);
            }
        }

        public void run() {
            BluetoothSocket socket = null;
            while (state != STATE_CONNECTED) {
                try {
                    socket = serverSocket.accept();
                } catch (IOException ex) {
                    send(serverObject, "server.error.COULD_NOT_ACCEPT");
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
                state = STATE_NONE;
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
                    Log.i(TAG, "Message from client: " + message);
                }catch (IOException e) {
                    send(serverObject, "socket.error.COULD_NOT_READ");
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
                send(serverObject, "socket.error.COULD_NOT_WRITE");
                Log.e(TAG, "Exception during write", e);
            }
        }

        void cancel(){
            try {
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

        ConnectThread(BluetoothDevice device, String uuid){
            btAdapter.cancelDiscovery();
            this.device = device;
            try{
                socket = this.device.createRfcommSocketToServiceRecord(UUID.fromString(uuid));
            }catch(IOException e) {
                send(serverObject, "client.error.COULD_NOT_CREATE_SOCKET");
                Log.e(TAG, "Socket creation failed");
            }
            state = STATE_CONNECTING;
        }

        public void run() {

            try {
                socket.connect();
                state = STATE_CONNECTED;
                send(serverObject, "client.connected");
            } catch (IOException e){
                send(serverObject, "client.error.COULD_NOT_CONNECT");
                Log.e(TAG,"Error", e);
                cancel();
            }
            synchronized (BluetoothService.this){
                connectThread = null;
            }
            connected(device, socket);
        }

        void cancel(){
            try{
                socket.close();
                state = STATE_NONE;
            } catch (IOException e){
                Log.e(TAG, "Could not close client socket");
            }
        }
    }
    public class BluetoothBroadcastReceiver extends BroadcastReceiver {
        @Override
        public void onReceive(Context context, Intent intent) {
            String action = intent.getAction();
            if (BluetoothAdapter.ACTION_STATE_CHANGED.equals(action)) {
                int state = intent.getExtras().getInt(BluetoothAdapter.EXTRA_STATE);
                switch (state) {
                    case BluetoothAdapter.STATE_ON:
                        send(serverObject, "bluetooth.on");
                        break;
                    case BluetoothAdapter.STATE_OFF:
                        send(serverObject, "bluetooth.off");
                        break;
                }
            }

            if (BluetoothAdapter.ACTION_SCAN_MODE_CHANGED.equals(action)) {
                int state = intent.getExtras().getInt(BluetoothAdapter.EXTRA_SCAN_MODE);
                switch (state) {
                    case BluetoothAdapter.SCAN_MODE_CONNECTABLE_DISCOVERABLE:
                        send(serverObject, "bluetooth.mode.discoverable");
                        break;
                    case BluetoothAdapter.SCAN_MODE_CONNECTABLE:
                        send(serverObject, "bluetooth.mode.connectable");
                        break;
                    case BluetoothAdapter.SCAN_MODE_NONE:
                        send(serverObject, "bluetooth.mode.none");
                        break;

                }
            }

            if (BluetoothDevice.ACTION_ACL_CONNECTED.equals(action)) {
                send(serverObject, "bluetooth.connected");
            }
            if (BluetoothDevice.ACTION_ACL_DISCONNECTED.equals(action)) {
                send(serverObject, "bluetooth.disconnected");
            }

            if (BluetoothDevice.ACTION_FOUND.equals(action)) {
                BluetoothDevice dev = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
                send(serverObject, "bluetooth.found." + dev.getAddress());
                BluetoothService.foundDevices.add(dev);
            }
        }
    }

    private static final String SERIAL_UUID = "00001101-0000-1000-8000-00805F9B34FB";
    private static final String TAG = "BluetoothManager-Plugin";
    private static final int STATE_NONE = 0;
    private static final int STATE_LISTENING = 1;
    private static final int STATE_CONNECTING = 2;
    private static final int STATE_CONNECTED = 3;
    private static final String BT_MESSAGE = "OnMessage";
    private static boolean runningUnity = true;
    private static List<BluetoothDevice> foundDevices;
    private static BluetoothAdapter btAdapter = BluetoothAdapter.getDefaultAdapter();

    public static void setUnity(boolean value) {
        runningUnity = value;
    }

    public static boolean isUnity() {
        return runningUnity;
    }

    public static boolean isEnabled() {
        return btAdapter.isEnabled();
    }

    private static void send(String to, String message) {
        if (runningUnity)
            UnityPlayer.UnitySendMessage(to, BT_MESSAGE, message);
    }

    public static String getSerialUUID() {
        return SERIAL_UUID;
    }

    public static BluetoothService createInstance(Activity activity){
        return new BluetoothService(activity);
    }

    public static void searchDevices() {
        if (foundDevices == null) {
            foundDevices = new ArrayList<>();
        } else {
            foundDevices.clear();
        }
        btAdapter.startDiscovery();
    }

    public static List<BluetoothDevice> getFoundDevices() {
        return foundDevices;
    }

    public static boolean enableAdapter() {
        return btAdapter.enable();
    }

    public static boolean disableAdapter() {
        return btAdapter.disable();
    }

    public static List<BluetoothDevice> getBondedDevices() {
        return new ArrayList<>(btAdapter.getBondedDevices());
    }

    // ---- Instance ----
    private int state;
    private AcceptThread acceptThread;
    private ConnectThread connectThread;
    private ConnectedThread connectedThread;
    private String gameObject;
    private String serverObject;
    private Activity activity;

    public BluetoothService(Activity activity) {
        this.activity = activity;
        state = STATE_NONE;
        gameObject = "BluetoothObject";
        serverObject = gameObject;
        registerIntent(activity);
    }

    public String getGameObject(){
        return gameObject;
    }
    public String getServerObject(){
        return serverObject;
    }
    public synchronized int getState() {
        return state;
    }
    public boolean isListening() { return state == STATE_LISTENING; }
    public boolean isConnected() { return state == STATE_CONNECTED; }

    public void requestEnableBluetooth() {
        if (!btAdapter.isEnabled()) {
            Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            this.activity.startActivityForResult(enableBtIntent, 1);
        }
    }

    public void requestEnableDiscoverability() {
        Intent discoverableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_DISCOVERABLE);
        discoverableIntent.putExtra(BluetoothAdapter.EXTRA_DISCOVERABLE_DURATION, 300);
        activity.startActivity(discoverableIntent);
    }

    public void setGameObject(String name){
        gameObject = name;
    }
    public void setServerObject(String name){
        serverObject = name;
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
        Log.i(TAG, "Server stopped");
        send(serverObject, "server.stopped");
        state = STATE_NONE;
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
        startServer("Serial port", SERIAL_UUID);
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

    private void registerIntent(Activity activity)
    {
        IntentFilter filter = new IntentFilter();
        filter.addAction(BluetoothAdapter.ACTION_STATE_CHANGED);
        filter.addAction(BluetoothAdapter.ACTION_SCAN_MODE_CHANGED);
        filter.addAction(BluetoothDevice.ACTION_ACL_CONNECTED);
        filter.addAction(BluetoothDevice.ACTION_ACL_DISCONNECTED);
        filter.addAction(BluetoothDevice.ACTION_FOUND);
        activity.getApplication().getApplicationContext().registerReceiver(new BluetoothBroadcastReceiver(), filter);
    }

    /* ========== Unity Helper Methods ========== */

    // address,name
    public static String[] u_getDiscoveredDevices() {
        String discovered[] = new String[foundDevices.size()];
        int i = 0;
        for (BluetoothDevice device : foundDevices) {
            discovered[i++] = device.getAddress() + "," + device.getName();
        }
        return discovered;
    }

    public static String[] u_getBondedDevices() {
        List<BluetoothDevice> bonded = getBondedDevices();
        String devices[] = new String[bonded.size()];
        int i = 0;
        for (BluetoothDevice device : bonded) {
            devices[i++] = device.getAddress() + "," + device.getName();
        }
        return devices;
    }

    public boolean u_connect(String address, String uuid) {
        BluetoothDevice dev = btAdapter.getRemoteDevice(address);
        if (dev == null) return false;
        connect(dev, uuid);
        return true;
    }

    public boolean u_connect(String address) {
        return u_connect(address, SERIAL_UUID);
    }

}
