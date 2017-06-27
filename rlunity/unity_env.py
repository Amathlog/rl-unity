import threading
import numpy as np
import socket
import subprocess
import os
import gym
from time import sleep
import time
import json
import sys
from gym import spaces
import logging

import matplotlib.pyplot as plt

logger = logging.getLogger('UnityEnv')

PLOT_FOLDER = "./plots/"
FINAL_REWARD_FILE = PLOT_FOLDER + "final_rewards.npy"
MEAN_DISTANCES_FILE = PLOT_FOLDER + "mean_distances.npy"
FINAL_DISTANCE_FILE = PLOT_FOLDER + "final_distance.npy"
POSITIONS_FILE = PLOT_FOLDER + "positions.npy"


class UnityEnv(gym.Env):
  """A base class for environments using Unity3D
  Implements the gym.Env interface. See
  https://gym.openai.com/docs
  and
  https://github.com/openai/gym/tree/master/gym/envs#how-to-create-new-environments-for-gym
  """
  metadata = {'render.modes': ['human', 'rgb_array'],
              'video.frames_per_second': 20}

  def __init__(self, batchmode=False):
    self.proc = None
    self.soc = None
    self.connected = False

    self.ad = 2
    self.sd = 18  # TODO: has to remain fixed
    self.batchmode = batchmode
    self.wp = None

    self.action_space = spaces.Box(-np.ones([self.ad]), np.ones([self.ad]))
    # if batchmode:
    #   sbm = 5
    #   self.observation_space = spaces.Box(-np.ones([sbm]), np.ones([sbm]))
    # else:
    #   self.observation_space = spaces.Box(np.zeros([self.w, self.h, 3]), np.ones([self.w, self.h, 3]))
    self.sbm = 6
    # self.observation_space = spaces.Box(-np.ones([self.sbm]), np.ones([self.sbm]))
    self.log_unity = False
    self.logfile = None
    self.restart = False
    self.configured = False
    self.current_level = 0

    # Metrics
    self.rewards = 0
    self.distances = []
    self.distance_driven = 0
    self.positions = []

    self.date = time.strftime('%d_%m_%Y_%H_%M_%S')
    self.testing = False



  def conf(self, loglevel='INFO', log_unity=False, logfile=None, w=128, h=128,frame=True,frame_w=128,frame_h=128, current_level=0, cont=False, *args, **kwargs):
    logger.setLevel(getattr(logging, loglevel.upper()))
    self.log_unity = log_unity
    if logfile:
      self.logfile = open(logfile, 'w')

    assert w >= 100 and h >= 100, 'the simulator does not support smaller resolutions than 100 at the moment'
    self.w = w
    self.h = h
    self.frame_h=frame_h
    self.frame_w = frame_w
    self.send_frame = frame
    self.configured = True
    self.current_level = current_level

    self.setup_metrics(cont)


  def connect(self):
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
      RL_UNITY_FRAME=str(self.send_frame),
      RL_UNITY_FRAME_WIDTH=str(self.frame_w),
      RL_UNITY_FRAME_HEIGHT=str(self.frame_h)
      # MESA_GL_VERSION_OVERRIDE=str(3.3),
    )  # insert env variables here

    logger.debug('Simulator binary' + bin)

    def stdw():
      for c in iter(lambda: self.proc.stdout.read(1), ''):
        sys.stdout.write(c)
        sys.stdout.flush()

    def poll():
      while not self.proc.poll():
        limit = 3
        if memory_usage(self.proc.pid) > limit * 1024 ** 3:
          logger.warning('Memory usage above ' + str(limit) + ' gb. Restarting after this episode.')
          self.restart = True
        sleep(5)
      logger.debug('Unity returned with ' + str(self.proc.returncode))

    # https://docs.unity3d.com/Manual/CommandLineArguments.html

    # TODO: ensure that the sim doesn't read or write any cache or config files
    config_dir = os.path.expanduser('~/.config/unity3d/DefaultCompany/rl-unity')  # TODO: only works on linux
    if os.path.isdir(config_dir):
      from shutil import rmtree
      rmtree(config_dir, ignore_errors=True)

    def limit():
      import resource
      l = 6 * 1024 ** 3  # allow 3 gb of address space (less than 3 gb makes the sim crash on startup)
      try:
        # resource.setrlimit(resource.RLIMIT_RSS, (l, l))
        # resource.setrlimit(resource.RLIMIT_DATA, (l, l))
        # resource.setrlimit(resource.RLIMIT_AS, (l, resource.RLIM_INFINITY))
        pass
      except Exception as e:
        print(e)
        raise

    stderr = self.logfile if self.logfile else (subprocess.PIPE if self.log_unity else subprocess.DEVNULL)
    import shutil
    self.proc = subprocess.Popen([bin,
                                  *(['-logfile'] if self.log_unity else []),
                                  *(['-batchmode', '-nographics'] if self.batchmode else []),
                                  '-screen-width {}'.format(self.w),
                                  '-screen-height {}'.format(self.h),
                                  ],
                                 env=env,
                                 stdout=stderr,
                                 stderr=stderr,
                                 universal_newlines=True)

    threading.Thread(target=poll, daemon=True).start()

    # threading.Thread(target=stdw, daemon=True).start()

    # wait until connection with simulator process
    timeout = 20
    for i in range(timeout * 10):
      if self.proc.poll():
        logger.debug('simulator died')
        break

      try:
        self.soc.connect((host, port))
        self.soc.settimeout(2)  # 20 minutes
        self.connected = True
        break
      except ConnectionRefusedError as e:
        if i == timeout * 10 - 1:
          print(e)

      sleep(.1)

    if not self.connected:
      raise ConnectionRefusedError('Connection with simulator could not be established.')

  def _reset(self):
    if not self.configured:
      self.conf()

    if self.restart:
      self.disconnect()
      self.restart = False

    if not self.connected:
      self.connect()
    else:
      self.send(np.zeros(2), reset=True)

    # skip first observation from simulator because it's faulty
    # TODO: fix first observation in simulator
    state, frame = self.receive()

    self.send(np.zeros(2), reset=False)

    self.reset_metrics()

    return self.receive_with_timeout_checker()

  def receive_with_timeout_checker(self):
    state = None
    frame = None
    nb_timeout = 0

    while state is None:
      state, frame = self.receive()
      if state is None:
        logger.debug("TIMEOUT n°" + str(nb_timeout+1))
        if nb_timeout >= 3:
          logger.debug("Can't reach the simulator, restarting...")
          self.restart = True
          return UnityEnv._reset(self)
        else:
          logger.debug("Retrying....")
          nb_timeout += 1
          self.send(np.zeros(2), reset=False)
    return state, frame


  def receive(self):
    pixel_buffer_size = 0 if self.batchmode or not self.send_frame else self.frame_w * self.frame_h * 4
    buffer_size = self.sd * 4 + pixel_buffer_size
    # receive data from simulator process
    data_in = b""
    while len(data_in) < buffer_size:
      # We don't want to have the simulator and the script to be both on receive mode.
      # Therefore in this case, there is a timeout on the recv function, returning None for both state and frame
      # It's the step and reset function duty to check if state is None. In this case, they should try to re-send the
      # previous action
      try:
        chunk = self.soc.recv(min(1024, buffer_size - len(data_in)))
      except socket.timeout:
        return None, None
      data_in += chunk

    # Checking data points are not None, if yes parse them.
    if self.wp is None:
      with open(os.path.join(self.sim_path, 'sim_Data', 'waypoints_SimpleTerrainTest.txt')) as f:
        try:
          wp = json.load(f)
          #self.wp = np.array([[e['x'], e['y'], e['z']] for e in wp])
          self.wp = np.array([[e['x'], e['z']] for e in wp])
          logger.debug(str(self.wp))
        except json.JSONDecodeError:
          self.wp = None

      from rlunity.utils.catmullrom import sampleRoad
      road = sampleRoad(self.wp)
      self.road_x = [p[0] for p in road]
      self.road_y = [p[1] for p in road]
      

    # Read the number of float sent by the C# side. It's the first number
    # sd = int(np.frombuffer(data_in, np.float32, 1, 0))
    # assert sd == self.sd, f'State dimension expected: {self.sd}, received: {sd}'

    state = np.frombuffer(data_in, np.float32, self.sd, 0)

    if self.batchmode or not self.send_frame:
      frame = None
    else:
      # convert frame pixel data into a numpy array of shape [width, height, 3]
      frame = np.frombuffer(data_in, np.uint8, -1, self.sd * 4)
      # logger.debug(str(len(frame)))
      frame = np.reshape(frame, [self.frame_w, self.frame_h, 4])
      frame = frame[::-1, :, :3]

    self.last_frame = frame
    self.last_state = state

    return state, frame

  def save_metrics(self):
    if self.testing:
        if not os.path.exists("./plots"):
            os.mkdir("./plots")

        self.final_rewards[-1].append(self.rewards)
        self.final_distance[-1].append(self.distance_driven)
        self.mean_distances[-1].append(np.mean(self.distances))

        final_rewards = np.array(self.final_rewards[-1]) / 10000.0
        final_distance = np.array(self.final_distance[-1]) / 5625.0
        mean_distances = np.array(self.mean_distances[-1])

        self.save_metrics_file()

        fig = plt.figure(1)
        fig.clf()
        plt.title("Metrics normalized")
        ax = fig.add_subplot(111)
        ax.plot(final_rewards, label='Final Reward')
        ax.plot(final_distance, label='Final distance')
        ax.plot(mean_distances, label='Mean distances')
        handles, labels = ax.get_legend_handles_labels()
        lgd = ax.legend(handles, labels, loc='upper center', bbox_to_anchor=(0.5,-0.1))
        fig.savefig("./plots/metrics_" + self.date + ".png", bbox_extra_artists=(lgd,), bbox_inches='tight')

        return self.rewards, self.distance_driven, np.mean(self.distances)

        # plt.clf()
        # plt.title("Tracks (road in blue)")
        # plt.plot(self.road_x, self.road_y)
        # nb_tracks = len(self.all_positions)
        # for i in range(nb_tracks):
        #   r_color = 1
        #   road_x = [p[0] for p in self.all_positions[i]]
        #   road_y = [p[1] for p in self.all_positions[i]]
        #   plt.plot(road_x, road_y, color=(r_color,0,0))
        # plt.savefig("./plots/positions_" + self.date + ".png")


  def setup_metrics(self, cont = False):
    if os.path.exists(FINAL_REWARD_FILE):
      self.final_rewards = np.load(FINAL_REWARD_FILE).tolist()
      self.mean_distances = np.load(MEAN_DISTANCES_FILE).tolist()
      self.final_distance = np.load(FINAL_DISTANCE_FILE).tolist()
    else:
      self.final_rewards = []
      self.mean_distances = []
      self.final_distance = []

    if not cont or not os.path.exists(FINAL_REWARD_FILE):
      self.final_rewards.append([])
      self.mean_distances.append([])
      self.final_distance.append([])

  def save_metrics_file(self):
    np.save(FINAL_REWARD_FILE, np.array(self.final_rewards))
    np.save(MEAN_DISTANCES_FILE, np.array(self.mean_distances))
    np.save(FINAL_DISTANCE_FILE, np.array(self.final_distance))

  def reset_metrics(self):
    self.rewards = 0
    self.distances = []
    self.distance_driven = 0

  def send(self, action, reset=False):
    a = np.concatenate((action, [1. if reset else 0., self.current_level]))
    a = np.array(a, dtype=np.float32)
    assert a.shape == (self.ad + 2,)

    data_out = a.tobytes()
    self.soc.sendall(data_out)

  def disconnect(self):
    if self.proc:
      self.proc.kill()
    if self.soc:
      self.soc.close()
    self.connected = False

  def _close(self):
    logger.debug('close')
    if self.proc:
      self.proc.kill()
    if self.soc:
      self.soc.close()
    if self.logfile:
      self.logfile.close()

  def _render(self, mode='human', close=False):
    if mode == 'rgb_array':
      return self.last_frame  # return RGB frame suitable for video
    elif mode is 'human':
      pass  # we do that anyway
    else:
      super()._render(mode, close)  # just raise an exception

  def change_level(self, level_number):
    self.current_level = level_number


def get_free_port(host):
  sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
  sock.bind((host, 0))
  port = sock.getsockname()[1]
  sock.close()
  return port


def memory_usage(pid):
  import psutil
  proc = psutil.Process(pid)
  mem = proc.memory_info().rss  # resident memory
  for child in proc.children(recursive=True):
    try:
      mem += child.memory_info().rss
    except psutil.NoSuchProcess:
      pass

  return mem
