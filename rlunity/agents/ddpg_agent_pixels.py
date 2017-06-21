import gym
import rlunity
from rl.agents import DDPGAgent
from rl.memory import SequentialMemory
from rl.random import OrnsteinUhlenbeckProcess

from keras.layers import Dense, Activation, Input, Flatten, concatenate, Permute, Conv2D
from keras.models import Model, Sequential
from keras.layers.merge import Concatenate
from keras.optimizers import Adam
from keras.callbacks import ModelCheckpoint
from OU import MultipleOUprocesses
from keras import initializers
from rl.core import Processor
import keras.backend as K
from PIL import Image
import numpy as np

import argparse

parser = argparse.ArgumentParser(description='DDPG training')
parser.add_argument('--test', action="store_true", default=False, help='Only testing')
parser.add_argument('--reset', action="store_true", default=False, help='Reset weights')
args = parser.parse_args()

INPUT_SHAPE = (84,84)
WINDOW_LENGTH = 3

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
HIDDEN_SIZE_2 = 200
OU_THETA = [0.6,1.0]
OU_MU = [0,0.6]
OU_SIGMA = [0.1, 0.3]

input_shape = (WINDOW_LENGTH,) + INPUT_SHAPE
critic = Sequential()

observation_input = Input(input_shape, name="ObservationInput")

if K.image_dim_ordering() == 'tf':
    # (width, height, channels)
    convnet = Permute((2, 3, 1))(observation_input)
elif K.image_dim_ordering() == 'th':
    # (channels, width, height)
    convnet = Permute((1, 2, 3))(observation_input)
else:
    raise RuntimeError('Unknown image_dim_ordering.')

convnet = Conv2D(32, kernel_size=(8, 8), kernel_initializer='he_normal', strides=(4,4))(convnet)
convnet = Activation('relu')(convnet)
convnet = Conv2D(32, kernel_size=(4, 4), kernel_initializer='he_normal', strides=(2,2))(convnet)
convnet = Activation('relu')(convnet)
convnet = Flatten()(convnet)

# Including the action now
action_input = Input(shape=(ACTION_SIZE,), name="ActionInput")

critic = concatenate([convnet, action_input])

critic = Dense(200, kernel_initializer='he_normal')(critic)
critic = Activation('relu')(critic)
critic = Dense(1)(critic)
critic = Activation('linear')(critic)

critic = Model(input=[observation_input, action_input], output=critic)

#critic.summary()

# # Actor
# observation_input1 = Input(input_shape, name="ObservationInput1")
# if K.image_dim_ordering() == 'tf':
#     # (width, height, channels)
#     actor = Permute((2, 3, 1))(observation_input1)
# elif K.image_dim_ordering() == 'th':
#     # (channels, width, height)
#     actor = Permute((1, 2, 3))(observation_input1)
# else:
#     raise RuntimeError('Unknown image_dim_ordering.')

actor = Dense(200, kernel_initializer='he_normal')(convnet)
actor = Activation('relu')(actor)

accl = Dense(1)(actor)
accl = Activation('sigmoid')(accl)
steer = Dense(1)(actor)
steer = Activation('tanh')(steer)
actor = concatenate([steer, accl])
actor = Model(input=observation_input, output=actor)
#actor.summary()

memory = SequentialMemory(limit=100000, window_length=WINDOW_LENGTH)
random_process = MultipleOUprocesses(ACTION_SIZE, OU_THETA, OU_MU, OU_SIGMA)
processor = UnityProcessor()
agent = DDPGAgent(nb_actions=ACTION_SIZE, actor=actor, critic=critic, critic_action_input=action_input,
                  memory=memory, nb_steps_warmup_critic=100, nb_steps_warmup_actor=100,
                   gamma=.99, target_model_update=1e-3, batch_size=16, processor=processor)
agent.compile(Adam(lr=.001, clipnorm=1.), metrics=['mae'])

filepath = "ddpg_pixels_weights"
actor_file = filepath + "_actor.h5f"
filepath += ".h5f"
import os
if args.reset:
    agent.save_weights(filepath, overwrite = True)

if os.path.exists(actor_file) and not args.reset:
    agent.load_weights(filepath)

if(args.test):
    print("Only testing...")
    while(True):
        env.unwrapped.change_level(1)
        agent.test(env, nb_episodes=1, visualize=False)
else:
    while(True):
        print("Start training...")
        agent.fit(env, 2500, log_interval=500)
        print("End of training, saving weights...")
        agent.save_weights(filepath, overwrite = True)
        print("Weights trained, testing...")
        env.unwrapped.change_level(1)
        env.unwrapped.testing = True
        agent.test(env, nb_episodes=1, nb_max_episode_steps=10000, visualize=False)
        env.unwrapped.save_metrics()
        print("Train over...")
        env.unwrapped.change_level(0)
        env.unwrapped.testing = False


