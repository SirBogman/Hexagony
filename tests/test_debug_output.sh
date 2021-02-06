#!/bin/sh

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd "${DIR}/.."

dotnet run -- -d tests/test_debug_output.hxg > tests/debug_output.txt 2>&1
dotnet run -- -D tests/test_debug_output.hxg > tests/debug_output_full.txt 2>&1
