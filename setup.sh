#!/bin/bash

set -e

function setup_python_env() {
    rootName=$1 # string

    cd $rootName
    if [ ! -d "venv" ]; then
        python -m venv --clear venv --prompt $rootName
    fi
    source ./venv/bin/activate
    pip install wheel
    pip install -r requirements.txt
    deactivate
    cd ..
}

function setup_node_env() {
    rootName=$1 # string
    cd $rootName
    npm install
    npm run build
    cd ..
}

setup_python_env "Server"
setup_node_env "Client"
