"""Acceptance-boundary checks for the exact JPEG packet comparator."""
import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location("comparator", Path(__file__).with_name("compare-jpeg-packets.py"))
comparator = importlib.util.module_from_spec(spec)
spec.loader.exec_module(comparator)


def segment(marker, payload):
    return bytes((255, marker)) + (len(payload) + 2).to_bytes(2, "big") + payload


def packet(comment=b"Lavc62.28.100\0", entropy=b"\x42\xff\x00\xff\xd0\x13", extra=b""):
    # Authored one-component framing, including stuffed FF and restart marker.
    return (b"\xff\xd8" + segment(0xFE, comment) + extra
            + segment(0xC0, b"\x08\x00\x08\x00\x08\x01\x01\x11\x00")
            + segment(0xDA, b"\x01\x01\x00\x00\x3f\x00") + entropy + b"\xff\xd9")


class ComparisonTests(unittest.TestCase):
    def test_complete_identical_packet(self):
        value = comparator.compare(packet(), packet())
        self.assertTrue(value["exact"])
        self.assertEqual(1, value["actual"]["markers"][-2]["restartMarkers"])

    def test_attribution_requires_explicit_exception(self):
        self.assertFalse(comparator.compare(packet(b"Lokad.Image\0"), packet())["accepted"])
        value = comparator.compare(packet(b"Lokad.Image\0"), packet(), True)
        self.assertTrue(value["accepted"])
        self.assertFalse(value["exact"])

    def test_other_comments_are_not_exempt(self):
        self.assertFalse(comparator.compare(packet(b"anything\0"), packet(), True)["accepted"])
        self.assertFalse(comparator.compare(packet(b"Lokad.Image\0"), packet(b"LavcOTHER\0"), True)["accepted"])

    def test_entropy_change_is_not_exempt(self):
        self.assertFalse(comparator.compare(packet(b"Lokad.Image\0", b"\x43"), packet(), True)["accepted"])

    def test_other_metadata_and_extra_comments_are_not_exempt(self):
        for extra in [segment(0xE1, b"Exif\0\0"), segment(0xFE, b"another")]:
            self.assertFalse(comparator.compare(packet(b"Lokad.Image\0", extra=extra), packet(), True)["accepted"])

    def test_truncation_and_trailing_data_fail(self):
        for value in [packet()[:-1], packet() + b"\0", b"\xff\xd8\xff\xdb\x00\x20"]:
            with self.assertRaises(ValueError):
                comparator.describe(value)


if __name__ == "__main__":
    unittest.main()
