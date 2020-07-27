package com.guevara.bluetooth;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
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

public abstract class BluetoothService {

    public static final String SERIAL_UUID = "00001101-0000-1000-8000-00805F9B34FB";
    public static final String MESSAGE = "com.guevara.bluetooth.MESSAGE";
    public static final String MESSAGE_EXTRA = "com.guevara.bluetooth.MESSAGE_EXTRA";
    public static final String TYPE_EXTRA = "com.guevara.bluetooth.TYPE_EXTRA";

    public static final String TAG = "BluetoothManager-Plugin";
    static final int STATE_NONE = 0;
    static final int STATE_LISTENING = 1;
    static final int STATE_CONNECTING = 2;
    static final int STATE_CONNECTED = 3;
    static final String BT_MESSAGE = "OnMessage";
    private static boolean runningUnity = true;
    private Activity activity;
    private static List<BluetoothDevice> foundDevices;
    static BluetoothAdapter btAdapter = BluetoothAdapter.getDefaultAdapter();

    public static void setUnity(boolean value) {
        runningUnity = value;
    }

    public static boolean isUnity() {
        return runningUnity;
    }

    public static boolean isEnabled() {
        return btAdapter.isEnabled();
    }


    public static String getSerialUUID() {
        return SERIAL_UUID;
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

    public static String getDeviceName(String address) {
        BluetoothDevice bt = btAdapter.getRemoteDevice(address);
        if (bt != null) {
            return bt.getName();
        }
        return "";
    }

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

    public static List<BluetoothDevice> getBondedDevices() {
        return new ArrayList<>(btAdapter.getBondedDevices());
    }

    private int state;
    private String gameObject;
    protected OnBluetoothMessageListener onMessageListener;
    protected OnBluetoothStatusListener onStatusListener;

    BluetoothService() {
        this(UnityPlayer.currentActivity);
    }

    BluetoothService(Activity activity) {
        state = STATE_NONE;
        gameObject = "GameObject";
        this.activity = activity;
        registerIntent(activity);
        onMessageListener = (message, fromAddress) -> {};
        onStatusListener = message -> {};
    }

    public void requestEnableBluetooth() {
        if (activity == null) {
            Log.e(TAG, "Activity is not set");
            return;
        }
        if (!btAdapter.isEnabled()) {
            Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            activity.startActivityForResult(enableBtIntent, 1);
        }
    }

    public void requestEnableDiscoverability() {
        if (activity == null) {
            Log.e(TAG, "Activity is not set");
            return;
        }
        Intent discoverableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_DISCOVERABLE);
        discoverableIntent.putExtra(BluetoothAdapter.EXTRA_DISCOVERABLE_DURATION, 300);
        activity.startActivity(discoverableIntent);
    }

    public String getGameObject(){
        return gameObject;
    }

    public synchronized int getState() {
        return state;
    }
    public boolean isListening() { return state == STATE_LISTENING; }
    public boolean isConnected() { return state == STATE_CONNECTED; }

    public void setGameObject(String name){
        gameObject = name;
    }

    public void setOnMessageListener(OnBluetoothMessageListener listener) {
        this.onMessageListener = listener;
    }

    public void setOnStatusListener(OnBluetoothStatusListener listener) {
        this.onStatusListener = listener;
    }

    synchronized void setState(int state) { this.state = state; }


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

    /* ========== Inner classes ========== */

    interface OnDisconnectEvent {
        void onDisconnect();
    }

    class ConnectedThread extends Thread {

        private OnDisconnectEvent onDisconnectEvent;
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

        public void setOnDisconnectEvent(OnDisconnectEvent event) {
            this.onDisconnectEvent = event;
        }

        public BluetoothSocket getSocket() {
            return socket;
        }

        public void run() {
            byte[] buffer = new byte[1024];
            int bytes;
            while (socket.isConnected()) {
                try{
                    bytes = inputStream.read(buffer);
                    String message = new String(buffer, 0, bytes);
                    onMessageListener.OnMessage(message, socket.getRemoteDevice().getAddress());
                    //send(gameObject, message);
                }catch (IOException e) {
                    onStatusListener.OnStatus("socket.error.COULD_NOT_READ");
                    //send(serverObject, "socket.error.COULD_NOT_READ");
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
                onStatusListener.OnStatus("socket.error.COULD_NOT_WRITE");
                //send(serverObject, "socket.error.COULD_NOT_WRITE");
                Log.e(TAG, "Exception during write", e);
            }
        }

        void cancel(){
            try {
                inputStream.close();
                outputStream.close();
                socket.close();
                onDisconnectEvent.onDisconnect();
            }catch (IOException e){
                Log.e(TAG, "Could not close", e);
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
                        onStatusListener.OnStatus("bluetooth.on");
                        //send(serverObject, "bluetooth.on");
                        break;
                    case BluetoothAdapter.STATE_OFF:
                        onStatusListener.OnStatus("bluetooth.off");
                        //send(serverObject, "bluetooth.off");
                        break;
                }
            }

            if (BluetoothAdapter.ACTION_SCAN_MODE_CHANGED.equals(action)) {
                int state = intent.getExtras().getInt(BluetoothAdapter.EXTRA_SCAN_MODE);
                switch (state) {
                    case BluetoothAdapter.SCAN_MODE_CONNECTABLE_DISCOVERABLE:
                        onStatusListener.OnStatus("bluetooth.mode.discoverable");
                        //send(serverObject, "bluetooth.mode.discoverable");
                        break;
                    case BluetoothAdapter.SCAN_MODE_CONNECTABLE:
                        onStatusListener.OnStatus("bluetooth.mode.connectable");
                        //send(serverObject, "bluetooth.mode.connectable");
                        break;
                    case BluetoothAdapter.SCAN_MODE_NONE:
                        onStatusListener.OnStatus("bluetooth.mode.none");
                        //send(serverObject, "bluetooth.mode.none");
                        break;
                }
            }

            if (BluetoothDevice.ACTION_ACL_CONNECTED.equals(action)) {
                onStatusListener.OnStatus("bluetooth.connected");
                //send(serverObject, "bluetooth.connected");
            }
            if (BluetoothDevice.ACTION_ACL_DISCONNECTED.equals(action)) {
                onStatusListener.OnStatus("bluetooth.disconnected");
                //send(serverObject, "bluetooth.disconnected");
            }

            if (BluetoothDevice.ACTION_FOUND.equals(action)) {
                BluetoothDevice dev = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
                onStatusListener.OnStatus("bluetooth.found." + dev.getAddress());
                //send(serverObject, "bluetooth.found." + dev.getAddress());
                BluetoothService.foundDevices.add(dev);
            }
        }
    }

    interface OnBluetoothMessageListener {
        void OnMessage(String message, String fromAddress);
    }

    interface OnBluetoothStatusListener {
        void OnStatus(String status);
    }
}

