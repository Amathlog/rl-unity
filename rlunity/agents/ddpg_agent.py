import gym
import rlunity
from rl.agents import DDPGAgent
from rl.memory import SequentialMemory
from rl.random import OrnsteinUhlenbeckProcess

from keras.layers import Dense, Activation, Input, Flatten, concatenate
from keras.models import Model
from keras.layers.merge import Concatenate
from keras.optimizers import Adam


# Create gym env
env = gym.make('UnityCar-v0')  # requires import rlunity
env.unwrapped.conf(loglevel='debug', log_unity=True)

# Sizes
STATE_SIZE = env.observation_space.shape[0]
ACTION_SIZE = 2
HIDDEN_SIZE_1 = 100
HIDDEN_SIZE_2 = 50

# Create the input
observation_input = Input(shape=(1,) + env.observation_space.shape, name="ObservationInput")
action_input = Input(shape=(ACTION_SIZE,), name="ActionInput")
flattened_observation = Flatten()(observation_input)

# Actor
a = flattened_observation
a = Dense(HIDDEN_SIZE_1)(a)
a = Activation('relu')(a)
a = Dense(HIDDEN_SIZE_2)(a)
a = Activation('relu')(a)
accl = Dense(1)(a)
accl = Activation('sigmoid')(accl)
steer = Dense(1)(a)
steer = Activation('tanh')(steer)
a = concatenate([accl, steer])
actor = Model(input=observation_input, output=a)
#actor.summary()

# Critic
c = concatenate([action_input, flattened_observation])
c = Dense(HIDDEN_SIZE_1)(c)
c = Activation('relu')(c)
c = Dense(HIDDEN_SIZE_2)(c)
c = Activation('relu')(c)
c = Dense(1)(c)
c = Activation('linear')(c)
critic = Model(input=[action_input, observation_input], output=c)
#critic.summary()

memory = SequentialMemory(limit=5000, window_length=1)
#random_process = OrnsteinUhlenbeckProcess(theta=0.15, mu=0, sigma=0.3)
agent = DDPGAgent(nb_actions=ACTION_SIZE, actor=actor, critic=critic, critic_action_input=action_input,
                  memory=memory, nb_steps_warmup_critic=100, nb_steps_warmup_actor=100,
                   gamma=.99, target_model_update=1e-3)
agent.compile(Adam(lr=.001, clipnorm=1.), metrics=['mae'])

agent.fit(env, 1000)


