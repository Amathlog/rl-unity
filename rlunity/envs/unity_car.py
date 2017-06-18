import gym
from gym import spaces

from rlunity.unity_env import UnityEnv, logger

import numpy as np

class UnityCar(UnityEnv):
  """
  a = [steering wheel (right positive), throttle]
  """
  def __init__(self):
    super().__init__(batchmode=False)
    self.t_max = 10000  # about 1h of driving at 30fps

    self.t0 = 7*60

    #sbm = self.sbm + 33
    self.sbm = 7
    sbm = 7
    self.observation_space = spaces.Box(-np.ones([sbm]), np.ones([sbm]))
    self.reward = self.reward_center_road
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

    # radial basis function features
    # pos = self.wp - raw_state[4:7]
    # pos_fp = np.exp(- np.sum(np.square(pos), axis=1) / 100)

    # direction = raw_state[14:17] - raw_state[11:14]
    

    # return np.concatenate(((
    #     raw_state[0] / 100,  # distance from road center (in meters) TODO: verify
    #     raw_state[1] / 12,  # speed projected onto road (in meters / s) TODO: verify
    #     raw_state[2] / 12, # speed projected perpendicular to the road
    #     av_speed,  # running average of the speed
    #   ),
    #   direction,  # direction relative to road
    #   pos_fp,  # radial basis functions of location w.r.t. the waypoints
    # ))

  def _reset(self):
    self.v = np.zeros(self.t_max)
    self.t = 0
    self.last_position = None
    state, frame = super()._reset()
    state = self.process_raw_state(state)
    return state

  def _step(self, action):
    action = np.clip(action, -1, 1)
    #logger.debug("Action taken=" + str(action))
    self.send(action)
    state, frame = self.receive()
    # If state is None, there was a timeout, retry...
    if state is None:
      logger.debug("Timeout in step, retry sending action.")
      return self._step(action)
      
    #logger.debug(str(frame))

    state = self.process_raw_state(state)
    # logger.debug("Action taken: " + str(action))
    # logger.debug("State: " + str(state))

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
      reward = self.reward(state)

    # logger.debug("State: " + str(state))
    # logger.debug("Reward: " + str(reward))
    # logger.debug("Speedy = " + str(speed_y))
    # logger.debug("")

    done = done or self.t+1 >= self.t_max

    self.t += 1

    self.rewards += reward
    self.distances.append(abs(distance))
    self.distance_driven += self.driven_distance

    return state, reward, done, {}

  def reward_center_road(self, state):
    distance = state[0]
    speed_x = state[2]
    speed_y = state[3]
    return np.clip(speed_x - abs(speed_y) - abs(distance), -1, 1)

  def reward_right_road(self, state):
    distance = state[0]
    speed_x = state[2]
    speed_y = state[3]
    return np.clip((speed_x - 0.4)*1.3 - abs(speed_y) - abs(0.3 - distance)*2, -1, 1)

  def reward_left_road(self, state):
    distance = state[0]
    speed_x = state[2]
    speed_y = state[3]
    return np.clip(speed_x - abs(speed_y) - abs(distance + 0.3)*2, -1, 1)

  def report(self):
    logger.info('Distance driven: ' + str(self.v.sum()))


def test_unity_car():
  import logging
  import rlunity
  logger.setLevel(logging.DEBUG)

  env = gym.make('UnityCar-v0')  # requires import rlunity
  env.unwrapped.conf(loglevel='debug', log_unity=True)
  env.reset()
  for i in range(10000):
    print(i)
    env.step([.0, 1.0])

    if (i + 1) % 300 == 0:
      env.reset()


if __name__ == '__main__':
  test_unity_car()
