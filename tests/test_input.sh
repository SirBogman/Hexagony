#!/bin/sh

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd "${DIR}/.."

A=$(dotnet run -- tests/in.hxg -900 a)
if [ "$A" != "-9000a" ]; then
	echo "Not working"
	exit 1
fi

A=$(dotnet run -- - -900 a < tests/in.hxg)
if [ "$A" != "-9000a" ]; then
	echo "Not working"
	exit 1
fi

A=$(dotnet run -- tests/in.hxg < tests/in.bin)
if [ "$A" != "-9000a" ]; then
	echo "Not working"
	exit 1
fi

echo "Working"

echo 😃