namespace Hexa.NET.ImPlot
{
    using System.Numerics;

    public partial struct ImPlotSpec
    {
        /// <summary>
        /// The value of a default-constructed C++ <c>ImPlotSpec</c> (implot.h, cimplot eb05976).
        /// </summary>
        /// <remarks>
        /// cimplot copies every field of the spec it is given, so the omitted-spec overloads must
        /// pass these initializers explicitly. <c>new ImPlotSpec()</c> is all zeros: stride 0 plots
        /// every point at the first value, with line weight 0 and transparent colors.
        /// </remarks>
        public static readonly ImPlotSpec Default = CreateDefault();

        private static unsafe ImPlotSpec CreateDefault()
        {
            // IMPLOT_AUTO is -1 and IMPLOT_AUTO_COL is ImVec4(0, 0, 0, -1).
            var autoColor = new Vector4(0, 0, 0, -1);
            return new ImPlotSpec(
                lineColor: autoColor,
                lineWeight: 1f,
                fillColor: autoColor,
                fillAlpha: 1f,
                marker: ImPlotMarker.None,
                markerSize: 4,
                markerLineColor: autoColor,
                markerFillColor: autoColor,
                size: 4,
                offset: 0,
                stride: -1,
                flags: ImPlotItemFlags.None);
        }
    }
}
