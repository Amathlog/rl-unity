from setuptools import setup
from setuptools import find_packages


setup(name='rl-unity',
      version='0.1',
      description='OpenAI gym environments powered by the Unity3D engine',
      author='Adrien Logut, Simon Ramstedt',
      author_email='simonramstedt@gmail.com',
      url='https://github.com/amathlog/rl-unity',
      download_url='',
      license='MIT',
      install_requires=['gym'],
      extras_require={

      },
      scripts=[''],
      packages=find_packages())
