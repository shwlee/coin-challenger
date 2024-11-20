cmake_minimum_required(VERSION 3.10)
project(CppBuilder VERSION 1.0 LANGUAGES CXX)

add_definitions(
-DNDEBUG    
-DCPPPLAYER_EXPORTS
-D_USRDLL
)

set(CMAKE_CXX_STANDARD 17)  # C++17 사용
set(CMAKE_CXX_STANDARD_REQUIRED ON)  # 필수 표준 설정
set(CMAKE_CXX_EXTENSIONS OFF)  # 확장 사용하지 않음 (예: GNU 확장)

set(SOURCES
    src/CppPlayer.cpp  
)

set(HEADERS
    src/CppPlayer.h  
)

add_library(CppBuilder SHARED ${SOURCES} ${HEADERS})
target_compile_definitions(CppBuilder PRIVATE CPPBUILDER_EXPORTS)

install(TARGETS CppBuilder
        EXPORT CppBuilderExports
        LIBRARY DESTINATION lib
        ARCHIVE DESTINATION lib)

install(DIRECTORY include/ DESTINATION include)

