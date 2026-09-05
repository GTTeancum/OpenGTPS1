#!/usr/bin/env python3
"""Convert supported US Gran Turismo content into a GT2 patch volume."""

from __future__ import annotations

import argparse
import bisect
import gzip
import hashlib
import json
import shutil
import struct
import zlib
from dataclasses import dataclass
from pathlib import Path

from gt2_vol import members_from_directory, read_entries, write_volume
from psx_iso import extract_image


REPO = Path(__file__).resolve().parents[1]
GT1_IMAGE_NAME = "Gran Turismo [U] [SCUS-94194].img"
GT1_IMAGE_SIZE = 693_668_304
GT1_IMAGE_SHA256 = "765a748c4f2975a063a47ba9e42708a4882954d765f9e352c5af3c0950eaefb6"
REQUIRED_DISC_FILES = {
    "SYSTEM.CNF": 68,
    "SCUS_941.94": 141_312,
    "ARCADE.DAT": 241_272,
    "BG.DAT": 204_800,
    "CAR.DAT": 16_379_904,
    "CARINF.DAT": 135_301,
    "COURSE.DAT": 23_969_792,
    "MENU_IMG.ARC": 121_235_456,
    "MENU_RAW.ARC": 294_076,
    "SYSTEM.DAT": 14_768,
}

GT1_ARCADE_SSR11_ENTRY = 81
GT1_SSR11_SKY_INDEX = 5
GT1_SSR11_SKY_NAME = "dawn3"
GT2_SSR11_SKY_STEM = "gt1_ssr11_sky"
GT2_GTD_PART_RECORD_SIZES = (
    0x0C,  # Brake
    0x10,  # BrakeController
    0x18,  # Steer
    0x14,  # Chassis
    0x0C,  # Lightweight
    0x1C,  # RacingModify
    0x4C,  # Engine
    0x0C,  # PortPolish
    0x0C,  # EngineBalance
    0x0C,  # Displacement
    0x0C,  # Computer
    0x0C,  # NATune
    0x14,  # TurbineKit
    0x10,  # Drivetrain
    0x0C,  # Flywheel
    0x10,  # Clutch
    0x0C,  # PropellerShaft
    0x24,  # Gear
    0x4C,  # Suspension
    0x0C,  # Intercooler
    0x0C,  # Muffler
    0x20,  # LSD
    0x10,  # TiresFront
    0x0C,  # TiresRear
)
# GT2's Car and CarArcade structures group LSD before Gear/Suspension even
# though the GTDT block directory stores LSD after Muffler. Values here are
# GTDT block indexes in their serialized car-record field order.
GT2_GTD_CAR_REF_BLOCKS = (
    *range(17),
    21,  # LSD
    17,  # Gear
    18,  # Suspension
    19,  # Intercooler
    20,  # Muffler
    22,  # TiresFront
    23,  # TiresRear
)
GT2_GTD_BLOCK_TO_CAR_REF = {
    block_index: reference_index
    for reference_index, block_index in enumerate(GT2_GTD_CAR_REF_BLOCKS)
}
GT2_GTMODE_CAR_BLOCK = 30
GT2_ARCADE_RACING_BLOCK = 32
GT2_ARCADE_DRIFT_BLOCK = 33
GT2_GTMODE_BLOCK_COUNT = 31
GT2_ARCADE_BLOCK_COUNT = 34
GT2_ARCADE_STRING_INDEX_POSITION = 0x208
# Stock Arcade expands this database at 0x800F84C0, immediately before live
# frontend state at 0x801034C0. The unified PC runtime already exposes the
# original guest to an 8 MiB devkit RAM map, so reserve a dedicated 1 MiB
# native guest arena above the retail 2 MiB address space. The overlay patch
# and recompiled guest enhancement both relocate the loader destination.
GT2_ARCADE_DATABASE_ADDRESS = 0x80200000
GT2_ARCADE_DATABASE_SAFE_SIZE = 0x100000
# Both original GT2 frontends advance each native CDO/CNO destination by
# 0x6000 bytes. Shipped models top out at 20,000 bytes; imported structural
# conversions may use the remainder but must never cross the actual slot.
GT2_CAR_MODEL_SAFE_SIZE = 0x6000
# Keep the native car-selection archive and its four adjacent frontend
# descriptors together inside a separate devkit-RAM MiB. The original
# 0x66000-byte loader allocation is now too small for the thirteenth Class B
# car: h-rxn grows arc_carlogo to 0x6630C bytes.
GT2_ARCADE_FRONTEND_ANCHOR = 0x80410000
GT2_ARCADE_FRONTEND_ARCHIVE_ADDRESS = (
    GT2_ARCADE_FRONTEND_ANCHOR - 0x6AE0
)
GT2_ARCADE_FRONTEND_DESCRIPTOR_ADDRESS = (
    GT2_ARCADE_FRONTEND_ANCHOR - 0x6B20
)
GT2_ARCADE_FRONTEND_ARCHIVE_SAFE_SIZE = 0xF0000
# Classes A-C have now been exercised with thirteen entries through complete
# races. Later imports must not silently exceed that proven native frontend
# capacity without an explicit array and call-site audit.
GT2_ARCADE_PROVEN_CLASS_CAPACITY = 13
GT1_FIRST_ARCADE_CAR = {
    "stem": "a-ian",
    "displayName": "EUNOS ROADSTER",
    "modelBasisStem": "aminn",
    # GT1's Arcade-only Roadster is an authored composite. Its complete SPEC
    # fingerprint matches the GT2-native V-Special conversion except for the
    # exact brake and wheel/tire fields below. Those fields have byte-exact
    # counterparts in GT2's RX-7 Type-R and Roadster S-Special conversions.
    "physicsBasisStem": "amivn",
    "physicsPartBasis": {
        0: "afo7n",
        22: "amisn",
        23: "amisn",
    },
    "arcadeLogoEntry": 2,
    "arcadeClass": 3,
    # Native GT2 descriptor 15 is the shared Mazda manufacturer mark used by
    # every Arcade roster stem in GT2's `a` family. The per-car EUNOS
    # ROADSTER wordmark remains the imported GT1 TIM below; this selects only
    # the separate maker badge.
    "manufacturerLogoIndex": 15,
    "ratings": (5, 10, 7),
    "stats": (130, 6500, 157, 4500, 990),
    "paintSources": (
        (104, "aminn"),
        (105, "a2rcn"),
        (114, "aminn"),
    ),
}
GT1_ROADSTER_ARCADE_CAR = {
    "stem": "amian",
    "displayName": "EUNOS ROADSTER ARCADE",
    "modelBasisStem": "amisn",
    "physicsBasisStem": "amisn",
    "physicsPartBasis": {},
    "arcadeLogoEntry": 2,
    "arcadeClass": 3,
    "manufacturerLogoIndex": 15,
    "ratings": (5, 10, 7),
    "stats": (130, 6500, 157, 4500, 990),
    "paintSources": (
        (49, "aminn"),
        (50, "amivn"),
        (52, "aminn"),
        (53, "amivn"),
        (54, "aminn"),
        (98, "aminn"),
        (101, "amisn"),
        (108, "amivn"),
        (109, "aminn"),
        (110, "ademn"),
        (112, "aminn"),
        (113, "amisn"),
        (115, "amisn"),
        (116, "aminn"),
    ),
}
GT1_ROADSTER_RS_CAR = {
    "stem": "a-odn",
    "displayName": "EUNOS ROADSTER RS",
    # The Arcade RS uses GT1's ROADSTER RS body, including its slightly
    # asymmetric authored wheel placement. GT2's native `arodn` supplies
    # only the corresponding GT2 header conventions; the three LODs and
    # shadow are converted from the GT1 model below.
    "modelBasisStem": "arodn",
    "convertModel": True,
    # GT1 names the graphics-only Arcade variant `a-odn`, while its complete
    # production specification is the `arodn` EUNOS ROADSTER RS record.
    "physicsSpecStem": "arodn",
    "physicsBasisStem": "arodn",
    "physicsPartBasis": {},
    "arcadeLogoEntry": 3,
    "arcadeClass": 3,
    "manufacturerLogoIndex": 15,
    "ratings": (5, 10, 7),
    "stats": (145, 6500, 163, 5000, 1030),
    "paintSources": tuple(
        (color_id, "arodn")
        for color_id in (49, 52, 54, 98, 103, 116)
    ),
}
GT1_CIVIC_RACER_CAR = {
    "stem": "h-vrn",
    "displayName": "CIVIC (Racer)",
    "modelBasisStem": "hcvrn",
    "convertModel": True,
    "physicsBasisStem": "hcvrn",
    "physicsPartBasis": {},
    # The physical payload is byte-identical to GT1's production Civic Racer
    # conversion. The five differences are identity, price, and string-table
    # references outside the serialized GT2 part records.
    "physicsExpectedDifferences": (0x01, 0x184, 0x185, 0x190, 0x192),
    "arcadeLogoEntry": 4,
    "arcadeClass": 2,
    "manufacturerLogoIndex": 10,
    "ratings": (6, 10, 9),
    "stats": (185, 8200, 160, 7500, 1050),
    "paintSources": (
        (104, "hnsbn"),
        (111, "hcfnn"),
        (118, "hinsn"),
    ),
}
GT1_DB7_COUPE_CAR = {
    "stem": "l-7cn",
    "displayName": "DB7 COUPE",
    # GT1's Arcade-only DB7 Coupe and its production DB7 share the exact
    # authored body model. GT2's native DB7 container therefore preserves
    # that geometry without a donor-body conversion; the exclusive GT1
    # paint package and selection artwork are imported below.
    "modelBasisStem": "ld7cn",
    "physicsBasisStem": "ld7cn",
    "physicsPartBasis": {},
    # The serialized physical parts match. These bytes are identity, price,
    # string references, and GT1's displayed torque statistic.
    "physicsExpectedDifferences": (
        0x01,
        0x184,
        0x185,
        0x186,
        0x190,
        0x192,
        0x19C,
        0x19D,
    ),
    "arcadeLogoEntry": 24,
    "arcadeClass": 1,
    "manufacturerLogoIndex": 1,
    "ratings": (10, 8, 8),
    "stats": (340, 6000, 361, 3000, 1725),
    "paintSources": (
        (49, "ld6cn"),
        (101, "ld7cn"),
        (117, "hnann"),
    ),
}
GT1_CRX_91_SI_CAR = {
    "stem": "h-rxn",
    "displayName": "CIVIC CR-X '91 Si",
    "modelBasisStem": "hcrxn",
    # The GT1 Arcade and production geometry is byte-identical, but GT2's
    # production compiler uses a later texture/UV layout. Preserve the GT1
    # packet UVs through the structural converter so the exclusive palette
    # package maps to the authored body.
    "convertModel": True,
    "physicsBasisStem": "hcrxn",
    "physicsPartBasis": {},
    # GT1's Arcade CR-X shares the production car's complete serialized
    # physical specification except for its deliberate 970 kg chassis.
    "physicsExpectedDifferences": (
        0x01,
        0x5A,
        0x184,
        0x185,
        0x186,
        0x190,
        0x192,
        0x198,
        0x19C,
        0x19D,
        0x1A0,
    ),
    # GT2 Chassis records store vehicle weight as a u16 at byte 0x0E.
    # Author a target-owned record rather than retaining the 986 kg source.
    "physicsU16Overrides": {3: {0x0E: 970}},
    "menuLogoName": "h-rx.tim",
    "arcadeClass": 2,
    "manufacturerLogoIndex": 10,
    "ratings": (6, 10, 9),
    "stats": (160, 7600, 152, 7000, 970),
    "paintSources": (
        (54, "hcrxn"),
        (104, "h2a0n"),
        (113, "h2csn"),
    ),
}
GT1_LIVERY_FOLD_OVERRIDES = {
    "v-rbr": {
        "bodyStem": "v1rbr",
        # Same-manufacturer GT2 Cerbera records provide native swatch and
        # localized name metadata for these exact embedded GT1 IDs.
        "paintSources": (
            (99, "vce5n"),
            (109, "vce5n"),
        ),
        "description": (
            "GT1 Cerbera LM grey/red and grey/green authored body/liveries"
        ),
    },
}

# GT1 also contains authored body packages whose short archive stem differs
# from the corresponding retail GT2 identity.  These are not additional cars:
# discovery verifies the source texture IDs directly, appends those choices to
# the existing target identity, and keeps the alternate body hidden.
GT1_CROSS_STEM_LIVERY_FOLDS = (
    {
        "sourceStem": "t-plr",
        "targetStem": "tsplr",
        "bodyStem": "z0tpl",
        "modelBasisStem": "tsplr",
        "description": (
            "GT1 t-plr alternate CASTROL SUPRA GT body/livery package"
        ),
    },
)

# The complete model-referenced-texel census of the supplied US GT1 and GT2
# archives proves these GT1 Racing Modification bodies contain paints with no
# visible GT2 counterpart. Their customer identity is the archive-derived
# stock stem obtained by replacing the terminal `r` with `n`; only the body
# and its two authored paints are additional. FTO's corresponding packages
# deliberately are not listed: all ten RM paints have the same IDs and exact
# RGBA values on every texel referenced by either native high-LOD model.
GT1_PROVEN_DISTINCT_RACING_MODIFICATION_STEMS = (
    "dvprr",
    "hpnvr",
    "mgnor",
    "mgntr",
    "mgoor",
    "mgotr",
    "mgtmr",
    "mgtor",
    "mgttr",
    "mlnnr",
    "mlnor",
    "mmgor",
    "mmgrr",
    "nn32r",
    "nplor",
    "nr02r",
    "nr32r",
    "ns13r",
    "nv12r",
    "nv22r",
    "nzx2r",
    "nzx3r",
    "nzxsr",
    "nzxvr",
    "tcelr",
    "tmrlr",
    "tsonr",
    "tsorr",
    "tspnr",
    "tsprr",
    "vcrbr",
)
GT1_ARCADE_LIVERY_SMOKE_CARS = {
    "tsplr": {
        "displayName": "CASTROL SUPRA GT",
        "menuLogoName": "tspl.tim",
        "physicsBasisStem": "tsplr",
        "physicsPartBasis": {},
        # The explicit smoke overlay replaces the last accessible Class A
        # slot instead of growing its already-proven thirteen-entry roster.
        "arcadeClass": 1,
        "manufacturerLogoIndex": 31,
        "ratings": (10, 10, 10),
    },
    "v-rbr": {
        "displayName": "Cerbera LM Edition",
        "menuLogoName": "v-rb.tim",
        "physicsBasisStem": "v-rbr",
        "physicsPartBasis": {},
        # Keep the alternate-body proof in the same already-proven Class A
        # replacement slot used by the Castrol smoke. The production Arcade
        # roster remains unchanged.
        "arcadeClass": 1,
        "manufacturerLogoIndex": 32,
        "ratings": (10, 10, 10),
    },
    "mgnon": {
        "displayName": "GTO SR",
        "menuLogoName": "mgno.tim",
        "physicsBasisStem": "mgnon",
        "physicsPartBasis": {},
        "arcadeClass": 1,
        "manufacturerLogoIndex": 19,
        "ratings": (9, 8, 7),
    },
    "mgntn": {
        "displayName": "GTO TWIN TURBO",
        "menuLogoName": "mgnt.tim",
        "physicsBasisStem": "mgntn",
        "physicsPartBasis": {},
        "arcadeClass": 1,
        "manufacturerLogoIndex": 19,
        "ratings": (9, 8, 7),
    },
    "mgoon": {
        "displayName": "GTO '92",
        "menuLogoName": "mgoo.tim",
        "physicsBasisStem": "mgoon",
        "physicsPartBasis": {},
        "arcadeClass": 1,
        "manufacturerLogoIndex": 19,
        "ratings": (9, 8, 7),
    },
    "mgotn": {
        "displayName": "GTO '92 TWIN TURBO",
        "menuLogoName": "mgot.tim",
        "physicsBasisStem": "mgotn",
        "physicsPartBasis": {},
        "arcadeClass": 1,
        "manufacturerLogoIndex": 19,
        "ratings": (9, 8, 7),
    },
    "mgtmn": {
        "displayName": "GTO '95 MR",
        "menuLogoName": "mgtm.tim",
        "physicsBasisStem": "mgtmn",
        "physicsPartBasis": {},
        "arcadeClass": 1,
        "manufacturerLogoIndex": 19,
        "ratings": (9, 8, 7),
    },
    "mgton": {
        "displayName": "GTO '95 SR",
        "menuLogoName": "mgto.tim",
        "physicsBasisStem": "mgton",
        "physicsPartBasis": {},
        "arcadeClass": 1,
        "manufacturerLogoIndex": 19,
        "ratings": (9, 8, 7),
    },
    "mgttn": {
        "displayName": "GTO '95 TWIN TURBO",
        "menuLogoName": "mgtt.tim",
        "physicsBasisStem": "mgttn",
        "physicsPartBasis": {},
        "arcadeClass": 1,
        "manufacturerLogoIndex": 19,
        "ratings": (9, 8, 7),
    },
}
GT1_IMPREZA_STI_V3_CAR = {
    "stem": "s-pbn",
    "displayName": "IMPREZA Sedan WRX-STi version III",
    # This Arcade body has no byte-identical GT1 production model. Preserve
    # the authored model, wheel placement, shadow, and matching texture UVs
    # through the structural GT-CAR-to-CDO/CNO converter.
    "modelBasisStem": "sipbn",
    "convertModel": True,
    "physicsBasisStem": "sipbn",
    "physicsPartBasis": {},
    # The sole physical difference from the production Version III is GT1's
    # deliberate 30 kg reduction; remaining bytes are identity/price/text.
    "physicsExpectedDifferences": (
        0x01,
        0x5A,
        0x184,
        0x185,
        0x186,
        0x188,
        0x190,
        0x192,
    ),
    "physicsU16Overrides": {3: {0x0E: 1220}},
    "arcadeLogoEntry": 18,
    "arcadeClass": 1,
    "manufacturerLogoIndex": 28,
    "ratings": (10, 8, 8),
    "stats": (280, 6500, 343, 4000, 1220),
    "paintSources": (
        (103, "a26sn"),
        (104, "siprn"),
        (111, "a2bin"),
    ),
}
GT1_SOARER_VVTI_CAR = {
    "stem": "t-oan",
    "displayName": "SOARER 2.5GT-T VVT-i",
    # GT1's Arcade Soarer has its own authored model quartet rather than a
    # renamed copy of the production car. Preserve that body and its matching
    # texture UVs through the structural GT-CAR-to-CDO/CNO converter.
    "modelBasisStem": "tsoan",
    "convertModel": True,
    "physicsBasisStem": "tsoan",
    "physicsPartBasis": {},
    # Its serialized physical specification is byte-identical to GT1's
    # production Soarer. The differences are identity, price, and strings.
    "physicsExpectedDifferences": (
        0x01,
        0x184,
        0x185,
        0x186,
        0x190,
        0x192,
    ),
    "arcadeLogoEntry": 23,
    "arcadeClass": 1,
    "manufacturerLogoIndex": 31,
    "ratings": (10, 8, 8),
    "stats": (280, 6200, 378, 2400, 1560),
    # Preserve the GT1 texture's wine-red, yellow, and purple palette IDs.
    # These GT2 records provide matching native menu swatches/name records;
    # the actual body palettes remain the converted GT1-authored CLUTs.
    "paintSources": (
        (101, "tsoan"),
        (104, "tmr2n"),
        (117, "t2vzr"),
    ),
}
GT1_SUPRA_RZ_CAR = {
    "stem": "t-pnn",
    "displayName": "SUPRA RZ",
    # GT1's Arcade Supra and production Supra RZ share the complete authored
    # model pair. The imported value is its separate three-palette texture
    # package and exact Arcade selection treatment.
    "modelBasisStem": "tspnn",
    "physicsBasisStem": "tspnn",
    "physicsPartBasis": {},
    # The serialized physical specification is byte-identical. Differences
    # are limited to identity, price, and string-table references.
    "physicsExpectedDifferences": (
        0x01,
        0x184,
        0x185,
        0x186,
        0x190,
        0x192,
    ),
    "arcadeLogoEntry": 22,
    "arcadeClass": 1,
    "manufacturerLogoIndex": 31,
    "ratings": (10, 8, 8),
    "stats": (280, 5600, 431, 3600, 1510),
    # The actual turquoise, purple, and bronze body palettes come from GT1.
    # These records provide native GT2 menu metadata for the same palette IDs;
    # they do not replace or recolour the imported GT1-authored CLUTs.
    "paintSources": (
        (103, "t2m2n"),
        (111, "t-rdr"),
        (119, "t2vzr"),
    ),
}
GT1_SILVIA_QS_1800_CAR = {
    "stem": "n-13n",
    "displayName": "S13 SILVIA Q's 1800cc",
    # GT1 proves this Arcade composite shares the production Q's complete
    # authored model pair. Its separate value is the three-palette texture
    # package and original named GT Mode logo.
    "modelBasisStem": "nq13n",
    "physicsBasisStem": "nq13n",
    "physicsPartBasis": {},
    "physicsExpectedDifferences": (
        0x01,
        0x184,
        0x185,
        0x186,
        0x188,
        0x190,
        0x192,
        0x1A0,
    ),
    "menuLogoName": "n-13.tim",
    "arcadeClass": 3,
    "manufacturerLogoIndex": 20,
    "ratings": (6, 8, 8),
    "stats": (135, 6400, 159, 5200, 1090),
    "paintSources": (
        (101, "nq13n"),
        (104, "nq23n"),
        (108, "nq13n"),
    ),
}
GT1_LANCER_EVO_IV_GSR_CAR = {
    "stem": "m-nnn",
    "displayName": "LANCER Evolution IV GSR",
    # The Arcade composite shares the production Evolution IV's complete
    # authored model pair. Its exclusive content is the three-palette GT1
    # texture package and its original named menu logo.
    "modelBasisStem": "mlnnn",
    "physicsBasisStem": "mlnnn",
    "physicsPartBasis": {},
    # The serialized physical specification is byte-identical. Differences
    # are limited to identity, price, and string-table references.
    "physicsExpectedDifferences": (
        0x01,
        0x184,
        0x185,
        0x186,
        0x190,
        0x192,
    ),
    "menuLogoName": "m-nn.tim",
    "arcadeClass": 1,
    "manufacturerLogoIndex": 19,
    "ratings": (9, 10, 10),
    "stats": (280, 6500, 353, 3000, 1350),
    # Preserve the exact GT1 palette IDs. These Mitsubishi records supply
    # matching native GT2 menu swatches and colour-name references only.
    "paintSources": (
        (104, "m2g5n"),
        (112, "m2lgn"),
        (119, "mgagn"),
    ),
}
GT1_ALCYONE_SVX_S4_CAR = {
    "stem": "s-v4n",
    "displayName": "ALCYONE SVX S4",
    # GT1's Arcade entry shares the production SVX S4's authored geometry.
    # Preserve its separate three-palette texture package and original logo.
    "modelBasisStem": "ssv4n",
    "physicsBasisStem": "ssv4n",
    "physicsPartBasis": {},
    # The physical payload is byte-identical. The remaining differences are
    # identity, price/string references, and a displayed-statistic field.
    "physicsExpectedDifferences": (
        0x01,
        0x184,
        0x185,
        0x186,
        0x190,
        0x192,
        0x1A0,
    ),
    "menuLogoName": "s-v4.tim",
    "arcadeClass": 2,
    "manufacturerLogoIndex": 28,
    "ratings": (8, 7, 7),
    "stats": (240, 6000, 309, 4800, 1590),
    "paintSources": (
        (49, "ssvxn"),
        (115, "slgnn"),
        (117, "sipzr"),
    ),
}
GT1_CELICA_SSII_CAR = {
    "stem": "t-eln",
    "displayName": "CELICA SS-II",
    # GT1's Arcade Celica shares the production `tceln` model pair. Its
    # separate value is the authored three-palette texture package and exact
    # named menu treatment.
    "modelBasisStem": "tceln",
    # GT2's production Celica compiler uses different UV/layout assumptions
    # despite the byte-identical GT1 source geometry. Preserve the GT1 packet
    # UVs through the structural model converter so its Arcade texture maps
    # correctly instead of projecting against GT2's later texture layout.
    "convertModel": True,
    "physicsBasisStem": "tceln",
    "physicsPartBasis": {},
    # The serialized physical payload is production-identical except for
    # GT1's deliberate 1,220 kg chassis. The final differences are displayed
    # power/torque statistics rather than consumed GT2 part data.
    "physicsExpectedDifferences": (
        0x01,
        0x5A,
        0x184,
        0x185,
        0x186,
        0x190,
        0x192,
        0x198,
        0x19A,
        0x19B,
    ),
    "physicsU16Overrides": {3: {0x0E: 1220}},
    "menuLogoName": "t-el.tim",
    "arcadeClass": 2,
    "manufacturerLogoIndex": 31,
    "ratings": (7, 8, 8),
    "stats": (170, 6600, 191, 4800, 1220),
    "paintSources": (
        (104, "tmr2n"),
        (112, "m2lgn"),
        (119, "t2vzr"),
    ),
}
GT1_ARCADE_CARS = (
    GT1_FIRST_ARCADE_CAR,
    GT1_ROADSTER_ARCADE_CAR,
    GT1_ROADSTER_RS_CAR,
    GT1_CIVIC_RACER_CAR,
    GT1_DB7_COUPE_CAR,
    GT1_IMPREZA_STI_V3_CAR,
    GT1_SOARER_VVTI_CAR,
    GT1_SUPRA_RZ_CAR,
    GT1_SILVIA_QS_1800_CAR,
    GT1_LANCER_EVO_IV_GSR_CAR,
    GT1_ALCYONE_SVX_S4_CAR,
    GT1_CELICA_SSII_CAR,
    GT1_CRX_91_SI_CAR,
)
# These records are archive-proven distinct cars with non-zero GT1 purchase
# prices and complete stock/Racing Modification graphic pairs. The five
# different-stem physical/model equivalents above are livery folds, not new
# GT Mode identities. `amian` remains excluded here because its authoritative
# GT1 price is zero; it requires a native prize-table path, not an invented
# dealership price.
GT1_GTMODE_DISTINCT_CARS = (
    GT1_FIRST_ARCADE_CAR,
    GT1_CIVIC_RACER_CAR,
    GT1_IMPREZA_STI_V3_CAR,
    GT1_SOARER_VVTI_CAR,
    GT1_CELICA_SSII_CAR,
    GT1_CRX_91_SI_CAR,
)

SSR11_VARIANTS = (
    ("gt1_ssr11", 26, "forward"),
    ("gt1_ssr11_r", 27, "reverse"),
    ("gt1_ssr11_a", 28, "Arcade forward"),
    ("gt1_ssr11_ar", 29, "Arcade reverse"),
    ("gt1_ssr11_2p", 30, "two-player forward"),
    ("gt1_ssr11_hifi", 31, "HiFi forward"),
)


@dataclass(frozen=True)
class GtArcEntry:
    index: int
    offset: int
    packed_size: int
    unpacked_size: int


@dataclass(frozen=True)
class TextureImageRelocation:
    name: str
    old_x: int
    old_y: int
    width: int
    height: int
    old_clut_id: int
    new_x: int
    new_y: int


@dataclass(frozen=True)
class TextureRelocation:
    clut_ids: dict[int, int]
    images: tuple[TextureImageRelocation, ...]


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def validate_gt1_image(path: Path) -> str:
    path = path.resolve()
    if not path.is_file():
        raise FileNotFoundError(f"US Gran Turismo disc image is missing: {path}")
    if path.stat().st_size != GT1_IMAGE_SIZE:
        raise ValueError(
            f"unsupported Gran Turismo image size: "
            f"{path.stat().st_size} != {GT1_IMAGE_SIZE}"
        )
    digest = sha256(path)
    if digest != GT1_IMAGE_SHA256:
        raise ValueError(
            f"unsupported Gran Turismo image hash: {digest}; "
            f"expected {GT1_IMAGE_SHA256}"
        )
    print(f"validated US Gran Turismo image: {path} sha256={digest}")
    return digest


def validate_disc_root(path: Path) -> None:
    for name, size in REQUIRED_DISC_FILES.items():
        candidate = path / name
        if not candidate.is_file() or candidate.stat().st_size != size:
            raise ValueError(
                f"extracted GT1 file is missing or wrong-sized: "
                f"{candidate} expected={size}"
            )
    system_cnf = (path / "SYSTEM.CNF").read_text(
        encoding="ascii", errors="replace"
    )
    if "SCUS_941.94" not in system_cnf:
        raise ValueError("GT1 SYSTEM.CNF does not boot SCUS_941.94")


def gt1_lzss_decompress(data: bytes, expected_size: int | None = None) -> bytes:
    output = bytearray()
    position = 0
    while position < len(data) and (
        expected_size is None or len(output) < expected_size
    ):
        mask = data[position]
        position += 1
        for bit in range(8):
            if expected_size is not None and len(output) >= expected_size:
                break
            if position >= len(data):
                if expected_size is None:
                    return bytes(output)
                raise ValueError("truncated GT1 LZSS stream")
            if not ((mask >> bit) & 1):
                output.append(data[position])
                position += 1
                continue

            length = data[position] + 3
            position += 1
            if position >= len(data):
                raise ValueError("truncated GT1 LZSS back-reference")
            encoded_distance = data[position]
            position += 1
            if encoded_distance & 0x80:
                if position >= len(data):
                    raise ValueError("truncated GT1 LZSS long distance")
                distance = (
                    ((encoded_distance & 0x7F) << 8) | data[position]
                ) + 1
                position += 1
            else:
                distance = encoded_distance + 1
            if distance > len(output):
                raise ValueError(
                    f"invalid GT1 LZSS distance {distance} at {len(output)}"
                )
            for _ in range(length):
                output.append(output[-distance])
                if expected_size is not None and len(output) >= expected_size:
                    break

    if expected_size is not None and len(output) != expected_size:
        raise ValueError(
            f"GT1 LZSS size mismatch: {len(output)} != {expected_size}"
        )
    return bytes(output)


def gt_lzss_compress(data: bytes) -> bytes:
    """Encode the shared GT1/GT2 LZSS stream without overlapping matches."""
    output = bytearray()
    positions: dict[bytes, list[int]] = {}
    cursor = 0
    while cursor < len(data):
        flag_offset = len(output)
        output.append(0)
        flags = 0
        for bit in range(8):
            if cursor >= len(data):
                break

            best_start = -1
            best_length = 0
            if cursor + 3 <= len(data):
                key = data[cursor : cursor + 3]
                candidates = positions.get(key, ())
                for candidate in reversed(candidates[-128:]):
                    distance = cursor - candidate
                    if distance > 0x8000:
                        break
                    # Polyphony's encoder deliberately avoids overlapping
                    # matches even though the decoder can repeat them.
                    maximum = min(258, len(data) - cursor, distance)
                    length = 3
                    while (
                        length < maximum
                        and data[candidate + length] == data[cursor + length]
                    ):
                        length += 1
                    if length > best_length:
                        best_start = candidate
                        best_length = length
                        if best_length == maximum:
                            break

            if best_length >= 3:
                flags |= 1 << bit
                output.append(best_length - 3)
                encoded_distance = cursor - best_start - 1
                if encoded_distance >= 0x80:
                    output.append(0x80 | (encoded_distance >> 8))
                    output.append(encoded_distance & 0xFF)
                else:
                    output.append(encoded_distance)
                next_cursor = cursor + best_length
            else:
                output.append(data[cursor])
                next_cursor = cursor + 1

            for position in range(cursor, next_cursor):
                if position + 3 <= len(data):
                    key = data[position : position + 3]
                    bucket = positions.setdefault(key, [])
                    bucket.append(position)
                    if len(bucket) > 256:
                        del bucket[:128]
            cursor = next_cursor
        output[flag_offset] = flags
    return bytes(output)


def build_gt_zip(data: bytes) -> bytes:
    compressed = gt_lzss_compress(data)
    if gt1_lzss_decompress(compressed, len(data)) != data:
        raise ValueError("GT-ZIP compressor failed its round-trip check")
    return b"@(#)GT-ZIP\0\0" + struct.pack("<I", len(data)) + compressed


def parse_gtarc(
    data: bytes,
    source: str = "<memory>",
) -> tuple[bytes, list[GtArcEntry]]:
    if data[:10] != b"@(#)GT-ARC":
        raise ValueError(f"not an uncompressed GT-ARC archive: {source}")
    raw_file_count = struct.unpack_from("<H", data, 14)[0]
    # BG.DAT sets the archive flag in bit 15 while retaining the entry count
    # in the lower fifteen bits. COURSE.DAT leaves the flag clear.
    file_count = raw_file_count & 0x7FFF
    if file_count <= 0:
        raise ValueError(
            f"invalid GT-ARC file count: {raw_file_count:#06x}"
        )
    entries = []
    for index in range(file_count):
        offset, packed_size, unpacked_size = struct.unpack_from(
            "<III", data, 16 + index * 12
        )
        if (
            offset < 16 + file_count * 12
            or offset + packed_size > len(data)
            or unpacked_size <= 0
        ):
            raise ValueError(f"invalid GT-ARC entry {index} in {source}")
        entries.append(
            GtArcEntry(index, offset, packed_size, unpacked_size)
        )
    return data, entries


def read_gtarc(path: Path) -> tuple[bytes, list[GtArcEntry]]:
    return parse_gtarc(path.read_bytes(), str(path))


def unpack_entry(archive: bytes, entry: GtArcEntry) -> bytes:
    return gt1_lzss_decompress(
        archive[entry.offset : entry.offset + entry.packed_size],
        entry.unpacked_size,
    )


