@echo off

cd .\2023-CyFi\
docker build . -t cyfi
cd ..\
docker run -it --rm -p 5000:5000 --name cyfi cyfi
