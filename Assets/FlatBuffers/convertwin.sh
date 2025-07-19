#!/bin/sh

SCRIPTDIR=`cd "$(dirname "$0")" && pwd`

cd $SCRIPTDIR

./flatc.exe --csharp --gen-mutable --cs-gen-json-serializer --gen-object-api $SCRIPTDIR/UserData.fbs

echo 'finish'