def read_gt1_menu_car_logo(disc_root: Path, name: str) -> bytes:
    """Read one original named GT1 car-logo TIM from the US menu archives."""

    raw_archive, raw_entries = read_gtarc(disc_root / "MENU_RAW.ARC")
    names_entry = raw_entries[1]
    if names_entry.packed_size != names_entry.unpacked_size:
        raise ValueError("GT1 menu-name table unexpectedly became compressed")
    names_data = raw_archive[
        names_entry.offset : names_entry.offset + names_entry.packed_size
    ]
    names = names_data.decode("ascii").splitlines()
    matches = [index for index, candidate in enumerate(names) if candidate == name]
    if len(matches) != 1 or matches[0] == 0:
        raise ValueError(f"GT1 named car logo is absent or ambiguous: {name}")
    # `gt.ins` is a real inner member at index zero. The name list therefore
    # maps directly to inner archive indices; only its final empty line has no
    # member.
    image_index = matches[0]

    outer, outer_entries = read_gtarc(disc_root / "MENU_IMG.ARC")
    if len(outer_entries) != 6:
        raise ValueError("GT1 MENU_IMG.ARC locale-bank count changed")
    bank = outer_entries[0]
    if bank.packed_size != bank.unpacked_size:
        raise ValueError("GT1 menu image bank unexpectedly became compressed")
    inner_data = outer[bank.offset : bank.offset + bank.packed_size]
    inner, inner_entries = parse_gtarc(inner_data, "MENU_IMG.ARC bank 0")
    if image_index >= len(inner_entries):
        raise ValueError(f"GT1 named car logo index is absent: {name}")
    logo = unpack_entry(inner, inner_entries[image_index])
    if logo[:8] != b"\x10\0\0\0\x08\0\0\0":
        raise ValueError(f"GT1 named car logo is not a 4-bit TIM: {name}")
    return logo


def read_gt1_definition_car_logo(
    disc_root: Path,
    definition: dict[str, object],
) -> tuple[bytes, dict[str, object]]:
    """Read the exact US GT1 wordmark selected by one car definition."""

    menu_logo_name = definition.get("menuLogoName")
    if menu_logo_name is not None:
        logo = read_gt1_menu_car_logo(
            disc_root, str(menu_logo_name)
        )
        source = {
            "archive": "MENU_IMG.ARC",
            "name": str(menu_logo_name),
        }
    else:
        arcade_archive, arcade_entries = read_gtarc(
            disc_root / "ARCADE.DAT"
        )
        logo_entry = int(definition["arcadeLogoEntry"])
        if logo_entry >= len(arcade_entries):
            raise ValueError(
                f"GT1 Arcade logo entry is absent: {logo_entry}"
            )
        logo = unpack_entry(
            arcade_archive, arcade_entries[logo_entry]
        )
        source = {
            "archive": "ARCADE.DAT",
            "entry": logo_entry,
        }
    return logo, source


def convert_gt1_car_texture(data: bytes) -> bytes:
    """Convert one native GT1 GT-CTEX texture to native GT2 CDP/CNP layout.

    Both formats store the same 256x224 4-bpp bitmap, sixteen 16-colour
    palettes per paint variant, paint IDs, illumination masks, and paint
    masks.  GT1 places the shared masks before the bitmap and its palette
    blocks after it; GT2 moves the bitmap to the end and reserves mask space
    after every palette block.  No pixels or authored palette values are
    synthesized here.
    """
    if (
        len(data) < 0x8060
        or not data.startswith(b"@(#)GT-CTEX\0")
        or struct.unpack_from("<H", data, 12)[0] != 2
    ):
        raise ValueError("unsupported GT1 car texture")
    color_count = struct.unpack_from("<H", data, 14)[0]
    if not 1 <= color_count <= 16:
        raise ValueError(f"invalid GT1 car color count: {color_count}")
    expected_size = 0x8060 + color_count * 0x200
    if len(data) != expected_size:
        raise ValueError(
            f"unexpected GT1 car texture size: {len(data):#x} != "
            f"{expected_size:#x}"
        )

    output = bytearray(0xB3A0)
    output[0] = color_count
    output[2 : 2 + color_count] = data[0x10 : 0x10 + color_count]
    for color_index in range(color_count):
        source = 0x8060 + color_index * 0x200
        target = 0x20 + color_index * 0x240
        output[target : target + 0x200] = data[source : source + 0x200]
    # GT1 stores one shared illumination-mask and paint-mask set.  GT2's
    # compiler places that authored set in the first colour record and leaves
    # the per-colour reserved mask areas zeroed.
    output[0x220:0x260] = data[0x20:0x60]
    output[0x43A0:0xB3A0] = data[0x1060:0x8060]
    return bytes(output)


def read_gt1_car_stems(path: Path) -> list[str]:
    data = path.read_bytes()
    start = data.index(b"0logn\0")
    stems: list[str] = []
    cursor = start
    valid = set("-0123456789abcdefghijklmnopqrstuvwxyz")
    while cursor + 6 <= len(data):
        raw = data[cursor : cursor + 6]
        if raw[5] != 0:
            break
        try:
            stem = raw[:5].decode("ascii")
        except UnicodeDecodeError:
            break
        if any(character not in valid for character in stem):
            break
        stems.append(stem)
        cursor += 6
    if len(stems) != 344:
        raise ValueError(
            f"expected 344 GT1 car graphic stems, found {len(stems)}"
        )
    return stems


def read_gt1_car_members(
    car_path: Path,
    stems: list[str],
    stem: str,
) -> tuple[bytes, bytes, bytes, bytes]:
    try:
        stem_index = stems.index(stem)
    except ValueError as exc:
        raise ValueError(f"GT1 car stem is absent: {stem}") from exc
    archive, entries = read_gtarc(car_path)
    if len(entries) != len(stems) * 4:
        raise ValueError(
            f"GT1 CAR.DAT has {len(entries)} entries for "
            f"{len(stems)} stems"
        )
    night_bank = len(stems) * 2
    indices = (
        stem_index * 2,
        stem_index * 2 + 1,
        night_bank + stem_index * 2,
        night_bank + stem_index * 2 + 1,
    )
    members = tuple(unpack_entry(archive, entries[index]) for index in indices)
    expected = (
        b"@(#)GT-CTEX",
        b"@(#)GT-CAR",
        b"@(#)GT-CTEX",
        b"@(#)GT-CAR",
    )
    if any(
        not member.startswith(header)
        for member, header in zip(members, expected)
    ):
        raise ValueError(f"GT1 car {stem} has an unexpected member layout")
    return members


def _signed_short(value: int) -> int:
    if value == -0x8000:
        raise ValueError("GT1 model coordinate cannot be negated")
    return -value


def _decode_gt1_car_polygon(
    data: bytes,
    offset: int,
    *,
    is_quad: bool,
    is_textured: bool,
    vertex_count: int,
    normal_count: int,
) -> tuple[bytes, int]:
    size = 28 if is_textured else 16
    if offset + size > len(data):
        raise ValueError("GT1 car polygon is truncated")
    vertex_data = data[offset : offset + 6]
    normal_data = data[offset + 6 : offset + 12]
    texture_flags = data[offset + 12 : offset + 15]
    face_type = data[offset + 15]

    vertex_refs = (
        vertex_data[0] | ((vertex_data[1] & 1) << 8),
        (vertex_data[1] >> 1) | ((vertex_data[2] & 3) << 7),
        (vertex_data[2] >> 2) | ((vertex_data[3] & 7) << 6),
        vertex_data[4] | ((vertex_data[5] & 1) << 8),
    )
    normal_refs = (
        ((vertex_data[5] | (normal_data[0] << 8)) >> 1) & 0x1FF,
        ((normal_data[0] | (normal_data[1] << 8)) >> 3) & 0x1FF,
        (normal_data[2] | (normal_data[3] << 8)) & 0x1FF,
        ((normal_data[3] | (normal_data[4] << 8)) >> 2) & 0x1FF,
    )
    used_vertices = vertex_refs if is_quad else vertex_refs[:3]
    if any(reference >= vertex_count for reference in used_vertices):
        raise ValueError("GT1 car polygon has an invalid vertex reference")
    if any(reference >= normal_count for reference in normal_refs):
        raise ValueError("GT1 car polygon has an invalid normal reference")
    if any(reference > 0xFF for reference in used_vertices):
        raise ValueError(
            "GT1 car polygon cannot fit GT2's 8-bit vertex references"
        )
    expected_face_type = (
        0x2D if is_quad and is_textured
        else 0x29 if is_quad
        else 0x25 if is_textured
        else 0x21
    )
    if face_type != expected_face_type:
        raise ValueError(
            f"unexpected GT1 car face type: {face_type:#x} != "
            f"{expected_face_type:#x}"
        )
    if (is_textured and texture_flags != b"\xFF\xFF\xFF") or (
        normal_data[5] != 0
    ):
        raise ValueError("GT1 car polygon flags changed")

    render_order = 17 if normal_data[1] & 0x80 else 16
    render_flags = 0
    gt2_face_type = face_type - 1 if face_type in (0x21, 0x29) else face_type
    output = bytearray(
        struct.pack(
            "<4BHHII",
            vertex_refs[0],
            vertex_refs[1],
            vertex_refs[2],
            vertex_refs[3] if is_quad else 0,
            (normal_refs[0] << 5) | render_order,
            0,
            (normal_refs[1] << 1)
            | (normal_refs[2] << 10)
            | (normal_refs[3] << 19),
            (gt2_face_type << 24)
            | (0 if is_textured else int.from_bytes(texture_flags, "little")),
        )
    )
    if is_textured:
        uv_data = data[offset + 16 : offset + 28]
        uv0 = (uv_data[0], uv_data[1] - (32 if uv_data[1] >= 32 else 0))
        raw_palette = struct.unpack_from("<H", uv_data, 2)[0]
        palette = (raw_palette >> 4) + (raw_palette & 0x3F)
        uv1 = (uv_data[4], uv_data[5] - (32 if uv_data[5] >= 32 else 0))
        if uv_data[6:8] != b"\0\0":
            raise ValueError("GT1 car textured-polygon padding changed")
        uv2 = (uv_data[8], uv_data[9] - (32 if uv_data[9] >= 32 else 0))
        uv3 = (uv_data[10], uv_data[11] - (32 if uv_data[11] >= 32 else 0))
        if not is_quad and uv3 != (0, 0):
            raise ValueError("GT1 car triangle has a fourth UV coordinate")
        if palette > 15:
            raise ValueError(f"GT1 car palette is out of range: {palette}")
        render_flags = 8 | (4 if palette == 14 else 0)
        struct.pack_into("<H", output, 6, render_flags << 12)
        output.extend(
            struct.pack(
                "<BBHBBBBBBBB",
                *uv0,
                ((palette & 0x0C) << 4) | (palette & 0x03),
                *uv1,
                0,
                0,
                *uv2,
                *uv3,
            )
        )
    return bytes(output), offset + size


def _pack_gt2_car_normal(x: int, y: int, z: int) -> int:
    magnitude_squared = x * x + y * y + z * z
    if magnitude_squared == 0:
        # A handful of stock GT1 models contain an unreferenced zero normal.
        return 0
    if not 15_840_000 <= magnitude_squared <= 16_160_000:
        raise ValueError(
            f"GT1 car normal is not a unit vector: {(x, y, z)}"
        )
    converted = (int(x / 8), int(y / 8), int(-z / 8))
    if any(not -512 <= value <= 511 for value in converted):
        raise ValueError("GT1 car normal escaped GT2's signed 10-bit range")
    return (
        ((converted[0] & 0x3FF) << 2)
        | ((converted[1] & 0x3FF) << 12)
        | ((converted[2] & 0x3FF) << 22)
    )


def _convert_gt1_car_lod(
    data: bytes,
    offset: int,
) -> tuple[bytes, int, dict[str, object]]:
    if offset + 40 > len(data):
        raise ValueError("GT1 car LOD header is truncated")
    (
        vertex_count,
        source_normal_count,
        triangle_count,
        quad_count,
        unknown_count_1,
        unknown_count_2,
        uv_triangle_count,
        uv_quad_count,
    ) = struct.unpack_from("<8H", data, offset)
    if unknown_count_1 or unknown_count_2:
        raise ValueError("GT1 car LOD has an unsupported polygon type")
    if vertex_count > 256:
        raise ValueError(
            f"GT1 car LOD has {vertex_count} vertices; GT2 supports 256"
        )
    # GT1 polygon packets encode normal references in nine bits, so indices
    # 512 and above are unreachable. A small number of authored models retain
    # trailing, unreferenced normals. Read them to preserve source validation
    # and cursor alignment, but omit only those unreachable tail records from
    # the native GT2 LOD.
    normal_count = min(source_normal_count, 512)
    if any(data[offset + 16 : offset + 20]):
        raise ValueError("GT1 car LOD header padding changed")
    source_bounds = struct.unpack_from("<8h", data, offset + 20)
    scale, scale_related = struct.unpack_from("<2H", data, offset + 36)

    cursor = offset + 40
    vertices: list[tuple[int, int, int, int]] = []
    for _ in range(vertex_count):
        if cursor + 8 > len(data):
            raise ValueError("GT1 car vertex array is truncated")
        x, y, z, w = struct.unpack_from("<4h", data, cursor)
        vertices.append((x, y, _signed_short(z), w))
        cursor += 8
    normals: list[int] = []
    for normal_index in range(source_normal_count):
        if cursor + 8 > len(data):
            raise ValueError("GT1 car normal array is truncated")
        x, y, z, w = struct.unpack_from("<4h", data, cursor)
        if w != 0:
            raise ValueError("GT1 car normal padding changed")
        packed_normal = _pack_gt2_car_normal(x, y, z)
        if normal_index < normal_count:
            normals.append(packed_normal)
        cursor += 8

    polygon_groups: list[list[bytes]] = []
    for count, is_quad, is_textured in (
        (triangle_count, False, False),
        (quad_count, True, False),
        (uv_triangle_count, False, True),
        (uv_quad_count, True, True),
    ):
        polygons: list[bytes] = []
        for _ in range(count):
            polygon, cursor = _decode_gt1_car_polygon(
                data,
                cursor,
                is_quad=is_quad,
                is_textured=is_textured,
                vertex_count=vertex_count,
                normal_count=normal_count,
            )
            polygons.append(polygon)
        polygon_groups.append(polygons)

    output = bytearray(0x50)
    struct.pack_into(
        "<8H",
        output,
        0,
        vertex_count,
        normal_count,
        triangle_count,
        quad_count,
        0,
        0,
        uv_triangle_count,
        uv_quad_count,
    )
    struct.pack_into("<I", output, 0x14, 0x50)
    low = tuple(min(vertex[axis] for vertex in vertices) for axis in range(3))
    high = tuple(max(vertex[axis] for vertex in vertices) for axis in range(3))
    expected_low = (
        source_bounds[0],
        source_bounds[1],
        _signed_short(source_bounds[6]),
    )
    expected_high = (
        source_bounds[4],
        source_bounds[5],
        _signed_short(source_bounds[2]),
    )
    if low != expected_low or high != expected_high:
        raise ValueError(
            "GT1 car LOD bounds do not match its vertices: "
            f"{low}/{high} != {expected_low}/{expected_high}"
        )
    struct.pack_into(
        "<8hHH",
        output,
        0x3C,
        *low,
        0,
        *high,
        0,
        scale,
        scale_related,
    )
    for vertex in vertices:
        output.extend(struct.pack("<4h", *vertex))

    group_offsets: list[int] = [len(output)]
    for normal in normals:
        output.extend(struct.pack("<I", normal))
    for polygons in polygon_groups:
        group_offsets.append(len(output))
        for polygon in polygons:
            output.extend(polygon)
    # The two unsupported arrays are empty and share the UV-triangle offset.
    normal_offset = group_offsets[0]
    triangle_offset = group_offsets[1]
    quad_offset = group_offsets[2]
    uv_triangle_offset = group_offsets[3]
    uv_quad_offset = group_offsets[4]
    struct.pack_into(
        "<7I",
        output,
        0x1C,
        normal_offset,
        triangle_offset,
        quad_offset,
        uv_triangle_offset,
        uv_triangle_offset,
        uv_triangle_offset,
        uv_quad_offset,
    )
    return bytes(output), cursor, {
        "vertices": vertex_count,
        "normals": normal_count,
        "sourceNormals": source_normal_count,
        "unreachableSourceNormalsDropped": (
            source_normal_count - normal_count
        ),
        "triangles": triangle_count,
        "quads": quad_count,
        "uvTriangles": uv_triangle_count,
        "uvQuads": uv_quad_count,
        "scale": scale,
        "scaleRelated": scale_related,
        "size": len(output),
    }


def _convert_gt1_car_shadow(
    data: bytes,
    offset: int,
) -> tuple[bytes, int, dict[str, object]]:
    if offset + 32 > len(data):
        raise ValueError("GT1 car shadow is truncated")
    unknown, quad_count, scale, unknown_2 = struct.unpack_from(
        "<4H", data, offset
    )
    if unknown or unknown_2 or quad_count != 4:
        raise ValueError(
            "unsupported GT1 car shadow header: "
            f"{(unknown, quad_count, scale, unknown_2)}"
        )
    cursor = offset + 32
    vertex_count = quad_count * 4
    vertices: list[tuple[int, int]] = []
    for _ in range(vertex_count):
        if cursor + 8 > len(data):
            raise ValueError("GT1 car shadow vertex array is truncated")
        x, y, z, padding = struct.unpack_from("<4h", data, cursor)
        if y or padding:
            raise ValueError("GT1 car shadow vertex padding changed")
        vertices.append((x, _signed_short(z)))
        cursor += 8

    output = bytearray(28)
    low_x = min(vertex[0] for vertex in vertices)
    low_z = min(vertex[1] for vertex in vertices)
    high_x = max(vertex[0] for vertex in vertices)
    high_z = max(vertex[1] for vertex in vertices)
    struct.pack_into(
        "<4H8hHH",
        output,
        0,
        vertex_count,
        0,
        quad_count,
        0,
        low_x,
        0,
        low_z,
        0,
        high_x,
        0,
        high_z,
        0,
        scale,
        0,
    )
    for vertex in vertices:
        output.extend(struct.pack("<2h", *vertex))
    mockups = (
        (0, 1, 2, 3),
        (3, 2, 7, 6),
        (6, 7, 4, 5),
        (5, 4, 8, 9),
    )
    for refs in mockups:
        packed = (
            refs[0]
            | (refs[1] << 6)
            | (refs[2] << 12)
            | (refs[3] << 18)
            | 0x80000000
        )
        output.extend(struct.pack("<I", packed))
    return bytes(output), cursor, {
        "vertices": vertex_count,
        "triangles": 0,
        "quads": quad_count,
        "scale": scale,
        "size": len(output),
    }


def _gt2_car_model_end(data: bytes) -> int:
    if len(data) < 0x884 or data[:3] != b"GT\x02":
        raise ValueError("GT2 car model header is missing")
    if struct.unpack_from("<I", data, 0x868)[0] != 3:
        raise ValueError("GT2 car model does not contain three LODs")
    cursor = 0x884
    for _ in range(3):
        if cursor + 0x50 > len(data):
            raise ValueError("GT2 car LOD is truncated")
        counts = struct.unpack_from("<8H", data, cursor)
        vertex_count, normal_count, tri_count, quad_count = counts[:4]
        uv_tri_count, uv_quad_count = counts[6:8]
        cursor += (
            0x50
            + vertex_count * 8
            + normal_count * 4
            + (tri_count + quad_count) * 16
            + (uv_tri_count + uv_quad_count) * 28
        )
    if cursor + 28 > len(data):
        raise ValueError("GT2 car shadow header is truncated")
    vertex_count, triangle_count, quad_count = struct.unpack_from(
        "<3H", data, cursor
    )
    return cursor + 28 + vertex_count * 4 + (triangle_count + quad_count) * 4


def convert_gt1_car_model(
    data: bytes,
    gt2_header_basis: bytes,
) -> tuple[bytes, dict[str, object]]:
    if len(data) < 0x80 or not data.startswith(b"@(#)GT-CAR\0"):
        raise ValueError("GT1 car model header is missing")
    if _gt2_car_model_end(gt2_header_basis) != len(gt2_header_basis):
        raise ValueError("GT2 car header basis has trailing data")
    lod_count = struct.unpack_from("<H", data, 0x3C)[0]
    if lod_count != 3:
        raise ValueError(f"GT1 car model has {lod_count} LODs; expected 3")
    lod_distances = tuple(
        struct.unpack_from("<H", data, 0x42 + index * 8)[0]
        for index in range(3)
    )
    if lod_distances != (5, 15, 300):
        raise ValueError(
            f"GT1 car LOD distances changed: {lod_distances}"
        )

    output = bytearray(gt2_header_basis[:0x868])
    gt1_wheels = [
        struct.unpack_from("<4h", data, 0x10 + index * 8)
        for index in range(4)
    ]
    gt1_wheels = [gt1_wheels[index] for index in (2, 3, 0, 1)]
    basis_wheels = [
        struct.unpack_from("<4h", gt2_header_basis, 0x20 + index * 8)
        for index in range(4)
    ]
    for index, ((x, y, z, padding), basis) in enumerate(
        zip(gt1_wheels, basis_wheels)
    ):
        if padding:
            raise ValueError("GT1 car wheel-position padding changed")
        menu_x = x + basis[3] - basis[0]
        if not -0x8000 <= menu_x <= 0x7FFF:
            raise ValueError("converted GT1 menu wheel position overflowed")
        struct.pack_into(
            "<4h", output, 0x20 + index * 8, x, y, z, menu_x
        )

    output.extend(struct.pack("<I", 3))
    distance_offsets: list[int] = []
    for distance in lod_distances:
        output.extend(struct.pack("<HHI", 0, distance, 0))
        distance_offsets.append(len(output) - 4)

    cursor = 0x80
    lods: list[dict[str, object]] = []
    lod_offsets: list[int] = []
    for index in range(3):
        lod_offsets.append(len(output))
        lod, cursor, metadata = _convert_gt1_car_lod(data, cursor)
        output.extend(lod)
        lods.append(metadata)
        if index != 2:
            if cursor + 40 > len(data) or any(data[cursor : cursor + 40]):
                raise ValueError("GT1 inter-LOD padding changed")
            cursor += 40
    struct.pack_into("<I", output, distance_offsets[1], lod_offsets[1])
    struct.pack_into("<I", output, distance_offsets[2], lod_offsets[2])

    shadow, cursor, shadow_metadata = _convert_gt1_car_shadow(data, cursor)
    output.extend(shadow)
    if cursor != len(data):
        raise ValueError(
            f"GT1 car model has {len(data) - cursor} trailing bytes"
        )
    if len(output) >= GT2_CAR_MODEL_SAFE_SIZE:
        raise ValueError(
            "converted GT2 car model exceeds its native "
            f"{GT2_CAR_MODEL_SAFE_SIZE:#x}-byte slot: {len(output):#x}"
        )
    if _gt2_car_model_end(output) != len(output):
        raise ValueError("converted GT2 car model failed structural validation")
    return bytes(output), {
        "method": "native GT-CAR to GT2 CDO/CNO structural conversion",
        "sourceSize": len(data),
        "convertedSize": len(output),
        "lodDistances": list(lod_distances),
        "lods": lods,
        "shadow": shadow_metadata,
        "sourceSha256": hashlib.sha256(data).hexdigest(),
        "convertedSha256": hashlib.sha256(output).hexdigest(),
        "headerBasisSha256": hashlib.sha256(
            gt2_header_basis[:0x868]
        ).hexdigest(),
    }


def encode_gt2_car_id(stem: str) -> int:
    characters = "-0123456789abcdefghijklmnopqrstuvwxyz"
    if len(stem) != 5 or any(character not in characters for character in stem):
        raise ValueError(f"invalid GT2 car stem: {stem!r}")
    value = 0
    for character in stem:
        value = (value << 6) | characters.index(character)
    return value


def decode_gt2_car_id(value: int) -> str:
    characters = "-0123456789abcdefghijklmnopqrstuvwxyz"
    decoded = []
    for shift in range(24, -1, -6):
        index = (value >> shift) & 0x3F
        if index >= len(characters):
            raise ValueError(f"invalid packed GT2 car ID: {value:#x}")
        decoded.append(characters[index])
    return "".join(decoded)


def _parse_gt2_carinfo(
    data: bytes,
) -> list[dict[str, object]]:
    if data[:4].lower() != b"car\0":
        raise ValueError("GT2 carinfo header is missing")
    count = struct.unpack_from("<I", data, 4)[0]
    header_end = 8 + count * 8
    records: list[dict[str, object]] = []
    for index in range(count):
        car_id, encoded = struct.unpack_from("<II", data, 8 + index * 8)
        color_count = ((encoded >> 18) & 0x1F) + 1
        offset = encoded & 0x3FFFF
        if offset < header_end or offset + color_count * 3 + 2 > len(data):
            raise ValueError(f"GT2 carinfo record {index} is invalid")
        main_colors = struct.unpack_from(
            f"<{color_count}H", data, offset
        )
        color_ids_start = offset + color_count * 2
        color_ids = tuple(
            data[color_ids_start : color_ids_start + color_count]
        )
        name_length = data[color_ids_start + color_count]
        name_start = color_ids_start + color_count + 1
        name_end = name_start + name_length
        if name_end >= len(data) or data[name_end] != 0:
            raise ValueError(f"GT2 carinfo name {index} is invalid")
        records.append(
            {
                "carId": car_id,
                "stem": decode_gt2_car_id(car_id),
                "mainColors": main_colors,
                "colorIds": color_ids,
                "name": data[name_start:name_end],
                "flags": encoded & ~0x7FFFFF,
            }
        )
    return records


def _parse_gt2_carcolor(
    data: bytes,
    records: list[dict[str, object]],
) -> list[tuple[int, ...]]:
    if data[:8] != b"CCOL00\0\0":
        raise ValueError("GT2 carcolor header is missing")
    header_end = 8 + len(records) * 2
    if header_end > len(data):
        raise ValueError("GT2 carcolor offset table is truncated")
    result: list[tuple[int, ...]] = []
    for index, record in enumerate(records):
        offset = struct.unpack_from("<H", data, 8 + index * 2)[0]
        count = len(record["colorIds"])
        if offset < header_end or offset + count * 2 > len(data):
            raise ValueError(f"GT2 carcolor record {index} is invalid")
        result.append(struct.unpack_from(f"<{count}H", data, offset))
    return result


def append_gt2_carinfo(
    carinfo: bytes,
    carcolor: bytes,
    stem: str,
    display_name: str,
    paint_sources: tuple[tuple[int, str], ...],
) -> tuple[bytes, bytes, dict[str, object]]:
    records = _parse_gt2_carinfo(carinfo)
    colors = _parse_gt2_carcolor(carcolor, records)
    if any(record["stem"] == stem for record in records):
        raise ValueError(f"GT2 carinfo already contains {stem}")
    by_stem = {str(record["stem"]): index for index, record in enumerate(records)}
    main_colors: list[int] = []
    color_names: list[int] = []
    color_ids: list[int] = []
    for color_id, source_stem in paint_sources:
        if source_stem not in by_stem:
            raise ValueError(f"GT2 paint source is absent: {source_stem}")
        source_index = by_stem[source_stem]
        source = records[source_index]
        try:
            source_color = source["colorIds"].index(color_id)
        except ValueError as exc:
            raise ValueError(
                f"GT2 paint source {source_stem} lacks ID {color_id}"
            ) from exc
        color_ids.append(color_id)
        main_colors.append(source["mainColors"][source_color])
        color_names.append(colors[source_index][source_color])

    records.append(
        {
            "carId": encode_gt2_car_id(stem),
            "stem": stem,
            "mainColors": tuple(main_colors),
            "colorIds": tuple(color_ids),
            "name": display_name.encode("cp1252"),
            "flags": 0,
        }
    )
    colors.append(tuple(color_names))
    combined = sorted(zip(records, colors), key=lambda item: item[0]["carId"])

    info = bytearray(b"CAR\0" + struct.pack("<I", len(combined)))
    info.extend(b"\0" * (len(combined) * 8))
    colour = bytearray(b"CCOL00\0\0")
    colour.extend(b"\0" * (len(combined) * 2))
    inserted_index = -1
    for index, (record, name_indices) in enumerate(combined):
        while len(info) & 1:
            info.append(0)
        info_offset = len(info)
        count = len(record["colorIds"])
        info.extend(struct.pack(f"<{count}H", *record["mainColors"]))
        info.extend(bytes(record["colorIds"]))
        name = bytes(record["name"])
        info.append(len(name))
        info.extend(name + b"\0")
        encoded = (
            info_offset
            | ((count - 1) << 18)
            | int(record["flags"])
        )
        struct.pack_into(
            "<II",
            info,
            8 + index * 8,
            int(record["carId"]),
            encoded,
        )

        while len(colour) & 1:
            colour.append(0)
        colour_offset = len(colour)
        if colour_offset > 0xFFFF:
            raise ValueError("GT2 carcolor escaped its 16-bit offset range")
        struct.pack_into("<H", colour, 8 + index * 2, colour_offset)
        colour.extend(struct.pack(f"<{count}H", *name_indices))
        if record["stem"] == stem:
            inserted_index = index

    if inserted_index < 0:
        raise AssertionError("new GT2 carinfo record was not serialized")
    verified = _parse_gt2_carinfo(bytes(info))
    _parse_gt2_carcolor(bytes(colour), verified)
    return bytes(info), bytes(colour), {
        "stem": stem,
        "index": inserted_index,
        "displayName": display_name,
        "colorIds": color_ids,
        "mainColors": main_colors,
        "colorNameIndices": color_names,
    }


def extend_gt2_carinfo(
    carinfo: bytes,
    carcolor: bytes,
    stem: str,
    paint_sources: tuple[tuple[int, str], ...],
    *,
    allow_duplicate_color_ids: bool = False,
) -> tuple[bytes, bytes, dict[str, object]]:
    """Append visual choices to one existing car identity.

    GT2's customer-visible car identity remains unchanged. The returned first
    color index is used by the alternate-body table when the appended GT1
    visual package cannot share GT2's original indexed bitmap.
    """

    records = _parse_gt2_carinfo(carinfo)
    colors = _parse_gt2_carcolor(carcolor, records)
    by_stem = {str(record["stem"]): index for index, record in enumerate(records)}
    if stem not in by_stem:
        raise ValueError(f"GT2 carinfo target is absent: {stem}")
    target_index = by_stem[stem]
    target = records[target_index]
    old_count = len(target["colorIds"])

    new_main_colors = list(target["mainColors"])
    new_color_ids = list(target["colorIds"])
    new_color_names = list(colors[target_index])
    appended: list[dict[str, object]] = []
    for color_id, source_stem in paint_sources:
        if color_id in new_color_ids and not allow_duplicate_color_ids:
            raise ValueError(
                f"GT2 {stem} already exposes color ID {color_id}"
            )
        if source_stem not in by_stem:
            raise ValueError(f"GT2 paint source is absent: {source_stem}")
        source_index = by_stem[source_stem]
        source = records[source_index]
        try:
            source_color = source["colorIds"].index(color_id)
        except ValueError as exc:
            raise ValueError(
                f"GT2 paint source {source_stem} lacks ID {color_id}"
            ) from exc
        main_color = source["mainColors"][source_color]
        color_name = colors[source_index][source_color]
        new_color_ids.append(color_id)
        new_main_colors.append(main_color)
        new_color_names.append(color_name)
        appended.append(
            {
                "colorId": color_id,
                "sourceStem": source_stem,
                "mainColor": main_color,
                "colorNameIndex": color_name,
            }
        )

    if len(new_color_ids) > 32:
        raise ValueError(
            f"GT2 {stem} color count exceeds its five-bit field"
        )
    target["mainColors"] = tuple(new_main_colors)
    target["colorIds"] = tuple(new_color_ids)
    colors[target_index] = tuple(new_color_names)

    info = bytearray(b"CAR\0" + struct.pack("<I", len(records)))
    info.extend(b"\0" * (len(records) * 8))
    colour = bytearray(b"CCOL00\0\0")
    colour.extend(b"\0" * (len(records) * 2))
    for index, (record, name_indices) in enumerate(zip(records, colors)):
        while len(info) & 1:
            info.append(0)
        info_offset = len(info)
        count = len(record["colorIds"])
        info.extend(struct.pack(f"<{count}H", *record["mainColors"]))
        info.extend(bytes(record["colorIds"]))
        name = bytes(record["name"])
        info.append(len(name))
        info.extend(name + b"\0")
        encoded = (
            info_offset
            | ((count - 1) << 18)
            | int(record["flags"])
        )
        struct.pack_into(
            "<II",
            info,
            8 + index * 8,
            int(record["carId"]),
            encoded,
        )

        while len(colour) & 1:
            colour.append(0)
        colour_offset = len(colour)
        if colour_offset > 0xFFFF:
            raise ValueError("GT2 carcolor escaped its 16-bit offset range")
        struct.pack_into("<H", colour, 8 + index * 2, colour_offset)
        colour.extend(struct.pack(f"<{count}H", *name_indices))

    verified = _parse_gt2_carinfo(bytes(info))
    verified_colors = _parse_gt2_carcolor(bytes(colour), verified)
    verified_target = next(
        record for record in verified if record["stem"] == stem
    )
    if list(verified_target["colorIds"]) != new_color_ids:
        raise ValueError(f"GT2 {stem} extended colors failed round-trip")
    verified_index = next(
        index
        for index, record in enumerate(verified)
        if record["stem"] == stem
    )
    if list(verified_colors[verified_index]) != new_color_names:
        raise ValueError(f"GT2 {stem} color names failed round-trip")
    return bytes(info), bytes(colour), {
        "targetStem": stem,
        "firstColorIndex": old_count,
        "colorCount": len(appended),
        "appended": appended,
        "finalColorIds": new_color_ids,
    }


GT2_LOCALIZED_CARINFO_DATABASES = (
    ".carinfoa",
    ".carinfoe",
    ".carinfoj",
)


