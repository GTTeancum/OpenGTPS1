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
    src/background_seams.cpp
    src/vehicle_shadow_source.cpp
    src/shadow_grid.cpp
    src/lighting.cpp
    src/world_gpu_renderer_native.cpp
)

set_target_properties(opengt_renderer_core PROPERTIES POSITION_INDEPENDENT_CODE ON)
if(CMAKE_SYSTEM_NAME STREQUAL "Linux")
    # One shader source, embedded offline without a driver SDK or a Python build dependency.
    set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/shaders/world.glsl")
    file(READ "${CMAKE_CURRENT_SOURCE_DIR}/shaders/world.glsl" OPENGT_GLSL)
    string(FIND "${OPENGT_GLSL}" ")OGTGL\"" OPENGT_GLSL_DELIMITER)
    if(NOT OPENGT_GLSL_DELIMITER EQUAL -1)
        message(FATAL_ERROR "GLSL contains reserved C++ raw-string delimiter")
    endif()
    file(MAKE_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/generated")
    file(WRITE "${CMAKE_CURRENT_BINARY_DIR}/generated/opengt_world_glsl.inc" "R\"OGTGL(${OPENGT_GLSL})OGTGL\"\n")
    target_include_directories(opengt_renderer_core PRIVATE "${CMAKE_CURRENT_BINARY_DIR}/generated")
    target_sources(opengt_renderer_core PRIVATE src/gl/api.cpp src/world_gpu_renderer_egl.cpp)
    target_link_libraries(opengt_renderer_core PUBLIC ${CMAKE_DL_LIBS})
endif()

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
    if(CMAKE_SYSTEM_NAME STREQUAL "Linux")
        add_executable(opengt_shadow_grid_tests tests/shadow_grid_tests.cpp)
        target_link_libraries(opengt_shadow_grid_tests PRIVATE opengt_renderer_core)
        add_test(NAME opengt_shadow_grid_tests COMMAND opengt_shadow_grid_tests)
    endif()
    add_executable(opengt_shadow_anchor_capture_tests tests/shadow_anchor_capture_tests.cpp)
    target_link_libraries(opengt_shadow_anchor_capture_tests PRIVATE opengt_renderer_core)
    add_test(NAME opengt_shadow_anchor_capture_tests COMMAND opengt_shadow_anchor_capture_tests)

    add_executable(opengt_lighting_tests tests/lighting_tests.cpp)
    target_link_libraries(opengt_lighting_tests PRIVATE opengt_renderer_core)
    add_test(NAME opengt_lighting_tests COMMAND opengt_lighting_tests)

    # Exercise the bridge's private resident classifier without exporting
    # diagnostic APIs from the live DLL.
    add_executable(opengt_resident_overlay_tests tests/resident_overlay_tests.cpp)
    target_link_libraries(opengt_resident_overlay_tests PRIVATE opengt_renderer_core)
    target_compile_features(opengt_resident_overlay_tests PRIVATE cxx_std_17)
    add_test(NAME opengt_resident_overlay_tests COMMAND opengt_resident_overlay_tests)

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
        add_executable(opengt_lighting_gpu_tests tests/lighting_gpu_tests.cpp)
        target_link_libraries(opengt_lighting_gpu_tests PRIVATE opengt_renderer_core)
        add_test(NAME opengt_lighting_gpu_tests COMMAND opengt_lighting_gpu_tests)
        set_tests_properties(opengt_lighting_gpu_tests PROPERTIES TIMEOUT 180)
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
    add_executable(opengt_lighting_inspect tools/lighting_inspect.cpp)
    target_link_libraries(opengt_lighting_inspect PRIVATE opengt_renderer_core)
    add_executable(opengt_solar_fit tools/solar_fit.cpp)
    target_compile_features(opengt_solar_fit PRIVATE cxx_std_17)
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

if(OPENGT_BUILD_TESTS AND CMAKE_SYSTEM_NAME STREQUAL "Linux")
    add_executable(opengt_lighting_egl_tests tests/lighting_egl_tests.cpp)
    target_link_libraries(opengt_lighting_egl_tests PRIVATE opengt_renderer_core)
    add_test(NAME opengt_lighting_egl_tests COMMAND opengt_lighting_egl_tests)
    set_tests_properties(opengt_lighting_egl_tests PROPERTIES TIMEOUT 180)
endif()

if(OPENGT_BUILD_TESTS AND CMAKE_SYSTEM_NAME STREQUAL "Linux")
    add_executable(opengt_lighting_live_tests tests/lighting_live_tests.cpp)
    target_link_libraries(opengt_lighting_live_tests PRIVATE opengt_live_renderer)
    target_compile_features(opengt_lighting_live_tests PRIVATE cxx_std_17)
    add_test(NAME opengt_lighting_live_tests COMMAND opengt_lighting_live_tests)
    set_tests_properties(opengt_lighting_live_tests PROPERTIES TIMEOUT 180)
endif()

if(OPENGT_BUILD_TESTS AND CMAKE_SYSTEM_NAME STREQUAL "Linux")
    add_executable(opengt_shadow_visibility_tests tests/shadow_visibility_tests.cpp)
    target_link_libraries(opengt_shadow_visibility_tests PRIVATE opengt_renderer_core)
    add_test(NAME opengt_shadow_visibility_tests COMMAND opengt_shadow_visibility_tests)
    set_tests_properties(opengt_shadow_visibility_tests PROPERTIES TIMEOUT 180)
endif()

if(OPENGT_BUILD_TESTS)
  add_executable(opengt_vehicle_shadow_source_tests tests/vehicle_shadow_source_tests.cpp)
  target_link_libraries(opengt_vehicle_shadow_source_tests PRIVATE opengt_renderer_core)
  add_test(NAME opengt_vehicle_shadow_source_tests COMMAND opengt_vehicle_shadow_source_tests)
endif()
if(OPENGT_BUILD_TESTS AND CMAKE_SYSTEM_NAME STREQUAL "Linux")
  add_executable(opengt_depth_precision_tests tests/depth_precision_tests.cpp)
  target_link_libraries(opengt_depth_precision_tests PRIVATE opengt_renderer_core)
  add_test(NAME opengt_depth_precision_tests COMMAND opengt_depth_precision_tests)
  add_test(NAME opengt_depth_precision_fallback_tests COMMAND opengt_depth_precision_tests)
  set_tests_properties(opengt_depth_precision_fallback_tests PROPERTIES ENVIRONMENT "OPENGT_GL_DEPTH_FALLBACK=1")
endif()

if(OPENGT_BUILD_TESTS AND CMAKE_SYSTEM_NAME STREQUAL "Linux")
  add_executable(opengt_shadow_projection_tests tests/shadow_projection_tests.cpp)
  target_link_libraries(opengt_shadow_projection_tests PRIVATE opengt_renderer_core)
  add_test(NAME opengt_shadow_projection_tests COMMAND opengt_shadow_projection_tests)
endif()
if(OPENGT_BUILD_TESTS AND CMAKE_SYSTEM_NAME STREQUAL "Linux")
  add_executable(opengt_track_shadow_material_tests tests/track_shadow_material_tests.cpp)
  target_link_libraries(opengt_track_shadow_material_tests PRIVATE opengt_renderer_core)
  add_test(NAME opengt_track_shadow_material_tests COMMAND opengt_track_shadow_material_tests)
endif()
