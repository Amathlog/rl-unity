#! bin/bash
/opt/Unity/Editor/Unity -logfile -batchmode -quit -projectPath $PWD/simulator -buildLinux64Player $PWD/simulator/bin/unix/sim.x86_64
