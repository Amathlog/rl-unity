# import tensorflow as tf
#
# from tensorflow.contrib import util
#
# util.make_tensor_proto

import socket


TCP_IP = '127.0.0.1'
TCP_PORT = 8888
BUFFER_SIZE = 12288
MESSAGE = b"Hello, World!"

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((TCP_IP, TCP_PORT))
s.send(MESSAGE)
# data = s.recv(BUFFER_SIZE)
s.close()

# print("received data:", data)

