cmake_minimum_required(VERSION 3.21)

project(OpenGTRendererCore VERSION 0.1.0 LANGUAGES CXX)

option(OPENGT_BUILD_TESTS "Build native renderer core tests" ON)

add_library(opengt_renderer_core STATIC
    src/geometry_stitcher.cpp
)

target_include_directories(opengt_renderer_core
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_compile_features(opengt_renderer_core PUBLIC cxx_std_17)

if(MSVC)
    target_compile_options(opengt_renderer_core PRIVATE /W4 /permissive-)
else()
    target_compile_options(opengt_renderer_core PRIVATE -Wall -Wextra -Wpedantic)
endif()

if(OPENGT_BUILD_TESTS)
    enable_testing()

    add_executable(opengt_renderer_core_tests
        tests/geometry_stitcher_tests.cpp
    )
    target_link_libraries(opengt_renderer_core_tests PRIVATE opengt_renderer_core)
    target_compile_features(opengt_renderer_core_tests PRIVATE cxx_std_17)

    add_test(
        NAME opengt_renderer_core_tests
        COMMAND opengt_renderer_core_tests
    )
endif()
