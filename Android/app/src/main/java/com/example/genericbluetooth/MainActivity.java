package com.example.genericbluetooth;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.BluetoothManager;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;

import com.example.bluetooth.BluetoothService;
import com.example.genericbluetooth.DeviceListAdapter.DeviceListClickListener;

public class MainActivity extends AppCompatActivity implements DeviceListClickListener {

    private Button btnServer;
    private RecyclerView deviceList;
    private DeviceListAdapter adapter;
    private BluetoothService bluetoothService;
    private boolean serverOpen = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        bluetoothService = BluetoothService.getInstance();

        adapter = new DeviceListAdapter(bluetoothService.getBondedDevices(), this);
        deviceList = findViewById(R.id.device_list);
        deviceList.setAdapter(adapter);
        deviceList.setLayoutManager(new LinearLayoutManager(this));

        btnServer = findViewById(R.id.btn_server);
        btnServer.setOnClickListener((view) -> {
            if (serverOpen){
                bluetoothService.stop();
                btnServer.setText(R.string.start_server);
                serverOpen = false;
            }else{
                bluetoothService.start();
                btnServer.setText(R.string.stop_server);
                serverOpen = true;
            }

        });

    }

    @Override
    public void deviceClicked(View v, int position) {
        int color = Color.TRANSPARENT;
        Drawable background = v.getBackground();

        if (background instanceof ColorDrawable){
            color = ((ColorDrawable) background).getColor();
            Log.i("Color", "Instance: " + color);
        }

        if (color == Color.GRAY) {
            bluetoothService.stop();
            v.setBackgroundColor(Color.TRANSPARENT);
        } else {
            bluetoothService.connect(adapter.getDevice(position));
            v.setBackgroundColor(Color.GRAY);
        }
    }
}
