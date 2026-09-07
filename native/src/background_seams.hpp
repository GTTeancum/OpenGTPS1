#pragma once
#include "opengt/world_draw_list.hpp"

namespace opengt::render {
// Returns the number of source-proven half-grid T-junctions conformed.
std::uint32_t repair_background_midpoint_seams(WorldDrawList& list);
}
