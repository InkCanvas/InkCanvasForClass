using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using InkCanvasForClassX.Libraries;
using InkCanvasForClassX.Libraries.Stroke;

namespace InkCanvasForClassX.Libraries
{
    /// <summary>
    /// 提供對JS庫<c>steveruizok/perfect-freehand</c>的C#包裝
    /// </summary>
    public class PerfectFreehand {

        /// <summary>
        /// Get an array of points as objects with an adjusted point, pressure, vector, distance, and runningLength.
        /// </summary>
        /// <param name="points">原注釋： An array of points (as `[x, y, pressure]` or `{x, y, pressure}`). Pressure is optional in both cases. 請使用<c>StylusPointCollection</c></param>
        /// <param name="options">An object with options.</param>
        /// <returns></returns>
        public static StrokePoint[] GetStrokePoints(StylusPointCollection points,
            StrokeOptions options) {
            var streamline = options.Streamline ?? 0.5;
            var size = options.Size ?? 16;
            var isComplete = options.Last ?? false;

            // If we don't have any points, return an empty array.
            if (points.Count == 0) return Array.Empty<StrokePoint>();

            // Find the interpolation level between points.
            double t = 0.15 + (1 - streamline) * 0.85;

            // Purify the StylusPointCollection
            var pts = new StylusPointCollection();
            foreach (var stylusPoint in points) {
                pts.Add(new StylusPoint(stylusPoint.X, stylusPoint.Y, stylusPoint.PressureFactor));
            }

            // Add extra points between the two, to help avoid "dash" lines
            // for strokes with tapered start and ends. Don't mutate the
            // input array!
            if (points.Count == 2) {
                var last = pts[1];
                pts.RemoveAt(pts.Count - 1);
                for (var i = 1; i < 5; i++) {
                    var _vec = Vector.InterpolateVectors(
                        new Vector(pts[0].X, pts[0].Y),
                        new Vector(last.X, last.Y),
                        i / 4
                    );
                    pts.Add(new StylusPoint(_vec.X, _vec.Y));
                }
            }

            // If there's only one point, add another point at a 1pt offset.
            // Don't mutate the input array!
            if (pts.Count == 1) {
                var onePt = new Vector(pts[0].X + 1, pts[0].Y + 1);
                pts.Add(new StylusPoint(onePt.X, onePt.Y, pts[0].PressureFactor));
            }

            // The strokePoints array will hold the points for the stroke.
            // Start it out with the first point, which needs no adjustment.
            var strokePoints = new List<StrokePoint>() {
                new StrokePoint() {
                    Point = new Vector(pts[0].X, pts[0].Y),
                    Pressure = pts[0].PressureFactor >= 0 ? pts[0].PressureFactor : 0.25,
                    Vector = new Vector(1, 1),
                    Distance = 0,
                    RunningLength = 0,
                }
            };

            // A flag to see whether we've already reached out minimum length
            var hasReachedMinimumLength = false;

            // We use the runningLength to keep track of the total distance
            double runningLength = 0;

            // We're set this to the latest point, so we can use it to calculate
            // the distance and vector of the next point.
            var prev = strokePoints[0];

            // const max = pts.length - 1
            var max = pts.Count - 1;

            // Iterate through all of the points, creating StrokePoints.
            for (var i = 1; i < pts.Count; i++) {
                var point = isComplete && i == max
                    ? // If we're at the last point, and `options.last` is true,
                    // then add the actual input point.
                    new Vector(pts[i].X, pts[i].Y)
                    : // Otherwise, using the t calculated from the streamline
                    // option, interpolate a new point between the previous
                    // point the current point.
                    Vector.InterpolateVectors(prev.Point, new Vector(pts[i].X, pts[i].Y), t);

                // If the new point is the same as the previous point, skip ahead.
                if (prev.Point.IsEqual(point)) continue;

                // How far is the new point from the previous point?
                var distance = Vector.DistLengthVectors(point, prev.Point);

                // Add this distance to the total "running length" of the line.
                runningLength += distance;

                // At the start of the line, we wait until the new point is a
                // certain distance away from the original point, to avoid noise
                if (i < max && !hasReachedMinimumLength) {
                    if (runningLength < size) continue;
                    hasReachedMinimumLength = true;
                    // TODO: Backfill the missing points so that tapering works correctly.
                }

                // Create a new strokepoint (it will be the new "previous" one).
                prev = new StrokePoint() {
                    // The adjusted point
                    Point = point,
                    // The input pressure (or .5 if not specified)
                    Pressure = pts[i].PressureFactor >= 0 ? pts[i].PressureFactor : 0.5,
                    // The vector from the current point to the previous point
                    Vector = Vector.UnitVector(Vector.SubtractVectors(prev.Point, point)),
                    // The distance between the current point and the previous point
                    Distance = distance,
                    // The total distance so far
                    RunningLength = runningLength,
                };

                // Push it to the strokePoints array.
                strokePoints.Add(prev);
            }

            // Set the vector of the first point to be the same as the second point.
            strokePoints[0].Vector = strokePoints[1]?.Vector ?? new Vector(0, 0);

            return strokePoints.ToArray();
        }

        /// <summary>
        /// Compute a radius based on the pressure.
        /// </summary>
        public static double GetStrokeRadius(
            double size,
            double thinning,
            double pressure,
            Func<double, double> easing = null)
        {
            if (easing == null) {
                easing = t => t; // 默認的 easing 函數
            }
            return size * easing(0.5 - thinning * (0.5 - pressure));
        }
    }

    namespace Stroke {
        public class StrokeOptions
        {
            /// <summary>
            /// The base size (diameter) of the stroke.
            /// </summary>
            public double? Size { get; set; }

            /// <summary>
            /// The effect of pressure on the stroke's size.
            /// </summary>
            public double? Thinning { get; set; }

            /// <summary>
            /// How much to soften the stroke's edges.
            /// </summary>
            public double? Smoothing { get; set; }
            public double? Streamline { get; set; }

            /// <summary>
            /// An easing function to apply to each point's pressure.
            /// </summary>
            public Func<double, double> Easing { get; set; }

            /// <summary>
            /// Whether to simulate pressure based on velocity.
            /// </summary>
            public bool? SimulatePressure { get; set; }

            /// <summary>
            /// Cap, taper and easing for the start of the line.
            /// </summary>
            public StrokeCapOptions Start { get; set; }

            /// <summary>
            /// Cap, taper and easing for the end of the line.
            /// </summary>
            public StrokeCapOptions End { get; set; }

            /// <summary>
            /// Whether to handle the points as a completed stroke.
            /// </summary>
            public bool? Last { get; set; }
        }

        public class StrokeCapOptions
        {
            /// <summary>
            /// Whether to apply a cap at the start/end of the line.
            /// </summary>
            public bool? Cap { get; set; }

            /// <summary>
            /// The taper value at the start/end of the line.
            /// </summary>
            public double? Taper { get; set; }

            /// <summary>
            /// An easing function to apply to the taper.
            /// </summary>
            public Func<double, double> Easing { get; set; }
        }

        public class StrokePoint
        {
            /// <summary>
            /// The point coordinates as [x, y].
            /// </summary>
            public Vector Point { get; set; }

            /// <summary>
            /// The pressure at the point.
            /// </summary>
            public double Pressure { get; set; }

            /// <summary>
            /// The distance from the previous point.
            /// </summary>
            public double Distance { get; set; }

            /// <summary>
            /// The vector at the point.
            /// </summary>
            public Vector Vector { get; set; }

            /// <summary>
            /// The running length of the stroke.
            /// </summary>
            public double RunningLength { get; set; }
        }
    }
}