def update_gt2_localized_carinfo(
    gt2_volume: Path,
    patch_root: Path,
    transform,
) -> dict[str, object]:
    """Apply one structural car-info edit to every native locale database.

    The US executables query `.carinfoa`; `.carinfoe` had historically been
    the converter's only target and therefore produced correct offline proofs
    that the live game could not see. All three archives contain the same
    ordered car identities and share one `.carcolor` table, so each localized
    car-info stream must be transformed from the same pre-edit color table.
    """

    staged_carcolor = patch_root / ".carcolor"
    source_carcolor = (
        staged_carcolor.read_bytes()
        if staged_carcolor.is_file()
        else read_gt2_member(gt2_volume, ".carcolor")
    )
    transformed_color: bytes | None = None
    metadata_by_locale: dict[str, dict[str, object]] = {}
    digests: dict[str, str] = {}
    for name in GT2_LOCALIZED_CARINFO_DATABASES:
        staged_carinfo = patch_root / name
        source_carinfo = (
            staged_carinfo.read_bytes()
            if staged_carinfo.is_file()
            else read_gt2_member(gt2_volume, name)
        )
        carinfo, carcolor, metadata = transform(
            source_carinfo, source_carcolor
        )
        if transformed_color is None:
            transformed_color = carcolor
        elif carcolor != transformed_color:
            raise ValueError(
                "GT2 localized car-info edits produced different shared "
                f"carcolor tables at {name}"
            )
        staged_carinfo.write_bytes(carinfo)
        metadata_by_locale[name] = metadata
        digests[name] = hashlib.sha256(carinfo).hexdigest()

    if transformed_color is None:
        raise AssertionError("GT2 localized car-info list is empty")
    staged_carcolor.write_bytes(transformed_color)
    canonical = metadata_by_locale[".carinfoa"]
    for name, metadata in metadata_by_locale.items():
        if metadata != canonical:
            raise ValueError(
                "GT2 localized car-info metadata diverged at "
                f"{name}: {metadata!r} != {canonical!r}"
            )
    return {
        **canonical,
        "localizedCarinfoSha256": digests,
        "carcolorSha256": hashlib.sha256(transformed_color).hexdigest(),
    }


def _gt2_livery_table_records(
    folds: list[dict[str, object]],
) -> list[dict[str, object]]:
    """Return alternate-body records plus required identity disambiguators."""

    records = [
        {
            "targetStem": str(fold["targetStem"]),
            "bodyStem": str(fold["bodyStem"]),
            **mapping,
        }
        for fold in folds
        for mapping in list(fold["bodyMappings"])
    ]

    # A reused color ID is ambiguous even when only one of its occurrences
    # needs an alternate body. Emit an explicit identity record for every
    # otherwise-unmapped occurrence so the runtime can detect that ID-only
    # lookup is unsafe. Palette-index lookup remains exact.
    final_ids_by_target: dict[str, list[int]] = {}
    mapped_indices_by_target: dict[str, set[int]] = {}
    for fold in folds:
        target_stem = str(fold["targetStem"])
        final_ids = [int(item) for item in fold["finalColorIds"]]
        if len(final_ids) >= len(final_ids_by_target.get(target_stem, [])):
            final_ids_by_target[target_stem] = final_ids
        mapped_indices_by_target.setdefault(target_stem, set()).update(
            int(mapping["targetColorIndex"])
            for mapping in fold["bodyMappings"]
        )
    for target_stem, final_ids in final_ids_by_target.items():
        counts = {
            color_id: final_ids.count(color_id)
            for color_id in set(final_ids)
        }
        mapped_indices = mapped_indices_by_target.get(target_stem, set())
        for palette_index, color_id in enumerate(final_ids):
            if counts[color_id] <= 1 or palette_index in mapped_indices:
                continue
            records.append(
                {
                    "targetStem": target_stem,
                    "bodyStem": target_stem,
                    "colorId": color_id,
                    "targetColorIndex": palette_index,
                    "bodyPaletteIndex": palette_index,
                    "identityDisambiguator": True,
                }
            )
    return records


def build_gt2_livery_body_table(
    folds: list[dict[str, object]],
) -> bytes:
    """Serialize the data-driven alternate native body selection table."""

    mappings = _gt2_livery_table_records(folds)
    if len(mappings) > 0xFFFF:
        raise ValueError("too many GT2 livery body mappings")
    output = bytearray(struct.pack("<4sHH", b"GTLV", 3, len(mappings)))
    seen: set[tuple[int, int]] = set()
    for mapping in mappings:
        target_id = encode_gt2_car_id(str(mapping["targetStem"]))
        body_id = encode_gt2_car_id(str(mapping["bodyStem"]))
        target_color = int(mapping["targetColorIndex"])
        body_palette = int(mapping["bodyPaletteIndex"])
        if not 0 <= target_color <= 0xFF or not 0 <= body_palette <= 0xFF:
            raise ValueError("GT2 livery color mapping is invalid")
        key = (target_id, target_color)
        if key in seen:
            raise ValueError("duplicate GT2 livery body mapping")
        seen.add(key)
        output.extend(
            struct.pack(
                "<IIBBH",
                target_id,
                body_id,
                target_color,
                body_palette,
                int(mapping["colorId"]),
            )
        )
    return bytes(output)


def normalize_arcade_car_logo(data: bytes) -> bytes:
    if len(data) < 64 or struct.unpack_from("<II", data, 0) != (0x10, 8):
        raise ValueError("GT1 Arcade car logo is not a 4-bit TIM")
    clut_size = struct.unpack_from("<I", data, 8)[0]
    image_offset = 8 + clut_size
    image_size, _, _, width_words, height = struct.unpack_from(
        "<IHHHH", data, image_offset
    )
    if image_offset + image_size != len(data) or width_words * 4 > 256:
        raise ValueError("GT1 Arcade car logo TIM is malformed")
    output = bytearray(data)
    struct.pack_into("<HH", output, 12, 0, 0)
    struct.pack_into("<HH", output, image_offset + 4, 0, 0)
    return bytes(output)


def stage_gt2_gtmode_car_logos(
    disc_root: Path,
    patch_root: Path,
    definition: dict[str, object],
    stock_stem: str,
    race_stem: str,
) -> dict[str, object]:
    """Install the archive-authored GT1 wordmark in every GT2 locale slot.

    GT2 stores two UI-context variants for each of three regional name
    groups (`l` through `q`). The supplied GT1 disc is the US release, so its
    one authoritative wordmark is used unchanged in every slot rather than
    synthesizing text or borrowing a visually incorrect GT2 donor logo.
    """

    source_logo, source = read_gt1_definition_car_logo(
        disc_root, definition
    )
    logo = normalize_arcade_car_logo(source_logo)
    clut_size = struct.unpack_from("<I", logo, 8)[0]
    image_offset = 8 + clut_size
    _, _, _, width_words, height = struct.unpack_from(
        "<IHHHH", logo, image_offset
    )
    width = width_words * 4
    if width > 256 or height > 64:
        raise ValueError(
            f"GT1 {stock_stem} wordmark is {width}x{height}, outside "
            "GT2's native car-logo envelope"
        )

    logo_root = patch_root / "carlogo"
    logo_root.mkdir(parents=True, exist_ok=True)
    members: list[str] = []
    for stem in (stock_stem, race_stem):
        for variant in "lmnopq":
            name = f"{stem}{variant}--.tim"
            (logo_root / name).write_bytes(logo)
            members.append(f"carlogo/{name}")
    digest = hashlib.sha256(logo).hexdigest()
    if any(
        hashlib.sha256((patch_root / member).read_bytes()).hexdigest()
        != digest
        for member in members
    ):
        raise ValueError(
            f"GT2 {stock_stem} car-logo staging failed round-trip"
        )
    return {
        **source,
        "width": width,
        "height": height,
        "sha256": digest,
        "members": members,
        "sourcePixelsPreserved": True,
        "synthesized": False,
    }


def append_arcade_car_logo(
    archive: bytes,
    logo: bytes,
) -> tuple[bytes, int]:
    if len(archive) < 8:
        raise ValueError("GT2 arc_carlogo is truncated")
    count = struct.unpack_from("<I", archive, 0)[0]
    if 4 + count * 4 > len(archive):
        raise ValueError("GT2 arc_carlogo offset table is truncated")
    offsets = list(struct.unpack_from(f"<{count}I", archive, 4))
    members = [
        archive[offsets[index] : (
            offsets[index + 1] if index + 1 < count else len(archive)
        )]
        for index in range(count)
    ]
    for index, member in enumerate(members):
        if member == logo:
            return archive, index
    members.append(logo)
    output = bytearray(struct.pack("<I", len(members)))
    output.extend(b"\0" * (len(members) * 4))
    for index, member in enumerate(members):
        struct.pack_into("<I", output, 4 + index * 4, len(output))
        output.extend(member)
    return bytes(output), count


def read_gt1_spec_data(carinf_path: Path) -> bytes:
    outer = carinf_path.read_bytes()
    if outer[:10] != b"@(#)GT-ARC":
        outer = gt1_lzss_decompress(outer)
    if outer[:10] != b"@(#)GT-ARC":
        raise ValueError("GT1 CARINF.DAT did not expand to GT-ARC")
    count = struct.unpack_from("<H", outer, 14)[0] & 0x7FFF
    if count <= 13:
        raise ValueError("GT1 CARINF.DAT has no SPEC member")
    offset, packed_size, unpacked_size = struct.unpack_from(
        "<III", outer, 16 + 13 * 12
    )
    packed = outer[offset : offset + packed_size]
    spec_data = (
        packed
        if packed_size == unpacked_size
        else gt1_lzss_decompress(packed, unpacked_size)
    )
    if spec_data[4:12].rstrip(b"\0") != b"SPEC":
        raise ValueError("GT1 CARINF.DAT member 13 is not SPEC")
    return spec_data


def read_gt1_spec_records(carinf_path: Path) -> dict[str, bytes]:
    spec_data = read_gt1_spec_data(carinf_path)
    spec_count, record_size = (
        struct.unpack_from("<H", spec_data, 14)[0],
        struct.unpack_from("<I", spec_data, 20)[0],
    )
    if record_size != 0x1A8:
        raise ValueError(f"unsupported GT1 SPEC size: {record_size:#x}")
    records: dict[str, bytes] = {}
    for index in range(spec_count):
        record = spec_data[
            24 + index * record_size : 24 + (index + 1) * record_size
        ]
        stem = record[:5].decode("ascii")
        if stem in records:
            raise ValueError(f"duplicate GT1 SPEC record: {stem}")
        records[stem] = record
    return records


def read_gt1_spec_identity(
    carinf_path: Path,
    stem: str,
) -> dict[str, object]:
    spec_data = read_gt1_spec_data(carinf_path)
    spec_count, record_size = (
        struct.unpack_from("<H", spec_data, 14)[0],
        struct.unpack_from("<I", spec_data, 20)[0],
    )
    records_end = 24 + spec_count * record_size
    table_count = struct.unpack_from("<I", spec_data, records_end)[0]
    cursor = records_end + 4 + table_count * 4
    tables: list[list[str]] = []
    for _ in range(table_count):
        string_count = struct.unpack_from("<H", spec_data, cursor)[0]
        cursor += 2
        strings: list[str] = []
        for _ in range(string_count):
            length = spec_data[cursor]
            cursor += 1
            raw = spec_data[cursor : cursor + length]
            cursor += length
            if cursor >= len(spec_data) or spec_data[cursor] != 0:
                raise ValueError("GT1 SPEC string is not null terminated")
            cursor += 1
            strings.append(raw.decode("cp932"))
        if cursor & 1:
            cursor += 1
        tables.append(strings)
    if len(tables) != 2:
        raise ValueError("GT1 SPEC string-table count changed")

    try:
        record = read_gt1_spec_records(carinf_path)[stem]
    except KeyError as exc:
        raise ValueError(f"GT1 SPEC has no record for {stem}") from exc
    first_index, first_table, second_index, second_table = (
        struct.unpack_from("<4H", record, 0x188)
    )
    if first_table >= len(tables) or second_table >= len(tables):
        raise ValueError(f"GT1 SPEC {stem} has invalid name tables")
    parts = (
        tables[first_table][first_index],
        tables[second_table][second_index],
    )
    price = struct.unpack_from("<I", record, 0x184)[0]
    return {
        "stem": stem,
        "nameParts": parts,
        "displayName": " ".join(part for part in parts if part),
        "sourcePrice": price,
        "gt2Price": price // 100,
        "recordSha256": hashlib.sha256(record).hexdigest(),
    }


def read_gt1_spec_stats(
    carinf_path: Path,
    stem: str,
) -> tuple[int, int, int, int, int]:
    try:
        record = read_gt1_spec_records(carinf_path)[stem]
    except KeyError as exc:
        raise ValueError(f"GT1 SPEC has no record for {stem}") from exc
    power, power_rpm, torque, torque_rpm = struct.unpack_from(
        "<4H", record, 0x198
    )
    # GT1 stores kgf-m * 100; GT2's Arcade stat panel stores whole N-m.
    torque_nm = round(torque * 9.80665 / 100)
    weight = struct.unpack_from("<H", record, 0x5A)[0]
    return power, power_rpm, torque_nm, torque_rpm, weight


def _parse_gtdt_blocks(data: bytes, expected_count: int) -> list[bytes]:
    if data[:6] != b"GTDTl\0":
        raise ValueError("GT2 parameter database header is missing")
    index_count = struct.unpack_from("<H", data, 6)[0]
    if index_count != expected_count * 2:
        raise ValueError(
            f"unexpected GT2 parameter index count: "
            f"{index_count} != {expected_count * 2}"
        )
    header_end = 8 + index_count * 8
    blocks: list[bytes] = []
    previous_end = header_end
    for index in range(expected_count):
        start, size = struct.unpack_from("<II", data, 8 * (index + 1))
        if start != previous_end or start + size > len(data):
            raise ValueError(
                f"GT2 parameter block {index} is not contiguous: "
                f"{start:#x}+{size:#x}, expected {previous_end:#x}"
            )
        blocks.append(data[start : start + size])
        previous_end = start + size
    return blocks


def _rebuild_arcade_gtdt(data: bytes, blocks: list[bytes]) -> bytes:
    if len(blocks) != GT2_ARCADE_BLOCK_COUNT:
        raise ValueError("GT2 Arcade parameter block count changed")
    original = _parse_gtdt_blocks(data, GT2_ARCADE_BLOCK_COUNT)
    old_tail = 8 + struct.unpack_from("<H", data, 6)[0] * 8
    for block in original:
        old_tail += len(block)
    string_start, string_size = struct.unpack_from(
        "<II", data, GT2_ARCADE_STRING_INDEX_POSITION
    )
    if string_start != old_tail or string_start + string_size != len(data):
        raise ValueError("GT2 Arcade string database is not the final payload")

    header_size = 8 + GT2_ARCADE_BLOCK_COUNT * 2 * 8
    output = bytearray(data[:header_size])
    for index, block in enumerate(blocks):
        start = len(output)
        output.extend(block)
        struct.pack_into(
            "<II", output, 8 * (index + 1), start, len(block)
        )
    struct.pack_into(
        "<II",
        output,
        GT2_ARCADE_STRING_INDEX_POSITION,
        len(output),
        string_size,
    )
    output.extend(data[string_start:])
    return bytes(output)


def _rebuild_gtmode_gtdt(data: bytes, blocks: list[bytes]) -> bytes:
    if len(blocks) != GT2_GTMODE_BLOCK_COUNT:
        raise ValueError("GT2 GT Mode parameter block count changed")
    _parse_gtdt_blocks(data, GT2_GTMODE_BLOCK_COUNT)
    header_size = 8 + struct.unpack_from("<H", data, 6)[0] * 8
    output = bytearray(data[:header_size])
    for index, block in enumerate(blocks):
        start = len(output)
        output.extend(block)
        struct.pack_into(
            "<II", output, 8 * (index + 1), start, len(block)
        )
    return bytes(output)


def parse_gt2_unistrdb(data: bytes) -> list[str]:
    if (
        len(data) < 10
        or struct.unpack_from("<I", data, 0)[0] != len(data)
        or data[4:8] != b"WSDB"
    ):
        raise ValueError("GT2 Unicode string database header is invalid")
    count = struct.unpack_from("<H", data, 8)[0]
    cursor = 10
    strings: list[str] = []
    for index in range(count):
        if cursor + 2 > len(data):
            raise ValueError(
                f"GT2 Unicode string {index} has no length"
            )
        length = struct.unpack_from("<H", data, cursor)[0]
        cursor += 2
        end = cursor + length * 2
        if (
            end + 2 > len(data)
            or data[end : end + 2] != b"\0\0"
        ):
            raise ValueError(
                f"GT2 Unicode string {index} is truncated"
            )
        strings.append(data[cursor:end].decode("utf-16le"))
        cursor = end + 2
    if cursor != len(data):
        raise ValueError("GT2 Unicode string database has trailing bytes")
    return strings


def append_gt2_unistrdb_strings(
    data: bytes,
    additions: tuple[str, ...],
) -> tuple[bytes, tuple[int, ...]]:
    strings = parse_gt2_unistrdb(data)
    indices: list[int] = []
    for addition in additions:
        try:
            index = strings.index(addition)
        except ValueError:
            index = len(strings)
            strings.append(addition)
        indices.append(index)
    if len(strings) > 0xFFFF:
        raise ValueError("GT2 Unicode string database exceeds 16-bit IDs")
    output = bytearray(b"\0\0\0\0WSDB")
    output.extend(struct.pack("<H", len(strings)))
    for value in strings:
        encoded = value.encode("utf-16le")
        length = len(encoded) // 2
        if length > 0xFFFF:
            raise ValueError("GT2 Unicode string is too long")
        output.extend(struct.pack("<H", length))
        output.extend(encoded)
        output.extend(b"\0\0")
    struct.pack_into("<I", output, 0, len(output))
    verified = parse_gt2_unistrdb(bytes(output))
    if tuple(verified[index] for index in indices) != additions:
        raise ValueError("GT2 Unicode string insertion failed round-trip")
    return bytes(output), tuple(indices)


def append_gt2_gtmode_car(
    gtmode_data: bytes,
    definition: dict[str, object],
    name_indices: tuple[int, int],
    price: int,
) -> tuple[bytes, dict[str, object]]:
    """Clone a complete GT2 upgrade family into one distinct GT1 identity."""

    blocks = _parse_gtdt_blocks(gtmode_data, GT2_GTMODE_BLOCK_COUNT)
    target_stem = str(definition["stem"])
    target_id = encode_gt2_car_id(target_stem)
    primary_stem = str(definition["physicsBasisStem"])
    primary_id = encode_gt2_car_id(primary_stem)
    target_race_stem = str(
        definition.get("gtModeRaceStem", target_stem[:4] + "r")
    )
    target_race_id = encode_gt2_car_id(target_race_stem)
    part_basis = {
        int(block): str(stem)
        for block, stem in dict(definition["physicsPartBasis"]).items()
    }
    u16_overrides = {
        int(block): {
            int(offset): int(value)
            for offset, value in dict(overrides).items()
        }
        for block, overrides in dict(
            definition.get("physicsU16Overrides", {})
        ).items()
    }

    car_block = blocks[GT2_GTMODE_CAR_BLOCK]
    if len(car_block) % 0x48:
        raise ValueError("GT2 GT Mode car block is malformed")
    cars = [
        bytearray(car_block[offset : offset + 0x48])
        for offset in range(0, len(car_block), 0x48)
    ]
    car_ids = [struct.unpack_from("<I", car, 0)[0] for car in cars]
    if car_ids != sorted(car_ids):
        raise ValueError("GT2 GT Mode cars are not ID-sorted")
    if target_id in car_ids:
        raise ValueError(f"GT2 GT Mode already contains {target_stem}")
    try:
        primary_index = car_ids.index(primary_id)
    except ValueError as exc:
        raise ValueError(
            f"GT2 GT Mode basis is absent: {primary_stem}"
        ) from exc

    target_refs = [0] * len(GT2_GTD_CAR_REF_BLOCKS)
    updated_blocks = list(blocks)
    cloned_counts: list[int] = []
    source_owners: list[str] = []
    for block_index, record_size in enumerate(GT2_GTD_PART_RECORD_SIZES):
        block = blocks[block_index]
        if len(block) % record_size:
            raise ValueError(
                f"GT2 part block {block_index} is malformed"
            )
        original = [
            bytearray(block[offset : offset + record_size])
            for offset in range(0, len(block), record_size)
        ]
        owners = [
            struct.unpack_from("<I", record, 0)[0]
            for record in original
        ]
        basis_stem = part_basis.get(block_index, primary_stem)
        basis_id = encode_gt2_car_id(basis_stem)
        basis_car = _find_gt2_gtdt_car(car_block, 0x48, basis_stem)
        reference_index = GT2_GTD_BLOCK_TO_CAR_REF[block_index]
        source_ref = struct.unpack_from(
            "<H", basis_car, 4 + reference_index * 2
        )[0]
        if source_ref >= len(original):
            raise ValueError(
                f"GT2 {basis_stem} part {block_index} is out of range"
            )

        clones: list[tuple[int, bytearray]] = []
        for old_index, record in enumerate(original):
            if owners[old_index] != basis_id:
                continue
            clone = bytearray(record)
            struct.pack_into("<I", clone, 0, target_id)
            for field_offset, value in u16_overrides.get(
                block_index, {}
            ).items():
                if (
                    field_offset < 4
                    or field_offset + 2 > record_size
                    or field_offset & 1
                    or not 0 <= value <= 0xFFFF
                ):
                    raise ValueError(
                        f"invalid GT2 part override: block {block_index}, "
                        f"offset {field_offset:#x}, value {value}"
                    )
                struct.pack_into("<H", clone, field_offset, value)
            if block_index == 5:
                body_id = struct.unpack_from("<I", clone, 8)[0]
                basis_race_id = encode_gt2_car_id(
                    basis_stem[:4] + "r"
                )
                if body_id == basis_id:
                    struct.pack_into("<I", clone, 8, target_id)
                elif body_id == basis_race_id:
                    struct.pack_into(
                        "<I", clone, 8, target_race_id
                    )
                else:
                    raise ValueError(
                        f"GT2 {basis_stem} RacingModify body "
                        f"{decode_gt2_car_id(body_id)} is not its stock "
                        "or racing body"
                    )
            clones.append((old_index, clone))

        combined = [
            ("old", index, record)
            for index, record in enumerate(original)
        ] + [
            ("clone", source_index, record)
            for source_index, record in clones
        ]
        combined.sort(
            key=lambda item: struct.unpack_from("<I", item[2], 0)[0]
        )
        old_to_new: dict[int, int] = {}
        clone_to_new: dict[int, int] = {}
        for new_index, (kind, source_index, _) in enumerate(combined):
            if kind == "old":
                old_to_new[source_index] = new_index
            else:
                clone_to_new[source_index] = new_index

        for car in cars:
            old_ref = struct.unpack_from(
                "<H", car, 4 + reference_index * 2
            )[0]
            struct.pack_into(
                "<H",
                car,
                4 + reference_index * 2,
                old_to_new[old_ref],
            )
        target_refs[reference_index] = (
            clone_to_new[source_ref]
            if owners[source_ref] == basis_id
            else old_to_new[source_ref]
        )
        rebuilt = b"".join(bytes(item[2]) for item in combined)
        rebuilt_owners = [
            struct.unpack_from("<I", rebuilt, offset)[0]
            for offset in range(0, len(rebuilt), record_size)
        ]
        if rebuilt_owners != sorted(rebuilt_owners):
            raise ValueError(
                f"GT2 part block {block_index} lost owner ordering"
            )
        updated_blocks[block_index] = rebuilt
        cloned_counts.append(len(clones))
        source_owners.append(basis_stem)

    target_car = bytearray(cars[primary_index])
    struct.pack_into("<I", target_car, 0, target_id)
    struct.pack_into(
        f"<{len(target_refs)}H", target_car, 4, *target_refs
    )
    struct.pack_into("<2H", target_car, 0x3C, *name_indices)
    struct.pack_into("<I", target_car, 0x44, price)
    cars.append(target_car)
    cars.sort(key=lambda car: struct.unpack_from("<I", car, 0)[0])
    updated_blocks[GT2_GTMODE_CAR_BLOCK] = b"".join(
        bytes(car) for car in cars
    )

    output = _rebuild_gtmode_gtdt(gtmode_data, updated_blocks)
    verified = _parse_gtdt_blocks(output, GT2_GTMODE_BLOCK_COUNT)
    verified_car = _find_gt2_gtdt_car(
        verified[GT2_GTMODE_CAR_BLOCK], 0x48, target_stem
    )
    if (
        struct.unpack_from("<2H", verified_car, 0x3C) != name_indices
        or struct.unpack_from("<I", verified_car, 0x44)[0] != price
    ):
        raise ValueError(
            f"GT2 GT Mode {target_stem} identity failed round-trip"
        )
    for block_index, record_size in enumerate(GT2_GTD_PART_RECORD_SIZES):
        owner_count = sum(
            struct.unpack_from("<I", verified[block_index], offset)[0]
            == target_id
            for offset in range(
                0, len(verified[block_index]), record_size
            )
        )
        if owner_count != cloned_counts[block_index]:
            raise ValueError(
                f"GT2 GT Mode {target_stem} part family "
                f"{block_index} failed round-trip"
            )
    return output, {
        "stem": target_stem,
        "basisStem": primary_stem,
        "raceStem": target_race_stem,
        "nameIndices": list(name_indices),
        "price": price,
        "manufacturerId": struct.unpack_from(
            "<H", verified_car, 0x3A
        )[0],
        "year": verified_car[0x41],
        "partSources": source_owners,
        "ownedPartRecordCounts": cloned_counts,
        "databaseSize": len(output),
        "uncompressedSha256": hashlib.sha256(output).hexdigest(),
    }


def _find_gt2_gtdt_car(
    block: bytes,
    record_size: int,
    stem: str,
) -> bytes:
    if len(block) % record_size:
        raise ValueError(
            f"GT2 car block size {len(block):#x} is not a multiple of "
            f"{record_size:#x}"
        )
    car_id = encode_gt2_car_id(stem)
    matches = [
        block[offset : offset + record_size]
        for offset in range(0, len(block), record_size)
        if struct.unpack_from("<I", block, offset)[0] == car_id
    ]
    if len(matches) != 1:
        raise ValueError(
            f"expected one GT2 parameter record for {stem}, "
            f"found {len(matches)}"
        )
    return matches[0]


def _validate_gt1_arcade_roadster_physics(
    carinf_path: Path,
    definition: dict[str, object],
) -> dict[str, object]:
    records = read_gt1_spec_records(carinf_path)
    target_stem = str(definition["stem"])
    spec_stem = str(definition.get("physicsSpecStem", target_stem))
    primary_stem = str(definition["physicsBasisStem"])
    part_basis = dict(definition["physicsPartBasis"])
    brake_stem = str(part_basis.get(0, primary_stem))
    tire_stem = str(part_basis.get(22, primary_stem))
    try:
        target = records[spec_stem]
        primary = records[primary_stem]
        brake = records[brake_stem]
        tire = records[tire_stem]
    except KeyError as exc:
        raise ValueError(f"GT1 physics basis is absent: {exc.args[0]}") from exc

    # These are the complete known difference fingerprints against each
    # closest GT1 road-car SPEC. They prove the Arcade composites have not
    # silently changed before their GT2-native basis records are selected.
    expected_by_stem = {
        "a-ian": {
            0x01,
            0x03,
            0x60,
            0x61,
            0x63,
            0x67,
            0x69,
            0x6B,
            0x184,
            0x185,
            0x186,
            0x18C,
            0x190,
            0x192,
        },
        "amian": {
            0x03,
            0x66,
            0x68,
            0x6A,
            0x184,
            0x185,
            0x186,
            0x18C,
        },
    }
    differences = {
        index
        for index, (target_value, basis_value) in enumerate(
            zip(target, primary)
        )
        if target_value != basis_value
    }
    explicit_differences = definition.get("physicsExpectedDifferences")
    if explicit_differences is not None:
        expected_differences = {
            int(offset) for offset in explicit_differences
        }
        if differences != expected_differences:
            raise ValueError(
                "GT1 car SPEC fingerprint changed: "
                f"{sorted(differences)} != {sorted(expected_differences)}"
            )
    elif spec_stem == primary_stem:
        if differences:
            raise ValueError("GT1 direct physics basis no longer matches")
    else:
        if target_stem not in expected_by_stem:
            raise ValueError(
                f"no GT1 Arcade Roadster fingerprint for {target_stem}"
            )
        expected_differences = expected_by_stem[target_stem]
        if differences != expected_differences:
            raise ValueError(
                "GT1 Arcade Roadster SPEC fingerprint changed: "
                f"{sorted(differences)}"
            )

    brake_signature = (target[0x60], target[0x61], target[0x63])
    if brake_signature != (brake[0x60], brake[0x61], brake[0x63]):
        raise ValueError("GT1 Arcade Roadster brake basis no longer matches")
    tire_signature = bytes(target[0x66:0x6C])
    # `amian` is the one deliberate exception: GT1 widened its front tire to
    # match the rear. GT2's native `amisn` conversion already uses that exact
    # 15/195/50 size-table entry on both axles, so its serialized GT2 tire
    # records are the desired target despite the older SPEC difference.
    if target_stem != "amian" and tire_signature != tire[0x66:0x6C]:
        raise ValueError("GT1 Arcade Roadster tire basis no longer matches")
    if target[0x6C:0x184] != primary[0x6C:0x184]:
        raise ValueError(
            "GT1 Arcade Roadster suspension/chassis basis no longer matches"
        )
    u16_overrides = dict(definition.get("physicsU16Overrides", {}))
    if u16_overrides:
        chassis_overrides = dict(u16_overrides.get(3, {}))
        target_weight = struct.unpack_from("<H", target, 0x5A)[0]
        if int(chassis_overrides.get(0x0E, -1)) != target_weight:
            raise ValueError(
                f"GT1 {target_stem} chassis override does not preserve "
                f"its {target_weight} kg specification"
            )
    return {
        "specStem": spec_stem,
        "primaryStem": primary_stem,
        "brakeStem": brake_stem,
        "tireStem": tire_stem,
        "brakeSignature": list(brake_signature),
        "tireSignature": list(tire_signature),
        "sourceSpecSha256": hashlib.sha256(target).hexdigest(),
    }


