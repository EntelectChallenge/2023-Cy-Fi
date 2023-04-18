#!/bin/bash

pushd ./2023-CyFi/
docker build . -t cyfi
popd

docker run -it --rm -p 5000:5000 --name cyfi cyfi
