# import tensorflow as tf
#
# from tensorflow.contrib import util
#
# util.make_tensor_proto
import threading

import numpy as np
import socket
import subprocess
import io
import os
import gym
from time import sleep
import json
import sys


class UnityEnv(gym.Env):

  metadata = {'render.modes': ['human', 'rgb_array']}

  def __init__(self):
    self.proc = None
    self.soc = None
    self._configure()

  def _configure(self, *args):
    self.ad = 2
    self.sd = 3
    self.w = 128
    self.h = 128
    n = self.w * self.h * 4
    self.BUFFER_SIZE = self.sd * 4 + n


  def _reset(self):
    self._close()  # reset
    self.connected = False
    host = '127.0.0.1'
    port = get_free_port(host)
    print('Port: {}'.format(port))
    assert port != 0
    import platform
    print(platform.platform())
    pl = 'windows' if 'Windows' in platform.platform() else 'unix'
    self.sim_path = os.path.join(os.path.dirname(__file__), '..', 'simulator', 'bin', pl)
    if(pl == 'windows'):
      bin = os.path.join(os.path.dirname(__file__), '..', 'simulator', 'bin', pl, 'sim.exe')
    else:
      bin = os.path.join(os.path.dirname(__file__), '..', 'simulator', 'bin', pl, 'sim.x86_64')
    bin = os.path.abspath(bin)
    env = os.environ.copy()
    env.update(RL_UNITY_PORT=str(port),
               RL_UNITY_WIDTH=str(self.w),
               RL_UNITY_HEIGHT=str(self.h))  # insert env variables here

    print(bin)
    def errw():
      for c in iter(lambda: self.proc.stderr.read(1), ''):
        sys.stderr.write(c)
        sys.stderr.flush()

    def stdw():
      for c in iter(lambda: self.proc.stdout.read(1), ''):
        sys.stdout.write(c)
        sys.stdout.flush()
      # self.close(status=0)  # finished successfully

    def poll():
      self.proc.wait()
      print(self.proc.returncode)

    # https://docs.unity3d.com/Manual/CommandLineArguments.html
    self.proc = subprocess.Popen([bin, '-logfile',
                                  '-screen-width {}'.format(self.w),
                                  '-screen-height {}'.format(self.h)],
                                 env=env,
                                 stderr=subprocess.PIPE,
                                 stdout=subprocess.PIPE,
                                 universal_newlines=True)

    threading.Thread(target=poll, daemon=True).start()
    threading.Thread(target=errw, daemon=True).start()
    threading.Thread(target=stdw, daemon=True).start()

    self.soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    while not self.proc.poll():
      try:
        self.soc.connect((host, port))
        self.connected = True
        break
      except Exception:
        pass

      sleep(.1)

    assert self.connected

    for _ in range(100):
      try:
        with open(os.path.join(self.sim_path, 'sim_Data', 'waypoints_SimpleTerrain.txt')) as f:
          wp = json.load(f)
      except FileNotFoundError:
        sleep(.05)

    self.wp = np.array([[e['x'], e['y'], e['z']] for e in wp])
    print(self.wp)


    state, frame = self.recv()

    return frame

  def recv(self):
    data_in = b""
    while len(data_in) < self.BUFFER_SIZE:
      # print('receiving stuff')
      chunk = self.soc.recv(min(1024, self.BUFFER_SIZE - len(data_in)))
      data_in += chunk

    state = np.frombuffer(data_in, np.float32, self.sd, 0)
    state = np.reshape(state, [3])
    print(state)
    self._metrics(state)
    frame = np.frombuffer(data_in, np.uint8, -1, self.sd * 4)
    # print(len(frame))
    frame = np.reshape(frame, [128, 128, 4])
    frame = frame[:, :, :3]

    self.last_frame = frame
    self.last_state = state

    return state, frame

  def _metrics(self, pos):
    # Distance to all points
    # Perhaps need to be accelerated

    # diff = self.wp - pos
    # allDist = [(np.linalg.norm(diff[x]), x) for x in range(len(diff))]
    #
    # allDist.sort(key=lambda tuples: tuples[0])
    # ia, ib = allDist[0][1], allDist[1][1]

    ia, ib = np.argsort(np.linalg.norm(self.wp-pos, axis=1))[:2]

    # print("pos : " + str(pos))
    # print("ia and ib : " + str(ia) + " " + str(ib))
    a, b = self.wp[ia, :], self.wp[ib, :]  # two closest points
    # print("a and b: " + str(a) + " " + str(b))
    u = b - a
    # u is the unit vector associated to AB
    u = u / np.linalg.norm(u)
    # v is the vector associated to AC (C is the position of the car)
    v = pos - a

    # The projected point on the vector AB is (AB.AC) * AB / |AB|² + A.
    # In this case with u = AB/|AB|, proj = (AC.u)*u + a
    #        *C
    #       /|
    #      / |
    #     /  |
    #    /   |
    #  A*----*----*B
    #       proj
    #
    proj = np.dot(u, v) * u + a
    # Square distance, Pythagorean theorem in the triangle A-C-proj
    sqrDist = np.linalg.norm(v)**2 - np.linalg.norm(proj - a)**2
    # dist = np.cross(np.abs(pos-a), np.abs(pos-b))/ np.abs(b-a)
    # n = b-a
    # proj = n/np.linalg.norm(n) * (pos-a) + a
    print(sqrDist, proj)


  def _step(self, action):
    a = np.array(action, dtype=np.float32)
    assert a.shape == (self.ad, )
    data_out = a.tobytes()
    self.soc.sendall(data_out)
    state, frame = self.recv()

    return frame, 0, False, {}

  def _close(self):
    if self.proc:
      self.proc.kill()
    if self.soc:
      self.soc.close()

  def render(self, mode='human', **kwargs):
    if mode == 'rgb_array':
      return self.last_frame  # return RGB frame suitable for video
    elif mode is 'human':
      pass  # we do that anyway
    else:
      super().render(mode=mode)  # just raise an exception


def get_free_port(host):
  sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
  sock.bind((host, 0))
  port = sock.getsockname()[1]
  sock.close()
  return port


if __name__ == '__main__':

  env = UnityEnv()
  env.reset()
  for i in range(10000):
    print(i)
    env.step([1., 1.])


