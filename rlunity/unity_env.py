import threading
import numpy as np
import socket
import subprocess
import os
import gym
from time import sleep
import json
import sys
from gym import spaces
import logging

logger = logging.getLogger('UnityEnv')


class UnityEnv(gym.Env):
  """A base class for environments using Unity3D
  Implements the gym.Env interface. See
  https://gym.openai.com/docs
  and
  https://github.com/openai/gym/tree/master/gym/envs#how-to-create-new-environments-for-gym
  """
  metadata = {'render.modes': ['human', 'rgb_array']}

  def __init__(self, w=128, h=128, batchmode=True):
    self.proc = None
    self.soc = None
    self.stopped = 0
    self.connected = False

    self.ad = 2
    self.sd = 15  # TODO: has to remain fixed
    self.w = w
    self.h = h
    self.batchmode = batchmode
    self.wp = None
    pixel_buffer_size = 0 if batchmode else self.w * self.h * 4
    self.buffer_size = (1 + self.sd) * 4 + pixel_buffer_size
    self.action_space = spaces.Box(-np.ones([self.ad]), np.ones([self.ad]))
    # if batchmode:
    #   sbm = 5
    #   self.observation_space = spaces.Box(-np.ones([sbm]), np.ones([sbm]))
    # else:
    #   self.observation_space = spaces.Box(np.zeros([self.w, self.h, 3]), np.ones([self.w, self.h, 3]))
    sbm = 5
    self.observation_space = spaces.Box(-np.ones([sbm]), np.ones([sbm]))
    self.log_unity = False

  def _configure(self, loglevel='info', log_unity=False, *args, **kwargs):
    logger.setLevel(getattr(logging, loglevel.upper()))
    self.log_unity = log_unity

  def connect(self):
    self._close()  # reset
    self.soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    host = '127.0.0.1'
    port = get_free_port(host)
    logger.debug('Port: {}'.format(port))
    assert port != 0
    import platform
    logger.debug('Platform ' + platform.platform())
    pl = 'windows' if 'Windows' in platform.platform() else 'unix'
    self.sim_path = os.path.join(os.path.dirname(__file__), '..', 'simulator', 'bin', pl)
    if (pl == 'windows'):
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

    logger.debug('Simulator binary' + bin)

    def stdw():
      for c in iter(lambda: self.proc.stdout.read(1), ''):
        sys.stdout.write(c)
        sys.stdout.flush()

    def poll():
      self.proc.wait()
      logger.debug(f'Unity returned with {self.proc.stdout.read()}')

    # https://docs.unity3d.com/Manual/CommandLineArguments.html

    # TODO: ensure that the sim doesn't read or write any cache or config files
    config_dir = os.path.expanduser('~/.config/unity3d/DefaultCompany/rl-unity')  # TODO: only works on linux
    if os.path.isdir(config_dir):
      from shutil import rmtree
      rmtree(config_dir)

    self.proc = subprocess.Popen([bin,
                                  *(['-logfile'] if self.log_unity else []),
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

    for i in range(100):
      if self.proc.poll():
        logger.debug('simulator died')
        break

      try:
        self.soc.connect((host, port))
        self.soc.settimeout(20.)
        self.connected = True
        break
      except ConnectionRefusedError:
        pass

      sleep(.1)

    if not self.connected:
      raise Exception()

  def _reset(self):
    if not self.connected:
      self.connect()

    else:
      self.send(self.action_space.sample(), reset=True)

    state, frame = self.receive()
    return state

  def receive(self):
    data_in = b""
    while len(data_in) < self.buffer_size:
      chunk = self.soc.recv(min(1024, self.buffer_size - len(data_in)))
      data_in += chunk

    # assert len(data_in) == self.buffer_size


    # Checking data points are not None, if yes parse them.
    if self.wp is None:
      with open(os.path.join(self.sim_path, 'sim_Data', 'waypoints_SimpleTerrain.txt')) as f:
        wp = json.load(f)
        self.wp = np.array([[e['x'], e['y'], e['z']] for e in wp])
        logger.debug(str(self.wp))

    # TODO: @Adrien self.sd has to remain constant after __init__
    # Read the number of float sent by the C# side. It's the first number
    sd = int(np.frombuffer(data_in, np.float32, 1, 0))
    logger.debug(f'State dimension expected: {self.sd}, received: {sd}')
    assert sd == self.sd

    state = np.frombuffer(data_in, np.float32, self.sd, 4)

    distance = state[0]
    speed = state[1]
    direction = state[12:15] - state[9:12]

    logger.debug("Distance = " + str(state[0]) + " ; Speed along road = " + str(state[1]))
    logger.debug("Position = " + str(state[2:5]) + " ; Projection = " + str(state[5:8]))
    logger.debug("Collision detected : " + ("True" if state[8] == 1.0 else "False"))
    logger.debug("Road direction : " + str(state[9:12]) + "; Car direction : " + str(state[12:15]))
    logger.debug("Relative direction: " + str(direction))

    state = np.concatenate(((distance / 100., speed * 1), direction))

    if self.batchmode:
      frame = None
    else:
      frame = np.frombuffer(data_in, np.uint8, -1, (self.sd + 1) * 4)
      # logger.debug(str(len(frame)))
      frame = np.reshape(frame, [self.w, self.h, 4])
      frame = frame[:, :, :3]

    self.last_frame = frame
    self.last_state = state

    return state, frame

  def send(self, action, reset=False):
    a = np.concatenate((action, [1. if reset else 0.]))
    a = np.array(a, dtype=np.float32)
    assert a.shape == (self.ad + 1,)

    data_out = a.tobytes()
    self.soc.sendall(data_out)

  def _step(self, action):
    action = np.clip(action, -1, 1)
    self.send(action)
    state, frame = self.receive()

    if np.abs(state[1]) < 0.01:
      self.stopped += 1
    else:
      self.stopped = 0

    done = np.abs(state[0]) > 4. or self.stopped > 60 * 7

    reward = - state[0] ** 2 + state[1]
    if done:
      self.stopped = 0
      reward -= 10

    return state, reward, done, {}

  def _close(self):
    if self.proc:
      self.proc.kill()
    if self.soc:
      self.soc.close()

  def render(self, mode='human', *args, **kwargs):
    if mode == 'rgb_array':
      return self.last_frame  # return RGB frame suitable for video
    elif mode is 'human':
      pass  # we do that anyway
    else:
      super().render(mode=mode)  # just raise an exception


class UnityCar(UnityEnv):
  def __init__(self):
    UnityEnv.__init__(self, w=128, h=128, batchmode=False)


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
  logger.debug('Batchmode ' + str(args.batchmode))
  bm = args.batchmode

  import rlunity
  env = gym.make('UnityCar-v0')  # requires import rlunity
  env.configure(loglevel='debug', log_unity=True)
  env.reset()
  for i in range(10000):
    print(i)
    env.step([1.0, 1.0])

    if (i+1) % 300 == 0:
      env.reset()
