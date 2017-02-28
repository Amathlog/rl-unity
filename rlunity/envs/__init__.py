from gym.envs.registration import register

register(
    id='unityenv-v0',
    entry_point='rlunity.unity_env:UnityEnv',
)
