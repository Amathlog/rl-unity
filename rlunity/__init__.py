from .unity_env import UnityCar

from gym.envs.registration import register
register(
  id='UnityCar-v0',
  entry_point='rlunity:UnityCar',
  reward_threshold=1000,
  tags={'wrapper_config.TimeLimit.max_episode_steps': 60*300},
  )
