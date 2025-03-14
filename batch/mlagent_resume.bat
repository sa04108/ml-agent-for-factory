@echo Activate ml-agent for MAFF
@echo config path: ..\config\matt_vehicle.yaml
@echo off

call conda activate mlagents
cd .. && mlagents-learn config\matt_vehicle.yaml --run-id=Vehicle --resume

pause