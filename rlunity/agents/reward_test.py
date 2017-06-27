from ddpg_agent import DDPGAgentUnity

with open("reward_file.txt", "w") as f:

    f.write("Alpha | Beta | Gamma | Max Reward | Max Distance | Mean distance\n")

    alpha = [1, 2, 4, 8]
    beta = 1
    gamma = [1, 2, 4, 8]

    for a in alpha:
        for g in gamma:
            r, d, m = DDPGAgentUnity(["--reset"], a, beta, g, 20)
            f.write(str(a) + "|" + str(beta) + "|" + str(g) + "|" + str(r) + "|" + str(d) + "|" + str(m) + "\n")