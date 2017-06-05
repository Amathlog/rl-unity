from gym.envs.registration import register
from .car_pixels import UnityCarPixels
from .unity_car import UnityCar

register(
  id='UnityCarPixels-v0',
  entry_point='rlunity.envs:UnityCarPixels',
  reward_threshold=1000,
  tags={'wrapper_config.TimeLimit.max_episode_steps': 60 * 300},
)

register(
  id='UnityCar-v0',
  entry_point='rlunity.envs:UnityCar',
  reward_threshold=1000,
  tags={'wrapper_config.TimeLimit.max_episode_steps': 60 * 300},
)
