#!/usr/bin/env python3
"""Validate complete developer-smoke coverage for proven GT1 RM bodies."""

from __future__ import annotations

import unittest
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import gt1_convert as convert


class Gt1RmLiveryMatrixTests(unittest.TestCase):
    def test_every_proven_rm_body_has_an_arcade_smoke_definition(self) -> None:
        targets = {
            source[:-1] + "n"
            for source in convert.GT1_PROVEN_DISTINCT_RACING_MODIFICATION_STEMS
        }
        self.assertEqual(31, len(targets))
        self.assertTrue(targets <= convert.GT1_ARCADE_LIVERY_SMOKE_CARS.keys())
        for source in convert.GT1_PROVEN_DISTINCT_RACING_MODIFICATION_STEMS:
            target = source[:-1] + "n"
            definition = convert.GT1_ARCADE_LIVERY_SMOKE_CARS[target]
            self.assertEqual(target, definition["physicsBasisStem"])
            self.assertEqual(source[:-1] + ".tim", definition["menuLogoName"])
            self.assertEqual(1, definition["arcadeClass"])
            self.assertEqual(0, definition["developerReplacementIndex"])

    def test_smoke_targets_do_not_change_the_proven_rm_census(self) -> None:
        sources = convert.GT1_PROVEN_DISTINCT_RACING_MODIFICATION_STEMS
        self.assertEqual(len(sources), len(set(sources)))
        self.assertTrue(all(len(source) == 5 for source in sources))
        self.assertTrue(all(source.endswith("r") for source in sources))


if __name__ == "__main__":
    unittest.main()
