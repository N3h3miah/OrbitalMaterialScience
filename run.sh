#!/bin/bash
SRC_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
KSP_DIR="${SRC_DIR}/../Kerbal Space Program"
DST_DIR="${KSP_DIR}/GameData/NehemiahInc"

rm -Rf "${DST_DIR}"
mkdir -p "${DST_DIR}/Plugins"
	
cp -ax "${SRC_DIR}/GameData/NehemiahInc/." "${DST_DIR}"
cp -ax "${SRC_DIR}/Plugin/NE Science/bin/Debug/NE_Science.dll" "${DST_DIR}/Plugins/"

if [ "$(uname)" == "Darwin" ]; then
    # Do something under Mac OS X platform
	echo "TODO: Add command to start KSP under OS X"
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
    # Do something under Linux platform
	"${KSP_DIR}/KSP.x86_64"
elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW32_NT" ]; then
    # Do something under Windows NT platform
	"${KSP_DIR}/KSP.exe"
fi