def append_gt2_arcade_car_physics(
    gtmode_data: bytes,
    arcade_data: bytes,
    definition: dict[str, object],
) -> tuple[bytes, dict[str, object]]:
    gtmode_blocks = _parse_gtdt_blocks(
        gtmode_data, GT2_GTMODE_BLOCK_COUNT
    )
    arcade_blocks = _parse_gtdt_blocks(
        arcade_data, GT2_ARCADE_BLOCK_COUNT
    )
    target_stem = str(definition["stem"])
    target_id = encode_gt2_car_id(target_stem)
    primary_stem = str(definition["physicsBasisStem"])
    part_basis = {
        int(block): str(stem)
        for block, stem in dict(definition["physicsPartBasis"]).items()
    }
    u16_overrides = {
        int(block): {
            int(offset): int(value)
            for offset, value in dict(overrides).items()
        }
        for block, overrides in dict(
            definition.get("physicsU16Overrides", {})
        ).items()
    }

    racing = arcade_blocks[GT2_ARCADE_RACING_BLOCK]
    drift = arcade_blocks[GT2_ARCADE_DRIFT_BLOCK]
    for block, label in ((racing, "racing"), (drift, "drift")):
        if any(
            struct.unpack_from("<I", block, offset)[0] == target_id
            for offset in range(0, len(block), 0x3C)
        ):
            raise ValueError(
                f"GT2 Arcade {label} data already contains {target_stem}"
            )

    primary_car = _find_gt2_gtdt_car(
        gtmode_blocks[GT2_GTMODE_CAR_BLOCK],
        0x48,
        primary_stem,
    )
    _, _, rims_code = struct.unpack_from("<3H", primary_car, 0x34)
    unknown = struct.unpack_from("<H", primary_car, 0x42)[0]
    if rims_code > 15:
        raise ValueError(f"invalid GT2 wheel code for {primary_stem}")
    # Arcade normally disables the two GT Mode driver-aid references. The
    # stock roster contains one deliberate four-wheel-steering exception;
    # this conventional Roadster uses the standard zero pair.
    auxiliary = (0, 0, 15 - rims_code, unknown)

    # Cross-check the otherwise undocumented final four Arcade fields against
    # every GT Mode car also present in the stock Arcade database.
    gtmode_by_id = {
        struct.unpack_from("<I", record, 0)[0]: record
        for record in (
            gtmode_blocks[GT2_GTMODE_CAR_BLOCK][
                offset : offset + 0x48
            ]
            for offset in range(
                0, len(gtmode_blocks[GT2_GTMODE_CAR_BLOCK]), 0x48
            )
        )
    }
    auxiliary_checks = 0
    for offset in range(0, len(racing), 0x3C):
        arcade_car = racing[offset : offset + 0x3C]
        car_id = struct.unpack_from("<I", arcade_car, 0)[0]
        if car_id not in gtmode_by_id:
            continue
        gtmode_car = gtmode_by_id[car_id]
        _, _, gt_rims = struct.unpack_from(
            "<3H", gtmode_car, 0x34
        )
        gt_unknown = struct.unpack_from("<H", gtmode_car, 0x42)[0]
        actual = struct.unpack_from("<4H", arcade_car, 0x34)
        expected_tail = (15 - gt_rims, gt_unknown)
        if actual[2:] != expected_tail:
            raise ValueError(
                "GT2 Arcade auxiliary-field relationship changed for "
                f"{decode_gt2_car_id(car_id)}: "
                f"{actual[2:]} != {expected_tail}"
            )
        auxiliary_checks += 1
    if auxiliary_checks < 40:
        raise ValueError(
            f"too few GT2 Arcade auxiliary checks: {auxiliary_checks}"
        )

    updated = [bytearray(block) for block in arcade_blocks]
    racing_refs: list[int] = []
    drift_refs: list[int] = []
    block_sources: dict[str, str] = {}
    reused_part_records = 0
    new_part_records = 0

    def intern_part_record(
        block_index: int,
        record_size: int,
        part: bytearray,
    ) -> int:
        """Reuse GT2-identical parts, ignoring the owning car identifier.

        Stock GT2 Arcade cars deliberately share most physical part records;
        their CarArcade references routinely point at a record owned by a
        different car. Only the bytes after the four-byte identifier define
        the consumed part. Mirroring that layout avoids needless database
        growth even with the unified port's expanded native guest arena.
        """

        nonlocal reused_part_records, new_part_records
        block = updated[block_index]
        payload = bytes(part[4:])
        for offset in range(0, len(block), record_size):
            if block[offset + 4 : offset + record_size] == payload:
                reused_part_records += 1
                return offset // record_size
        reference = len(block) // record_size
        block.extend(part)
        new_part_records += 1
        return reference

    for block_index, record_size in enumerate(GT2_GTD_PART_RECORD_SIZES):
        basis_stem = part_basis.get(block_index, primary_stem)
        basis_car = _find_gt2_gtdt_car(
            gtmode_blocks[GT2_GTMODE_CAR_BLOCK],
            0x48,
            basis_stem,
        )
        reference_index = GT2_GTD_BLOCK_TO_CAR_REF[block_index]
        basis_ref = struct.unpack_from(
            "<H", basis_car, 4 + reference_index * 2
        )[0]
        source_block = gtmode_blocks[block_index]
        source_offset = basis_ref * record_size
        if source_offset + record_size > len(source_block):
            raise ValueError(
                f"GT2 {basis_stem} part {block_index} is out of range"
            )
        part = bytearray(
            source_block[source_offset : source_offset + record_size]
        )
        struct.pack_into("<I", part, 0, target_id)
        for field_offset, value in u16_overrides.get(
            block_index, {}
        ).items():
            if (
                field_offset < 4
                or field_offset + 2 > record_size
                or field_offset & 1
                or not 0 <= value <= 0xFFFF
            ):
                raise ValueError(
                    f"invalid GT2 part override: block {block_index}, "
                    f"offset {field_offset:#x}, value {value}"
                )
            struct.pack_into("<H", part, field_offset, value)
        if block_index == 5:
            # Arcade consumes RacingModify.BodyId even for the stock body.
            # Keep the imported car's own model/texture stem through race and
            # replay instead of retaining its GT2 basis car's visual ID.
            struct.pack_into("<I", part, 8, target_id)
        new_ref = intern_part_record(block_index, record_size, part)
        racing_refs.append(new_ref)
        drift_refs.append(new_ref)
        block_sources[str(block_index)] = basis_stem

        if block_index not in (22, 23):
            continue
        drift_part = bytearray(part)
        stage_offset = 8 if block_index == 22 else 4
        drift_part[stage_offset] = 1
        drift_part[stage_offset + 4] = 6 if block_index == 22 else 7
        drift_ref = intern_part_record(
            block_index, record_size, drift_part
        )
        drift_refs[block_index] = drift_ref

    racing_serialized_refs = [
        racing_refs[block_index]
        for block_index in GT2_GTD_CAR_REF_BLOCKS
    ]
    drift_serialized_refs = [
        drift_refs[block_index]
        for block_index in GT2_GTD_CAR_REF_BLOCKS
    ]
    racing_record = struct.pack(
        "<I28H", target_id, *racing_serialized_refs, *auxiliary
    )
    drift_record = struct.pack(
        "<I28H", target_id, *drift_serialized_refs, *auxiliary
    )
    racing_ids = [
        struct.unpack_from("<I", racing, offset)[0]
        for offset in range(0, len(racing), 0x3C)
    ]
    drift_ids = [
        struct.unpack_from("<I", drift, offset)[0]
        for offset in range(0, len(drift), 0x3C)
    ]
    if racing_ids != sorted(racing_ids) or drift_ids != racing_ids:
        raise ValueError("stock GT2 Arcade car-parameter order changed")
    racing_index = bisect.bisect_left(racing_ids, target_id)
    drift_index = bisect.bisect_left(drift_ids, target_id)
    updated[GT2_ARCADE_RACING_BLOCK][
        racing_index * 0x3C : racing_index * 0x3C
    ] = racing_record
    updated[GT2_ARCADE_DRIFT_BLOCK][
        drift_index * 0x3C : drift_index * 0x3C
    ] = drift_record
    output = _rebuild_arcade_gtdt(
        arcade_data, [bytes(block) for block in updated]
    )
    if len(output) > GT2_ARCADE_DATABASE_SAFE_SIZE:
        raise ValueError(
            "converted GT2 Arcade parameter database exceeds its native "
            f"0x{GT2_ARCADE_DATABASE_SAFE_SIZE:X}-byte workspace: "
            f"{len(output)} bytes"
        )
    verified = _parse_gtdt_blocks(output, GT2_ARCADE_BLOCK_COUNT)
    if _find_gt2_gtdt_car(
        verified[GT2_ARCADE_RACING_BLOCK], 0x3C, target_stem
    ) != racing_record:
        raise ValueError("GT2 Arcade racing record failed round-trip")
    if _find_gt2_gtdt_car(
        verified[GT2_ARCADE_DRIFT_BLOCK], 0x3C, target_stem
    ) != drift_record:
        raise ValueError("GT2 Arcade drift record failed round-trip")
    return output, {
        "primaryStem": primary_stem,
        "partBasis": block_sources,
        "u16Overrides": {
            str(block): {
                f"{offset:#x}": value
                for offset, value in sorted(overrides.items())
            }
            for block, overrides in sorted(u16_overrides.items())
        },
        "racingIndex": racing_index,
        "driftIndex": drift_index,
        "racingPartRefs": racing_serialized_refs,
        "driftPartRefs": drift_serialized_refs,
        "racingPartRefsByBlock": racing_refs,
        "driftPartRefsByBlock": drift_refs,
        "auxiliary": list(auxiliary),
        "auxiliaryChecks": auxiliary_checks,
        "reusedPartRecords": reused_part_records,
        "newPartRecords": new_part_records,
        "databaseSize": len(output),
        "databaseSafeSize": GT2_ARCADE_DATABASE_SAFE_SIZE,
        "uncompressedSha256": hashlib.sha256(output).hexdigest(),
    }


def stage_gt2_arcade_car_physics(
    gt2_arcade_volume: Path,
    patch_root: Path,
    definition: dict[str, object],
) -> dict[str, object]:
    source_pairs = (
        ("arcade_data.dat.gz", "gtmode_data.dat.gz"),
        ("usa_arcade_data.dat.gz", "usa_gtmode_data.dat.gz"),
    )
    output_root = patch_root / "carparam"
    output_root.mkdir(parents=True, exist_ok=True)
    metadata: dict[str, object] = {}
    for arcade_name, gtmode_name in source_pairs:
        arcade_path = f"carparam/{arcade_name}"
        gtmode_path = f"carparam/{gtmode_name}"
        staged_arcade = output_root / arcade_name
        arcade_data = (
            gzip.decompress(staged_arcade.read_bytes())
            if staged_arcade.is_file()
            else read_gt2_gzip_member(gt2_arcade_volume, arcade_path)
        )
        converted, details = append_gt2_arcade_car_physics(
            read_gt2_gzip_member(gt2_arcade_volume, gtmode_path),
            arcade_data,
            definition,
        )
        (output_root / arcade_name).write_bytes(
            gzip.compress(converted, compresslevel=9, mtime=0)
        )
        metadata[arcade_name] = details
    return metadata


def stage_gt2_arcade_livery_smoke_car(
    disc_root: Path,
    gt2_arcade_volume: Path,
    patch_root: Path,
    stem: str,
) -> dict[str, object]:
    """Expose one existing GT2 identity in Arcade for livery visual proof.

    This developer-only entry adds no car identity or body. It reuses the
    target's native GT Mode physics and the livery layer's exact car-info and
    body packages so the ordinary Arcade selector can render and cycle every
    authored choice.
    """

    definition = {
        "stem": stem,
        **GT1_ARCADE_LIVERY_SMOKE_CARS[stem],
    }
    source_logo = read_gt1_menu_car_logo(
        disc_root, str(definition["menuLogoName"])
    )
    logo = normalize_arcade_car_logo(source_logo)
    arcade_output = patch_root / "arcade"
    arcade_output.mkdir(parents=True, exist_ok=True)
    staged_logos = arcade_output / "arc_carlogo"
    merged_logos, logo_index = append_arcade_car_logo(
        (
            staged_logos.read_bytes()
            if staged_logos.is_file()
            else read_gt2_member(
                gt2_arcade_volume, "arcade/arc_carlogo"
            )
        ),
        logo,
    )
    if len(merged_logos) > GT2_ARCADE_FRONTEND_ARCHIVE_SAFE_SIZE:
        raise ValueError(
            "GT2 Arcade livery-smoke logo archive exceeds its "
            "reserved frontend arena"
        )
    staged_logos.write_bytes(merged_logos)

    stats = read_gt1_spec_stats(
        disc_root / "CARINF.DAT", stem
    )
    arcade_physics = stage_gt2_arcade_car_physics(
        gt2_arcade_volume, patch_root, definition
    )
    return {
        "stem": stem,
        "displayName": definition["displayName"],
        "arcadeLogoIndex": logo_index,
        "arcadeLogoSha256": hashlib.sha256(logo).hexdigest(),
        "arcadeClass": definition["arcadeClass"],
        "manufacturerLogoIndex": definition["manufacturerLogoIndex"],
        "ratings": list(definition["ratings"]),
        "stats": list(stats),
        "arcadePhysics": arcade_physics,
        "developerLiverySmoke": True,
    }


GT2_GTMODE_LOCALIZED_DATABASES = (
    ("gtmode_data.dat.gz", "jpn_unistrdb.dat.gz"),
    ("eng_gtmode_data.dat.gz", "eng_unistrdb.dat.gz"),
    ("fra_gtmode_data.dat.gz", "fra_unistrdb.dat.gz"),
    ("ger_gtmode_data.dat.gz", "ger_unistrdb.dat.gz"),
    ("ita_gtmode_data.dat.gz", "ita_unistrdb.dat.gz"),
    ("spa_gtmode_data.dat.gz", "spa_unistrdb.dat.gz"),
    ("usa_gtmode_data.dat.gz", "usa_unistrdb.dat.gz"),
)


GT2_USED_CAR_DATABASES = (
    ".usedcar",
    ".usedcar_jpn",
    ".usedcar_usa",
)


GT2_GTMODE_MENU_LANGUAGES = ("usa", "fra", "ger", "ita", "spa")
GT2_TVR_NEW_CAR_LAYOUT_FROM_END = 1088
GT2_TVR_SPECIAL_LAYOUT_FROM_END = 1059
GT2_TVR_SPEED_12_ID = 0x207420D8
GT2_TVR_TUSCAN_SPEED_SIX_ID = 0x2075E1D8
GT2_TVR_NEW_CAR_IDS = (
    0x2035C318,  # vcrbn - Cerbera 4.2
    0x2034F198,  # vce5n - Cerbera 4.5
    0x2035D1D8,  # vcs6n - Cerbera Speed 6
    0x203520D8,  # vch2n - Chimaera 4.0
    0x2045C418,  # vgrfn - Griffith Blackpool B340
    0x2045C198,  # vgr5n - Griffith 500
    0x20346058,  # vc50n - Chimaera 5.0
    0x20352198,  # vch5n - Chimaera 4.5
)


def _parse_gt2_used_car_database(
    data: bytes,
) -> list[list[list[tuple[int, int, int, int]]]]:
    """Parse GT2's 60 native used-dealer rotations.

    Each rotation contains 39 manufacturer descriptors followed by packed
    `(CarId, Price, reserved, ColorId)` records. The outer header stores all
    60 rotation offsets plus an end sentinel.
    """

    if len(data) < 0xFC or data[:4] != b"UCAR":
        raise ValueError("GT2 used-car database header is invalid")
    first_rotation = struct.unpack_from("<I", data, 8)[0]
    if first_rotation < 12 or (first_rotation - 12) % 4:
        raise ValueError("GT2 used-car outer offset table is invalid")
    outer_count = (first_rotation - 12) // 4
    offsets = [
        first_rotation,
        *struct.unpack_from(f"<{outer_count}I", data, 12),
    ]
    if (
        len(offsets) != 61
        or offsets != sorted(offsets)
        or offsets[-1] != len(data)
    ):
        raise ValueError("GT2 used-car rotation offsets are invalid")

    rotations: list[list[list[tuple[int, int, int, int]]]] = []
    for rotation_index, (base, end) in enumerate(
        zip(offsets, offsets[1:])
    ):
        if base + 4 > end:
            raise ValueError(
                f"GT2 used-car rotation {rotation_index} is truncated"
            )
        first_records = struct.unpack_from("<I", data, base)[0] & 0xFFFF
        if first_records % 4:
            raise ValueError(
                f"GT2 used-car rotation {rotation_index} descriptors "
                "are misaligned"
            )
        manufacturer_count = first_records // 4
        if manufacturer_count != 39:
            raise ValueError(
                f"GT2 used-car manufacturer count changed: "
                f"{manufacturer_count}"
            )
        descriptors = struct.unpack_from(
            f"<{manufacturer_count}I", data, base
        )
        manufacturers: list[list[tuple[int, int, int, int]]] = []
        record_total = 0
        for descriptor in descriptors:
            relative_offset = descriptor & 0xFFFF
            record_count = descriptor >> 16
            if relative_offset != (
                manufacturer_count * 4 + record_total * 8
            ):
                raise ValueError(
                    f"GT2 used-car rotation {rotation_index} has a "
                    "non-canonical manufacturer offset"
                )
            records: list[tuple[int, int, int, int]] = []
            cursor = base + relative_offset
            for _ in range(record_count):
                car_id, price, reserved, color_id = struct.unpack_from(
                    "<IHBB", data, cursor
                )
                records.append((car_id, price, reserved, color_id))
                cursor += 8
            manufacturers.append(records)
            record_total += record_count
        if base + manufacturer_count * 4 + record_total * 8 != end:
            raise ValueError(
                f"GT2 used-car rotation {rotation_index} has trailing data"
            )
        rotations.append(manufacturers)
    return rotations


def _build_gt2_used_car_database(
    rotations: list[list[list[tuple[int, int, int, int]]]],
) -> bytes:
    if len(rotations) != 60:
        raise ValueError("GT2 used-car database must have 60 rotations")
    packed_rotations: list[bytes] = []
    for rotation_index, manufacturers in enumerate(rotations):
        if len(manufacturers) != 39:
            raise ValueError(
                f"GT2 used-car rotation {rotation_index} must have "
                "39 manufacturers"
            )
        output = bytearray(b"\0" * (len(manufacturers) * 4))
        record_total = 0
        for manufacturer_id, records in enumerate(manufacturers):
            if len(records) > 0xFFFF:
                raise ValueError(
                    f"GT2 used-car manufacturer {manufacturer_id} "
                    "has too many records"
                )
            relative_offset = (
                len(manufacturers) * 4 + record_total * 8
            )
            if relative_offset > 0xFFFF:
                raise ValueError(
                    f"GT2 used-car rotation {rotation_index} escaped "
                    "its 16-bit offsets"
                )
            struct.pack_into(
                "<I",
                output,
                manufacturer_id * 4,
                relative_offset | (len(records) << 16),
            )
            for car_id, price, flags, color_id in records:
                if (
                    not 0 <= price <= 0xFFFF
                    or not 0 <= flags <= 0xFF
                    or not 0 <= color_id <= 0xFF
                ):
                    raise ValueError(
                        "GT2 used-car price, flags, or color ID is "
                        "out of range"
                    )
                output.extend(
                    struct.pack("<IHBB", car_id, price, flags, color_id)
                )
            record_total += len(records)
        packed_rotations.append(bytes(output))

    header_size = 8 + (len(packed_rotations) + 1) * 4
    offsets = [header_size]
    for packed in packed_rotations:
        offsets.append(offsets[-1] + len(packed))
    output = bytearray(b"UCAR\0\0\0\0")
    output.extend(struct.pack(f"<{len(offsets)}I", *offsets))
    for packed in packed_rotations:
        output.extend(packed)
    verified = _parse_gt2_used_car_database(bytes(output))
    if verified != rotations:
        raise ValueError("GT2 used-car database failed round-trip")
    return bytes(output)


def stage_gt2_simulation_used_cars(
    gt2_simulation_volume: Path,
    patch_root: Path,
    cars: tuple[dict[str, object], ...],
    smoke_stem: str | None = None,
    smoke_existing: dict[str, object] | None = None,
) -> dict[str, object]:
    """Make every imported older GT1 identity natively purchasable.

    GT1's Arcade-only identities have a real nonzero archive price but no
    GT2-era new-car model year. Keep their authoritative price and rotate one
    of their three archive color IDs per dealer refresh instead of inventing
    a 1999 model year or permanently crowding the used-car screen.
    """

    additions = []
    for car in cars:
        stem = str(car["stem"])
        localized = dict(car["gtMode"]["localizedDatabases"])
        usa = dict(localized["usa_gtmode_data.dat.gz"])
        color_ids = tuple(int(value) for value in car["carinfo"]["colorIds"])
        if not color_ids:
            raise ValueError(
                f"GT2 imported used car has no colors: {stem}"
            )
        additions.append(
            {
                "stem": stem,
                "carId": encode_gt2_car_id(stem),
                "manufacturerId": int(usa["manufacturerId"]),
                "price": int(car["gtMode"]["identity"]["gt2Price"]),
                "colorIds": color_ids,
            }
        )
    if smoke_stem is not None and smoke_existing is not None:
        raise ValueError("only one GT Mode used-car smoke target is allowed")
    if smoke_stem is not None and smoke_stem not in {
        str(addition["stem"]) for addition in additions
    }:
        raise ValueError(
            f"unknown GT Mode used-car smoke target: {smoke_stem}"
        )
    metadata: dict[str, object] = {}
    for name in GT2_USED_CAR_DATABASES:
        source = gzip.decompress(read_gt2_member(
            gt2_simulation_volume, name
        ))
        rotations = _parse_gt2_used_car_database(source)
        for rotation_index, manufacturers in enumerate(rotations):
            for addition in additions:
                manufacturer_id = int(addition["manufacturerId"])
                if not 0 <= manufacturer_id < len(manufacturers):
                    raise ValueError(
                        f"GT2 {addition['stem']} manufacturer "
                        f"{manufacturer_id} is absent from {name}"
                    )
                car_id = int(addition["carId"])
                if any(
                    record[0] == car_id
                    for records in manufacturers
                    for record in records
                ):
                    raise ValueError(
                        f"GT2 used-car database already contains "
                        f"{addition['stem']}"
                    )
                color_ids = tuple(addition["colorIds"])
                manufacturers[manufacturer_id].append(
                    (
                        car_id,
                        int(addition["price"]),
                        0,
                        int(color_ids[rotation_index % len(color_ids)]),
                    )
                )
                manufacturers[manufacturer_id].sort(
                    key=lambda record: (
                        record[1],
                        record[0],
                        record[3],
                    )
                )
            if smoke_stem is not None:
                featured = next(
                    addition
                    for addition in additions
                    if addition["stem"] == smoke_stem
                )
                manufacturer_id = int(featured["manufacturerId"])
                records = manufacturers[manufacturer_id]
                index = next(
                    index
                    for index, record in enumerate(records)
                    if record[0] == int(featured["carId"])
                )
                record = records[index]
                # Development-only deterministic smoke placement: GT2's
                # controller fixture activates the first Mazda dealer row.
                # A non-Mazda target is duplicated into that one test roster;
                # its real manufacturer record remains price-sorted. Normal
                # conversion never takes this branch.
                smoke_records = manufacturers[18]
                if manufacturer_id == 18:
                    records.pop(index)
                smoke_records.insert(0, record)
            elif smoke_existing is not None:
                car_id = int(smoke_existing["carId"])
                if any(
                    record[0] == car_id
                    for records in manufacturers
                    for record in records
                ):
                    raise ValueError(
                        "existing livery smoke car unexpectedly appears "
                        f"in {name}: {smoke_existing['stem']}"
                    )
                manufacturers[18].insert(
                    0,
                    (
                        car_id,
                        int(smoke_existing["smokePrice"]),
                        0,
                        int(smoke_existing["smokeColorId"]),
                    ),
                )
        converted = _build_gt2_used_car_database(rotations)
        output = patch_root / name
        output.write_bytes(
            gzip.compress(converted, compresslevel=9, mtime=0)
        )
        verified = _parse_gt2_used_car_database(
            gzip.decompress(output.read_bytes())
        )
        for addition in additions:
            car_id = int(addition["carId"])
            manufacturer_id = int(addition["manufacturerId"])
            actual = [
                next(
                    record
                    for record in rotation[manufacturer_id]
                    if record[0] == car_id
                )
                for rotation in verified
            ]
            if (
                {record[1] for record in actual}
                != {int(addition["price"])}
                or {record[3] for record in actual}
                != set(addition["colorIds"])
            ):
                raise ValueError(
                    f"GT2 {addition['stem']} used-car rotation failed "
                    f"round-trip in {name}"
                )
        if smoke_existing is not None:
            car_id = int(smoke_existing["carId"])
            actual = [rotation[18][0] for rotation in verified]
            if (
                {record[0] for record in actual} != {car_id}
                or {record[1] for record in actual}
                != {int(smoke_existing["smokePrice"])}
                or {record[3] for record in actual}
                != {int(smoke_existing["smokeColorId"])}
            ):
                raise ValueError(
                    "existing GT2 livery smoke placement failed "
                    f"round-trip in {name}"
                )
        metadata[name] = {
            "rotationCount": len(verified),
            "manufacturerCount": len(verified[0]),
            "uncompressedSize": len(converted),
            "uncompressedSha256": hashlib.sha256(
                converted
            ).hexdigest(),
            "cars": [
                {
                    "stem": addition["stem"],
                    "manufacturerId": addition["manufacturerId"],
                    "price": addition["price"],
                    "colorIds": list(addition["colorIds"]),
                    "appearances": len(verified),
                }
                for addition in additions
            ],
            "smokeFeaturedStem": (
                smoke_stem
                if smoke_stem is not None
                else (
                    str(smoke_existing["stem"])
                    if smoke_existing is not None
                    else None
                )
            ),
            "smokeDealerManufacturerId": (
                18
                if smoke_stem is not None or smoke_existing is not None
                else None
            ),
            "smokeDealerSlot": (
                1
                if smoke_stem is not None or smoke_existing is not None
                else None
            ),
            "smokeColorId": (
                int(smoke_existing["smokeColorId"])
                if smoke_existing is not None
                else None
            ),
        }
    return metadata


def build_gt2_existing_livery_smoke_car(
    gt2_simulation_volume: Path,
    definitions: list[dict[str, object]],
    stem: str,
) -> dict[str, object]:
    """Describe one existing GT2 identity for a transient purchase smoke."""

    target_definitions = [
        definition
        for definition in definitions
        if str(definition["targetStem"]) == stem
    ]
    if not target_definitions:
        raise ValueError(f"GT2 livery smoke target is absent: {stem}")
    records = _parse_gt2_carinfo(
        read_gt2_member(gt2_simulation_volume, ".carinfoa")
    )
    target = next(
        (record for record in records if record["stem"] == stem),
        None,
    )
    if target is None:
        raise ValueError(f"GT2 livery smoke carinfo is absent: {stem}")
    color_ids = list(int(value) for value in target["colorIds"])
    for definition in target_definitions:
        color_ids.extend(
            int(item[0]) for item in definition["paintSources"]
        )
    gtmode = read_gt2_gzip_member(
        gt2_simulation_volume, "carparam/usa_gtmode_data.dat.gz"
    )
    car = _find_gt2_gtdt_car(
        _parse_gtdt_blocks(
            gtmode, GT2_GTMODE_BLOCK_COUNT
        )[GT2_GTMODE_CAR_BLOCK],
        0x48,
        stem,
    )
    return {
        "stem": stem,
        "carId": encode_gt2_car_id(stem),
        "manufacturerId": struct.unpack_from("<H", car, 0x3A)[0],
        "nativePrice": struct.unpack_from("<I", car, 0x44)[0],
        # Development output only: keep the deterministic 100,000-credit
        # fixture able to acquire the existing prize/race identity.
        "smokePrice": 20_000,
        "colorIds": tuple(color_ids),
        # A prize/race identity has no ordinary used-car color picker. Keep
        # this developer-only alias pinned to the newest imported paint so a
        # fresh-save smoke proves the alternate native body deterministically.
        "smokeColorId": color_ids[-1],
    }


def _unpack_gtmenu_asset(data: bytes) -> bytes:
    """Inflate the first gzip member from a GT Mode menu-data slot."""

    inflater = zlib.decompressobj(31)
    unpacked = inflater.decompress(data) + inflater.flush()
    if not unpacked or not inflater.eof:
        raise ValueError("GT Mode menu asset is not a complete gzip member")
    return unpacked


def _gtmenu_offsets(index: bytes) -> tuple[int, list[int]]:
    if len(index) < 12 or (len(index) - 8) % 4:
        raise ValueError("GT Mode menu index size is invalid")
    count = struct.unpack_from("<I", index, 0)[0]
    if len(index) != 4 + (count + 1) * 4:
        raise ValueError(
            f"GT Mode menu index count is inconsistent: {count}"
        )
    offsets = list(struct.unpack_from(f"<{count + 1}I", index, 4))
    aligned = [offset & ~3 for offset in offsets]
    if aligned != sorted(aligned) or len(set(aligned)) != len(aligned):
        raise ValueError("GT Mode menu index offsets are not increasing")
    return count, offsets


def _gtmenu_records(
    asset: bytes,
) -> tuple[list[tuple[int, int]], int, int, list[tuple[int, bytes]]]:
    """Return GM groups, trailing-record metadata, and all 0x4c records."""

    if len(asset) < 12 or asset[:2] != b"GM":
        raise ValueError("GT Mode menu layout has no GM header")
    cursor = 8
    groups: list[tuple[int, int]] = []
    records: list[tuple[int, bytes]] = []
    for group_index in range(struct.unpack_from("<I", asset, 4)[0]):
        if cursor + 4 > len(asset):
            raise ValueError(
                f"GT Mode menu group {group_index} header is truncated"
            )
        header_count, record_count = struct.unpack_from(
            "<HH", asset, cursor
        )
        count_offset = cursor + 2
        cursor += 4 + header_count * 0x0C
        end = cursor + record_count * 0x4C
        if end > len(asset):
            raise ValueError(
                f"GT Mode menu group {group_index} records are truncated"
            )
        groups.append((count_offset, record_count))
        records.extend(
            (cursor + index * 0x4C, asset[
                cursor + index * 0x4C : cursor + (index + 1) * 0x4C
            ])
            for index in range(record_count)
        )
        cursor = end
    if cursor + 4 > len(asset):
        raise ValueError("GT Mode menu trailing-record count is missing")
    trailing_count_offset = cursor
    trailing_count = struct.unpack_from("<I", asset, cursor)[0]
    cursor += 4
    trailing_records_offset = cursor
    end = cursor + trailing_count * 0x4C
    if end > len(asset):
        raise ValueError("GT Mode menu trailing records are truncated")
    records.extend(
        (cursor + index * 0x4C, asset[
            cursor + index * 0x4C : cursor + (index + 1) * 0x4C
        ])
        for index in range(trailing_count)
    )
    return (
        groups,
        trailing_count_offset,
        trailing_records_offset,
        records,
    )


def _tvr_new_car_records(asset: bytes) -> tuple[
    int,
    int,
    list[tuple[int, bytes]],
]:
    (
        _,
        trailing_count_offset,
        trailing_records_offset,
        records,
    ) = _gtmenu_records(asset)
    native_records: dict[int, tuple[int, bytes]] = {}
    for offset, record in records:
        flags = struct.unpack_from("<I", record, 8)[0]
        record_car_id = struct.unpack_from("<I", record, 0x10)[0]
        if (flags & 0xFFFF) == 6 and record_car_id in GT2_TVR_NEW_CAR_IDS:
            if record_car_id in native_records:
                raise ValueError(
                    f"duplicate TVR new-car record {record_car_id:08X}"
                )
            native_records[record_car_id] = (offset, record)
    if set(native_records) != set(GT2_TVR_NEW_CAR_IDS):
        found = ", ".join(f"{value:08X}" for value in native_records)
        raise ValueError(
            f"TVR new-car layout roster changed; found [{found}]"
        )
    ordered = sorted(native_records.values())
    expected_offsets = list(
        range(ordered[0][0], ordered[0][0] + 8 * 0x4C, 0x4C)
    )
    if [offset for offset, _ in ordered] != expected_offsets:
        raise ValueError("TVR new-car records are no longer contiguous")
    trailing_count = struct.unpack_from("<I", asset, trailing_count_offset)[0]
    first_trailing_index = (
        ordered[0][0] - trailing_records_offset
    ) // 0x4C
    if (
        ordered[0][0] < trailing_records_offset
        or first_trailing_index < 0
        or first_trailing_index + 8 > trailing_count
    ):
        raise ValueError("TVR new-car roster is not in the trailing layout")
    return trailing_count_offset, trailing_records_offset, ordered


def _insert_cerbera_lm_after_speed_12(
    asset: bytes,
    tuscan_layout_id: int,
    cerbera_layout_id: int,
    car_id: int,
) -> bytes:
    _, _, _, records = _gtmenu_records(asset)
    matches = [
        (offset, record)
        for offset, record in records
        if struct.unpack_from("<I", record, 0x0C)[0]
        == tuscan_layout_id
        and struct.unpack_from("<I", record, 0x10)[0]
        == GT2_TVR_TUSCAN_SPEED_SIX_ID
    ]
    if len(matches) != 1:
        raise ValueError(
            f"TVR Speed 12 page expected one native Tuscan link, found "
            f"{len(matches)}"
        )
    output = bytearray(asset)
    offset, _ = matches[0]
    # Use the ordinary purchasable-new-car transition so the cloned destination
    # renders GT2's native NEW CAR / INFORMATION page with an active BUY action.
    struct.pack_into("<I", output, offset + 8, 0x05000006)
    struct.pack_into("<I", output, offset + 0x0C, cerbera_layout_id)
    struct.pack_into("<I", output, offset + 0x10, car_id)
    _, _, _, verified_records = _gtmenu_records(bytes(output))
    verified = [
        record
        for _, record in verified_records
        if struct.unpack_from("<I", record, 0x0C)[0]
        == cerbera_layout_id
    ]
    if (
        len(verified) != 1
        or struct.unpack_from("<I", verified[0], 8)[0] != 0x05000006
        or struct.unpack_from("<I", verified[0], 0x10)[0] != car_id
    ):
        raise ValueError("Cerbera LM Special-page link failed round-trip")
    return bytes(output)


def _retarget_gtmenu_back_link(
    asset: bytes,
    old_target: int,
    new_target: int,
) -> bytes:
    _, _, _, records = _gtmenu_records(asset)
    matches = [
        offset
        for offset, record in records
        if struct.unpack_from("<I", record, 8)[0] == 0x01000000
        and struct.unpack_from("<I", record, 0x0C)[0] == old_target
        and struct.unpack_from("<I", record, 0x10)[0] == 0
    ]
    if len(matches) != 1:
        raise ValueError(
            f"GT Mode detail page expected one back link to {old_target}, "
            f"found {len(matches)}"
        )
    output = bytearray(asset)
    struct.pack_into("<I", output, matches[0] + 0x0C, new_target)
    _, _, _, verified_records = _gtmenu_records(bytes(output))
    if sum(
        struct.unpack_from("<I", record, 0x0C)[0] == new_target
        and struct.unpack_from("<I", record, 8)[0] == 0x01000000
        for _, record in verified_records
    ) != 1:
        raise ValueError("GT Mode detail-page back link failed round-trip")
    return bytes(output)


def _replace_gtmenu_asset(
    data: bytes,
    index: bytes,
    asset_index: int,
    replacement: bytes,
) -> tuple[bytes, bytes, dict[str, int]]:
    count, offsets = _gtmenu_offsets(index)
    if not 0 <= asset_index < count:
        raise ValueError(
            f"GT Mode menu asset index is out of range: {asset_index}"
        )
    start = offsets[asset_index] & ~3
    end = offsets[asset_index + 1] & ~3
    original = _unpack_gtmenu_asset(data[start:end])
    packed = gzip.compress(replacement, compresslevel=9, mtime=0)
    padded = packed + b"\0" * ((-len(packed)) & 3)
    delta = len(padded) - (end - start)
    if delta % 4:
        raise ValueError("GT Mode menu replacement is not word-aligned")
    rebuilt_data = data[:start] + padded + data[end:]
    adjusted = offsets[:]
    for offset_index in range(asset_index + 1, len(adjusted)):
        flags = adjusted[offset_index] & 3
        aligned = (adjusted[offset_index] & ~3) + delta
        adjusted[offset_index] = aligned | flags
    rebuilt_index = bytearray(index)
    struct.pack_into(
        f"<{len(adjusted)}I", rebuilt_index, 4, *adjusted
    )
    verify_count, verify_offsets = _gtmenu_offsets(bytes(rebuilt_index))
    verify_start = verify_offsets[asset_index] & ~3
    verify_end = verify_offsets[asset_index + 1] & ~3
    if (
        verify_count != count
        or _unpack_gtmenu_asset(rebuilt_data[verify_start:verify_end])
        != replacement
    ):
        raise ValueError("GT Mode menu replacement failed round-trip")
    return bytes(rebuilt_data), bytes(rebuilt_index), {
        "assetIndex": asset_index,
        "originalPackedAllocation": end - start,
        "replacementPackedSize": len(packed),
        "replacementPackedAllocation": len(padded),
        "originalUnpackedSize": len(original),
        "replacementUnpackedSize": len(replacement),
        "dataSizeDelta": delta,
    }


