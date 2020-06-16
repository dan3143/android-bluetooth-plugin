package com.example.genericbluetooth;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import android.bluetooth.BluetoothDevice;
import android.content.DialogInterface;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.Toast;

import com.example.bluetooth.BluetoothService;

import java.util.List;

public class Test extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_test);
        BluetoothService.setUnity(false);
        BluetoothService bt = new BluetoothService();

        // Client
        Spinner spinner = findViewById(R.id.device_spinner);
        List<BluetoothDevice> devices = bt.getBondedDevices();
        String[] names = new String[devices.size()];
        int i = 0;
        for (BluetoothDevice device : bt.getBondedDevices()) {
            names[i++] = device.getName();
        }
        ArrayAdapter<String> adapter = new ArrayAdapter<>(this, android.R.layout.simple_spinner_dropdown_item, names);
        spinner.setAdapter(adapter);
        spinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                Toast.makeText(Test.this, "Selected: " + names[position], Toast.LENGTH_SHORT).show();
                new AlertDialog.Builder(Test.this)
                        .setTitle("Connect")
                        .setMessage("Are you sure you want to connect?")
                        .setPositiveButton(android.R.string.yes, (dialog, which) -> bt.connect(devices.get(position)))
                        .setNegativeButton(android.R.string.no, null)
                        .setIcon(android.R.drawable.ic_dialog_alert)
                        .show();
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {

            }
        });

        // Server
        Button btn = findViewById(R.id.button);
        btn.setOnClickListener(v -> {
            if (bt.isEnabled()) {
                bt.startServer();
            } else {
                bt.enable();
            }
        });

        Button btnsend = findViewById(R.id.send_btn);
        EditText text = findViewById(R.id.text);
        btnsend.setOnClickListener(v -> {
            bt.write(text.getText().toString());
            text.setText("");
        });
    }
}
