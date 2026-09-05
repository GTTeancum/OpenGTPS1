cmake_minimum_required(VERSION 3.21)

project(OpenGTRendererCore VERSION 0.1.0 LANGUAGES CXX)

option(OPENGT_BUILD_TESTS "Build native renderer core tests" ON)
option(OPENGT_BUILD_DEV_TOOLS "Build renderer diagnostic/oracle tools" ON)

add_library(opengt_renderer_core STATIC
    src/geometry_stitcher.cpp
    src/world_capture.cpp
    src/world_draw_list.cpp
    src/world_gpu_renderer_d3d11.cpp
    src/world_topology.cpp
    src/vehicle_paint.cpp
)

# Synthetic midpoint reconstruction remains available only to explicitly
# requested development tests/tools. A shipping build has no interpolation
# object code and the live DLL exposes no pair-rendering ABI.
if(OPENGT_BUILD_TESTS OR OPENGT_BUILD_DEV_TOOLS)
    target_sources(opengt_renderer_core PRIVATE src/world_interpolation.cpp)
    set(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT ON)
else()
    set(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT OFF)
endif()

add_library(opengt_live_renderer SHARED
    src/live_renderer_bridge.cpp
)
target_link_libraries(opengt_live_renderer PRIVATE opengt_renderer_core)
target_include_directories(opengt_live_renderer
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)
target_compile_features(opengt_live_renderer PRIVATE cxx_std_17)
if(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
    target_compile_definitions(
        opengt_live_renderer
        PUBLIC OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT=1)
endif()

target_include_directories(opengt_renderer_core
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_compile_features(opengt_renderer_core PUBLIC cxx_std_17)

# Compatibility/reference rasterization and image serialization are
# development evidence tools. Keep them out of the live renderer dependency
# graph so a release DLL cannot expose an oracle or capture renderer.
if(OPENGT_BUILD_TESTS OR OPENGT_BUILD_DEV_TOOLS)
    add_library(opengt_renderer_dev_support STATIC
        src/image_writer.cpp
        src/projected_capture.cpp
        src/projected_reference_renderer.cpp
    )
    target_link_libraries(
        opengt_renderer_dev_support PUBLIC opengt_renderer_core)
    target_include_directories(opengt_renderer_dev_support
        PUBLIC
            ${CMAKE_CURRENT_SOURCE_DIR}/include
    )
    target_compile_features(opengt_renderer_dev_support PUBLIC cxx_std_17)
endif()

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

if(WIN32)
    target_link_libraries(
        opengt_renderer_core
        PUBLIC
            d3d11
            d3dcompiler
            dxgi
    )
endif()

if(OPENGT_BUILD_TESTS)
    enable_testing()

    add_executable(opengt_vehicle_paint_tests tests/vehicle_paint_tests.cpp)
    target_link_libraries(opengt_vehicle_paint_tests PRIVATE opengt_renderer_core)
    add_test(NAME opengt_vehicle_paint_tests COMMAND opengt_vehicle_paint_tests)

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
            opengt_renderer_dev_support
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

    add_executable(opengt_world_draw_list_tests
        tests/world_draw_list_tests.cpp
    )
    target_link_libraries(
        opengt_world_draw_list_tests
        PRIVATE
            opengt_renderer_core
    )
    target_compile_features(opengt_world_draw_list_tests PRIVATE cxx_std_17)

    add_test(
        NAME opengt_world_draw_list_tests
        COMMAND opengt_world_draw_list_tests
    )

    add_executable(opengt_world_topology_tests
        tests/world_topology_tests.cpp
    )
    target_link_libraries(
        opengt_world_topology_tests
        PRIVATE
            opengt_renderer_core
    )
    target_compile_features(opengt_world_topology_tests PRIVATE cxx_std_17)

    add_test(
        NAME opengt_world_topology_tests
        COMMAND opengt_world_topology_tests
    )

    add_executable(opengt_world_interpolation_tests
        tests/world_interpolation_tests.cpp
    )
    target_link_libraries(
        opengt_world_interpolation_tests
        PRIVATE
            opengt_renderer_core
    )
    target_compile_features(
        opengt_world_interpolation_tests PRIVATE cxx_std_17)

    add_test(
        NAME opengt_world_interpolation_tests
        COMMAND opengt_world_interpolation_tests
    )

    if(WIN32)
        add_executable(opengt_world_gpu_renderer_tests
            tests/world_gpu_renderer_tests.cpp
        )
        target_link_libraries(
            opengt_world_gpu_renderer_tests
            PRIVATE
                opengt_renderer_core
        )
        target_compile_features(
            opengt_world_gpu_renderer_tests PRIVATE cxx_std_17)

        add_test(
            NAME opengt_world_gpu_renderer_tests
            COMMAND opengt_world_gpu_renderer_tests
        )
    endif()
endif()

if(OPENGT_BUILD_DEV_TOOLS)
    add_executable(opengt_capture_render
        tools/capture_render.cpp
    )
    target_link_libraries(
        opengt_capture_render PRIVATE opengt_renderer_dev_support)
    target_compile_features(opengt_capture_render PRIVATE cxx_std_17)

    add_executable(opengt_world_capture_inspect
        tools/world_capture_inspect.cpp
    )
    target_link_libraries(
        opengt_world_capture_inspect PRIVATE opengt_renderer_core)
    target_compile_features(opengt_world_capture_inspect PRIVATE cxx_std_17)

    add_executable(opengt_world_viewer
        tools/world_viewer.cpp
    )
    target_link_libraries(
        opengt_world_viewer PRIVATE opengt_renderer_dev_support)
    target_compile_features(opengt_world_viewer PRIVATE cxx_std_17)

    add_executable(opengt_live_benchmark
        tools/live_renderer_benchmark.cpp
    )
    target_link_libraries(opengt_live_benchmark PRIVATE opengt_live_renderer)
    target_compile_features(opengt_live_benchmark PRIVATE cxx_std_17)

    add_executable(opengt_world_interpolation_inspect
        tools/world_interpolation_inspect.cpp
    )
    target_link_libraries(
        opengt_world_interpolation_inspect PRIVATE opengt_renderer_dev_support)
    target_compile_features(
        opengt_world_interpolation_inspect PRIVATE cxx_std_17)

    add_executable(opengt_live_pair_inspect
        tools/live_pair_inspect.cpp
    )
    target_link_libraries(
        opengt_live_pair_inspect
        PRIVATE
            opengt_live_renderer
            opengt_renderer_dev_support
    )
    target_compile_features(opengt_live_pair_inspect PRIVATE cxx_std_17)
endif()