def _append_gtmenu_asset_clone(
    data: bytes,
    index: bytes,
    source_asset_index: int,
) -> tuple[bytes, bytes, dict[str, int]]:
    count, offsets = _gtmenu_offsets(index)
    if not 0 <= source_asset_index < count:
        raise ValueError(
            f"GT Mode menu clone source is out of range: "
            f"{source_asset_index}"
        )
    source_start = offsets[source_asset_index] & ~3
    source_end = offsets[source_asset_index + 1] & ~3
    final_offset = offsets[-1] & ~3
    if final_offset != len(data):
        raise ValueError(
            f"GT Mode menu data has an unindexed tail: "
            f"{len(data) - final_offset} bytes"
        )
    payload = data[source_start:source_end]
    source_layout = _unpack_gtmenu_asset(payload)
    new_asset_index = count
    new_end = final_offset + len(payload)
    new_offsets = offsets[:-1] + [
        final_offset | (offsets[source_asset_index] & 3),
        new_end | (offsets[-1] & 3),
    ]
    rebuilt_index = bytearray(4 + len(new_offsets) * 4)
    struct.pack_into("<I", rebuilt_index, 0, count + 1)
    struct.pack_into(
        f"<{len(new_offsets)}I", rebuilt_index, 4, *new_offsets
    )
    rebuilt_data = data + payload
    verify_count, verify_offsets = _gtmenu_offsets(bytes(rebuilt_index))
    verify_start = verify_offsets[new_asset_index] & ~3
    verify_end = verify_offsets[new_asset_index + 1] & ~3
    if (
        verify_count != count + 1
        or _unpack_gtmenu_asset(rebuilt_data[verify_start:verify_end])
        != source_layout
    ):
        raise ValueError("GT Mode menu asset clone failed round-trip")
    return rebuilt_data, bytes(rebuilt_index), {
        "sourceAssetIndex": source_asset_index,
        "newAssetIndex": new_asset_index,
        "packedAllocation": len(payload),
        "unpackedSize": len(source_layout),
    }


def _stage_cerbera_lm_tvr_new_car_layouts(
    gt2_simulation_volume: Path,
    patch_root: Path,
    car_id: int,
) -> dict[str, object]:
    localized: dict[str, object] = {}
    for language in GT2_GTMODE_MENU_LANGUAGES:
        data_name = f"gtmenu/{language}/gtmenudat.dat"
        index_name = f"gtmenu/{language}/gtmenudat.idx"
        data = read_gt2_member(gt2_simulation_volume, data_name)
        index = read_gt2_member(gt2_simulation_volume, index_name)
        count, offsets = _gtmenu_offsets(index)
        new_car_asset_index = count - GT2_TVR_NEW_CAR_LAYOUT_FROM_END
        new_car_detail_asset_index = new_car_asset_index + 1
        special_asset_index = count - GT2_TVR_SPECIAL_LAYOUT_FROM_END
        tuscan_asset_index = special_asset_index + 1
        new_car_start = offsets[new_car_asset_index] & ~3
        new_car_end = offsets[new_car_asset_index + 1] & ~3
        new_car_layout = _unpack_gtmenu_asset(
            data[new_car_start:new_car_end]
        )
        _tvr_new_car_records(new_car_layout)
        special_start = offsets[special_asset_index] & ~3
        special_end = offsets[special_asset_index + 1] & ~3
        special_layout = _unpack_gtmenu_asset(
            data[special_start:special_end]
        )
        detail_start = offsets[new_car_detail_asset_index] & ~3
        detail_end = offsets[new_car_detail_asset_index + 1] & ~3
        new_car_detail_layout = _unpack_gtmenu_asset(
            data[detail_start:detail_end]
        )
        converted_data, converted_index, clone_metadata = (
            _append_gtmenu_asset_clone(
                data,
                index,
                new_car_detail_asset_index,
            )
        )
        cerbera_asset_index = clone_metadata["newAssetIndex"]
        converted_special = _insert_cerbera_lm_after_speed_12(
            special_layout,
            tuscan_asset_index,
            cerbera_asset_index,
            car_id,
        )
        converted_data, converted_index, special_metadata = (
            _replace_gtmenu_asset(
                converted_data,
                converted_index,
                special_asset_index,
                converted_special,
            )
        )
        converted_cerbera_detail = _retarget_gtmenu_back_link(
            new_car_detail_layout,
            new_car_asset_index,
            special_asset_index,
        )
        converted_data, converted_index, detail_metadata = (
            _replace_gtmenu_asset(
                converted_data,
                converted_index,
                cerbera_asset_index,
                converted_cerbera_detail,
            )
        )
        data_path = patch_root / data_name
        index_path = patch_root / index_name
        data_path.parent.mkdir(parents=True, exist_ok=True)
        data_path.write_bytes(converted_data)
        index_path.write_bytes(converted_index)
        localized[language] = {
            "cerberaPageClone": clone_metadata,
            "speed12Page": special_metadata,
            "cerberaDetailPage": detail_metadata,
            "nativeLineupAssetIndex": new_car_asset_index,
            "nativeNewCarDetailAssetIndex": new_car_detail_asset_index,
            "speed12AssetIndex": special_asset_index,
            "tuscanAssetIndex": tuscan_asset_index,
            "cerberaAssetIndex": cerbera_asset_index,
            "speed12LayoutSha256": hashlib.sha256(
                converted_special
            ).hexdigest(),
            "cerberaDetailLayoutSha256": hashlib.sha256(
                converted_cerbera_detail
            ).hexdigest(),
            "dataSha256": hashlib.sha256(converted_data).hexdigest(),
            "indexSha256": hashlib.sha256(converted_index).hexdigest(),
        }
    return {
        "carId": car_id,
        "speed12CarId": GT2_TVR_SPEED_12_ID,
        "tuscanSpeedSixCarId": GT2_TVR_TUSCAN_SPEED_SIX_ID,
        "nativeRoster": list(GT2_TVR_NEW_CAR_IDS),
        "localizedMenus": localized,
    }


def stage_gt2_existing_livery_new_car_smoke(
    gt2_simulation_volume: Path,
    patch_root: Path,
    smoke: dict[str, object],
) -> dict[str, object]:
    """Expose one existing TVR identity in TVR's native new-car lineup.

    TVR has no used-car page, and its authored lineup is an explicit eight-car
    GM layout rather than a scan of the GT Mode database. This transient smoke
    layer preserves that lineup and temporarily links Speed 12 to a cloned
    native new-car detail page for Cerbera LM. It marks the Cerbera as a
    purchasable new car and lowers its smoke price. The original car ID,
    names, manufacturer, parts, body, and livery data remain intact. Normal
    conversion never calls this helper; the smoke-only link temporarily
    bypasses Tuscan Speed Six without changing its data or production path.
    """

    stem = str(smoke["stem"])
    smoke_price = int(smoke["smokePrice"])
    metadata: dict[str, object] = {
        "stem": stem,
        "manufacturerId": int(smoke["manufacturerId"]),
        "nativePrice": int(smoke["nativePrice"]),
        "smokePrice": smoke_price,
        "smokeColorId": int(smoke["smokeColorId"]),
        "localizedDatabases": {},
    }
    if int(smoke["manufacturerId"]) != 34 or stem != "v-rbr":
        raise ValueError(
            "native new-car livery smoke currently supports only TVR v-rbr"
        )
    metadata["dealerLayouts"] = _stage_cerbera_lm_tvr_new_car_layouts(
        gt2_simulation_volume,
        patch_root,
        int(smoke["carId"]),
    )
    for database_name, _ in GT2_GTMODE_LOCALIZED_DATABASES:
        database_path = patch_root / "carparam" / database_name
        gtmode_data = (
            gzip.decompress(database_path.read_bytes())
            if database_path.is_file()
            else read_gt2_gzip_member(
                gt2_simulation_volume,
                f"carparam/{database_name}",
            )
        )
        blocks = _parse_gtdt_blocks(
            gtmode_data, GT2_GTMODE_BLOCK_COUNT
        )
        car_block = blocks[GT2_GTMODE_CAR_BLOCK]
        cars = [
            bytearray(car_block[offset : offset + 0x48])
            for offset in range(0, len(car_block), 0x48)
        ]
        target_id = int(smoke["carId"])
        matches = [
            car
            for car in cars
            if struct.unpack_from("<I", car, 0)[0] == target_id
        ]
        if len(matches) != 1:
            raise ValueError(
                f"GT2 new-car livery smoke expected one {stem} record "
                f"in {database_name}, found {len(matches)}"
            )
        target = matches[0]
        original_acquisition_flag = target[0x40]
        original_manufacturer = struct.unpack_from("<H", target, 0x3A)[0]
        original_price = struct.unpack_from("<I", target, 0x44)[0]
        if (
            original_acquisition_flag == 0
            or original_manufacturer != int(smoke["manufacturerId"])
            or original_price != int(smoke["nativePrice"])
        ):
            raise ValueError(
                f"GT2 {stem} acquisition record changed in {database_name}: "
                f"flag={original_acquisition_flag}, "
                f"manufacturer={original_manufacturer}, price={original_price}"
            )
        target[0x40] = 0
        struct.pack_into("<I", target, 0x44, smoke_price)
        updated_blocks = list(blocks)
        updated_blocks[GT2_GTMODE_CAR_BLOCK] = b"".join(
            bytes(car) for car in cars
        )
        converted = _rebuild_gtmode_gtdt(gtmode_data, updated_blocks)
        verified = _find_gt2_gtdt_car(
            _parse_gtdt_blocks(
                converted, GT2_GTMODE_BLOCK_COUNT
            )[GT2_GTMODE_CAR_BLOCK],
            0x48,
            stem,
        )
        if (
            verified[0x40] != 0
            or struct.unpack_from("<H", verified, 0x3A)[0]
            != original_manufacturer
            or struct.unpack_from("<I", verified, 0x44)[0]
            != smoke_price
        ):
            raise ValueError(
                f"GT2 {stem} new-car smoke failed round-trip in "
                f"{database_name}"
            )
        database_path.parent.mkdir(parents=True, exist_ok=True)
        database_path.write_bytes(
            gzip.compress(converted, compresslevel=9, mtime=0)
        )
        metadata["localizedDatabases"][database_name] = {
            "originalAcquisitionFlag": original_acquisition_flag,
            "smokeAcquisitionFlag": 0,
            "uncompressedSize": len(converted),
            "uncompressedSha256": hashlib.sha256(converted).hexdigest(),
        }
    return metadata


def stage_gt2_simulation_car_physics(
    disc_root: Path,
    gt2_simulation_volume: Path,
    patch_root: Path,
    definition: dict[str, object],
) -> dict[str, object]:
    stem = str(definition["stem"])
    identity = read_gt1_spec_identity(
        disc_root / "CARINF.DAT", stem
    )
    if identity["displayName"] != definition["displayName"]:
        raise ValueError(
            f"GT1 {stem} identity changed: "
            f"{identity['displayName']!r} != "
            f"{definition['displayName']!r}"
        )
    source_price = int(identity["sourcePrice"])
    if source_price <= 0 or source_price % 100:
        raise ValueError(
            f"GT1 {stem} has no directly convertible purchase price: "
            f"{source_price}"
        )

    output_root = patch_root / "carparam"
    output_root.mkdir(parents=True, exist_ok=True)
    metadata: dict[str, object] = {
        "identity": identity,
        "localizedDatabases": {},
    }
    for database_name, strings_name in GT2_GTMODE_LOCALIZED_DATABASES:
        database_path = output_root / database_name
        strings_path = output_root / strings_name
        gtmode_data = (
            gzip.decompress(database_path.read_bytes())
            if database_path.is_file()
            else read_gt2_gzip_member(
                gt2_simulation_volume,
                f"carparam/{database_name}",
            )
        )
        strings_data = (
            gzip.decompress(strings_path.read_bytes())
            if strings_path.is_file()
            else read_gt2_gzip_member(
                gt2_simulation_volume,
                f"carparam/{strings_name}",
            )
        )
        strings_data, name_indices = append_gt2_unistrdb_strings(
            strings_data, tuple(identity["nameParts"])
        )
        gtmode_data, details = append_gt2_gtmode_car(
            gtmode_data,
            definition,
            name_indices,
            int(identity["gt2Price"]),
        )
        database_path.write_bytes(
            gzip.compress(gtmode_data, compresslevel=9, mtime=0)
        )
        strings_path.write_bytes(
            gzip.compress(strings_data, compresslevel=9, mtime=0)
        )
        metadata["localizedDatabases"][database_name] = {
            **details,
            "stringsMember": strings_name,
            "stringsCount": len(parse_gt2_unistrdb(strings_data)),
            "stringsSha256": hashlib.sha256(strings_data).hexdigest(),
        }
    return metadata


def stage_gt1_simulation_car(
    disc_root: Path,
    gt2_simulation_volume: Path,
    patch_root: Path,
    definition: dict[str, object],
) -> dict[str, object]:
    """Stage stock and Racing Modification bodies plus a full GT Mode family."""

    target_stem = str(definition["stem"])
    basis_stem = str(definition["modelBasisStem"])
    target_race_stem = target_stem[:4] + "r"
    basis_race_stem = basis_stem[:4] + "r"
    stems = read_gt1_car_stems(disc_root / "SYSTEM.DAT")
    stem_set = set(stems)
    if target_race_stem not in stem_set or basis_race_stem not in stem_set:
        raise ValueError(
            f"GT1 {target_stem} lacks a complete Racing Modification "
            f"graphic pair: {target_race_stem}, {basis_race_stem}"
        )

    car_output = patch_root / "carobj"
    car_output.mkdir(parents=True, exist_ok=True)
    convert_model = bool(definition.get("convertModel", False))
    body_metadata: dict[str, object] = {}
    for source_stem, native_basis in (
        (target_stem, basis_stem),
        (target_race_stem, basis_race_stem),
    ):
        day_texture, day_model, night_texture, night_model = (
            read_gt1_car_members(
                disc_root / "CAR.DAT", stems, source_stem
            )
        )
        _, basis_day_model, _, basis_night_model = read_gt1_car_members(
            disc_root / "CAR.DAT", stems, native_basis
        )
        if (
            not convert_model
            and (
                day_model != basis_day_model
                or night_model != basis_night_model
            )
        ):
            raise ValueError(
                f"GT1 {source_stem} no longer shares authored geometry "
                f"with {native_basis}"
            )
        converted_day = convert_gt1_car_texture(day_texture)
        converted_night = convert_gt1_car_texture(night_texture)
        if (
            converted_day[0] != converted_night[0]
            or converted_day[
                2 : 2 + converted_day[0]
            ] != converted_night[
                2 : 2 + converted_night[0]
            ]
        ):
            raise ValueError(
                f"GT1 {source_stem} day/night paint IDs differ"
            )
        (car_output / f"{source_stem}.cdp.gz").write_bytes(
            gzip.compress(converted_day, compresslevel=9, mtime=0)
        )
        (car_output / f"{source_stem}.cnp.gz").write_bytes(
            gzip.compress(converted_night, compresslevel=9, mtime=0)
        )
        models: dict[str, object] = {}
        for source_model, extension in (
            (day_model, "cdo"),
            (night_model, "cno"),
        ):
            native_model = read_gt2_gzip_member(
                gt2_simulation_volume,
                f"carobj/{native_basis}.{extension}.gz",
            )
            if convert_model:
                converted_model, conversion = convert_gt1_car_model(
                    source_model, native_model
                )
            else:
                converted_model = native_model
                conversion = {
                    "reusedNativeBasis": native_basis,
                    "size": len(converted_model),
                }
            if len(converted_model) > GT2_CAR_MODEL_SAFE_SIZE:
                raise ValueError(
                    f"GT2 {source_stem}.{extension} exceeds its native "
                    f"0x{GT2_CAR_MODEL_SAFE_SIZE:X}-byte slot"
                )
            (car_output / f"{source_stem}.{extension}.gz").write_bytes(
                gzip.compress(
                    converted_model, compresslevel=9, mtime=0
                )
            )
            models[extension] = {
                **conversion,
                "sha256": hashlib.sha256(converted_model).hexdigest(),
            }
        body_metadata[source_stem] = {
            "basisStem": native_basis,
            "paintIds": list(
                converted_day[2 : 2 + converted_day[0]]
            ),
            "dayTextureSha256": hashlib.sha256(
                converted_day
            ).hexdigest(),
            "nightTextureSha256": hashlib.sha256(
                converted_night
            ).hexdigest(),
            "models": models,
        }

    carinfo_metadata = update_gt2_localized_carinfo(
        gt2_simulation_volume,
        patch_root,
        lambda carinfo, carcolor: append_gt2_carinfo(
            carinfo,
            carcolor,
            target_stem,
            str(definition["displayName"]),
            tuple(definition["paintSources"]),
        ),
    )
    gtmode_logo = stage_gt2_gtmode_car_logos(
        disc_root,
        patch_root,
        definition,
        target_stem,
        target_race_stem,
    )
    physics = stage_gt2_simulation_car_physics(
        disc_root,
        gt2_simulation_volume,
        patch_root,
        {
            **definition,
            "gtModeRaceStem": target_race_stem,
        },
    )
    return {
        "stem": target_stem,
        "raceStem": target_race_stem,
        "displayName": definition["displayName"],
        "carinfo": carinfo_metadata,
        "bodies": body_metadata,
        "carLogo": gtmode_logo,
        "gtMode": physics,
    }


def stage_gt1_arcade_car(
    disc_root: Path,
    gt2_arcade_volume: Path,
    patch_root: Path,
    definition: dict[str, object],
) -> dict[str, object]:
    stem = str(definition["stem"])
    model_basis = str(definition["modelBasisStem"])
    stems = read_gt1_car_stems(disc_root / "SYSTEM.DAT")
    day_texture, day_model, night_texture, night_model = (
        read_gt1_car_members(disc_root / "CAR.DAT", stems, stem)
    )
    _, basis_day_model, _, basis_night_model = read_gt1_car_members(
        disc_root / "CAR.DAT", stems, model_basis
    )
    convert_model = bool(definition.get("convertModel", False))
    if (
        not convert_model
        and (day_model != basis_day_model or night_model != basis_night_model)
    ):
        raise ValueError(
            f"GT1 {stem} does not share authored geometry with {model_basis}"
        )

    spec_stem = str(definition.get("physicsSpecStem", stem))
    stats = read_gt1_spec_stats(disc_root / "CARINF.DAT", spec_stem)
    if tuple(definition["stats"]) != stats:
        raise ValueError(
            f"GT1 {stem} stats changed: {stats} != "
            f"{tuple(definition['stats'])}"
        )
    physics_source = _validate_gt1_arcade_roadster_physics(
        disc_root / "CARINF.DAT", definition
    )

    car_output = patch_root / "carobj"
    car_output.mkdir(parents=True, exist_ok=True)
    converted_day = convert_gt1_car_texture(day_texture)
    converted_night = convert_gt1_car_texture(night_texture)
    (car_output / f"{stem}.cdp.gz").write_bytes(
        gzip.compress(converted_day, compresslevel=9, mtime=0)
    )
    (car_output / f"{stem}.cnp.gz").write_bytes(
        gzip.compress(converted_night, compresslevel=9, mtime=0)
    )
    native_model_hashes: dict[str, str] = {}
    model_conversions: dict[str, object] = {}
    for source_model, source_extension, output_extension in (
        (day_model, "cdo", "cdo"),
        (night_model, "cno", "cno"),
    ):
        member = read_gt2_member(
            gt2_arcade_volume,
            f"carobj/{model_basis}.{source_extension}.gz",
        )
        if convert_model:
            converted_model, conversion = convert_gt1_car_model(
                source_model, gzip.decompress(member)
            )
            staged_member = gzip.compress(
                converted_model, compresslevel=9, mtime=0
            )
            model_conversions[output_extension] = conversion
        else:
            converted_model = gzip.decompress(member)
            staged_member = member
        (car_output / f"{stem}.{output_extension}.gz").write_bytes(
            staged_member
        )
        native_model_hashes[output_extension] = hashlib.sha256(
            converted_model
        ).hexdigest()

    menu_logo_name = definition.get("menuLogoName")
    logo_entry = definition.get("arcadeLogoEntry")
    source_logo, _ = read_gt1_definition_car_logo(
        disc_root, definition
    )
    logo = normalize_arcade_car_logo(source_logo)
    arcade_output = patch_root / "arcade"
    arcade_output.mkdir(parents=True, exist_ok=True)
    staged_logos = arcade_output / "arc_carlogo"
    merged_logos, logo_index = append_arcade_car_logo(
        (
            staged_logos.read_bytes()
            if staged_logos.is_file()
            else read_gt2_member(gt2_arcade_volume, "arcade/arc_carlogo")
        ),
        logo,
    )
    if len(merged_logos) > GT2_ARCADE_FRONTEND_ARCHIVE_SAFE_SIZE:
        raise ValueError(
            f"GT2 Arcade logo archive is {len(merged_logos):#x} bytes, "
            "larger than the reserved native frontend arena "
            f"{GT2_ARCADE_FRONTEND_ARCHIVE_SAFE_SIZE:#x}"
        )
    (arcade_output / "arc_carlogo").write_bytes(merged_logos)

    carinfo_metadata = update_gt2_localized_carinfo(
        gt2_arcade_volume,
        patch_root,
        lambda carinfo, carcolor: append_gt2_carinfo(
            carinfo,
            carcolor,
            stem,
            str(definition["displayName"]),
            tuple(definition["paintSources"]),
        ),
    )
    arcade_physics = stage_gt2_arcade_car_physics(
        gt2_arcade_volume, patch_root, definition
    )
    return {
        "stem": stem,
        "displayName": definition["displayName"],
        "modelBasisStem": model_basis,
        "modelConverted": convert_model,
        "modelConversions": model_conversions,
        "sourceModelPairSha256": hashlib.sha256(
            day_model + night_model
        ).hexdigest(),
        "nativeModelSha256": native_model_hashes,
        "sourceDayTextureSha256": hashlib.sha256(day_texture).hexdigest(),
        "sourceNightTextureSha256": hashlib.sha256(
            night_texture
        ).hexdigest(),
        "dayTextureSha256": hashlib.sha256(converted_day).hexdigest(),
        "nightTextureSha256": hashlib.sha256(converted_night).hexdigest(),
        "colorIds": list(converted_day[2 : 2 + converted_day[0]]),
        "arcadeLogoEntry": logo_entry,
        "menuLogoName": menu_logo_name,
        "arcadeLogoIndex": logo_index,
        "arcadeLogoSha256": hashlib.sha256(logo).hexdigest(),
        "arcadeClass": definition["arcadeClass"],
        "manufacturerLogoIndex": definition["manufacturerLogoIndex"],
        "ratings": list(definition["ratings"]),
        "stats": list(stats),
        "carinfo": carinfo_metadata,
        "physicsSource": physics_source,
        "arcadePhysics": arcade_physics,
    }


def stage_gt1_livery_fold(
    disc_root: Path,
    gt2_volume: Path,
    patch_root: Path,
    definition: dict[str, object],
) -> dict[str, object]:
    """Stage a GT1 visual package under one existing GT2 car identity."""

    target_stem = str(definition["targetStem"])
    source_stem = str(definition["sourceStem"])
    body_stem = str(definition["bodyStem"])
    model_basis = str(definition["modelBasisStem"])
    if any(
        entry.name.startswith(f"carobj/{body_stem}.")
        for entry in read_entries(gt2_volume)
    ):
        raise ValueError(f"GT2 alternate livery body already exists: {body_stem}")

    stems = read_gt1_car_stems(disc_root / "SYSTEM.DAT")
    day_texture, day_model, night_texture, night_model = (
        read_gt1_car_members(
            disc_root / "CAR.DAT", stems, source_stem
        )
    )
    converted_day = convert_gt1_car_texture(day_texture)
    converted_night = convert_gt1_car_texture(night_texture)
    imported_ids = [
        int(item[0]) for item in tuple(definition["paintSources"])
    ]
    day_ids = list(converted_day[2 : 2 + converted_day[0]])
    night_ids = list(converted_night[2 : 2 + converted_night[0]])
    if day_ids != night_ids or any(
        color_id not in day_ids for color_id in imported_ids
    ):
        raise ValueError(
            f"GT1 {source_stem} embedded livery IDs changed: "
            f"day={day_ids}, night={night_ids}, imported={imported_ids}"
        )

    car_output = patch_root / "carobj"
    car_output.mkdir(parents=True, exist_ok=True)
    (car_output / f"{body_stem}.cdp.gz").write_bytes(
        gzip.compress(converted_day, compresslevel=9, mtime=0)
    )
    (car_output / f"{body_stem}.cnp.gz").write_bytes(
        gzip.compress(converted_night, compresslevel=9, mtime=0)
    )
    model_conversions: dict[str, object] = {}
    model_hashes: dict[str, str] = {}
    for source_model, extension in (
        (day_model, "cdo"),
        (night_model, "cno"),
    ):
        basis = read_gt2_gzip_member(
            gt2_volume, f"carobj/{model_basis}.{extension}.gz"
        )
        converted, details = convert_gt1_car_model(source_model, basis)
        (car_output / f"{body_stem}.{extension}.gz").write_bytes(
            gzip.compress(converted, compresslevel=9, mtime=0)
        )
        model_conversions[extension] = details
        model_hashes[extension] = hashlib.sha256(converted).hexdigest()

    def fold_carinfo(
        carinfo: bytes, carcolor: bytes
    ) -> tuple[bytes, bytes, dict[str, object]]:
        carinfo, carcolor, body_metadata = append_gt2_carinfo(
            carinfo,
            carcolor,
            body_stem,
            "delete",
            tuple(definition["bodyPaintSources"]),
        )
        carinfo, carcolor, target_metadata = extend_gt2_carinfo(
            carinfo,
            carcolor,
            target_stem,
            tuple(definition["paintSources"]),
            allow_duplicate_color_ids=bool(
                definition.get("allowDuplicateColorIds", False)
            ),
        )
        return carinfo, carcolor, {
            "body": body_metadata,
            "target": target_metadata,
        }

    localized_metadata = update_gt2_localized_carinfo(
        gt2_volume, patch_root, fold_carinfo
    )
    body_carinfo_metadata = dict(localized_metadata["body"])
    color_metadata = dict(localized_metadata["target"])
    localized_digests = dict(
        localized_metadata["localizedCarinfoSha256"]
    )
    body_mappings = [
        {
            "colorId": color_id,
            "targetColorIndex": color_metadata["firstColorIndex"] + index,
            "bodyPaletteIndex": day_ids.index(color_id),
        }
        for index, color_id in enumerate(imported_ids)
    ]
    return {
        "targetStem": target_stem,
        "sourceStem": source_stem,
        "bodyStem": body_stem,
        "modelBasisStem": model_basis,
        "description": definition["description"],
        "bodyCarinfo": body_carinfo_metadata,
        **color_metadata,
        "localizedCarinfoSha256": localized_digests,
        "carcolorSha256": localized_metadata["carcolorSha256"],
        "bodyMappings": body_mappings,
        "sourceDayTextureSha256": hashlib.sha256(day_texture).hexdigest(),
        "sourceNightTextureSha256": hashlib.sha256(
            night_texture
        ).hexdigest(),
        "dayTextureSha256": hashlib.sha256(converted_day).hexdigest(),
        "nightTextureSha256": hashlib.sha256(converted_night).hexdigest(),
        "modelConversions": model_conversions,
        "modelSha256": model_hashes,
    }


def merge_gt2_car_texture_palettes(
    primary: bytes,
    additions: tuple[tuple[bytes, int], ...],
) -> bytes:
    """Append authored palettes when native GT2 textures share one bitmap."""

    if len(primary) != 0xB3A0 or not 1 <= primary[0] <= 16:
        raise ValueError("primary GT2 car texture is malformed")
    output = bytearray(primary)
    color_ids = list(primary[2 : 2 + primary[0]])
    for texture, palette_index in additions:
        if (
            len(texture) != len(primary)
            or not 0 <= palette_index < texture[0]
        ):
            raise ValueError("added GT2 car texture palette is malformed")
        if texture[0x43A0:] != primary[0x43A0:]:
            raise ValueError(
                "GT2 car texture palettes do not share their authored bitmap"
            )
        if len(color_ids) >= 16:
            raise ValueError("GT2 car texture exceeds sixteen palettes")
        source = 0x20 + palette_index * 0x240
        target = 0x20 + len(color_ids) * 0x240
        output[target : target + 0x240] = texture[
            source : source + 0x240
        ]
        color_ids.append(texture[2 + palette_index])
    output[0] = len(color_ids)
    output[2:18] = bytes(color_ids) + bytes(16 - len(color_ids))
    return bytes(output)


def stage_gt1_castrol_supra_palette_fold(
    disc_root: Path,
    gt2_volume: Path,
    patch_root: Path,
    same_stem_definition: dict[str, object],
    black_definition: dict[str, object],
) -> dict[str, object]:
    """Fold all four accepted Supra liveries into one native GT2 body."""

    if (
        same_stem_definition["targetStem"] != "tsplr"
        or same_stem_definition["sourceStem"] != "tsplr"
        or black_definition["targetStem"] != "tsplr"
        or black_definition["sourceStem"] != "t-plr"
    ):
        raise ValueError("Castrol Supra native-palette definitions changed")

    stems = read_gt1_car_stems(disc_root / "SYSTEM.DAT")
    white_members = read_gt1_car_members(
        disc_root / "CAR.DAT", stems, "tsplr"
    )
    black_members = read_gt1_car_members(
        disc_root / "CAR.DAT", stems, "t-plr"
    )
    white_day = convert_gt1_car_texture(white_members[0])
    white_night = convert_gt1_car_texture(white_members[2])
    black_day = convert_gt1_car_texture(black_members[0])
    black_night = convert_gt1_car_texture(black_members[2])
    if (
        list(white_day[2 : 2 + white_day[0]]) != [108, 113]
        or list(white_night[2 : 2 + white_night[0]]) != [108, 113]
        or list(black_day[2 : 2 + black_day[0]]) != [108, 113]
        or list(black_night[2 : 2 + black_night[0]]) != [108, 113]
    ):
        raise ValueError("GT1 Castrol Supra palette IDs changed")

    merged_day = merge_gt2_car_texture_palettes(
        white_day, ((black_day, 0), (black_day, 1))
    )
    merged_night = merge_gt2_car_texture_palettes(
        white_night, ((black_night, 0), (black_night, 1))
    )
    expected_ids = [108, 113, 108, 113]
    if (
        list(merged_day[2:6]) != expected_ids
        or list(merged_night[2:6]) != expected_ids
    ):
        raise AssertionError("Castrol Supra four-palette merge failed")

    car_output = patch_root / "carobj"
    car_output.mkdir(parents=True, exist_ok=True)
    (car_output / "tsplr.cdp.gz").write_bytes(
        gzip.compress(merged_day, compresslevel=9, mtime=0)
    )
    (car_output / "tsplr.cnp.gz").write_bytes(
        gzip.compress(merged_night, compresslevel=9, mtime=0)
    )
    model_conversions: dict[str, object] = {}
    model_hashes: dict[str, str] = {}
    for source_model, extension in (
        (white_members[1], "cdo"),
        (white_members[3], "cno"),
    ):
        basis = read_gt2_gzip_member(
            gt2_volume, f"carobj/tsplr.{extension}.gz"
        )
        converted, details = convert_gt1_car_model(source_model, basis)
        (car_output / f"tsplr.{extension}.gz").write_bytes(
            gzip.compress(converted, compresslevel=9, mtime=0)
        )
        model_conversions[extension] = details
        model_hashes[extension] = hashlib.sha256(converted).hexdigest()

    paint_sources = (
        tuple(same_stem_definition["paintSources"])
        + tuple(black_definition["paintSources"])
    )
    localized_metadata = update_gt2_localized_carinfo(
        gt2_volume,
        patch_root,
        lambda carinfo, carcolor: extend_gt2_carinfo(
            carinfo,
            carcolor,
            "tsplr",
            paint_sources,
            allow_duplicate_color_ids=True,
        ),
    )
    first_index = int(localized_metadata["firstColorIndex"])
    body_mappings = [
        {
            "colorId": color_id,
            "targetColorIndex": first_index + index,
            "bodyPaletteIndex": first_index + index,
        }
        for index, color_id in enumerate((113, 108, 113))
    ]
    return {
        "targetStem": "tsplr",
        "sourceStem": "tsplr+t-plr",
        "sourceStems": ["tsplr", "t-plr"],
        "bodyStem": "tsplr",
        "modelBasisStem": "tsplr",
        "description": (
            "GT1 Castrol Supra white/blue, black/green, and black/blue "
            "palettes folded into the existing GT2 white/green body"
        ),
        "nativePaletteFold": True,
        **localized_metadata,
        "bodyMappings": body_mappings,
        "acceptedVisualChoices": [
            "white/green",
            "white/blue",
            "black/green",
            "black/blue",
        ],
        "paletteSources": [
            {
                "targetPaletteIndex": 0,
                "sourceStem": "tsplr",
                "sourcePaletteIndex": 0,
                "colorId": 108,
                "retailEquivalent": True,
            },
            {
                "targetPaletteIndex": 1,
                "sourceStem": "tsplr",
                "sourcePaletteIndex": 1,
                "colorId": 113,
            },
            {
                "targetPaletteIndex": 2,
                "sourceStem": "t-plr",
                "sourcePaletteIndex": 0,
                "colorId": 108,
            },
            {
                "targetPaletteIndex": 3,
                "sourceStem": "t-plr",
                "sourcePaletteIndex": 1,
                "colorId": 113,
            },
        ],
        "dayTextureSha256": hashlib.sha256(merged_day).hexdigest(),
        "nightTextureSha256": hashlib.sha256(merged_night).hexdigest(),
        "sharedDayBitmapSha256": hashlib.sha256(
            merged_day[0x43A0:]
        ).hexdigest(),
        "sharedNightBitmapSha256": hashlib.sha256(
            merged_night[0x43A0:]
        ).hexdigest(),
        "modelConversions": model_conversions,
        "modelSha256": model_hashes,
    }


