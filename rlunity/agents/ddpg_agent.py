import gym
import rlunity
from rl.agents import DDPGAgent
from rl.memory import SequentialMemory
from rl.random import OrnsteinUhlenbeckProcess

from keras.layers import Dense, Activation, Input, Flatten, concatenate
from keras.models import Model
from keras import initializers
from keras.layers.merge import Concatenate
from keras.optimizers import Adam
from keras.callbacks import ModelCheckpoint
from OU import MultipleOUprocesses

import argparse


# Reward formula : r = alpha * v_x - beta * abs(v_y) - gamma * abs(distance) 
def DDPGAgentUnity(args = None, alpha = 1, beta = 1, gamma = 1, number_of_test = -1):

    parser = argparse.ArgumentParser(description='DDPG training')
    parser.add_argument('--test', action="store_true", default=False, help='Only testing')
    parser.add_argument('--reset', action="store_true", default=False, help='Reset weights')
    parser.add_argument('--cont', action="store_true", default=False, help='Continue to plot into the same simulation')
    if args is None:
        args = parser.parse_args()
    else:
        args = parser.parse_args(args)


    # Create gym env
    env = gym.make('UnityCar-v0')  # requires import rlunity
    env.unwrapped.conf(loglevel='debug', log_unity=True, w=1024, h=768, frame=False, frame_w=128, frame_h=128, cont=args.cont)
    env.unwrapped.setup_reward(alpha, beta, gamma)

    # Sizes
    STATE_SIZE = env.observation_space.shape[0]
    ACTION_SIZE = 2
    HIDDEN_SIZE_1 = 200
    HIDDEN_SIZE_2 = 100
    OU_THETA = [0.6,1.0]
    OU_MU = [0,0.6]
    OU_SIGMA = [0.1, 0.3]

    # Create the input
    observation_input = Input(shape=(1,) + env.observation_space.shape, name="ObservationInput0")
    observation_input1 = Input(shape=(1,) + env.observation_space.shape, name="ObservationInput1")
    action_input = Input(shape=(ACTION_SIZE,), name="ActionInput")
    flattened_observation = Flatten()(observation_input)
    flattened_observation1 = Flatten()(observation_input1)

    # Actor
    a = flattened_observation
    a = Dense(HIDDEN_SIZE_1, kernel_initializer='he_normal')(a)
    a = Activation('relu')(a)
    a = Dense(HIDDEN_SIZE_2, kernel_initializer='he_normal')(a)
    a = Activation('relu')(a)
    accl = Dense(1)(a)
    accl = Activation('sigmoid')(accl)
    steer = Dense(1)(a)
    steer = Activation('tanh')(steer)
    a = concatenate([steer, accl])
    actor = Model(input=observation_input, output=a)
    #actor.summary()

    # Critic
    c = concatenate([action_input, flattened_observation1])
    c = Dense(HIDDEN_SIZE_1, kernel_initializer='he_normal')(c)
    c = Activation('relu')(c)
    c = Dense(HIDDEN_SIZE_2, kernel_initializer='he_normal')(c)
    c = Activation('relu')(c)
    c = Dense(1)(c)
    c = Activation('linear')(c)
    critic = Model(input=[action_input, observation_input1], output=c)
    #critic.summary()

    memory = SequentialMemory(limit=5000, window_length=1)
    random_process = MultipleOUprocesses(ACTION_SIZE, OU_THETA, OU_MU, OU_SIGMA)
    agent = DDPGAgent(nb_actions=ACTION_SIZE, actor=actor, critic=critic, critic_action_input=action_input,
                      memory=memory, nb_steps_warmup_critic=100, nb_steps_warmup_actor=100,
                       gamma=.99, target_model_update=1e-3, batch_size=64)
    agent.compile(Adam(lr=.001, clipnorm=1., decay=1e-08), metrics=['mae'])

    fileToLoad = "ddpg_weights_simpleReward"
    filepath = "ddpg_weights_simpleReward"
    actor_file = fileToLoad + "_actor.h5f"
    filepath += ".h5f"
    fileToLoad += ".h5f"
    import os
    if args.reset:
        agent.save_weights(filepath, overwrite = True)

    if os.path.exists(actor_file) and not args.reset:
        agent.load_weights(fileToLoad)

    max_reward = -1
    max_distance = 0
    mean_distance = 1

    env.unwrapped.reward = env.unwrapped.reward_right_road
    #env.unwrapped.reward = env.unwrapped.reward_center_road_harder

    if(args.test):
        print("Only testing...")
        while(True):
            env.unwrapped.change_level(0)
            agent.test(env, nb_episodes=1, visualize=False)
    else:
        while(number_of_test != 0):
            env.unwrapped.change_level(1)
            print("Start training...")
            agent.fit(env, 2500, log_interval=500)
            print("End of training, saving weights...")
            agent.save_weights(filepath, overwrite = True)
            print("Weights trained, testing...")
            env.unwrapped.change_level(0)
            env.unwrapped.testing = True
            agent.test(env, nb_episodes=1, nb_max_episode_steps=50000, visualize=False)
            final_reward, final_distance, mean_dist = env.unwrapped.save_metrics()
            if(max_reward < final_reward):
                max_reward = final_reward
            if(max_distance < final_distance):
                max_distance = final_distance
            if(mean_distance > mean_dist and max_distance >= 0.5):
                mean_distance = mean_dist
            print("Train over...")
            env.unwrapped.change_level(1)
            env.unwrapped.testing = False
            number_of_test -= 1

    return max_reward, max_distance, mean_distance


if __name__ == "__main__":
    DDPGAgentUnity()


