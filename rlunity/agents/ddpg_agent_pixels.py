import gym
import rlunity
from rl.agents import DDPGAgent
from rl.memory import SequentialMemory
from rl.random import OrnsteinUhlenbeckProcess

from keras.layers import Dense, Activation, Input, Flatten, concatenate
from keras.models import Model
from keras.layers.merge import Concatenate
from keras.optimizers import Adam
from keras.callbacks import ModelCheckpoint
from OU import MultipleOUprocesses
from keras import initializers

import argparse

parser = argparse.ArgumentParser(description='DDPG training')
parser.add_argument('--test', action="store_true", default=False, help='Only testing')
parser.add_argument('--reset', action="store_true", default=False, help='Reset weights')
args = parser.parse_args()

class UnityProcessor(Processor):
    def process_observation(self, observation):
        assert observation.ndim == 3  # (height, width, channel)
        img = Image.fromarray(observation)
        img = img.resize(INPUT_SHAPE).convert('L')  # resize and convert to grayscale
        processed_observation = np.array(img)
        assert processed_observation.shape == INPUT_SHAPE
        return processed_observation.astype('uint8')  # saves storage in experience memory

    def process_state_batch(self, batch):
        # We could perform this processing step in `process_observation`. In this case, however,
        # we would need to store a `float32` array instead, which is 4x more memory intensive than
        # an `uint8` array. This matters if we store 1M observations.
        processed_batch = batch.astype('float32') / 255.
        return processed_batch

    def process_reward(self, reward):
        return np.clip(reward, -1., 1.)


# Create gym env
env = gym.make('UnityCarPixels-v0')  # requires import rlunity
env.unwrapped.conf(loglevel='debug', log_unity=True, w=1024, h=768, frame=True, frame_w=84, frame_h=84)

# Sizes
STATE_SIZE = env.observation_space.shape[0]
ACTION_SIZE = 2
HIDDEN_SIZE_1 = 200
HIDDEN_SIZE_2 = 100
OU_THETA = [0.6,1.0]
OU_MU = [0,0.6]
OU_SIGMA = [0.1, 0.3]

input_shape = (3,) + 84
critic = Sequential()

convnet = Sequential()
if K.image_dim_ordering() == 'tf':
    # (width, height, channels)
    convnet.add(Permute((2, 3, 1), input_shape=input_shape))
elif K.image_dim_ordering() == 'th':
    # (channels, width, height)
    convnet.add(Permute((1, 2, 3), input_shape=input_shape))
else:
    raise RuntimeError('Unknown image_dim_ordering.')

convnet.add(Conv2D(32, 8, 8, kernel_initializer='he_normal'))
convnet.add(Activation('relu'))
convnet.add(Conv2D(32, 4, 4, kernel_initializer='he_normal'))
convnet.add(Activation('relu'))
convnet.add(Conv2D(32, 3, 3, kernel_initializer='he_normal'))
convnet.add(Activation('relu'))
convnet.add(Flatten())
convnet.add(Dense(200, kernel_initializer='he_normal'))
convnet.add(Activation('relu'))

# Including the action now
action_input = Input(shape=(ACTION_SIZE,), name="ActionInput")
actions = Sequential()
actions.add(action_input)

critic.add(Concatenate([convnet, actions]))
critic.add(Dense(200, kernel_initializer='he_normal'))
critic.add(Activation('relu'))
critic.add(Dense(1, kernel_initializer=initializers.random_uniform(minval=-0.0003, maxval=0.0003)))
critic.add(Activation('linear'))
critic.summary()

# Create the input
action_input = Input(shape=(ACTION_SIZE,), name="ActionInput")

# Actor
actor = Sequential()
if K.image_dim_ordering() == 'tf':
    # (width, height, channels)
    actor.add(Permute((2, 3, 1), input_shape=input_shape))
elif K.image_dim_ordering() == 'th':
    # (channels, width, height)
    actor.add(Permute((1, 2, 3), input_shape=input_shape))
else:
    raise RuntimeError('Unknown image_dim_ordering.')

actor.add(Conv2D(32, 8, 8, kernel_initializer='he_normal'))
actor.add(Activation('relu'))
actor.add(Conv2D(32, 4, 4, kernel_initializer='he_normal'))
actor.add(Activation('relu'))
actor.add(Conv2D(32, 3, 3, kernel_initializer='he_normal'))
actor.add(Activation('relu'))
actor.add(Flatten())
actor.add(Dense(200, kernel_initializer='he_normal'))
actor.add(Activation('relu'))
actor.add(Dense(200, kernel_initializer='he_normal'))
actor.add(Activation('relu'))

accl = Sequential()
accl.add(Dense(1, kernel_initializer=initializers.random_uniform(minval=-0.0003, maxval=0.0003)))
accl.add(Activation('sigmoid'))
steer = Sequential()
steer.add(Dense(1, kernel_initializer=initializers.random_uniform(minval=-0.0003, maxval=0.0003)))
steer.add(Activation('tanh'))

actor.add(Concatenate([steer, accl]))
actor.summary()

memory = SequentialMemory(limit=100000, window_length=4)
random_process = MultipleOUprocesses(ACTION_SIZE, OU_THETA, OU_MU, OU_SIGMA)
agent = DDPGAgent(nb_actions=ACTION_SIZE, actor=actor, critic=critic, critic_action_input=action_input,
                  memory=memory, nb_steps_warmup_critic=100, nb_steps_warmup_actor=100,
                   gamma=.99, target_model_update=1e-3, batch_size=16)
agent.compile(Adam(lr=.001, clipnorm=1.), metrics=['mae'])

filepath = "ddpg_weights"
actor_file = filepath + "_actor.h5f"
filepath += ".h5f"
import os
if args.reset:
    agent.save_weights(filepath, overwrite = True)

if os.path.exists(actor_file) and not args.reset:
    agent.load_weights(filepath)

env.reward = env.reward_right_road

if(args.test):
    print("Only testing...")
    while(True):
        env.change_level(1)
        agent.test(env, nb_episodes=1, visualize=False)
else:
    while(True):
        print("Start training...")
        agent.fit(env, 10000)
        print("End of training, saving weights...")
        agent.save_weights(filepath, overwrite = True)
        print("Weights trained, testing...")
        env.change_level(1)
        agent.test(env, nb_episodes=2, nb_max_episode_steps=50000, visualize=False)
        print("Train over...")
        env.change_level(0)