def _hidden_livery_body_stem(index: int) -> str:
    characters = "0123456789abcdefghijklmnopqrstuvwxyz"
    if not 0 <= index < len(characters) ** 4:
        raise ValueError("GT2 hidden livery-body index is out of range")
    suffix = []
    for _ in range(4):
        suffix.append(characters[index % len(characters)])
        index //= len(characters)
    return "z" + "".join(reversed(suffix))


def _select_gt2_paint_source(
    records: list[dict[str, object]],
    target_stem: str,
    color_id: int,
) -> str:
    candidates = [
        record
        for record in records
        if color_id in record["colorIds"]
    ]
    if not candidates:
        raise ValueError(
            f"GT2 has no native metadata source for color ID {color_id}"
        )

    def score(record: dict[str, object]) -> tuple[int, int, int, int]:
        stem = str(record["stem"])
        name = bytes(record["name"]).decode(
            "cp1252", errors="replace"
        )
        return (
            int(stem[0] == target_stem[0]),
            int(stem[:2] == target_stem[:2]),
            int(stem.endswith("n")),
            int(name.lower() != "delete"),
        )

    # Stable lexical tie-breaking makes the source choice reproducible.
    return str(
        max(
            sorted(candidates, key=lambda record: str(record["stem"])),
            key=score,
        )["stem"]
    )


def discover_gt1_livery_folds(
    disc_root: Path,
    gt2_volume: Path,
) -> list[dict[str, object]]:
    """Derive every same-car GT1-only color ID directly from the databases."""

    records = _parse_gt2_carinfo(read_gt2_member(gt2_volume, ".carinfoe"))
    by_stem = {str(record["stem"]): record for record in records}
    gtmode = read_gt2_gzip_member(
        gt2_volume, "carparam/usa_gtmode_data.dat.gz"
    )
    gtmode_blocks = _parse_gtdt_blocks(
        gtmode, GT2_GTMODE_BLOCK_COUNT
    )
    gtmode_car_block = gtmode_blocks[GT2_GTMODE_CAR_BLOCK]
    if len(gtmode_car_block) % 0x48:
        raise ValueError("GT2 GT Mode car block is malformed")
    gtmode_stems = {
        decode_gt2_car_id(
            struct.unpack_from("<I", gtmode_car_block, offset)[0]
        )
        for offset in range(0, len(gtmode_car_block), 0x48)
    }

    stems = read_gt1_car_stems(disc_root / "SYSTEM.DAT")
    used_stems = {
        Path(entry.name).name.split(".", 1)[0]
        for entry in read_entries(gt2_volume)
        if entry.name.startswith("carobj/")
    }
    definitions: list[dict[str, object]] = []
    generated_body_index = 0
    for stem in stems:
        if stem not in gtmode_stems or stem not in by_stem:
            continue
        day_texture = read_gt1_car_members(
            disc_root / "CAR.DAT", stems, stem
        )[0]
        converted = convert_gt1_car_texture(day_texture)
        gt1_ids = list(converted[2 : 2 + converted[0]])
        gt2_ids = list(by_stem[stem]["colorIds"])
        unique_ids = [
            color_id for color_id in gt1_ids if color_id not in gt2_ids
        ]
        if not unique_ids:
            continue

        override = GT1_LIVERY_FOLD_OVERRIDES.get(stem, {})
        body_stem = override.get("bodyStem")
        if body_stem is None:
            while True:
                candidate = _hidden_livery_body_stem(
                    generated_body_index
                )
                generated_body_index += 1
                if candidate not in used_stems:
                    body_stem = candidate
                    break
        body_stem = str(body_stem)
        if body_stem in used_stems:
            raise ValueError(
                f"GT2 hidden livery body stem collides: {body_stem}"
            )
        used_stems.add(body_stem)

        paint_sources = override.get("paintSources")
        if paint_sources is None:
            paint_sources = tuple(
                (
                    color_id,
                    _select_gt2_paint_source(
                        records, stem, color_id
                    ),
                )
                for color_id in unique_ids
            )
        if [int(item[0]) for item in paint_sources] != unique_ids:
            raise ValueError(
                f"GT2 {stem} livery override does not match its "
                f"database-derived IDs: {paint_sources} != {unique_ids}"
            )
        unique_sources = {
            int(color_id): str(source_stem)
            for color_id, source_stem in paint_sources
        }
        body_paint_sources = tuple(
            (
                color_id,
                (
                    stem
                    if color_id in gt2_ids
                    else unique_sources[color_id]
                ),
            )
            for color_id in gt1_ids
        )
        definitions.append(
            {
                "targetStem": stem,
                "sourceStem": stem,
                "bodyStem": body_stem,
                "modelBasisStem": stem,
                "paintSources": tuple(paint_sources),
                "bodyPaintSources": body_paint_sources,
                "gt1PaintIds": gt1_ids,
                "gt2PaintIds": gt2_ids,
                "description": override.get(
                    "description",
                    "GT1-only color IDs "
                    + ", ".join(str(item) for item in unique_ids),
                ),
            }
        )

    for source_stem in GT1_PROVEN_DISTINCT_RACING_MODIFICATION_STEMS:
        target_stem = source_stem[:-1] + "n"
        if source_stem not in stems:
            raise ValueError(
                "proven GT1 Racing Modification source is absent: "
                f"{source_stem}"
            )
        if source_stem not in by_stem:
            raise ValueError(
                "GT2 Racing Modification model basis is absent: "
                f"{source_stem}"
            )
        if target_stem not in gtmode_stems or target_stem not in by_stem:
            raise ValueError(
                "GT2 Racing Modification customer identity is absent: "
                f"{target_stem}"
            )

        source_texture = read_gt1_car_members(
            disc_root / "CAR.DAT", stems, source_stem
        )[0]
        converted_source = convert_gt1_car_texture(source_texture)
        source_ids = list(
            converted_source[2 : 2 + converted_source[0]]
        )
        if len(source_ids) != 2 or len(set(source_ids)) != 2:
            raise ValueError(
                f"GT1 {source_stem} Racing Modification paint set changed: "
                f"{source_ids}"
            )
        paint_sources = tuple(
            (
                color_id,
                _select_gt2_paint_source(
                    records, target_stem, color_id
                ),
            )
            for color_id in source_ids
        )
        while True:
            body_stem = _hidden_livery_body_stem(generated_body_index)
            generated_body_index += 1
            if body_stem not in used_stems:
                break
        used_stems.add(body_stem)
        definitions.append(
            {
                "targetStem": target_stem,
                "sourceStem": source_stem,
                "bodyStem": body_stem,
                "modelBasisStem": source_stem,
                "paintSources": paint_sources,
                "bodyPaintSources": paint_sources,
                "gt1PaintIds": source_ids,
                "gt2PaintIds": list(by_stem[target_stem]["colorIds"]),
                "allowDuplicateColorIds": True,
                "racingModificationBody": True,
                "description": (
                    "GT1-authored Racing Modification body and paints for "
                    f"existing GT2 identity {target_stem}"
                ),
            }
        )

    variant_count = sum(
        len(definition["paintSources"]) for definition in definitions
    )
    if len(definitions) != 65 or variant_count != 113:
        raise ValueError(
            "GT1/GT2 color-ID and proven Racing Modification inventory "
            "changed: "
            f"{len(definitions)} cars, {variant_count} variants"
        )

    for cross_fold in GT1_CROSS_STEM_LIVERY_FOLDS:
        source_stem = str(cross_fold["sourceStem"])
        target_stem = str(cross_fold["targetStem"])
        body_stem = str(cross_fold["bodyStem"])
        model_basis = str(cross_fold["modelBasisStem"])
        if source_stem not in stems:
            raise ValueError(
                f"GT1 cross-stem livery source is absent: {source_stem}"
            )
        if target_stem not in gtmode_stems or target_stem not in by_stem:
            raise ValueError(
                f"GT2 cross-stem livery target is absent: {target_stem}"
            )
        if model_basis not in by_stem:
            raise ValueError(
                f"GT2 cross-stem model basis is absent: {model_basis}"
            )
        if body_stem in used_stems:
            raise ValueError(
                f"GT2 cross-stem hidden body collides: {body_stem}"
            )
        used_stems.add(body_stem)

        source_texture = read_gt1_car_members(
            disc_root / "CAR.DAT", stems, source_stem
        )[0]
        converted_source = convert_gt1_car_texture(source_texture)
        source_ids = list(
            converted_source[2 : 2 + converted_source[0]]
        )
        if not source_ids:
            raise ValueError(
                f"GT1 cross-stem livery has no palettes: {source_stem}"
            )
        paint_sources = tuple(
            (
                color_id,
                _select_gt2_paint_source(
                    records, target_stem, color_id
                ),
            )
            for color_id in source_ids
        )
        definitions.append(
            {
                "targetStem": target_stem,
                "sourceStem": source_stem,
                "bodyStem": body_stem,
                "modelBasisStem": model_basis,
                "paintSources": paint_sources,
                "bodyPaintSources": paint_sources,
                "gt1PaintIds": source_ids,
                "gt2PaintIds": list(by_stem[target_stem]["colorIds"]),
                "allowDuplicateColorIds": True,
                "description": str(cross_fold["description"]),
            }
        )

    total_variant_count = sum(
        len(definition["paintSources"]) for definition in definitions
    )
    if len(definitions) != 66 or total_variant_count != 115:
        raise ValueError(
            "GT1 livery fold inventory changed after cross-stem analysis: "
            f"{len(definitions)} cars, {total_variant_count} variants"
        )
    return definitions


def stage_gt1_livery_folds(
    disc_root: Path,
    gt2_volume: Path,
    patch_root: Path,
    definitions: list[dict[str, object]],
) -> list[dict[str, object]]:
    supra_definitions = {
        str(definition["sourceStem"]): definition
        for definition in definitions
        if str(definition["targetStem"]) == "tsplr"
    }
    if set(supra_definitions) != {"tsplr", "t-plr"}:
        raise ValueError("Castrol Supra livery sources are incomplete")
    regular_definitions = [
        definition
        for definition in definitions
        if str(definition["targetStem"]) != "tsplr"
    ]
    folds = [
        stage_gt1_livery_fold(
            disc_root, gt2_volume, patch_root, definition
        )
        for definition in regular_definitions
    ]
    folds.append(
        stage_gt1_castrol_supra_palette_fold(
            disc_root,
            gt2_volume,
            patch_root,
            supra_definitions["tsplr"],
            supra_definitions["t-plr"],
        )
    )
    (patch_root / ".gtlivery").write_bytes(
        build_gt2_livery_body_table(folds)
    )
    validate_gt2_livery_layer(patch_root, folds)
    return folds


def validate_gt2_livery_layer(
    patch_root: Path,
    folds: list[dict[str, object]],
) -> None:
    """Prove every resolver record agrees with both native carinfo records."""

    for locale_name in GT2_LOCALIZED_CARINFO_DATABASES:
        records = _parse_gt2_carinfo(
            (patch_root / locale_name).read_bytes()
        )
        by_stem = {
            str(record["stem"]): record for record in records
        }
        for fold in folds:
            target_stem = str(fold["targetStem"])
            body_stem = str(fold["bodyStem"])
            target = by_stem.get(target_stem)
            body = by_stem.get(body_stem)
            if target is None or body is None:
                raise ValueError(
                    f"GT2 livery carinfo records are missing from "
                    f"{locale_name}: {target_stem}, {body_stem}"
                )
            for extension in ("cdp", "cnp", "cdo", "cno"):
                if not (
                    patch_root
                    / "carobj"
                    / f"{body_stem}.{extension}.gz"
                ).is_file():
                    raise ValueError(
                        f"GT2 livery body asset is missing: "
                        f"{body_stem}.{extension}.gz"
                    )
            for mapping in fold["bodyMappings"]:
                color_id = int(mapping["colorId"])
                target_index = int(mapping["targetColorIndex"])
                body_index = int(mapping["bodyPaletteIndex"])
                if (
                    target_index >= len(target["colorIds"])
                    or int(target["colorIds"][target_index]) != color_id
                    or body_index >= len(body["colorIds"])
                    or int(body["colorIds"][body_index]) != color_id
                ):
                    raise ValueError(
                        "GT2 livery mapping does not preserve its "
                        f"{locale_name} database color ID: "
                        f"{target_stem} ID {color_id}"
                    )
    expected_mappings = len(_gt2_livery_table_records(folds))
    table = (patch_root / ".gtlivery").read_bytes()
    magic, version, count = struct.unpack_from("<4sHH", table)
    if (
        magic != b"GTLV"
        or version != 3
        or count != expected_mappings
        or len(table) != 8 + expected_mappings * 12
    ):
        raise ValueError("GT2 livery resolver table failed layer validation")

    # These four archive-authored Castrol Supra presentations are accepted
    # content, not color-ID aliases that may be deduplicated. Lock their exact
    # resolver shape so future inventory work cannot silently drop one.
    supra_folds = [
        fold
        for fold in folds
        if str(fold["targetStem"]) == "tsplr"
    ]
    if supra_folds:
        if len(supra_folds) != 1:
            raise ValueError("Castrol Supra must be one native palette fold")
        supra = supra_folds[0]
        if (
            not supra.get("nativePaletteFold")
            or list(supra["finalColorIds"]) != [108, 113, 108, 113]
            or str(supra["bodyStem"]) != "tsplr"
        ):
            raise ValueError("Castrol Supra four-palette body changed")
        actual_records = {
            (
                str(record["targetStem"]),
                str(record["bodyStem"]),
                int(record["targetColorIndex"]),
                int(record["bodyPaletteIndex"]),
                int(record["colorId"]),
            )
            for record in _gt2_livery_table_records(folds)
        }
        required_records = {
            ("tsplr", "tsplr", 0, 0, 108),
            ("tsplr", "tsplr", 1, 1, 113),
            ("tsplr", "tsplr", 2, 2, 108),
            ("tsplr", "tsplr", 3, 3, 113),
        }
        if not required_records.issubset(actual_records):
            raise ValueError(
                "accepted white/green, white/blue, black/green, or black/blue "
                "Castrol Supra resolver choice was dropped"
            )


def named_tim_members(data: bytes) -> list[tuple[str, bytes]]:
    """Return the native TIM members from one GT1 named texture package."""
    if len(data) < 4:
        raise ValueError("truncated GT1 texture package")
    texture_count = struct.unpack_from("<I", data, 0)[0]
    directory_end = 4 + texture_count * 20
    if texture_count <= 0 or directory_end > len(data):
        raise ValueError(
            f"invalid GT1 texture package count: {texture_count}"
        )

    offsets: list[int] = []
    names: list[str] = []
    for index in range(texture_count):
        record = 4 + index * 20
        name_bytes = data[record : record + 16]
        if not name_bytes.rstrip(b"\0"):
            raise ValueError(f"GT1 texture {index} has an empty name")
        names.append(
            name_bytes.split(b"\0", 1)[0].decode("ascii", errors="strict")
        )
        offset = struct.unpack_from("<I", data, record + 16)[0]
        if offset < directory_end or offset >= len(data):
            raise ValueError(
                f"GT1 texture {index} has invalid offset {offset:#x}"
            )
        if offsets and offset <= offsets[-1]:
            raise ValueError("GT1 texture offsets are not strictly increasing")
        if data[offset : offset + 4] != b"\x10\0\0\0":
            raise ValueError(
                f"GT1 texture {index} is not a PlayStation TIM"
            )
        offsets.append(offset)

    return [
        (
            names[index],
            data[
                start : (
                    offsets[index + 1]
                    if index + 1 < len(offsets)
                    else len(data)
                )
            ],
        )
        for index, start in enumerate(offsets)
    ]


def tim_stream_members(data: bytes) -> list[bytes]:
    """Read the counted, unnamed 4-bit TIM stream used by GT2 scenery."""
    if len(data) < 4:
        raise ValueError("truncated GT2 texture package")
    count = struct.unpack_from("<I", data)[0]
    offset = 4
    members: list[bytes] = []
    for _ in range(count):
        start = offset
        if (
            offset + 8 > len(data)
            or struct.unpack_from("<II", data, offset) != (16, 8)
        ):
            raise ValueError("expected an indexed 4-bit scenery TIM")
        offset += 8
        for _ in range(2):
            if offset + 12 > len(data):
                raise ValueError("truncated scenery TIM block")
            size, _, _, width, height = struct.unpack_from("<I4H", data, offset)
            if size != 12 + width * height * 2 or offset + size > len(data):
                raise ValueError("invalid scenery TIM block size")
            offset += size
        members.append(data[start:offset])
    if offset != len(data):
        raise ValueError("unexpected trailing scenery TIM data")
    return members


def convert_gt1_sky_texture_package(data: bytes, native_template: bytes) -> bytes:
    """Keep GT1 artwork, but upload its palettes where the native BSO reads.

    The shared dawn image planes are identical in both games. Their palettes
    are not at the same VRAM addresses: leaving GT1's x=0 CLUTs unchanged made
    the GT2 BSO sample unrelated course palettes at x=624 instead.
    """
    members = named_tim_members(data)
    native_members = tim_stream_members(native_template)
    if len(members) != len(native_members):
        raise ValueError("GT1/GT2 sky TIM counts differ")
    output = bytearray(struct.pack("<I", len(members)))
    for (_, tim), native in zip(members, native_members, strict=True):
        if struct.unpack_from("<II", tim) != (16, 8):
            raise ValueError("expected an indexed 4-bit GT1 sky TIM")
        source_image = 8 + struct.unpack_from("<I", tim, 8)[0]
        native_image = 8 + struct.unpack_from("<I", native, 8)[0]
        if tim[source_image:] != native[native_image:]:
            raise ValueError("GT1/GT2 sky image planes differ")
        if (
            struct.unpack_from("<HH", tim, 16) != (16, 1)
            or native[16:20] != tim[16:20]
        ):
            raise ValueError("unexpected sky palette dimensions")
        converted = bytearray(tim)
        converted[12:16] = native[12:16]
        output.extend(converted)
    return bytes(output)


def _shift_sky_vertex_pair(value: int, amount: int) -> int:
    first = value & 0xFFF
    second = (value >> 12) & 0xFFF
    flags = value & 0xFF000000
    first += amount
    second += amount
    if not (0 <= first <= 0xFFF and 0 <= second <= 0xFFF):
        raise ValueError("GT1 sky vertex pair escaped its packed field")
    return flags | first | (second << 12)


def convert_gt1_sky_model(
    reference: bytes,
    desired: bytes,
    gt2_template: bytes,
) -> tuple[bytes, dict[str, int]]:
    """Transfer GT1 dawn3's authored colours into native GT2 BSO records.

    GT1's ``dawn``, ``dawn2`` and ``dawn3`` models are structurally identical;
    only the authored Gouraud colours in two byte-identical render pools vary.
    GT2's shipped ``dawn.bso`` is the native conversion of that same model.
    Its compiler adds four vertices, shifts the packed vertex indices by four,
    and expands each GT1 quad tail into two native triangle tails. Reapply that
    exact layout to the desired GT1 variant rather than substituting GT2 art.
    """
    if (
        not reference.startswith(b"@(#)GT-SKY")
        or not desired.startswith(b"@(#)GT-SKY")
        or struct.unpack_from("<H", reference, 14)[0] != 2
        or struct.unpack_from("<H", desired, 14)[0] != 2
    ):
        raise ValueError("GT1 sky model is not supported revision-2 GT-SKY")
    if len(reference) != 8_588 or len(desired) != len(reference):
        raise ValueError("unexpected GT1 dawn sky model size")
    if len(gt2_template) != 7_912 or not gt2_template.startswith(b"BG\0\0"):
        raise ValueError("unexpected GT2 dawn BSO template")

    source_pool = (0x660, 0xD40)
    duplicate_pool = (0x1508, 0x1BE8)
    if (
        reference[source_pool[0] : source_pool[1]]
        != reference[duplicate_pool[0] : duplicate_pool[1]]
        or desired[source_pool[0] : source_pool[1]]
        != desired[duplicate_pool[0] : duplicate_pool[1]]
    ):
        raise ValueError("GT1 dawn sky render pools are not exact duplicates")

    reference_structure = bytearray(reference)
    desired_structure = bytearray(desired)
    for start, end in (source_pool, duplicate_pool):
        reference_structure[start:end] = b"\0" * (end - start)
        desired_structure[start:end] = b"\0" * (end - start)
    if reference_structure != desired_structure:
        raise ValueError("GT1 dawn variants differ outside authored colours")

    output = bytearray(gt2_template)
    source_stride = 0x2C
    target_stride = 0x50
    source_start = source_pool[0]
    target_start = 0x870
    record_count = 40
    changed_words = 0
    for index in range(record_count):
        source = source_start + index * source_stride
        target = target_start + index * target_stride
        source_indices = struct.unpack_from("<II", reference, source)
        expected_indices = tuple(
            _shift_sky_vertex_pair(value, 4)
            for value in source_indices
        )
        if struct.unpack_from("<II", output, target) != expected_indices:
            raise ValueError(
                f"GT2 dawn BSO record {index} does not match GT1 dawn"
            )

        reference_tail = reference[source + 8 : source + source_stride]
        desired_tail = desired[source + 8 : source + source_stride]
        if (
            output[target + 8 : target + 0x2C] != reference_tail
            or output[target + 0x2C : target + target_stride]
            != reference_tail
        ):
            raise ValueError(
                f"GT2 dawn BSO tail {index} does not match GT1 dawn"
            )
        output[target + 8 : target + 0x2C] = desired_tail
        output[target + 0x2C : target + target_stride] = desired_tail
        changed_words += 2 * sum(
            reference_tail[offset : offset + 4]
            != desired_tail[offset : offset + 4]
            for offset in range(0, len(reference_tail), 4)
        )

    # GT2 adds one horizon-closing quad immediately before the converted
    # forty-record pool. It inherits the first GT1 horizon colour twice.
    reference_horizon = reference[source_start + 0x1C : source_start + 0x20]
    desired_horizon = desired[source_start + 0x1C : source_start + 0x20]
    horizon_offsets = [
        offset
        for offset in range(0x820, 0x870, 4)
        if output[offset : offset + 4] == reference_horizon
    ]
    if len(horizon_offsets) != 2:
        raise ValueError(
            "GT2 dawn BSO does not contain two inherited horizon colours"
        )
    for offset in horizon_offsets:
        output[offset : offset + 4] = desired_horizon
    changed_words += len(horizon_offsets)

    if changed_words != 242:
        raise ValueError(
            f"unexpected GT1 dawn3 colour transfer count: {changed_words}"
        )
    return bytes(output), {
        "sourceQuadCount": record_count,
        "nativeTriangleTailCount": record_count * 2,
        "transferredColourWords": changed_words,
    }


def read_gt2_member(volume: Path, name: str) -> bytes:
    matches = [entry for entry in read_entries(volume) if entry.name == name]
    if len(matches) != 1:
        raise ValueError(
            f"expected one GT2 member named {name!r}, found {len(matches)}"
        )
    entry = matches[0]
    with volume.open("rb") as stream:
        stream.seek(entry.offset)
        return stream.read(entry.size)


def read_gt2_gzip_member(volume: Path, name: str) -> bytes:
    return gzip.decompress(read_gt2_member(volume, name))


def _parse_4bpp_tim(data: bytes) -> tuple[bytes, int, int, list[int]]:
    if len(data) < 64 or struct.unpack_from("<II", data, 0) != (0x10, 8):
        raise ValueError("Arcade selection art is not a 4-bit TIM")
    clut_size = struct.unpack_from("<I", data, 8)[0]
    _, _, clut_width, clut_height = struct.unpack_from("<HHHH", data, 12)
    if clut_size != 44 or (clut_width, clut_height) != (16, 1):
        raise ValueError(
            "Arcade selection art does not have one 16-colour CLUT"
        )
    image_offset = 8 + clut_size
    image_size, _, _, width_words, height = struct.unpack_from(
        "<IHHHH", data, image_offset
    )
    width = width_words * 4
    expected_size = 12 + width * height // 2
    if image_size != expected_size or image_offset + image_size != len(data):
        raise ValueError("Arcade selection TIM payload size is inconsistent")
    pixels: list[int] = []
    for value in data[image_offset + 12 :]:
        pixels.extend((value & 0xF, value >> 4))
    return data[20:52], width, height, pixels


def build_gt2_arcade_preview(
    source_tim: bytes,
) -> tuple[bytes, dict[str, int]]:
    """Place GT1's exact route art in GT2's native 188x200 preview canvas."""
    palette, source_width, source_height, source_pixels = _parse_4bpp_tim(
        source_tim
    )
    target_width = 188
    target_height = 200
    if source_width > target_width or source_height > target_height:
        raise ValueError("GT1 Arcade route art is larger than GT2's canvas")
    left = (target_width - source_width) // 2
    top = (target_height - source_height) // 2
    target_pixels = [0] * (target_width * target_height)
    for y in range(source_height):
        source = y * source_width
        target = (top + y) * target_width + left
        target_pixels[target : target + source_width] = source_pixels[
            source : source + source_width
        ]
    packed_pixels = bytes(
        target_pixels[index] | (target_pixels[index + 1] << 4)
        for index in range(0, len(target_pixels), 2)
    )
    output = bytearray(struct.pack("<II", 0x10, 8))
    output.extend(struct.pack("<IHHHH", 44, 0, 0, 16, 1))
    output.extend(palette)
    output.extend(
        struct.pack(
            "<IHHHH",
            12 + len(packed_pixels),
            0,
            0,
            target_width // 4,
            target_height,
        )
    )
    output.extend(packed_pixels)
    if len(output) != 0x49B0:
        raise ValueError(
            f"GT2 Arcade preview has wrong size: {len(output):#x}"
        )
    return bytes(output), {
        "sourceWidth": source_width,
        "sourceHeight": source_height,
        "targetWidth": target_width,
        "targetHeight": target_height,
        "left": left,
        "top": top,
    }


def gt2_track_id(name: str) -> int:
    value = 0
    for character in name.encode("ascii"):
        value = (
            ((value << 6) & 0xFFFFFFFF)
            | (value >> 26)
        )
        value = (value + character) & 0xFFFFFFFF
    return value


def _append_arcade_course_maps(
    course_map: bytes,
    mapinfo: bytes,
    preview_tim: bytes,
) -> tuple[bytes, bytes, list[dict[str, int | str]]]:
    if len(course_map) % 0x800:
        raise ValueError("GT2 Arcade course_map is not sector aligned")
    if len(mapinfo) < 4:
        raise ValueError("GT2 Arcade course_mapinfo is truncated")
    count = struct.unpack_from("<I", mapinfo, 0)[0]
    table_end = 4 + count * 16
    if table_end > len(mapinfo):
        raise ValueError("GT2 Arcade course_mapinfo table is truncated")

    records = [
        list(struct.unpack_from("<IIII", mapinfo, 4 + index * 16))
        for index in range(count)
    ]
    strings = mapinfo[table_end:]
    added_specs = (
        ("gt1_ssr11_a", 0x00),
        ("gt1_ssr11_ar", 0x10),
        ("gt1_ssr11_2p", 0x08),
    )
    new_record_bytes = len(added_specs) * 16
    for record in records:
        record[3] += new_record_bytes

    compressed_preview = build_gt_zip(preview_tim)
    output_map = bytearray(course_map)
    added: list[dict[str, int | str]] = []
    new_strings = bytearray(strings)
    new_table_end = table_end + new_record_bytes
    for name, flags in added_specs:
        while len(output_map) % 0x800:
            output_map.append(0)
        sector = len(output_map) // 0x800
        output_map.extend(compressed_preview)
        name_offset = new_table_end + len(new_strings)
        new_strings.extend(name.encode("ascii") + b"\0")
        records.append(
            [flags, len(compressed_preview), sector, name_offset]
        )
        added.append(
            {
                "stem": name,
                "flags": flags,
                "sector": sector,
                "packedSize": len(compressed_preview),
            }
        )
    while len(output_map) % 0x800:
        output_map.append(0)

    output_info = bytearray(struct.pack("<I", len(records)))
    for record in records:
        output_info.extend(struct.pack("<IIII", *record))
    output_info.extend(new_strings)
    return bytes(output_map), bytes(output_info), added


def _merge_arcade_course_info(
    data: bytes,
    gt2_volume: Path,
) -> tuple[bytes, dict[str, object]]:
    if len(data) < 8 or data[:4] != b"CRS\0":
        raise ValueError("GT2 .crsinfo has an invalid header")
    version, count = struct.unpack_from("<HH", data, 4)
    if version != 2:
        raise ValueError(f"unsupported GT2 .crsinfo version: {version}")
    table_end = 8 + count * 24
    if table_end > len(data):
        raise ValueError("GT2 .crsinfo table is truncated")

    records = [
        list(struct.unpack_from("<IIBBH6H", data, 8 + index * 24))
        for index in range(count)
    ]
    strings = bytearray(data[table_end:])
    base_sky_members = [
        entry.name
        for entry in read_entries(gt2_volume)
        if entry.name.startswith("bgsobj/")
        and entry.name.endswith(".bso.gz")
    ]
    base_sky_stems = [
        Path(name).name.removesuffix(".bso.gz")
        for name in base_sky_members
    ]
    if len(base_sky_stems) != 34 or len(set(base_sky_stems)) != 34:
        raise ValueError("unexpected GT2 Arcade background directory")
    merged_sky_stems = sorted(base_sky_stems + [GT2_SSR11_SKY_STEM])
    ssr11_sky_index = merged_sky_stems.index(GT2_SSR11_SKY_STEM)
    old_to_new_sky = {
        old: merged_sky_stems.index(stem)
        for old, stem in enumerate(base_sky_stems)
    }

    base_course_members = [
        entry.name
        for entry in read_entries(gt2_volume)
        if entry.name.startswith("crsobj/")
        and entry.name.endswith(".tro.gz")
    ]
    base_course_stems = [
        Path(name).name.removesuffix(".tro.gz")
        for name in base_course_members
    ]
    if len(base_course_stems) != count or len(set(base_course_stems)) != count:
        raise ValueError(
            "GT2 .crsinfo and crsobj directory counts do not match"
        )
    for index, (stem, record) in enumerate(
        zip(base_course_stems, records)
    ):
        if record[1] != gt2_track_id(stem):
            raise ValueError(
                f"GT2 .crsinfo record {index} does not match crsobj/{stem}"
            )

    added_specs = (
        ("gt1_ssr11", 0x41),
        ("gt1_ssr11_2p", 0x49),
        ("gt1_ssr11_a", 0x41),
        ("gt1_ssr11_ar", 0x51),
        ("gt1_ssr11_hifi", 0x41),
        ("gt1_ssr11_r", 0x51),
    )
    merged_course_stems = sorted(
        base_course_stems + [name for name, _ in added_specs]
    )
    record_growth = len(added_specs) * 24
    for record in records:
        record[0] += record_growth
        old_sky = record[4]
        if old_sky not in old_to_new_sky:
            raise ValueError(
                f"GT2 .crsinfo references unknown sky index {old_sky}"
            )
        record[4] = old_to_new_sky[old_sky]

    source_hash = gt2_track_id("highway")
    templates = [record for record in records if record[1] == source_hash]
    if len(templates) != 1:
        raise ValueError(
            "cannot identify GT2 Special Stage Route 5 template"
        )
    template = templates[0]
    string_offset = table_end + record_growth + len(strings)
    strings.extend(b"Special Stage Route 11\0")
    records_by_stem = dict(zip(base_course_stems, records))
    for name, flags in added_specs:
        record = list(template)
        record[0] = string_offset
        record[1] = gt2_track_id(name)
        record[2] = flags
        record[3] = 0
        record[4] = ssr11_sky_index
        records_by_stem[name] = record
    records = [records_by_stem[name] for name in merged_course_stems]

    output = bytearray(b"CRS\0")
    output.extend(struct.pack("<HH", version, len(records)))
    for record in records:
        output.extend(struct.pack("<IIBBH6H", *record))
    output.extend(strings)
    return bytes(output), {
        "sourceCount": count,
        "mergedCount": len(records),
        "skyStem": GT2_SSR11_SKY_STEM,
        "skyIndex": ssr11_sky_index,
        "shiftedSkyIndices": sum(
            old != new for old, new in old_to_new_sky.items()
        ),
        "courseStems": [name for name, _ in added_specs],
    }


def integrate_ssr11_arcade_menu(
    disc_root: Path,
    gt2_volume: Path,
    patch_root: Path,
) -> dict[str, object]:
    archive, entries = read_gtarc(disc_root / "ARCADE.DAT")
    if GT1_ARCADE_SSR11_ENTRY >= len(entries):
        raise ValueError("GT1 ARCADE.DAT does not contain SSR11 entry 81")
    source_tim = unpack_entry(
        archive, entries[GT1_ARCADE_SSR11_ENTRY]
    )
    preview_tim, placement = build_gt2_arcade_preview(source_tim)
    course_map, mapinfo, added_maps = _append_arcade_course_maps(
        read_gt2_member(gt2_volume, "arcade/course_map"),
        read_gt2_member(gt2_volume, "arcade/course_mapinfo"),
        preview_tim,
    )
    course_info, course_info_metadata = _merge_arcade_course_info(
        read_gt2_member(gt2_volume, ".crsinfo"),
        gt2_volume,
    )
    arcade_output = patch_root / "arcade"
    arcade_output.mkdir(parents=True, exist_ok=True)
    (arcade_output / "course_map").write_bytes(course_map)
    (arcade_output / "course_mapinfo").write_bytes(mapinfo)
    (patch_root / ".crsinfo").write_bytes(course_info)
    return {
        "sourceEntry": GT1_ARCADE_SSR11_ENTRY,
        "sourceTimSize": len(source_tim),
        "sourceTimSha256": hashlib.sha256(source_tim).hexdigest(),
        "nativeTimSize": len(preview_tim),
        "nativeTimSha256": hashlib.sha256(preview_tim).hexdigest(),
        "placement": placement,
        "mapEntries": added_maps,
        "courseInfo": course_info_metadata,
    }


