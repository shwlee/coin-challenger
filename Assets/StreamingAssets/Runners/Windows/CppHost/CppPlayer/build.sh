#!/bin/bash

# 인수로 전달된 외부 변수 설정
SCRIPT_DIR=$(dirname "$0")
OUTPUT_NAME="$1"

# "result" 디렉터리로 이동
cd "$SCRIPT_DIR/result" || { echo "경로로 이동할 수 없습니다: ${PROJECT_PATH}/result"; exit 1; }

rm "$SCRIPT_DIR/result/CMakeFiles/CppBuilder.dir/src/CppPlayer.cpp.o"
rm "$SCRIPT_DIR/result/CMakeFiles/CppBuilder.dir/src/CppPlayer.cpp.o.d"
rm "$SCRIPT_DIR/result/libCppBuilder.dylib"

# CMake 명령 실행
cmake ..
cmake --build . --config Release

# 빌드된 DLL 파일 이름 변경
cp "$SCRIPT_DIR/result/libCppBuilder.dylib" "$SCRIPT_DIR/result/Release/${OUTPUT_NAME}.dylib"
