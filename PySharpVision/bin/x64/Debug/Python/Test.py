import mmap
import cv2
import numpy as np
import time
import os
import socket


# starttime = datetime.datetime.now()
# byteSize = 15040566
# file_name = '5MImage'
# f = mmap.mmap(0, byteSize, file_name, mmap.ACCESS_READ)
# img = cv2.imdecode(np.frombuffer(f, np.uint8), cv2.IMREAD_COLOR)
# endtime = datetime.datetime.now()
# print(endtime-starttime)
# cv2.imwrite(os.path.join("E:\Test",str(1)+".bmp"),img)

def main():
    # 设置服务器IP和端口
    server_ip = "127.0.0.1"
    server_port = 8080

        # 连接服务器
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client_socket:
        client_socket.connect((server_ip, server_port))
        i = 0
        while True:
            # 发送消息
            message = "Please send image memory!"
            client_socket.sendall(message.encode())
            response = client_socket.recv(1024)
            if response.decode()=="DisTcpConnect!":
                print("DisTcpConnect!")
                break
            else:
                print("Received:", response.decode()+"Predict")
                byteSize = 15116598 #20054070
                file_name = '5MImage'
                f = mmap.mmap(0, byteSize, file_name, mmap.ACCESS_READ)
                img = cv2.imdecode(np.frombuffer(f, np.uint8), cv2.IMREAD_COLOR)
                gray_image = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
                _, binary_image = cv2.threshold(gray_image, 128, 255, cv2.THRESH_BINARY)
                cv2.imwrite(os.path.join(r"D:\Chimingkuei\repos\PySharpVision\Output Image",str(i)+".bmp"), binary_image)
                i+=1
                time.sleep(1)
        

if __name__ == "__main__":
    main()
