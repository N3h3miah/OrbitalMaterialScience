#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cp -ax "${DIR}/GameData/NehemiaInc" "${DIR}/../Kerbal Space Program/GameData/"
"${DIR}/../Kerbal Space Program/KSP.x86_64"
