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
import rlunity
from gym import spaces

DEBUG = False

pre = print
if not DEBUG:
  print = lambda *a: None

class UnityEnv(gym.Env):

  metadata = {'render.modes': ['human', 'rgb_array']}

  def __init__(self):
    self.proc = None
    self.soc = None
    self._configure()
    self.stopped = 0

  def _configure(self, w=128, h=128, batchmode=True, *args, **kwargs):
    self.ad = 3
    self.sd = 16
    self.w = w
    self.h = h
    self.batchmode = batchmode
    self.wp = None
    n = 0 if batchmode else self.w * self.h * 4
    self.BUFFER_SIZE = self.sd * 4 + n
    self.action_space = spaces.Box(-np.ones([self.ad]), np.ones([self.ad]))
    # if batchmode:
    #   sbm = 5
    #   self.observation_space = spaces.Box(-np.ones([sbm]), np.ones([sbm]))
    # else:
    #   self.observation_space = spaces.Box(np.zeros([self.w, self.h, 3]), np.ones([self.w, self.h, 3]))
    sbm = 5
    self.observation_space = spaces.Box(-np.ones([sbm]), np.ones([sbm]))

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

    env.update(
      RL_UNITY_PORT=str(port),
      RL_UNITY_WIDTH=str(self.w),
      RL_UNITY_HEIGHT=str(self.h),
      # MESA_GL_VERSION_OVERRIDE=str(3.3),
      )  # insert env variables here

    print(bin)

    def stdw():
      for c in iter(lambda: self.proc.stdout.read(1), ''):
        sys.stdout.write(c)
        sys.stdout.flush()

    def poll():
      self.proc.wait()
      print(self.proc.stdout.read())
      #print(f'Unity returned with {self.proc.returncode}')

    # https://docs.unity3d.com/Manual/CommandLineArguments.html

    # TODO: ensure that the sim doesn't read or write any cache or config files
    self.proc = subprocess.Popen([bin,
                                  *(['-logfile'] if DEBUG else []),
                                  *(['-batchmode', '-nographics'] if self.batchmode else []),
                                  '-screen-width {}'.format(self.w),
                                  '-screen-height {}'.format(self.h),
                                  ],
                                 env=env,
                                 stdout=subprocess.PIPE,
                                 stderr=subprocess.STDOUT,
                                 universal_newlines=True)

    threading.Thread(target=poll, daemon=True).start()
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

    if not self.connected:
      raise Exception()

    state, frame = self.recv()

    return state

  def recv(self):
    data_in = b""
    while len(data_in) < self.BUFFER_SIZE:
      # print('receiving stuff')
      chunk = self.soc.recv(min(1024, self.BUFFER_SIZE - len(data_in)))
      data_in += chunk

    # while True:
    #   chunk = self.soc.recv(1024)
    #   if not chunk:
    #     break
    #   data_in += chunk

    # Checking data points are not None, if yes parse them.
    if self.wp is None:
      with open(os.path.join(self.sim_path, 'sim_Data', 'waypoints_SimpleTerrain.txt')) as f:
        wp = json.load(f)
        self.wp = np.array([[e['x'], e['y'], e['z']] for e in wp])
        print(self.wp)

    # Read the number of float sent by the C# side. It's the first number
    self.sd = int(np.frombuffer(data_in, np.float32, 1, 0))
    print(self.sd)

    state = np.frombuffer(data_in, np.float32, self.sd, 4)

    distance = state[0]
    speed = state[1]
    direction = state[12:15] - state[9:12]

    if DEBUG or True:
      print("Distance = " + str(state[0]) + " ; Speed along road = " + str(state[1]))
      print("Position = " + str(state[2:5]) + " ; Projection = " + str(state[5:8]))
      print("Collision detected : " + ("True" if state[8]==1.0 else "False"))
      print("Road direction : " + str(state[9:12]) + "; Car direction : " + str(state[12:15]))
      print("Relative direction:", direction)

    state = np.concatenate(((distance/100., speed * 1), direction))

    if self.batchmode:
      frame = None
    else:
      frame = np.frombuffer(data_in, np.uint8, -1, (self.sd + 1) * 4)
      # print(len(frame))
      frame = np.reshape(frame, [self.w, self.h, 4])
      frame = frame[:, :, :3]

    self.last_frame = frame
    self.last_state = state

    return state, frame


  def _step(self, action):
    a = np.array(action, dtype=np.float32)
    assert a.shape == (self.ad, )
    data_out = a.tobytes()
    self.soc.sendall(data_out)
    state, frame = self.recv()

    
    if np.abs(state[1]) < 0.01:
      self.stopped += 1
    else:
      self.stopped = 0

    done = np.abs(state[0]) > 4. or self.stopped > 60*7

    reward = - state[0]**2 + state[1]
    if done:
      self.stopped = 0
      reward -= 10

    # pre(state, reward)
    

    return state, reward, done, {}

  def _close(self):
    if self.proc:
      self.proc.kill()
    if self.soc:
      self.soc.close()

  def render(self, mode='human', *args, **kwargs):
    if mode == 'rgb_array':
      # pre(self.last_frame)
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

  import argparse

  parser = argparse.ArgumentParser(description='Unity Gym Environment')
  parser.add_argument('--batchmode', action='store_true', help='Run the simulator in batch mode with no graphics')
  args = parser.parse_args()
  print(args.batchmode)
  bm = True
  bm = args.batchmode

  env = gym.make('UnityCar-v0')
  env.configure(batchmode=bm)
  env.reset()
  for i in range(10000):
    print(i)
    if i % 300 == 0:
        env.step([0.0, 1.0, 1.0])
    else:
        env.step([0.0, 1.0, 0.0])


