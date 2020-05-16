package com.example.genericbluetooth;

import android.bluetooth.BluetoothDevice;
import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.recyclerview.widget.RecyclerView;

import java.util.List;

public class DeviceListAdapter extends RecyclerView.Adapter<DeviceListAdapter.DeviceViewHolder> {

    public static class DeviceViewHolder extends RecyclerView.ViewHolder implements View.OnClickListener {
        public TextView deviceNameView;
        public TextView macAddressView;

        public DeviceViewHolder(View itemView) {
            super(itemView);
            deviceNameView = itemView.findViewById(R.id.device_name);
            macAddressView = itemView.findViewById(R.id.mac_address);
            itemView.setOnClickListener(this);
        }

        @Override
        public void onClick(View v) {
            listener.deviceClicked(v, this.getLayoutPosition());
        }
    }

    public interface DeviceListClickListener {
        void deviceClicked(View v, int position);
    }

    private List<BluetoothDevice> devices;
    private Context context;
    private static DeviceListClickListener listener;

    public DeviceListAdapter(List<BluetoothDevice> devices, DeviceListClickListener listener){
        this.devices = devices;
        this.listener = listener;
    }

    public BluetoothDevice getDevice(int i){
        return devices.get(i);
    }

    @Override
    public DeviceViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        LayoutInflater inflater = LayoutInflater.from(parent.getContext());
        View deviceView = inflater.inflate(R.layout.device_view, parent, false);
        return new DeviceViewHolder(deviceView);
    }

    @Override
    public void onBindViewHolder(DeviceViewHolder holder, int position) {
        BluetoothDevice device = devices.get(position);
        holder.deviceNameView.setText(device.getName());
        holder.macAddressView.setText(device.getAddress());
    }

    @Override
    public int getItemCount() {
        return devices.size();
    }

    @Override
    public void onAttachedToRecyclerView(RecyclerView recyclerView){
        super.onAttachedToRecyclerView(recyclerView);
        context = recyclerView.getContext();
    }
}