def _read_gt2_overlay_container(
    path: Path,
) -> tuple[list[bytes], list[bytes]]:
    data = path.read_bytes()
    if len(data) < 48 or struct.unpack_from("<I", data, 0)[0] != 48:
        raise ValueError(f"GT2.OVL has an invalid header: {path}")
    packed: list[bytes] = []
    unpacked: list[bytes] = []
    for index in range(6):
        offset, size = struct.unpack_from("<II", data, index * 8)
        if offset < 48 or size <= 0 or offset + size > len(data):
            raise ValueError(f"GT2.OVL entry {index} is invalid: {path}")
        member = data[offset : offset + size]
        packed.append(member)
        unpacked.append(gzip.decompress(member))
    return packed, unpacked


def _write_gt2_overlay_container(
    destination: Path,
    packed: list[bytes],
) -> None:
    if len(packed) != 6:
        raise ValueError("GT2.OVL requires exactly six members")
    output = bytearray(48)
    for index, member in enumerate(packed):
        offset = len(output)
        struct.pack_into("<II", output, index * 8, offset, len(member))
        output.extend(member)
        while len(output) % 4:
            output.append(0)
    destination.write_bytes(output)


def patch_ssr11_arcade_overlay(
    source: Path,
    destination: Path,
    arcade_cars: tuple[dict[str, object], ...] = (),
) -> dict[str, object]:
    """Extend GT2's native null-terminated Arcade course tables."""
    packed, unpacked = _read_gt2_overlay_container(source)
    arcade = bytearray(unpacked[2])
    if len(arcade) != 275_312:
        raise ValueError(
            f"unexpected GT2 Arcade overlay size: {len(arcade)}"
        )
    memory_base = 0x80010000
    address_base = 0x80050000

    # Stock code at 0x80013DDC constructs 0x800F84C0 with LUI/ADDIU before
    # expanding `arcade_data.dat`. Relocate that persistent database into the
    # unified port's reserved devkit-RAM arena. The following A1=0xB000 delay
    # slot is intentionally unchanged: func_80076D14 overwrites A1 with A0
    # before its first call, so it never acts as a size bound.
    database_lui_offset = 0x3DDC
    database_addiu_offset = 0x3DE0
    expected_database_instructions = (0x3C048010, 0x248484C0)
    actual_database_instructions = struct.unpack_from(
        "<2I", arcade, database_lui_offset
    )
    if actual_database_instructions != expected_database_instructions:
        raise ValueError(
            "GT2 Arcade database destination instructions changed: "
            f"{tuple(hex(value) for value in actual_database_instructions)}"
        )
    database_hi = (GT2_ARCADE_DATABASE_ADDRESS >> 16) & 0xFFFF
    database_lo = GT2_ARCADE_DATABASE_ADDRESS & 0xFFFF
    if database_lo >= 0x8000:
        database_hi = (database_hi + 1) & 0xFFFF
    struct.pack_into(
        "<2I",
        arcade,
        database_lui_offset,
        0x3C040000 | database_hi,
        0x24840000 | database_lo,
    )

    # The stock frontend keeps arc_carlogo at 0x80129520 and four related
    # descriptors immediately below it. Its loader allows exactly 0x66000
    # bytes. The thirteenth Class B car grows the native archive to 0x6630C,
    # so merely increasing the byte count would overwrite retail game state.
    # Relocate the complete address family, not only the loader destination,
    # into a dedicated devkit-RAM MiB and raise the loader bound to 0xF0000.
    frontend_lui_instructions = (
        (0x36FC, 0x3C028013),
        (0x3888, 0x3C058013),
        (0x3AE8, 0x3C048013),
        (0x3B14, 0x3C048013),
        (0x3B38, 0x3C048013),
        (0x3CFC, 0x3C108013),
        (0x3E54, 0x3C048013),
        (0x3F28, 0x3C148013),
        (0x3FBC, 0x3C038013),
        (0x3FE8, 0x3C038013),
        (0x4040, 0x3C038013),
        (0x4440, 0x3C048013),
        (0x45FC, 0x3C048013),
        (0x4918, 0x3C058013),
        (0x5174, 0x3C058013),
        (0x8720, 0x3C058013),
        (0x12A68, 0x3C058013),
    )
    frontend_anchor_hi = (
        GT2_ARCADE_FRONTEND_ANCHOR >> 16
    ) & 0xFFFF
    for instruction_offset, expected_instruction in (
        frontend_lui_instructions
    ):
        actual_instruction = struct.unpack_from(
            "<I", arcade, instruction_offset
        )[0]
        if actual_instruction != expected_instruction:
            raise ValueError(
                "GT2 Arcade frontend address instruction changed at "
                f"{instruction_offset:#x}: {actual_instruction:#x}"
            )
        struct.pack_into(
            "<I",
            arcade,
            instruction_offset,
            (actual_instruction & 0xFFFF0000) | frontend_anchor_hi,
        )

    frontend_size_instructions = (
        (0x4920, 0x3C060006, 0x3C06000F),
        (0x4928, 0x34C66000, 0x34C60000),
    )
    for instruction_offset, expected_instruction, replacement in (
        frontend_size_instructions
    ):
        actual_instruction = struct.unpack_from(
            "<I", arcade, instruction_offset
        )[0]
        if actual_instruction != expected_instruction:
            raise ValueError(
                "GT2 Arcade frontend size instruction changed at "
                f"{instruction_offset:#x}: {actual_instruction:#x}"
            )
        struct.pack_into("<I", arcade, instruction_offset, replacement)

    table_specs = (
        (
            "roadForward",
            0x40730,
            21,
            "gt1_ssr11_a",
            0x00,
            (0xD218, 0x128FC),
        ),
        (
            "roadReverse",
            0x409F0,
            21,
            "gt1_ssr11_ar",
            0x10,
            (0xD240, 0x12918),
        ),
        (
            "trialForward",
            0x40CB0,
            23,
            "gt1_ssr11_a",
            0x00,
            (0xD258, 0x12924),
        ),
        (
            "trialReverse",
            0x40FB0,
            23,
            "gt1_ssr11_ar",
            0x10,
            (0xD270, 0x12940),
        ),
        (
            "twoPlayer",
            0x413F0,
            21,
            "gt1_ssr11_2p",
            0x08,
            (0xD2A0, 0x12980),
        ),
    )
    for _, start, count, _, _, _ in table_specs:
        sentinel = start + count * 32
        if arcade[sentinel : sentinel + 32] != b"\0" * 32:
            raise ValueError(
                f"GT2 Arcade course table at {start:#x} has no sentinel"
            )

    while len(arcade) % 16:
        arcade.append(0)

    def append_c_string(value: str) -> int:
        offset = len(arcade)
        arcade.extend(value.encode("ascii") + b"\0")
        return memory_base + offset

    stem_addresses = {
        stem: append_c_string(stem)
        for stem in ("gt1_ssr11_a", "gt1_ssr11_ar", "gt1_ssr11_2p")
    }
    display_address = append_c_string("Special Stage Route 11")
    while len(arcade) % 4:
        arcade.append(0)
    stats_offset = len(arcade)
    # Native GT1 layout: total length, elevation change, longest straight,
    # and corner count. The exact Route 11 route/model supplies a 4.889 km
    # lap; the remaining values are retained as conservative menu metadata
    # until the GT1 centreline analysis is promoted into the converter.
    arcade.extend(struct.pack("<4H", 4889, 0, 806, 20))
    stats_address = memory_base + stats_offset
    while len(arcade) % 16:
        arcade.append(0)

    table_metadata: list[dict[str, int | str]] = []
    for label, source_start, count, stem, flags, instructions in table_specs:
        source_records = arcade[
            source_start : source_start + count * 32
        ]
        insertion_index = 0
        while insertion_index < count:
            unlock = struct.unpack_from(
                "<I", source_records, insertion_index * 32 + 20
            )[0]
            if unlock != 0xFFFF:
                break
            insertion_index += 1
        table_offset = len(arcade)
        insertion_offset = insertion_index * 32
        arcade.extend(source_records[:insertion_offset])
        arcade.extend(
            struct.pack(
                "<8I",
                stem_addresses[stem],
                display_address,
                stats_address,
                flags,
                0,
                0xFFFF,
                0xFFFFFFFF,
                0,
            )
        )
        arcade.extend(source_records[insertion_offset:])
        arcade.extend(b"\0" * 32)
        table_address = memory_base + table_offset
        relative = table_address - address_base
        if not (0 <= relative <= 0x7FFF):
            raise ValueError(
                f"extended Arcade course table escaped addiu range: "
                f"{table_address:#x}"
            )
        for instruction in instructions:
            if struct.unpack_from("<H", arcade, instruction)[0] not in (
                0x0730,
                0x09F0,
                0x0CB0,
                0x0FB0,
                0x13F0,
            ):
                raise ValueError(
                    f"GT2 Arcade table instruction changed at "
                    f"{instruction:#x}"
                )
            struct.pack_into("<H", arcade, instruction, relative)
        table_metadata.append(
            {
                "label": label,
                "sourceOffset": source_start,
                "sourceCount": count,
                "mergedOffset": table_offset,
                "mergedAddress": table_address,
                "relativeAddress": relative,
                "mergedCount": count + 1,
                "stem": stem,
                "insertionIndex": insertion_index,
            }
        )

    car_roster_metadata: list[dict[str, object]] = []
    roster_pointer_tables = (
        (0x41F14, 4, "stemPointers"),
        (0x41F30, 1, "unlocked"),
        (0x41F4C, 2, "logoIndices"),
        (0x41F68, 2, "manufacturerLogoIndices"),
        (0x41F84, 3, "ratings"),
        (0x41FA0, 10, "stats"),
    )
    for car in arcade_cars:
        class_index = int(car["arcadeClass"])
        replace_for_livery_smoke = bool(
            car.get("developerLiverySmoke", False)
        )
        if not 0 <= class_index < 7:
            raise ValueError(f"invalid GT2 Arcade class index: {class_index}")
        count_offset = 0x419A0 + class_index * 2
        old_count = struct.unpack_from("<H", arcade, count_offset)[0]
        if old_count <= 0:
            raise ValueError(
                f"GT2 Arcade class {class_index} has no native roster"
            )
        if (
            not replace_for_livery_smoke
            and class_index <= 3
            and old_count >= GT2_ARCADE_PROVEN_CLASS_CAPACITY
        ):
            raise ValueError(
                f"GT2 Arcade class {class_index} already has {old_count} "
                "cars; native frontend capacity above "
                f"{GT2_ARCADE_PROVEN_CLASS_CAPACITY} is not proven"
            )

        while len(arcade) % 4:
            arcade.append(0)
        stem_address = memory_base + len(arcade)
        stem = str(car["stem"])
        arcade.extend(stem.encode("ascii") + b"\0\0\0")

        values = (
            struct.pack("<I", stem_address),
            b"\x01",
            struct.pack("<H", int(car["arcadeLogoIndex"])),
            struct.pack("<H", int(car["manufacturerLogoIndex"])),
            bytes(int(value) for value in car["ratings"]),
            struct.pack("<5H", *(int(value) for value in car["stats"])),
        )
        patched_tables: list[dict[str, object]] = []
        for (pointer_table, stride, label), appended in zip(
            roster_pointer_tables, values
        ):
            pointer_offset = pointer_table + class_index * 4
            source_address = struct.unpack_from(
                "<I", arcade, pointer_offset
            )[0]
            source_offset = source_address - memory_base
            size = old_count * stride
            if (
                source_offset < 0
                or source_offset + size > len(arcade)
                or len(appended) != stride
            ):
                raise ValueError(
                    f"GT2 Arcade {label} table is invalid for class "
                    f"{class_index}"
                )
            while len(arcade) % 4:
                arcade.append(0)
            source_table = bytes(
                arcade[source_offset : source_offset + size]
            )
            merged_offset = len(arcade)
            if replace_for_livery_smoke:
                merged_table = bytearray(source_table)
                replacement_offset = (old_count - 1) * stride
                merged_table[
                    replacement_offset : replacement_offset + stride
                ] = appended
                arcade.extend(merged_table)
            else:
                arcade.extend(source_table)
                arcade.extend(appended)
            merged_address = memory_base + merged_offset
            struct.pack_into("<I", arcade, pointer_offset, merged_address)
            patched_tables.append(
                {
                    "label": label,
                    "sourceAddress": source_address,
                    "mergedAddress": merged_address,
                    "stride": stride,
                    "replacementIndex": (
                        old_count - 1
                        if replace_for_livery_smoke
                        else None
                    ),
                }
            )
        new_count = old_count if replace_for_livery_smoke else old_count + 1
        struct.pack_into("<H", arcade, count_offset, new_count)
        car_roster_metadata.append(
            {
                "stem": stem,
                "classIndex": class_index,
                "oldCount": old_count,
                "newCount": new_count,
                "developerReplacement": replace_for_livery_smoke,
                "replacementIndex": (
                    old_count - 1 if replace_for_livery_smoke else None
                ),
                "stemAddress": stem_address,
                "tables": patched_tables,
            }
        )

    packed[2] = gzip.compress(bytes(arcade), compresslevel=9, mtime=0)
    _write_gt2_overlay_container(destination, packed)
    _, verified = _read_gt2_overlay_container(destination)
    if verified[2] != bytes(arcade):
        raise ValueError("rebuilt GT2.OVL failed its round-trip check")
    return {
        "sourceSize": source.stat().st_size,
        "outputSize": destination.stat().st_size,
        "sourceArcadeOverlaySize": len(unpacked[2]),
        "outputArcadeOverlaySize": len(arcade),
        "displayName": "Special Stage Route 11",
        "stats": {
            "totalLength": 4889,
            "elevationChange": 0,
            "longestStraight": 806,
            "cornerCount": 20,
        },
        "tables": table_metadata,
        "carRosters": car_roster_metadata,
        "arcadeDatabaseArena": {
            "stockAddress": 0x800F84C0,
            "address": GT2_ARCADE_DATABASE_ADDRESS,
            "size": GT2_ARCADE_DATABASE_SAFE_SIZE,
            "instructionOffsets": [
                database_lui_offset,
                database_addiu_offset,
            ],
        },
        "arcadeFrontendArena": {
            "stockArchiveAddress": 0x80129520,
            "stockArchiveSize": 0x66000,
            "descriptorAddress": GT2_ARCADE_FRONTEND_DESCRIPTOR_ADDRESS,
            "archiveAddress": GT2_ARCADE_FRONTEND_ARCHIVE_ADDRESS,
            "archiveSize": GT2_ARCADE_FRONTEND_ARCHIVE_SAFE_SIZE,
            "archiveEnd": (
                GT2_ARCADE_FRONTEND_ARCHIVE_ADDRESS
                + GT2_ARCADE_FRONTEND_ARCHIVE_SAFE_SIZE
            ),
            "addressInstructionOffsets": [
                offset for offset, _ in frontend_lui_instructions
            ],
            "sizeInstructionOffsets": [
                offset for offset, _, _ in frontend_size_instructions
            ],
        },
    }


