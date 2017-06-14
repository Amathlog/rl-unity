from rl.random import OrnsteinUhlenbeckProcess,AnnealedGaussianProcess

class MultipleOUprocesses(AnnealedGaussianProcess):

    def __init__(self, nb, theta, mu, sigma):
        self.nb = nb
        self.processes = []
        for i in range(self.nb):
            self.processes.append(OrnsteinUhlenbeckProcess(theta[i], mu[i], sigma[i]))

    def sample(self):
        res = []
        for pr in self.processes:
            res.append(pr.sample())
        return np.array(res).reshape(1, self.nb)