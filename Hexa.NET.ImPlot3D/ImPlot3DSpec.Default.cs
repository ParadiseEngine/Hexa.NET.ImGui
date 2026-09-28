namespace Hexa.NET.ImPlot3D
{
    using System.Numerics;

    public partial struct ImPlot3DSpec
    {
        /// <summary>
        /// The value of a default-constructed C++ <c>ImPlot3DSpec</c> (implot3d.h 0.4).
        /// </summary>
        /// <remarks>
        /// cimplot3d copies every field of the spec it is given, so the omitted-spec overloads must
        /// pass these initializers explicitly. <c>new ImPlot3DSpec()</c> is all zeros: stride 0,
        /// line weight 0 and transparent colors instead of the automatic defaults.
        /// </remarks>
        public static readonly ImPlot3DSpec Default = CreateDefault();

        private static unsafe ImPlot3DSpec CreateDefault()
        {
            // IMPLOT3D_AUTO is -1 and IMPLOT3D_AUTO_COL is ImVec4(0, 0, 0, -1).
            var autoColor = new Vector4(0, 0, 0, -1);
            return new ImPlot3DSpec(
                lineColor: autoColor,
                lineWeight: 1f,
                fillColor: autoColor,
                fillAlpha: -1,
                marker: ImPlot3DMarker.Auto,
                markerSize: -1,
                markerLineColor: autoColor,
                markerFillColor: autoColor,
                offset: 0,
                stride: -1,
                flags: ImPlot3DItemFlags.None);
        }
    }
}
