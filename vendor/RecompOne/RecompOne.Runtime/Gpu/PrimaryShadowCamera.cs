namespace RecompOne.Runtime;

/// <summary>Immutable per-origin source data, not a global last-camera cache.</summary>
public readonly record struct PrimaryCameraAnchor(
    int NegativeX, int NegativeY, int NegativeZ,
    int SectorX, int SectorY, int SectorZ, bool Valid);

internal static class PrimaryShadowCamera
{
    // Authored auxiliary instances/billboards can outnumber primary track
    // triangles, but their object rotation is not the course camera. Prefer
    // source-verified PRIMARY metadata in the same submitted view/generation.
    // Never import a previous frame or a different projection/mirror camera.
    internal static GteProjectionOrigin SelectVerified(in GteProjectionOrigin dominant,
        IEnumerable<(int Count, GteProjectionOrigin Origin)> candidates)
    {
        if (TryGet(in dominant, out _, out _, out _)) return dominant;
        int maximum = 0;
        GteProjectionOrigin selected = dominant;
        foreach (var candidate in candidates)
        {
            var origin = candidate.Origin;
            if (candidate.Count <= maximum ||
                origin.ProjectionPlane != dominant.ProjectionPlane ||
                origin.ProjectionOffsetX != dominant.ProjectionOffsetX ||
                origin.ProjectionOffsetY != dominant.ProjectionOffsetY ||
                origin.Object.SceneGeneration != dominant.Object.SceneGeneration ||
                !TryGet(in origin, out _, out _, out _)) continue;
            selected = origin;
            maximum = candidate.Count;
        }
        return selected;
    }

    // The primary path shifts its source matrix by adjustment=0..2; the
    // shared OT normalization exponent is exactly 10-adjustment.
    internal static bool TryGet(in GteProjectionOrigin camera,
        out int x, out int y, out int z)
    {
        x = y = z = 0;
        var owner = camera.Object;
        var anchor = owner.ShadowCamera;
        if (!camera.Valid || owner.Kind != WorldObjectKind.Track ||
            owner.ScenePass != WorldScenePass.Main || !anchor.Valid ||
            !owner.DepthScaleValid || owner.DepthScaleExponent is < 8 or > 10)
            return false;
        int ix = unchecked(anchor.NegativeX + anchor.SectorX) >> 10;
        int iy = unchecked(anchor.NegativeY + anchor.SectorY) >> 10;
        int iz = unchecked(anchor.NegativeZ + anchor.SectorZ) >> 10;
        // Exact source MVMVA result. The bounded inputs/products here cannot
        // overflow the GTE 44-bit accumulators. Reject a cached/aligned origin
        // unless its CURRENT translation is explained by its own source data.
        long tx = ((long)camera.R00 * ix + (long)camera.R01 * iy + (long)camera.R02 * iz) >> 12;
        long ty = ((long)camera.R10 * ix + (long)camera.R11 * iy + (long)camera.R12 * iz) >> 12;
        long tz = ((long)camera.R20 * ix + (long)camera.R21 * iy + (long)camera.R22 * iz) >> 12;
        if (tx != camera.TranslateX || ty != camera.TranslateY || tz != camera.TranslateZ)
            return false;
        // Preserve the authored arithmetic >>10 precision. Low bits do not
        // contribute to the original primary geometry translation.
        x = anchor.NegativeX & ~1023;
        y = anchor.NegativeY & ~1023;
        z = anchor.NegativeZ & ~1023;
        return true;
    }
}
