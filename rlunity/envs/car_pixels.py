import gym
from PIL import Image
from gym import spaces

from rlunity.unity_env import UnityEnv, logger

import numpy as np


class UnityCarPixels(UnityEnv):
  """
  a = [steering wheel (right positive), throttle]
  """
  def __init__(self):
    super().__init__(batchmode=False)
    self.t_max = 20 * 60 * 10

    self.t0 = 20 * 7

    sbm = self.sbm
    self.observation_space = spaces.Box(0.0, 1.0, shape=[84, 84, 3])
    self.reward_range = (-.1, .1)
    self.r = 0
    self.last_position = None
    self.driven_distance = 0

  def process_raw_state(self, raw_state):
    # logger.debug("Distance = " + str(raw_state[0]) + " ; Speed Projected road = " + str(raw_state[1:4]))
    # logger.debug("Position = " + str(raw_state[4:7]) + " ; Projection = " + str(raw_state[7:10]))
    # logger.debug("Collision detected : " + ("True" if raw_state[10] == 1.0 else "False"))
    # logger.debug("Road direction : " + str(raw_state[11:14]) + "; Car direction : " + str(raw_state[14:17]))
    # logger.debug("Next angle :" + str(raw_state[17]))

    # The state is :
    # - Distance from the road (positive and nagative values = right-left position)
    # - Angle with the road 
    # - speed along the road (X)
    # - speed perpendicular to the road(Y)
    # - speed up to the road (Z)
    # - Average speed along road
    # - next angle

    distance = raw_state[0]
    car_direction = raw_state[14:17]
    road_direction = raw_state[11:14]
    angle = np.math.acos(np.dot(car_direction, road_direction))
    speed_x, speed_y, speed_z = raw_state[1:4]
    next_angle = raw_state[17]

    position = raw_state[4:7]

    if self.last_position is None:
      self.last_position = position

    self.driven_distance = np.linalg.norm(self.last_position - position)
    self.last_position = position

    #logger.debug("Angle :" + str(angle))

    self.v[self.t] = raw_state[1]
    av_speed = self.v[max(0, self.t - self.t0):self.t+1].mean()

    return np.array([distance/40, angle, speed_x/11.0, speed_y/11.0, speed_z/11.0, av_speed/11.0, next_angle])

  def _reset(self):
    self.v = np.zeros(self.t_max)
    self.t = 0
    state, frame = super()._reset()
    state = self.process_raw_state(state)
    return frame

  def proc_frame(self, frame):
    if np.shape(frame) != (84, 84, 3):
      img = Image.fromarray(frame)
      img = img.resize([84, 84]).convert('L')
      frame = np.asarray(img, dtype=np.uint8)

    return frame

  def _step(self, action):
    action = np.clip(action, -1, 1)
    self.send(action)
    state, frame = self.receive()

    # If state is None, there was a timeout, retry...
    if state is None:
      logger.debug("Timeout in step, retry sending action.")
      return self._step(action)

    state = self.process_raw_state(state)

    distance = state[0]
    angle = state[1]
    speed_x = state[2]
    speed_y = state[3]
    av_speed = state[5]

    # logger.info(f'd0 = {d0}')
    done = np.abs(distance) > 1 or (self.t > self.t0 and av_speed < 0.1)

    #r_speed = speed
    # r_speed = 0.1 - .9 * (speed - .2)**2

    # reward = .2 - 1 * (distance - .5) ** 2 + r_speed
    # if done:
    #   reward -= 30

    # reward = 1 * reward
    if done:
      reward = -1
    else:
      reward = np.clip(speed_x - abs(speed_y) - abs(distance)*2, -1, 1)

    # logger.debug("State: " + str(state))
    # logger.debug("Reward: " + str(reward))
    # logger.debug("Speedy = " + str(speed_y))
    # logger.debug("")

    done = done or self.t+1 >= self.t_max

    self.t += 1

    return frame, reward, done, {'distance_from_road': distance, 'speed': speed_x, 'average_speed': av_speed, 'unwrapped_reward': reward}

  def report(self):
    logger.info('Distance driven: ' + str(self.v.sum()))


def draw_rect(a, pos, color):
  """
  :param a:
  :param pos: (x0, y0), (x1, y1) in normalized cartesian coordinates
  :param color:
  :return:
  """
  pos = np.asarray(pos) * (1, -1) + (0, 1)  # transform to matrix coordinates (flip y axis)
  pos = pos * (a.shape[1], a.shape[0])
  (x0, y1), (x1, y0) = np.asarray(pos, dtype=np.int32)
  a[y0:y1, x0:x1, ...] = color
  return a

if __name__ == '__main__':
  import argparse

  parser = argparse.ArgumentParser(description='Unity Gym Environment')
  parser.add_argument('--batchmode', action='store_true', help='Run the simulator in batch mode with no graphics')
  args = parser.parse_args()
  logger.debug('Batchmode ' + str(args.batchmode))
  bm = args.batchmode

  import rlunity

  env = gym.make('UnityCarPixels-v0')  # requires import rlunity
  env.unwrapped.conf(loglevel='debug', log_unity=True, frame_w = 84, frame_h = 84)
  env.reset()
  for i in range(10000):
    print(i)
    env.step([.0, 1.0])

    if (i + 1) % 300 == 0:
      env.reset()