def convert_gt1_texture_package(
    data: bytes,
    *,
    reserved_cluts: frozenset[tuple[int, int]] = frozenset(),
) -> tuple[bytes, bytes, TextureRelocation]:
    """Convert a named GT1 course package into native GT2 TRP/crsmap data.

    The second named TIM is already the native in-race course map (the shared
    GT1/GT2 High Speed Ring files are byte-identical), so it belongs in
    ``crsmap`` rather than the course texture atlas. GT2 packs the remaining
    4-bit images into twelve protected 256x256 texture pages at x=640..1023
    and its CLUTs into x=496..703, y=496..511. Reproduce that layout, excluding
    any palette slots used by the accompanying sky, and return the relocation
    metadata needed to rewrite the real polygon packets.
    """
    textures: list[dict[str, object]] = []
    for name, member in named_tim_members(data):
        flags = struct.unpack_from("<I", member, 4)[0]
        if flags != 8:
            raise ValueError(
                f"unsupported GT1 course TIM flags for {name}: "
                f"{flags:#x}"
            )
        clut_block_size = struct.unpack_from("<I", member, 8)[0]
        clut_x, clut_y, clut_width, clut_height = struct.unpack_from(
            "<HHHH", member, 12
        )
        if clut_width != 16 or clut_height != 1:
            raise ValueError(
                f"unsupported GT1 course CLUT dimensions: "
                f"{clut_width}x{clut_height}"
            )
        image_block = 8 + clut_block_size
        image_x, image_y, image_width, image_height = struct.unpack_from(
            "<HHHH", member, image_block + 4
        )
        if image_width <= 0 or image_width > 64 or image_height <= 0:
            raise ValueError(
                f"unsupported GT1 course image dimensions for "
                f"{name}: {image_width}x{image_height}"
            )
        textures.append(
            {
                "name": name,
                "data": bytearray(member),
                "clut": (clut_x, clut_y),
                "image": (image_x, image_y, image_width, image_height),
            }
        )

    if len(textures) < 3:
        raise ValueError("GT1 course package has no map/atlas payload")
    course_map = bytes(textures[1]["data"])
    atlas = textures[:1] + textures[2:]

    unique_cluts = list(
        dict.fromkeys(texture["clut"] for texture in atlas)
    )
    clut_columns = 13
    clut_slots = [
        (496 + column * 16, 496 + row)
        for row in range(16)
        for column in range(clut_columns)
        if (496 + column * 16, 496 + row) not in reserved_cluts
    ]
    if len(unique_cluts) > len(clut_slots):
        raise ValueError(
            f"GT1 course needs {len(unique_cluts)} CLUT slots; "
            f"GT2 bank has {len(clut_slots)} unreserved slots"
        )
    relocated_coordinates = {
        coordinate: clut_slots[index]
        for index, coordinate in enumerate(unique_cluts)
    }
    clut_id_map = {
        old_y * 64 + old_x // 16: new_y * 64 + new_x // 16
        for (old_x, old_y), (new_x, new_y)
        in relocated_coordinates.items()
    }

    # GT1 already packs all scenery into the same twelve x=640..1023 pages
    # reserved by GT2. Preserve those placements exactly: overlapping source
    # rectangles intentionally describe the final VRAM upload state, so
    # separating/repacking every named TIM changes which pixels a polygon
    # samples. Only ``refrect.tim`` lives in GT1's x=384 HUD page. Move that
    # single image into the first free rectangle in the native course bank.
    occupancy = [
        [[False] * 256 for _ in range(64)]
        for _ in range(12)
    ]
    placements: dict[str, tuple[int, int]] = {}
    for item in atlas[1:]:
        image_x, image_y, width, height = item["image"]
        if (
            image_x < 640
            or image_x + width > 1024
            or image_y + height > 512
            or image_x // 64 != (image_x + width - 1) // 64
            or image_y // 256 != (image_y + height - 1) // 256
        ):
            raise ValueError(
                f"GT1 course image is outside a native GT2 texture page: "
                f"{item['name']} at {item['image']}"
            )
        page = (image_y // 256) * 6 + image_x // 64 - 10
        local_x = image_x % 64
        local_y = image_y % 256
        for x in range(local_x, local_x + width):
            for y in range(local_y, local_y + height):
                occupancy[page][x][y] = True
        placements[item["name"]] = (image_x, image_y)

    # GT2's palettes occupy x=496..703 at y=496..511, including the lower
    # sixteen rows of the x=640 texture page.
    for x in range(64):
        for y in range(240, 256):
            occupancy[6][x][y] = True

    reflection = atlas[0]
    _, _, reflection_width, reflection_height = reflection["image"]
    reflection_placement: tuple[int, int] | None = None
    for page in range(12):
        for local_y in range(257 - reflection_height):
            for local_x in range(65 - reflection_width):
                if all(
                    not occupancy[page][x][y]
                    for x in range(local_x, local_x + reflection_width)
                    for y in range(local_y, local_y + reflection_height)
                ):
                    reflection_placement = (
                        640 + (page % 6) * 64 + local_x,
                        (page // 6) * 256 + local_y,
                    )
                    break
            if reflection_placement is not None:
                break
        if reflection_placement is not None:
            break
    if reflection_placement is None:
        raise ValueError("GT1 reflection texture does not fit GT2's atlas")
    placements[reflection["name"]] = reflection_placement

    relocations: list[TextureImageRelocation] = []
    output = bytearray(struct.pack("<I", len(atlas)))
    for item in sorted(atlas, key=lambda texture: texture["name"].lower()):
        texture = item["data"]
        old_x, old_y = item["clut"]
        new_x, new_y = relocated_coordinates[(old_x, old_y)]
        struct.pack_into("<HH", texture, 12, new_x, new_y)
        image_block = 8 + struct.unpack_from("<I", texture, 8)[0]
        source_x, source_y, width, height = item["image"]
        image_x, image_y = placements[item["name"]]
        struct.pack_into("<HH", texture, image_block + 4, image_x, image_y)
        output.extend(texture)
        relocations.append(
            TextureImageRelocation(
                name=item["name"],
                old_x=source_x,
                old_y=source_y,
                width=width,
                height=height,
                old_clut_id=old_y * 64 + old_x // 16,
                new_x=image_x,
                new_y=image_y,
            )
        )
    return (
        bytes(output),
        course_map,
        TextureRelocation(clut_id_map, tuple(relocations)),
    )


class Gt1CourseDeserializer:
    """Rebuild the pointer metadata written by GT1's revision-28 loader.

    GT1 stores course structures in traversal order and leaves their pointer
    slots zeroed.  Its loader (SCUS-94194 0x8002452c..0x80024b14) walks those
    structures and fills the slots before the renderer sees them.  GT2
    revision 31 stores the same metadata as file-relative offsets and merely
    relocates it at load time.  Replaying the GT1 walk offline therefore
    produces a native relocatable course rather than requiring a runtime
    compatibility hook.
    """

    OBJECT_POOL_SIZES = (12, 12, 20, 24, 12, 12, 20, 24)
    MODEL_POOL_SIZES = (12, 12, 20, 24, 24, 24, 32, 36)
    IMPORTED_SCREEN_BILLBOARD_MARKER = 0x31525353  # "SSR1"

    def __init__(
        self,
        data: bytes,
        texture_relocation: TextureRelocation | None = None,
        mark_imported_screen_billboards: bool = False,
    ):
        self.data = bytearray(data)
        self.pointer_fields: set[int] = set()
        self.texture_relocation = texture_relocation
        self.mark_imported_screen_billboards = mark_imported_screen_billboards
        self.relocated_texture_packets = 0
        self.converted_auxiliary_anchors = 0
        self.marked_screen_billboards = 0

    def require(self, offset: int, size: int = 1) -> None:
        if offset < 0 or size < 0 or offset + size > len(self.data):
            raise ValueError(
                f"GT1 course traversal escaped the payload: "
                f"offset={offset:#x} size={size:#x} length={len(self.data):#x}"
            )

    def u16(self, offset: int) -> int:
        self.require(offset, 2)
        return struct.unpack_from("<H", self.data, offset)[0]

    def u32(self, offset: int) -> int:
        self.require(offset, 4)
        return struct.unpack_from("<I", self.data, offset)[0]

    def write_u32(self, offset: int, value: int) -> None:
        self.require(offset, 4)
        struct.pack_into("<I", self.data, offset, value & 0xFFFFFFFF)

    def write_i16(self, offset: int, value: int) -> None:
        self.require(offset, 2)
        if value < -0x8000 or value > 0x7FFF:
            raise ValueError(
                f"GT1 course value escaped int16 at {offset:#x}: {value}"
            )
        struct.pack_into("<h", self.data, offset, value)

    def write_pointer(self, offset: int, target: int) -> None:
        self.require(target, 0)
        self.write_u32(offset, target)
        self.pointer_fields.add(offset)

    def remap_texture_packet(self, base: int, uv_offsets: tuple[int, ...]) -> None:
        relocation = self.texture_relocation
        if relocation is None:
            return

        old_clut = self.u16(base + 2)
        old_tpage = self.u16(base + 6)
        old_page_x = (old_tpage & 0x0F) * 64
        old_page_y = 256 if old_tpage & 0x10 else 0
        coordinates = [
            (
                old_page_x * 4 + self.data[base + offset],
                old_page_y + self.data[base + offset + 1],
            )
            for offset in uv_offsets
        ]
        candidates = [
            image
            for image in relocation.images
            if image.old_clut_id == old_clut
            and (image.old_x // 64) * 64 == old_page_x
            and all(
                image.old_x * 4 <= x < (image.old_x + image.width) * 4
                and image.old_y <= y < image.old_y + image.height
                for x, y in coordinates
            )
        ]
        if not candidates:
            raise ValueError(
                f"cannot resolve GT1 texture packet at {base:#x}: "
                f"clut={old_clut:#06x} tpage={old_tpage:#06x} "
                f"uv={coordinates}"
            )
        image = min(
            candidates,
            key=lambda item: (item.width * item.height, item.name),
        )
        new_page_x = (image.new_x // 64) * 64
        new_page_y = 256 if image.new_y >= 256 else 0
        for offset, (old_x, old_y) in zip(uv_offsets, coordinates):
            new_u = image.new_x * 4 + (old_x - image.old_x * 4)
            new_v = image.new_y + (old_y - image.old_y)
            new_u -= new_page_x * 4
            new_v -= new_page_y
            if not (0 <= new_u <= 0xFF and 0 <= new_v <= 0xFF):
                raise ValueError(
                    f"relocated GT1 UV escaped its texture page for "
                    f"{image.name}: ({new_u}, {new_v})"
                )
            self.data[base + offset] = new_u
            self.data[base + offset + 1] = new_v
        new_tpage = (
            (old_tpage & ~0x1F)
            | ((new_page_x // 64) & 0x0F)
            | (0x10 if new_page_y else 0)
        )
        struct.pack_into("<H", self.data, base + 6, new_tpage)
        try:
            new_clut = relocation.clut_ids[old_clut]
        except KeyError as error:
            raise ValueError(
                f"GT1 texture packet uses unstaged CLUT {old_clut:#06x}"
            ) from error
        struct.pack_into("<H", self.data, base + 2, new_clut)
        self.relocated_texture_packets += 1

    def remap_texture_packets(self, primary: int, models: int) -> None:
        if self.texture_relocation is None:
            return

        descriptors = self.u32(primary + 8)
        for index in range(self.u16(primary + 6)):
            descriptor = descriptors + index * 32
            self.remap_texture_packet(descriptor, (0, 4, 8, 10))
            self.remap_texture_packet(descriptor + 16, (0, 4, 8, 10))

        for model_index in range(self.u32(models)):
            model = self.u32(models + 4 + model_index * 4)
            for pool_type in range(4, 8):
                count = self.u16(model + 0x30 + pool_type * 2)
                size = self.MODEL_POOL_SIZES[pool_type]
                records = self.u32(model + 4 + pool_type * 4)
                uv_offsets = (0, 4, 8) if pool_type < 6 else (0, 4, 8, 10)
                for record_index in range(count):
                    self.remap_texture_packet(
                        records + record_index * size + 12,
                        uv_offsets,
                    )

    def deserialize_dynamic_objects(self) -> int:
        cursor = 0x194
        for index in range(33):
            self.write_pointer(0x110 + index * 4, cursor)
            count = self.u32(cursor)
            for object_index in range(count):
                item = cursor + 4 + object_index * 28
                # Common GT1/GT2 tracks preserve the first 24 bytes and
                # negate the final signed axis coordinate.
                self.write_u32(item + 24, -self.u32(item + 24))
            cursor += 4 + count * 28
            self.require(cursor, 0)
        return cursor

    def deserialize_object_pool(self, base: int) -> int:
        cursor = base + 0x44
        self.write_pointer(base, cursor)
        cursor += self.u32(base + 0x2C) * 8
        for index, item_size in enumerate(self.OBJECT_POOL_SIZES):
            self.write_pointer(base + 4 + index * 4, cursor)
            cursor += self.u16(base + 0x30 + index * 2) * item_size
            self.require(cursor, 0)
        self.write_pointer(base + 0x24, cursor)
        cursor += self.u16(base + 0x40) * 16
        self.write_pointer(base + 0x28, cursor)
        cursor += self.u16(base + 0x42) * 20
        self.require(cursor, 0)
        return cursor

    def deserialize_index_pool(self, base: int, owner: int) -> int:
        cursor = base + 0x68
        for index in range(16):
            self.write_pointer(base + 0x28 + index * 4, cursor)
            count = self.u16(base + 8 + index * 2)
            for _ in range(count):
                descriptor = self.u32(cursor)
                pool_index = (descriptor >> 16) & 7
                item_index = descriptor & 0xFFFF
                pool = self.u32(owner + 0xA8 + pool_index * 4)
                target = (
                    pool
                    + item_index * self.OBJECT_POOL_SIZES[pool_index]
                )
                self.write_pointer(cursor, target)
                cursor += 4
        return cursor

    def deserialize_primary(self, base: int) -> int:
        count = self.u16(base + 4)
        cursor = base + 0x0C + count * 4
        objects: list[int] = []
        for index in range(count):
            obj = cursor
            objects.append(obj)
            self.write_pointer(base + 0x0C + index * 4, obj)

            pool_a = obj + 0xA4
            pool_b = self.deserialize_object_pool(pool_a)
            self.write_pointer(obj + 0x94, pool_b)
            vertex_pool = self.deserialize_object_pool(pool_b)
            self.write_pointer(obj + 0x98, vertex_pool)
            vertex_count = self.u32(vertex_pool)
            index_pool = vertex_pool + 4 + vertex_count * 8
            self.require(index_pool, 0)
            self.write_pointer(obj + 0x9C, index_pool)
            tail = self.deserialize_index_pool(index_pool, obj)
            self.write_pointer(obj + 0xA0, tail)
            tail_count = self.u16(tail)
            cursor = tail + 2 + ((tail_count | 1) * 2)
            self.require(cursor, 0)

        self.write_pointer(base + 8, cursor)
        texture_descriptor_count = self.u16(base + 6)
        for descriptor_index in range(texture_descriptor_count):
            descriptor = cursor + descriptor_index * 32
            source = bytes(self.data[descriptor : descriptor + 32])
            # GT1 stores its distance threshold before the two GPU texture
            # packets.  GT2's renderer indexes the same 32-byte descriptor
            # but expects packet A first and the threshold at +0x0c.
            #
            # Keep the original packets here. remap_texture_packets later
            # applies the paired TRP's palette/image allocation to both tails.
            self.data[descriptor : descriptor + 32] = (
                source[4:16] + source[0:4] + source[16:32]
            )
        cursor += texture_descriptor_count * 32
        final_count = self.u32(cursor)
        cursor += 4 + final_count * 16
        self.require(cursor, 0)

        for obj in objects:
            # GT1's loader expands the two u16 neighbour indices at +0x00
            # into runtime pointers at +0x04/+0x08.  GT2 retains the indices
            # but deliberately leaves those pointer slots null; its course
            # selection path uses the +0xA0 neighbourhood list instead.
            self.write_u32(obj + 4, 0)
            self.write_u32(obj + 8, 0)

            # GT2 changed the handedness of the course coordinate system.
            # Common GT1/GT2 circuits retain byte-identical object records
            # apart from these axis-bearing fields (for example, all 194
            # High Speed Ring objects match after this transform).
            for field in (
                0x20,
                0x38,
                0x48,
                0x50,
                0x58,
                0x60,
                0x70,
                0x78,
                0x80,
                0x88,
            ):
                self.write_u32(obj + field, -self.u32(obj + field))
            for field in (0x28, 0x40):
                value = self.u32(obj + field)
                low = (-value) & 0xFFFF
                self.write_u32(obj + field, (value & 0xFFFF0000) | low)

            x_bias = ((self.u32(obj + 0x30) & 0x003FFFFF) >> 20) << 10
            y_bias = (
                ((self.u32(obj + 0x38) & 0x003FFFFF) >> 20) + 1
            ) << 10
            z_bias = ((self.u32(obj + 0x34) & 0x003FFFFF) >> 20) << 10
            for pool in (obj + 0xA4, self.u32(obj + 0x94)):
                vertices = self.u32(pool)
                for vertex_index in range(self.u32(pool + 0x2C)):
                    vertex = vertices + vertex_index * 8
                    x, y, z = struct.unpack_from("<hhh", self.data, vertex)
                    converted = (
                        (x >> 2) + x_bias,
                        -(y >> 2) + y_bias,
                        (z >> 2) + z_bias,
                    )
                    if any(value < -0x8000 or value > 0x7FFF for value in converted):
                        raise ValueError(
                            f"GT1 course vertex escaped int16 at "
                            f"object={index} vertex={vertex_index}: {converted}"
                        )
                    struct.pack_into("<hhh", self.data, vertex, *converted)

                # These pools hold camera-facing course props. The 16-byte
                # records describe authored quads; the 20-byte records begin
                # with the GTE anchor used to build light flares and tree
                # billboards in screen space. They share the normal vertex
                # coordinate system. Native GT1/GT2 High Speed Ring records
                # match exactly after this same scale, handedness, and object
                # bias conversion.
                for pointer_field, count_field, record_size in (
                    (0x24, 0x40, 16),
                    (0x28, 0x42, 20),
                ):
                    records = self.u32(pool + pointer_field)
                    for record_index in range(self.u16(pool + count_field)):
                        record = records + record_index * record_size
                        x, y, z = struct.unpack_from("<hhh", self.data, record)
                        converted = (
                            (x >> 2) + x_bias,
                            -(y >> 2) + y_bias,
                            (z >> 2) + z_bias,
                        )
                        if any(
                            value < -0x8000 or value > 0x7FFF
                            for value in converted
                        ):
                            raise ValueError(
                                f"GT1 course auxiliary anchor escaped int16 at "
                                f"object={index} record={record_index}: "
                                f"{converted}"
                            )
                        struct.pack_into(
                            "<hhh", self.data, record, *converted
                        )
                        self.converted_auxiliary_anchors += 1
                        if record_size == 20 and self.mark_imported_screen_billboards:
                            # GT2's flare routine leaves +8..+15 unread. Mark
                            # imported anchors so modern projection can undo
                            # the 4x apparent-radius increase caused by the
                            # exact GT1 -> GT2 quarter-scale world conversion.
                            struct.pack_into(
                                "<I",
                                self.data,
                                record + 8,
                                self.IMPORTED_SCREEN_BILLBOARD_MARKER,
                            )
                            self.marked_screen_billboards += 1

            # GT1 bins road polygons in a 4x4 index using 1 MiB world chunks
            # and 256-unit local coordinates. GT2's otherwise-equivalent
            # lookup uses 4 MiB chunks and 1024-unit local coordinates. Scale
            # the bin header alongside the vertices, then reverse its rows for
            # the handedness change. Leaving this metadata in GT1 space makes
            # the course render normally but causes every surface query to
            # select an unrelated polygon list.
            index_pool = self.u32(obj + 0x9C)
            origin_x, origin_z, shift_x, shift_z = struct.unpack_from(
                "<hhhh", self.data, index_pool
            )
            if shift_x < 2 or shift_z < 2:
                raise ValueError(
                    f"GT1 course collision shift is too small at "
                    f"object={index}: ({shift_x}, {shift_z})"
                )
            lists = [
                [
                    self.u32(
                        self.u32(index_pool + 0x28 + bin_index * 4)
                        + item_index * 4
                    )
                    for item_index in range(
                        self.u16(index_pool + 8 + bin_index * 2)
                    )
                ]
                for bin_index in range(16)
            ]
            converted_lists = [
                lists[(3 - row) * 4 + column]
                for row in range(4)
                for column in range(4)
            ]
            self.write_i16(index_pool, (origin_x >> 2) + x_bias)
            self.write_i16(
                index_pool + 2,
                y_bias
                - (
                    (
                        origin_z
                        + 4 * (1 << shift_z)
                        - 1
                    )
                    >> 2
                ),
            )
            self.write_i16(index_pool + 4, shift_x - 2)
            self.write_i16(index_pool + 6, shift_z - 2)
            index_cursor = index_pool + 0x68
            for bin_index, items in enumerate(converted_lists):
                struct.pack_into(
                    "<H", self.data, index_pool + 8 + bin_index * 2, len(items)
                )
                self.write_pointer(
                    index_pool + 0x28 + bin_index * 4, index_cursor
                )
                for target in items:
                    self.write_pointer(index_cursor, target)
                    index_cursor += 4

            # The separate edge block at +0x98 drives GT2's track-edge walk.
            # Its first two words remain indices into pool A; the third signed
            # word carries the handedness-dependent edge term. Common High
            # Speed Ring data proves that GT2 negates only this word while
            # preserving the indices and fourth word.
            edge_pool = self.u32(obj + 0x98)
            for edge_index in range(self.u32(edge_pool)):
                edge = edge_pool + 4 + edge_index * 8
                edge_term = struct.unpack_from("<h", self.data, edge + 4)[0]
                struct.pack_into("<h", self.data, edge + 4, -edge_term)
        return cursor

    def deserialize_secondary(self, base: int) -> int:
        count = self.u32(base)
        cursor = base + 4 + count * 4
        for index in range(count):
            self.write_pointer(base + 4 + index * 4, cursor)
            item_count = self.u16(cursor)
            cursor += 4 + item_count * 8
            self.require(cursor, 0)
        return cursor

    def deserialize_model_pool(self, base: int) -> int:
        cursor = base + 0x58
        self.write_pointer(base, cursor)
        cursor += self.u16(base + 0x2C) * 8
        for index, item_size in enumerate(self.MODEL_POOL_SIZES):
            self.write_pointer(base + 4 + index * 4, cursor)
            cursor += self.u16(base + 0x30 + index * 2) * item_size
            self.require(cursor, 0)
        self.write_pointer(base + 0x24, cursor)
        cursor += self.u16(base + 0x40) * 28
        self.write_pointer(base + 0x28, cursor)
        cursor += self.u16(base + 0x42) * 20
        self.require(cursor, 0)
        return cursor

    def deserialize_models(self, base: int) -> int:
        count = self.u32(base)
        cursor = base + 4 + count * 4
        for index in range(count):
            self.write_pointer(base + 4 + index * 4, cursor)
            cursor = self.deserialize_model_pool(cursor)
            cursor += 4 + self.u32(cursor) * 16
            self.require(cursor, 0)
        return cursor

    def link_secondary_to_models(self, secondary: int, models: int) -> None:
        model_count = self.u32(models)
        model_pointers = [
            self.u32(models + 4 + index * 4)
            for index in range(model_count)
        ]
        for index in range(self.u32(secondary)):
            item = self.u32(secondary + 4 + index * 4)
            count = self.u16(item)
            for entry_index in range(count):
                entry = item + entry_index * 8
                model_index = self.u32(entry + 8)
                if model_index == 0xFFFFFFFF:
                    continue
                if model_index >= len(model_pointers):
                    raise ValueError(
                        f"GT1 secondary model index {model_index} "
                        f"is outside {len(model_pointers)} models"
                    )
                self.write_pointer(entry + 8, model_pointers[model_index])

    def deserialize_tail_item(self, base: int) -> int:
        item_type = self.u16(base)
        if item_type == 0:
            count = self.u16(base + 2)
            return base + 4 + count * 16
        if item_type == 1:
            count = self.u16(base + 2)
            return base + 4 + count * 56
        raise ValueError(f"unsupported GT1 tail subtype {item_type}")

    def deserialize_tail(self, base: int) -> int:
        count = self.u16(base)
        cursor = base + 4 + count * 4
        fixed_sizes = {1: 0x28, 2: 0x14}
        for index in range(count):
            self.write_pointer(base + 4 + index * 4, cursor)
            item_type = self.u16(cursor)
            if item_type == 0:
                cursor = self.deserialize_tail_item(cursor + 0x18)
            elif item_type == 3:
                cursor = self.deserialize_tail_item(cursor + 0x1C)
            elif item_type in fixed_sizes:
                cursor += fixed_sizes[item_type]
            else:
                raise ValueError(f"unsupported GT1 tail type {item_type}")
            self.require(cursor, 0)
        return cursor

    @staticmethod
    def _align_four(value: int) -> int:
        return (value + 3) & ~3

    def compact_primary(
        self,
        primary: int,
        secondary: int,
        models: int,
        tail: int,
        final: int,
    ) -> tuple[int, int, int, int, int, int]:
        """Share byte-identical, immutable render pools.

        GT2 reserves exactly 700,000 bytes between its course buffer and the
        first vehicle work area.  The full-detail GT1 SSR11 payload exceeds
        that boundary by 11,784 bytes.  GT1 serialises repeated track render
        pools independently even when their bytes are identical.  GT2's
        runtime follows pointers to those read-only pools, so one canonical
        copy is sufficient and preserves the original geometry and artwork.
        Mutable pointer tables and per-object headers remain private.
        """

        old = bytes(self.data)
        old_pointer_fields = set(self.pointer_fields)
        object_count = self.u16(primary + 4)
        objects = [
            self.u32(primary + 0x0C + index * 4)
            for index in range(object_count)
        ]
        trailing = self.u32(primary + 8)
        pool_sizes = self.OBJECT_POOL_SIZES

        primary_header_size = 0x0C + object_count * 4
        compact = bytearray(old[primary : primary + primary_header_size])
        ranges: list[tuple[int, int, int]] = [
            (primary, primary + primary_header_size, primary)
        ]
        pointer_writes: list[tuple[int, int]] = []

        def reserve(size: int, *, align: bool = True) -> int:
            if align:
                aligned = self._align_four(len(compact))
                compact.extend(b"\0" * (aligned - len(compact)))
            start = len(compact)
            compact.extend(b"\0" * size)
            return start

        def copy_range(start: int, size: int, *, align: bool = True) -> int:
            local = reserve(size, align=align)
            compact[local : local + size] = old[start : start + size]
            ranges.append((start, start + size, primary + local))
            return primary + local

        object_map: dict[int, int] = {}
        pool_a_map: dict[int, int] = {}
        for obj in objects:
            new_obj = copy_range(obj, 0xE8)
            object_map[obj] = new_obj
            pool_a_map[obj + 0xA4] = new_obj + 0xA4

        pool_b_map: dict[int, int] = {}
        for obj in objects:
            pool_b = self.u32(obj + 0x94)
            pool_b_map[pool_b] = copy_range(pool_b, 0x44)

        canonical_arrays: dict[bytes, int] = {}
        pool_array_targets: dict[tuple[int, int], int] = {}
        vertex_targets: dict[int, int] = {}
        bytes_saved = 0

        def intern_array(start: int, size: int) -> int:
            nonlocal bytes_saved
            if size == 0:
                return primary + self._align_four(len(compact))
            payload = old[start : start + size]
            target = canonical_arrays.get(payload)
            if target is None:
                target = copy_range(start, size)
                canonical_arrays[payload] = target
            else:
                bytes_saved += size
                ranges.append((start, start + size, target))
            return target

        def pool_spans(pool: int) -> list[tuple[int, int, int]]:
            spans = [(0, self.u32(pool + 0x2C) * 8, self.u32(pool))]
            spans.extend(
                (
                    4 + index * 4,
                    self.u16(pool + 0x30 + index * 2) * item_size,
                    self.u32(pool + 4 + index * 4),
                )
                for index, item_size in enumerate(pool_sizes)
            )
            spans.extend(
                (
                    (0x24, self.u16(pool + 0x40) * 16, self.u32(pool + 0x24)),
                    (0x28, self.u16(pool + 0x42) * 20, self.u32(pool + 0x28)),
                )
            )
            return spans

        for obj in objects:
            for pool in (obj + 0xA4, self.u32(obj + 0x94)):
                for field, size, source in pool_spans(pool):
                    pool_array_targets[(pool, field)] = intern_array(
                        source, size
                    )
            vertex = self.u32(obj + 0x98)
            vertex_size = 4 + self.u32(vertex) * 8
            vertex_targets[obj] = intern_array(vertex, vertex_size)

        index_targets: dict[int, int] = {}
        for obj in objects:
            index_pool = self.u32(obj + 0x9C)
            index_size = 0x68 + sum(
                self.u16(index_pool + 8 + index * 2) * 4
                for index in range(16)
            )
            object_tail = index_pool + index_size
            tail_count = self.u16(object_tail)
            block_size = (
                index_size + 2 + ((tail_count | 1) * 2)
            )
            index_targets[obj] = copy_range(index_pool, block_size)

        new_trailing = copy_range(trailing, secondary - trailing)

        ranges.sort()
        range_starts = [item[0] for item in ranges]

        def map_primary_target(target: int) -> int:
            index = bisect.bisect_right(range_starts, target) - 1
            if index >= 0:
                start, end, replacement = ranges[index]
                if target < end:
                    return replacement + target - start
            raise ValueError(
                f"GT1 primary compaction cannot map pointer {target:#x}"
            )

        def write_compact_pointer(field: int, target: int) -> None:
            local = field - primary
            struct.pack_into("<I", compact, local, target & 0xFFFFFFFF)
            pointer_writes.append((field, target))

        write_compact_pointer(primary + 8, new_trailing)
        for index, obj in enumerate(objects):
            new_obj = object_map[obj]
            write_compact_pointer(primary + 0x0C + index * 4, new_obj)
            for field in (4, 8):
                target = self.u32(obj + field)
                write_compact_pointer(
                    new_obj + field,
                    0 if target == 0 else object_map[target],
                )
            write_compact_pointer(
                new_obj + 0x94, pool_b_map[self.u32(obj + 0x94)]
            )
            write_compact_pointer(new_obj + 0x98, vertex_targets[obj])
            write_compact_pointer(new_obj + 0x9C, index_targets[obj])
            old_index = self.u32(obj + 0x9C)
            index_size = 0x68 + sum(
                self.u16(old_index + 8 + pool_index * 2) * 4
                for pool_index in range(16)
            )
            write_compact_pointer(
                new_obj + 0xA0, index_targets[obj] + index_size
            )

            for old_pool, new_pool in (
                (obj + 0xA4, pool_a_map[obj + 0xA4]),
                (
                    self.u32(obj + 0x94),
                    pool_b_map[self.u32(obj + 0x94)],
                ),
            ):
                for field, _, _ in pool_spans(old_pool):
                    write_compact_pointer(
                        new_pool + field,
                        pool_array_targets[(old_pool, field)],
                    )

            new_index = index_targets[obj]
            cursor = old_index + 0x68
            new_cursor = new_index + 0x68
            for index in range(16):
                write_compact_pointer(
                    new_index + 0x28 + index * 4, new_cursor
                )
                for _ in range(self.u16(old_index + 8 + index * 2)):
                    write_compact_pointer(
                        new_cursor, map_primary_target(self.u32(cursor))
                    )
                    cursor += 4
                    new_cursor += 4

        delta = len(compact) - (secondary - primary)
        combined = bytearray(old[:primary] + compact + old[secondary:])

        new_pointer_fields = {
            field for field in old_pointer_fields if field < primary
        }
        new_pointer_fields.update(field for field, _ in pointer_writes)

        for field in sorted(old_pointer_fields):
            if primary <= field < secondary:
                continue
            new_field = field if field < primary else field + delta
            target = struct.unpack_from("<I", old, field)[0]
            if target == 0:
                new_target = 0
            elif primary <= target < secondary:
                new_target = map_primary_target(target)
            elif target >= secondary:
                new_target = target + delta
            else:
                new_target = target
            struct.pack_into("<I", combined, new_field, new_target)
            new_pointer_fields.add(new_field)

        self.data = combined
        self.pointer_fields = new_pointer_fields
        return (
            primary,
            secondary + delta,
            models + delta,
            tail + delta,
            final + delta,
            bytes_saved,
        )

    def convert_to_revision_31(self) -> tuple[bytes, dict[str, int]]:
        primary = self.deserialize_dynamic_objects()
        self.write_pointer(0x10, primary)
        secondary = self.deserialize_primary(primary)
        self.write_pointer(0x14, secondary)
        models = self.deserialize_secondary(secondary)
        self.write_pointer(0x18, models)
        tail = self.deserialize_models(models)
        self.link_secondary_to_models(secondary, models)
        self.remap_texture_packets(primary, models)
        final = self.deserialize_tail(tail)
        if final >= len(self.data):
            raise ValueError(
                f"GT1 final course section begins outside the payload: "
                f"{final:#x} >= {len(self.data):#x}"
            )

        (
            primary,
            secondary,
            models,
            tail,
            final,
            bytes_saved,
        ) = self.compact_primary(primary, secondary, models, tail, final)

        old = self.data
        converted = bytearray(old[:0x1C] + b"\0" * 8 + old[0x1C:])
        for field in self.pointer_fields:
            target = struct.unpack_from("<I", old, field)[0]
            converted_field = field + (8 if field >= 0x1C else 0)
            converted_target = target + (8 if target >= 0x1C else 0)
            struct.pack_into(
                "<I", converted, converted_field, converted_target
            )

        # The fixed course header contains the starting-grid heading at +0x50
        # and fifteen XYZ grid records from +0x58 through +0x114.  GT2 retained
        # their layout but reversed the third world axis.  Shared High Speed
        # Ring data proves every other component is byte-identical while these
        # fields are the exact signed negation.  Leaving the GT1 signs intact
        # starts every car below/beside the converted course even though the
        # renderer and minimap have loaded the correct track.
        for field in (0x50, *range(0x60, 0x109, 0x0C)):
            value = struct.unpack_from("<I", converted, field)[0]
            struct.pack_into("<I", converted, field, (-value) & 0xFFFFFFFF)

        struct.pack_into("<H", converted, 0x0E, 31)
        struct.pack_into("<I", converted, 0x1C, tail + 8)
        struct.pack_into("<I", converted, 0x20, final + 8)
        return bytes(converted), {
            "primaryOffset": primary + 8,
            "secondaryOffset": secondary + 8,
            "modelsOffset": models + 8,
            "tailOffset": tail + 8,
            "finalOffset": final + 8,
            "payloadSize": len(converted),
            "pointerCount": len(self.pointer_fields),
            "sharedRenderPoolBytesSaved": bytes_saved,
            "relocatedTexturePackets": self.relocated_texture_packets,
            "convertedAuxiliaryAnchors": self.converted_auxiliary_anchors,
            "markedScreenBillboards": self.marked_screen_billboards,
        }


def convert_gt1_geometry(
    data: bytes,
    texture_relocation: TextureRelocation | None = None,
    mark_imported_screen_billboards: bool = False,
) -> tuple[bytes, dict[str, int]]:
    if not data.startswith(b"@(#)GT-PS"):
        raise ValueError("GT1 geometry is not native GT-PS data")
    revision = struct.unpack_from("<H", data, 14)[0]
    if revision != 28:
        raise ValueError(f"unsupported GT1 geometry revision: {revision}")
    return Gt1CourseDeserializer(
        data, texture_relocation, mark_imported_screen_billboards
    ).convert_to_revision_31()


def prepare_output_directory(path: Path) -> None:
    resolved = path.resolve()
    work_root = (REPO / "work").resolve()
    if work_root not in resolved.parents:
        raise ValueError(f"refusing to clean output outside work/: {resolved}")
    if resolved.exists():
        shutil.rmtree(resolved)
    resolved.mkdir(parents=True)


def convert_ssr11(
    disc_root: Path,
    gt2_arcade_volume: Path,
    output: Path,
    smoke_variant: str | None,
    smoke_targets: list[str],
) -> tuple[
    list[dict[str, object]],
    dict[str, object],
    dict[str, object],
]:
    archive, entries = read_gtarc(disc_root / "COURSE.DAT")
    background_archive, background_entries = read_gtarc(
        disc_root / "BG.DAT"
    )
    course_output = output / "patch" / "crsobj"
    map_output = output / "patch" / "crsmap"
    background_output = output / "patch" / "bgsobj"
    course_output.mkdir(parents=True, exist_ok=True)
    map_output.mkdir(parents=True, exist_ok=True)
    background_output.mkdir(parents=True, exist_ok=True)
    converted: list[dict[str, object]] = []
    by_stem: dict[str, tuple[bytes, bytes, bytes]] = {}

    source_sky_texture = unpack_entry(
        background_archive,
        background_entries[GT1_SSR11_SKY_INDEX * 2],
    )
    source_sky_model = unpack_entry(
        background_archive,
        background_entries[GT1_SSR11_SKY_INDEX * 2 + 1],
    )
    if not source_sky_model.startswith(b"@(#)GT-SKY"):
        raise ValueError("GT1 SSR11 background is not native GT-SKY data")
    source_sky_revision = struct.unpack_from("<H", source_sky_model, 14)[0]
    if source_sky_revision != 2:
        raise ValueError(
            f"unsupported GT1 SSR11 sky revision: {source_sky_revision}"
        )

    reference_sky_model = unpack_entry(
        background_archive,
        background_entries[3 * 2 + 1],
    )
    sky_texture = convert_gt1_sky_texture_package(
        source_sky_texture,
        read_gt2_gzip_member(gt2_arcade_volume, "bgsobj/dawn.bsp.gz"),
    )
    # Sky and course uploads share VRAM. Moving the sky palettes alone would
    # overwrite live track materials; reserve the BSO's native slots in all
    # six variant TRPs and remap every corresponding TRO packet together.
    sky_cluts = frozenset(
        struct.unpack_from("<HH", tim, 12)
        for tim in tim_stream_members(sky_texture)
    )
    sky_model, sky_model_conversion = convert_gt1_sky_model(
        reference_sky_model,
        source_sky_model,
        read_gt2_gzip_member(
            gt2_arcade_volume, "bgsobj/dawn.bso.gz"
        ),
    )
    (background_output / f"{GT2_SSR11_SKY_STEM}.bsp.gz").write_bytes(
        gzip.compress(sky_texture, compresslevel=9, mtime=0)
    )
    (background_output / f"{GT2_SSR11_SKY_STEM}.bso.gz").write_bytes(
        gzip.compress(sky_model, compresslevel=9, mtime=0)
    )
    sky_metadata: dict[str, object] = {
        "stem": GT2_SSR11_SKY_STEM,
        "sourceBackgroundIndex": GT1_SSR11_SKY_INDEX,
        "sourceName": GT1_SSR11_SKY_NAME,
        "sourceTextureSize": len(source_sky_texture),
        "textureSize": len(sky_texture),
        "sourceModelSize": len(source_sky_model),
        "modelSize": len(sky_model),
        "sourceTextureSha256": hashlib.sha256(
            source_sky_texture
        ).hexdigest(),
        "textureSha256": hashlib.sha256(sky_texture).hexdigest(),
        "sourceModelSha256": hashlib.sha256(source_sky_model).hexdigest(),
        "modelSha256": hashlib.sha256(sky_model).hexdigest(),
        "modelBasis": "GT2 native dawn BSO layout with GT1 dawn3 colours",
        "modelConversion": sky_model_conversion,
        "reservedClutSlots": sorted(sky_cluts),
    }

    for stem, course_index, label in SSR11_VARIANTS:
        source_texture = unpack_entry(archive, entries[course_index * 2])
        texture, course_map, relocation = convert_gt1_texture_package(
            source_texture, reserved_cluts=sky_cluts
        )
        source_geometry = unpack_entry(
            archive, entries[course_index * 2 + 1]
        )
        geometry, geometry_metadata = convert_gt1_geometry(
            source_geometry,
            relocation,
            mark_imported_screen_billboards=True,
        )
        texture_name = f"{stem}.trp.gz"
        geometry_name = f"{stem}.tro.gz"
        (course_output / texture_name).write_bytes(
            gzip.compress(texture, compresslevel=9, mtime=0)
        )
        (course_output / geometry_name).write_bytes(
            gzip.compress(geometry, compresslevel=9, mtime=0)
        )
        (map_output / f"{stem}.tim.gz").write_bytes(
            gzip.compress(course_map, compresslevel=9, mtime=0)
        )
        by_stem[stem] = (texture, geometry, course_map)
        converted.append(
            {
                "stem": stem,
                "label": label,
                "sourceCourseIndex": course_index,
                "sourceTextureSize": len(source_texture),
                "textureSize": len(texture),
                "sourceGeometrySize": len(source_geometry),
                "geometrySize": len(geometry),
                "sourceTextureSha256": hashlib.sha256(
                    source_texture
                ).hexdigest(),
                "textureSha256": hashlib.sha256(texture).hexdigest(),
                "courseMapSize": len(course_map),
                "courseMapSha256": hashlib.sha256(course_map).hexdigest(),
                "sourceGeometrySha256": hashlib.sha256(
                    source_geometry
                ).hexdigest(),
                "geometrySha256": hashlib.sha256(geometry).hexdigest(),
                "geometryConversion": geometry_metadata,
            }
        )

    if smoke_variant is not None:
        if smoke_variant not in by_stem:
            raise ValueError(f"unknown SSR11 smoke variant: {smoke_variant}")
        texture, geometry, course_map = by_stem[smoke_variant]
        for smoke_target in smoke_targets:
            (course_output / f"{smoke_target}.trp.gz").write_bytes(
                gzip.compress(texture, compresslevel=9, mtime=0)
            )
            (course_output / f"{smoke_target}.tro.gz").write_bytes(
                gzip.compress(geometry, compresslevel=9, mtime=0)
            )
            (map_output / f"{smoke_target}.tim.gz").write_bytes(
                gzip.compress(course_map, compresslevel=9, mtime=0)
            )
            print(
                f"staged {smoke_variant} over native smoke target "
                f"crsobj/{smoke_target}"
            )
        (background_output / "speedsky.bsp.gz").write_bytes(
            gzip.compress(sky_texture, compresslevel=9, mtime=0)
        )
        (background_output / "speedsky.bso.gz").write_bytes(
            gzip.compress(sky_model, compresslevel=9, mtime=0)
        )
        print(
            "staged GT1 SSR11 dawn3 background over native smoke target "
            "bgsobj/speedsky"
        )
    menu_metadata = integrate_ssr11_arcade_menu(
        disc_root, gt2_arcade_volume, output / "patch"
    )
    return converted, sky_metadata, menu_metadata


def default_image() -> Path:
    for candidate in (
        REPO / GT1_IMAGE_NAME,
        REPO.parents[1] / GT1_IMAGE_NAME,
    ):
        if candidate.is_file():
            return candidate
    return REPO / GT1_IMAGE_NAME


def default_simulation_volume() -> Path:
    for candidate in (
        REPO / "work" / "simulation-disc-data" / "GT2.VOL",
        REPO.parents[1] / "OpenGTPS1" / "GT2.VOL",
    ):
        if candidate.is_file():
            return candidate
    return REPO / "work" / "simulation-disc-data" / "GT2.VOL"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--image", type=Path, default=default_image())
    parser.add_argument("--disc-root", type=Path, default=REPO / "work" / "gt1-disc")
    parser.add_argument(
        "--output", type=Path, default=REPO / "work" / "gt1-converted"
    )
    parser.add_argument(
        "--gt2-arcade-volume",
        type=Path,
        default=REPO / "work" / "arcade-disc" / "GT2.VOL",
    )
    parser.add_argument(
        "--gt2-simulation-volume",
        type=Path,
        default=default_simulation_volume(),
    )
    parser.add_argument(
        "--gt2-arcade-overlay",
        type=Path,
        default=REPO / "work" / "arcade-disc" / "GT2.OVL",
    )
    parser.add_argument(
        "--smoke-variant",
        choices=[item[0] for item in SSR11_VARIANTS],
    )
    parser.add_argument(
        "--smoke-target",
        action="append",
        dest="smoke_targets",
        help="Existing GT2 course stem temporarily replaced for native smoke testing.",
    )
    parser.add_argument(
        "--smoke-gtmode-car",
        choices=[
            str(definition["stem"])
            for definition in GT1_GTMODE_DISTINCT_CARS
        ],
        help=(
            "Place one imported used car in Mazda dealer row one for "
            "a deterministic purchase/race smoke test. Normal conversion "
            "retains native price ordering."
        ),
    )
    parser.add_argument(
        "--smoke-gtmode-livery",
        choices=sorted(GT1_ARCADE_LIVERY_SMOKE_CARS),
        help=(
            "Place one existing livery-fold target in a native dealer lineup "
            "at a temporary 20,000-credit price for deterministic native "
            "purchase, upgrade, save, and race validation. Normal conversion "
            "does not alter that car's acquisition path or price."
        ),
    )
    parser.add_argument(
        "--smoke-arcade-livery",
        choices=sorted(GT1_ARCADE_LIVERY_SMOKE_CARS),
        help=(
            "Expose one existing GT2 identity in the native Arcade selector "
            "to render and cycle its converted livery choices. This entry is "
            "emitted only in explicit smoke output."
        ),
    )
    parser.add_argument("--extract-clean", action="store_true")
    args = parser.parse_args()

    image_digest = validate_gt1_image(args.image)
    required_present = all(
        (args.disc_root / name).is_file() for name in REQUIRED_DISC_FILES
    )
    if args.extract_clean or not required_present:
        extract_image(args.image, args.disc_root, clean=True)
    validate_disc_root(args.disc_root)

    prepare_output_directory(args.output)
    converted, sky, arcade_menu = convert_ssr11(
        args.disc_root,
        args.gt2_arcade_volume,
        args.output,
        args.smoke_variant,
        args.smoke_targets or ["circuit"],
    )
    arcade_cars = tuple(
        stage_gt1_arcade_car(
            args.disc_root,
            args.gt2_arcade_volume,
            args.output / "patch",
            definition,
        )
        for definition in GT1_ARCADE_CARS
    )
    arcade_patch_root = args.output / "patch"
    arcade_livery_smoke_car = None
    if args.smoke_arcade_livery is not None:
        arcade_livery_smoke_car = stage_gt2_arcade_livery_smoke_car(
            args.disc_root,
            args.gt2_arcade_volume,
            arcade_patch_root,
            args.smoke_arcade_livery,
        )
        arcade_cars += (arcade_livery_smoke_car,)
    arcade_livery_root = args.output / "livery-arcade-patch"
    simulation_patch_root = args.output / "gt1-cars-simulation-patch"
    simulation_livery_root = args.output / "livery-simulation-patch"
    simulation_cars = tuple(
        stage_gt1_simulation_car(
            args.disc_root,
            args.gt2_simulation_volume,
            simulation_patch_root,
            definition,
        )
        for definition in GT1_GTMODE_DISTINCT_CARS
    )
    arcade_livery_definitions = discover_gt1_livery_folds(
        args.disc_root, args.gt2_arcade_volume
    )
    simulation_livery_definitions = discover_gt1_livery_folds(
        args.disc_root, args.gt2_simulation_volume
    )
    smoke_existing_livery = (
        build_gt2_existing_livery_smoke_car(
            args.gt2_simulation_volume,
            simulation_livery_definitions,
            args.smoke_gtmode_livery,
        )
        if args.smoke_gtmode_livery is not None
        else None
    )
    simulation_new_car_smoke = (
        stage_gt2_existing_livery_new_car_smoke(
            args.gt2_simulation_volume,
            simulation_patch_root,
            smoke_existing_livery,
        )
        if smoke_existing_livery is not None
        else None
    )
    simulation_used_cars = stage_gt2_simulation_used_cars(
        args.gt2_simulation_volume,
        simulation_patch_root,
        simulation_cars,
        args.smoke_gtmode_car,
        None,
    )
    # The Arcade base patch already has appended carinfo records. Seed the
    # gated fold layer from that result so applying it after the base layer
    # preserves every current Arcade import.
    arcade_livery_root.mkdir(parents=True, exist_ok=True)
    for name in (*GT2_LOCALIZED_CARINFO_DATABASES, ".carcolor"):
        (arcade_livery_root / name).write_bytes(
            (arcade_patch_root / name).read_bytes()
        )
    # The Simulation fold layer is applied after the distinct-car layer.
    # Seed it from that staged database so neither layer can erase the
    # other's appended native carinfo records.
    simulation_livery_root.mkdir(parents=True, exist_ok=True)
    for name in (*GT2_LOCALIZED_CARINFO_DATABASES, ".carcolor"):
        (simulation_livery_root / name).write_bytes(
            (simulation_patch_root / name).read_bytes()
        )
    definition_signature = lambda definitions: [
        (
            definition["targetStem"],
            definition["bodyStem"],
            tuple(
                int(item[0])
                for item in definition["paintSources"]
            ),
        )
        for definition in definitions
    ]
    if definition_signature(arcade_livery_definitions) != (
        definition_signature(simulation_livery_definitions)
    ):
        raise ValueError(
            "GT2 Simulation and Arcade same-car color-ID folds differ"
        )
    arcade_livery_folds = stage_gt1_livery_folds(
        args.disc_root,
        args.gt2_arcade_volume,
        arcade_livery_root,
        arcade_livery_definitions,
    )
    simulation_livery_folds = stage_gt1_livery_folds(
        args.disc_root,
        args.gt2_simulation_volume,
        simulation_livery_root,
        simulation_livery_definitions,
    )
    arcade_overlay = patch_ssr11_arcade_overlay(
        args.gt2_arcade_overlay,
        args.output / "GT2.OVL",
        arcade_cars,
    )
    arcade_patch = args.output / "GTPATCH.ARCADE.VOL"
    arcade_livery_patch = args.output / "GTPATCH.LIVERY.ARCADE.VOL"
    simulation_livery_patch = (
        args.output / "GTPATCH.LIVERY.SIMULATION.VOL"
    )
    simulation_cars_patch = (
        args.output / "GTPATCH.GT1CARS.SIMULATION.VOL"
    )
    write_volume(
        arcade_patch, members_from_directory(args.output / "patch")
    )
    write_volume(
        arcade_livery_patch,
        members_from_directory(arcade_livery_root),
    )
    write_volume(
        simulation_livery_patch,
        members_from_directory(simulation_livery_root),
    )
    write_volume(
        simulation_cars_patch,
        members_from_directory(simulation_patch_root),
    )
    # Preserve the original development filename for older scripts. The
    # unified installer prefers the explicit mode-targeted layers.
    legacy_arcade_patch = args.output / "GTPATCH.VOL"
    write_volume(
        legacy_arcade_patch,
        members_from_directory(args.output / "patch"),
    )
    manifest = {
        "formatVersion": 1,
        "source": {
            "serial": "SCUS-94194",
            "region": "NTSC-U",
            "imageSize": GT1_IMAGE_SIZE,
            "imageSha256": image_digest,
        },
        "ssr11": converted,
        "ssr11ArcadeSelectionEntry": GT1_ARCADE_SSR11_ENTRY,
        "ssr11ArcadeMenu": arcade_menu,
        "ssr11ArcadeOverlay": arcade_overlay,
        "ssr11Sky": sky,
        "arcadeCars": list(arcade_cars),
        "simulationCars": list(simulation_cars),
        "simulationNewCarSmoke": simulation_new_car_smoke,
        "simulationUsedCars": simulation_used_cars,
        "liveryFolds": {
            "arcade": arcade_livery_folds,
            "simulation": simulation_livery_folds,
        },
        "patchVolumes": {
            "arcade": arcade_patch.name,
            "legacyArcade": legacy_arcade_patch.name,
            "gatedLiveryArcade": arcade_livery_patch.name,
            "gatedLiverySimulation": simulation_livery_patch.name,
            "gatedGt1CarsSimulation": simulation_cars_patch.name,
            "liveryRuntimeResolverRequired": True,
            "gt1CarsRuntimeSmokeRequired": True,
        },
        "smokeVariant": args.smoke_variant,
        "smokeGtModeCar": args.smoke_gtmode_car,
        "smokeGtModeLivery": args.smoke_gtmode_livery,
        "smokeArcadeLivery": args.smoke_arcade_livery,
        "smokeTargets": (
            args.smoke_targets or ["circuit"]
            if args.smoke_variant
            else []
        ),
    }
    (args.output / "manifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n",
        encoding="utf-8",
    )
    print(
        "GT1 content patches ready: "
        f"{arcade_patch}; gated livery folds: "
        f"{simulation_livery_patch}, {arcade_livery_patch}; "
        f"gated GT Mode cars: {simulation_cars_patch}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
