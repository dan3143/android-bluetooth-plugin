package com.guevara.bluetoothplugin;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;

import com.guevara.bluetooth.BluetoothClient;
import com.guevara.bluetooth.BluetoothServer;
import com.guevara.bluetooth.BluetoothService;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import java.util.List;

public class MainActivity extends AppCompatActivity {

    BluetoothServer server;
    BluetoothClient client;
    Button serverBtn;
    TextView receivedText;
    EditText sentText;
    boolean serverOn = false;
    boolean first = true;
    BroadcastReceiver receiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            Bundle b = intent.getExtras();
            for (String key: b.keySet()) {
                Log.i(BluetoothService.TAG, key + ": " + b.get(key));
            }

            int type = intent.getIntExtra(BluetoothService.TYPE_EXTRA, 0);
            String data = intent.getStringExtra(BluetoothService.MESSAGE_EXTRA);
            if (type == BluetoothService.TYPE_STATUS) {
                switch (data) {
                    case "server.listening":
                        serverBtn.setText(R.string.stop);
                        serverOn = true;
                        break;
                    case "server.stopped":
                        serverBtn.setText(R.string.start);
                        serverOn = false;
                        break;
                }
            } else {
                receivedText.setText(data);
            }
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        BluetoothService.setUnity(false);

        registerReceiver(receiver, new IntentFilter(BluetoothService.MESSAGE));

        server = new BluetoothServer(this);
        client = new BluetoothClient(this);

        serverBtn = findViewById(R.id.server_btn);
        receivedText = findViewById(R.id.received_txt);
        sentText = findViewById(R.id.send_text);

        serverBtn.setOnClickListener(view -> {
            client = null;
            if (serverOn) {
                server.stop();
            } else {
                server.start();
            }
        });

        findViewById(R.id.send_btn).setOnClickListener(v -> {
            if (!sentText.getText().toString().trim().equals("")) {
                if (server != null) {
                    server.sendAll(sentText.getText().toString());
                } else if (client != null){
                    client.send(sentText.getText().toString());
                }
                sentText.setText("");
            }
        });

        List<BluetoothDevice> devices = BluetoothService.getBondedDevices();
        String names[] = devices.stream()
                        .map(BluetoothDevice::getName).toArray(String[]::new);
        Spinner spinner = findViewById(R.id.device_spinner);
        spinner.setAdapter(new ArrayAdapter<>(this, android.R.layout.simple_spinner_dropdown_item, names));
        spinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                if (first) {
                    first = false;
                    return;
                }
                server = null;
                new AlertDialog.Builder(MainActivity.this)
                        .setTitle("Connect")
                        .setMessage("Are you sure you want to connect?")
                        .setPositiveButton(android.R.string.yes, (dialog, which) -> client.connect(devices.get(position)))
                        .setNegativeButton(android.R.string.no, null)
                        .setIcon(android.R.drawable.ic_dialog_alert)
                        .show();
            }
            @Override
            public void onNothingSelected(AdapterView<?> parent) {
            }
        });
    }

    @Override
    public void onDestroy() {
        unregisterReceiver(receiver);
        super.onDestroy();
    }


}
