#!/bin/sh

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd "${DIR}/.."

A=$(dotnet run -- tests/in.hxg -900 a)
if [ "$A" != "-9000a" ]; then
	echo "Not working 1"
	exit 1
fi

A=$(dotnet run -- - -900 a < tests/in.hxg)
if [ "$A" != "-9000a" ]; then
	echo "Not working 2"
	exit 2
fi

A=$(dotnet run -- tests/in.hxg < tests/in.bin)
if [ "$A" != "-9000a" ]; then
	echo "Not working 3"
	exit 3
fi

A=$(dotnet run -- tests/echo.hxg < tests/emoji.txt)
if [ "$A" != "😃" ]; then
	echo "Not working 4"
	exit 4
fi

A=$(dotnet run -- tests/echo.hxg 😃)
if [ "$A" != "😃" ]; then
	echo "Not working 5"
	exit 5
fi

echo "Working"

echo 😃
