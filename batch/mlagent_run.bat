@echo Activate ml-agent for MAFF
@echo config path: %userprofile%\ml-agent-for-factory\config\matt_vehicle.yaml
@echo off

cd %userprofile%
call conda activate mlagents
mlagents-learn %userprofile%\ml-agent-for-factory\config\matt_vehicle.yaml --run-id=Vehicle --resume

pause