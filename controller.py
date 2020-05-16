import tkinter as tk
import bluetooth as bt
import sys
import os
import platform

def connect():
    if socket != None:
        return
    print ("Searching for service...")
    uuid = "00001101-0000-1000-8000-00805F9B34FB"
    services = bt.find_service(uuid=uuid, address=sys.argv[1])
    if (len(services) == 0):
        print("Could not find service")
    else:
        service = services[0]
    
        port = service["port"]
        name = service["name"]
        host = service["host"]

        print ("Connecting to",name,"on",host)
        global_list = globals()
        global_list["socket"] = bt.BluetoothSocket(bt.RFCOMM)
        try:
            global_list["socket"].connect((sys.argv[1], port))
        except:
            return
        print ("Connected to", name)
        for button in btns.values():
            button["state"] = tk.NORMAL

def send(message):
    if socket != None:
        try:
            socket.send(message)
        except:
            disconnect()


def disconnect():
    if socket != None:
        print ("Closing socket...")
        global_list = globals()
        global_list["socket"].close()
        global_list["socket"] = None
        print ("Socket closed")
        for button in btns.values():
            button["state"] = tk.DISABLED

def keydown(e):
    key = e.keysym.lower()
    if key == 'up':
        send('up')
    elif key == 'down':
        send('down')
    elif key == 'left':
        send('left')
    elif key == 'right':
        send('right')
    elif key == 'a':
        send('a')
    elif key == 'b':
        send('a')

if __name__ == '__main__':
    socket = None
    window = tk.Tk()
    window.title("Bluetooth controller")
    window.geometry("120x120")
    btns = {
            "up": tk.Button(window, text="↑", state=tk.DISABLED, command=lambda:send('up')),
            "down": tk.Button(window, text="↓", state=tk.DISABLED, command=lambda:send('down')),
            "right": tk.Button(window, text="→", state=tk.DISABLED, command=lambda:send('right')),
            "left": tk.Button(window, text="←", state=tk.DISABLED, command=lambda:send('left')),
            "a": tk.Button(window, text="A", state=tk.DISABLED, command=lambda:send('a')),
            "b": tk.Button(window, text="B", state=tk.DISABLED, command=lambda:send('b')),
    }

    window.bind('<KeyPress>', keydown)
 
    btn_disconnect = tk.Button(window, text="x", command=disconnect)
    btn_connect = tk.Button(window, text="o", command=connect)

    btns["up"].grid(column=1,row=0)
    btns["down"].grid(column=1,row=3)
    btns["right"].grid(column=2,row=2)
    btns["left"].grid(column=0,row=2)
    btns["a"].grid(column=0,row=3)
    btns["b"].grid(column=2,row=3)
    btn_disconnect.grid(column=0, row=4)
    btn_connect.grid(column=1, row=4)

    window.mainloop()

