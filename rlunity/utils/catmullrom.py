import numpy as np

def CatmullRom(t, p0, p1, p2, p3):
    m0 = (p2 - p0) / 2.0
    m1 = (p3 - p1) / 2.0
    return (2 * t * t * t - 3 * t * t + 1) * p1 + (t * t * t - 2 * t * t + t) * m0 + (-2 * t * t * t + 3 * t * t) * p2 + (t * t * t - t * t) * m1

def sampleRoad(points):
    res = []
    for i in range(len(points)):
        res.append(points[i])
        for t in [0.2, 0.4, 0.6, 0.8]:
            aux = i - 1
            if (i - 1) < 0:
                aux = len(points) - 1
            res.append(CatmullRom(t, points[aux], points[i], points[(i + 1) % len(points)], points[(i + 2) % len(points)]))
    res.append(points[0])
    return res