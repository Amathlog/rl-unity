import UnityEngine as unity

from priorityQueue import PriorityQueue


queue = PriorityQueue()

queue.enqueue("Test", 2)
queue.enqueue("DoubleTest", 3)

car = unity.GameObject.Find("Car")
unity.Debug.Log("Python side : " + car.name)
