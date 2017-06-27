from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
from matplotlib import cm
import numpy as np

fig = plt.figure()
ax = fig.gca(projection='3d')

with open("reward_file.txt", "r") as f:

    legend = f.readline().split('|')
    alpha = []
    beta = []
    gamma = []
    max_reward = []
    max_distance = []
    mean_distance = []

    for line in f:
        values = line.split('|')
        alpha.append(int(values[0]))
        beta.append(int(values[1]))
        gamma.append(int(values[2]))
        max_reward.append(float(values[3]))
        max_distance.append(float(values[4]))
        mean_distance.append(float(values[5]))

    alpha = np.array([1,2,4,8])
    beta = np.array(beta)
    gamma = np.array([1,2,4,8])
    max_reward = np.array(max_reward) / max(max_reward)
    max_distance = np.array(max_distance) / max(max_distance)
    mean_distance = np.array(mean_distance)

    alpha, gamma = np.meshgrid(alpha, gamma)
    max_reward = max_reward.reshape(alpha.shape).transpose()
    max_distance = max_distance.reshape(alpha.shape).transpose()
    mean_distance = mean_distance.reshape(alpha.shape).transpose()
    print(alpha)
    print(gamma)
    print(mean_distance)
    # Plot the surface.
    m_r = ax.plot_wireframe(alpha, gamma, max_reward, cmap=cm.coolwarm,
                       linewidth=1, antialiased=True, color='b', label='Max Reward')

    ax.set_xlabel("Alpha")
    ax.set_ylabel("Gamma")

    m_d = ax.plot_wireframe(alpha, gamma, max_distance, cmap=cm.coolwarm,
                       linewidth=1, antialiased=True, color='r',label='Max Distance')
    m_d = ax.plot_wireframe(alpha, gamma, mean_distance, cmap=cm.coolwarm,
                       linewidth=1, antialiased=True, color='g', label='Mean Distance')
    handles, labels = ax.get_legend_handles_labels()
    lgd = ax.legend(handles, labels, loc='right center', bbox_to_anchor=(0.15,0.1))
    plt.show()


