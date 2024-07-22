#!/bin/bash
# Not being used at this time. Keeping it here as reference
echo "executing from entrypoint.sh"

dotnet HttpServerSim.App.dll

# To use with an interactive session with command 'docker run -i -t http-server-sim-build'
# /bin/bash "$@" 
