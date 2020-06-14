package com.example.genericbluetooth;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.widget.Button;
import android.widget.EditText;

import com.example.bluetooth.BluetoothService;

public class Test extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_test);
        BluetoothService.setUnity(false);
        BluetoothService bt = BluetoothService.getInstance();
        Button btn = findViewById(R.id.button);
        btn.setOnClickListener(v -> {
            if (bt.isEnabled()) {
                bt.start();
            } else {
                bt.enable();
            }
        });
        Button btnsend = findViewById(R.id.send_btn);
        EditText text = findViewById(R.id.text);
        btnsend.setOnClickListener(v -> {
            BluetoothService.getInstance().write(text.getText().toString());
            text.setText("");
        });
    }
}
