cmake_minimum_required(VERSION 3.21)

project(OpenGTRendererCore VERSION 0.1.0 LANGUAGES CXX)

option(OPENGT_BUILD_TESTS "Build native renderer core tests" ON)

add_library(opengt_renderer_core STATIC
    src/geometry_stitcher.cpp
    src/projected_capture.cpp
    src/projected_reference_renderer.cpp
    src/world_capture.cpp
)

target_include_directories(opengt_renderer_core
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_compile_features(opengt_renderer_core PUBLIC cxx_std_17)

if(MSVC)
    target_compile_options(opengt_renderer_core PRIVATE /W4 /permissive-)
    target_compile_definitions(
        opengt_renderer_core
        PRIVATE
            _CRT_SECURE_NO_WARNINGS
    )
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

    add_executable(opengt_projected_capture_tests
        tests/projected_capture_tests.cpp
    )
    target_link_libraries(
        opengt_projected_capture_tests
        PRIVATE
            opengt_renderer_core
    )
    target_compile_features(opengt_projected_capture_tests PRIVATE cxx_std_17)

    add_test(
        NAME opengt_projected_capture_tests
        COMMAND opengt_projected_capture_tests
    )

    add_executable(opengt_world_capture_tests
        tests/world_capture_tests.cpp
    )
    target_link_libraries(
        opengt_world_capture_tests
        PRIVATE
            opengt_renderer_core
    )
    target_compile_features(opengt_world_capture_tests PRIVATE cxx_std_17)

    add_test(
        NAME opengt_world_capture_tests
        COMMAND opengt_world_capture_tests
    )
endif()

add_executable(opengt_capture_render
    tools/capture_render.cpp
)
target_link_libraries(opengt_capture_render PRIVATE opengt_renderer_core)
target_compile_features(opengt_capture_render PRIVATE cxx_std_17)

add_executable(opengt_world_capture_inspect
    tools/world_capture_inspect.cpp
)
target_link_libraries(opengt_world_capture_inspect PRIVATE opengt_renderer_core)
target_compile_features(opengt_world_capture_inspect PRIVATE cxx_std_17)
