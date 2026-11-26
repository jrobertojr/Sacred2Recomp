# Sacred2Recomp - ReXGlue Recompiled Project
#
# The SDK package provides rexglue_project and the rexglue_setup_* functions.

cmake_minimum_required(VERSION 3.25)
project(Sacred2Recomp LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if (REXSDK_DIR)
  add_subdirectory("${REXSDK_DIR}" rexglue-sdk)
else ()
  find_package(rexglue 0.11.0 REQUIRED CONFIG)
endif ()
rexglue_project(Sacred2Recomp OUTPUT_DIR generated/default)

# Host sources. src/main.cpp stays on the executable.
set(SACRED2RECOMP_HOST_SOURCES src/Sacred2Recomp_hooks.cpp)

# OBJECT and never STATIC, an archive drops the REX_EXPORT symbols.
add_library(Sacred2Recomp_common OBJECT ${SACRED2RECOMP_HOST_SOURCES})
rexglue_setup_host_library(Sacred2Recomp_common)

if (WIN32)
  add_executable(Sacred2Recomp WIN32 src/main.cpp)
else ()
  add_executable(Sacred2Recomp src/main.cpp)
endif ()

rexglue_setup_target(Sacred2Recomp)
target_link_libraries(Sacred2Recomp PRIVATE Sacred2Recomp_common)
