# import tensorflow as tf
#
# from tensorflow.contrib import util
#
# util.make_tensor_proto

import numpy as np
import socket
import io

TCP_IP = '127.0.0.1'
TCP_PORT = 8887

ad = 2
sd = 2
n = 128*128*4
BUFFER_SIZE = sd * 4 + n
# BUFFER_SIZE = n

soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
soc.connect((TCP_IP, TCP_PORT))

a = np.zeros(ad, dtype=np.float32)


import imageio
writer = imageio.get_writer('test.mp4', mode='I', fps=25)



for i in range(10):

  a[1] = i
  data_out = a.tobytes()

  print(len(data_out))
  soc.send(data_out)

  data_in = b""
  while len(data_in) < BUFFER_SIZE:
    # print('fdjks')
    chunk = soc.recv(min(1024, BUFFER_SIZE-len(data_in)))
    data_in += chunk
  # data_in = soc.recv(BUFFER_SIZE)

  print('fds')
  state = np.frombuffer(data_in, np.float32, sd, 0)
  frame = np.frombuffer(data_in, np.uint8, -1, sd*4)
  print(len(frame))
  img = np.reshape(frame, [128, 128, 4])
  img = img[:, :, :3]
  print("r: ", state)

  writer.append_data(img)
  # np.transpose(frame, [1, 0, 2])


from matplotlib import pyplot as pl
pl.imshow(img)
pl.show()

soc.close()
