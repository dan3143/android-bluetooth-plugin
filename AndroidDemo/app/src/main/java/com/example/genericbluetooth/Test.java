package com.example.genericbluetooth;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.widget.Button;

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
                bt.stop();
                bt.disable();
            } else {
                bt.enable();
            }
        });
    }
}
