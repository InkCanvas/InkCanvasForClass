using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkCanvasForClassX.Libraries
{
    /// <summary>
    /// 提供了對平面向量的坐標運算
    /// </summary>
    public class Vector {

        private double _x = 0;
        private double _y = 0;

        public Vector(double x, double y) {
            _x = x;
            _y = y;
        }

        public double X {
            get => _x;
            set => _x = value;
        }

        public double Y {
            get => _y;
            set => _y = value;
        }

        /// <summary>
        /// 將該向量改變為其相反向量
        /// </summary>
        public void Negate() {
            _x = -_x;
            _y = -_y;
        }

        /// <summary>
        /// 提供一個<c>Vector</c>，返回該<c>Vector</c>的相反向量
        /// </summary>
        public static Vector NegateVector(Vector vec) {
            return new Vector(-vec.X, -vec.Y);
        }

        /// <summary>
        /// Csharp中的<c>Math.Hypot</c>實現
        /// </summary>
        private static double Hypot(params double[] values)
        {
            double sum = 0;
            foreach (var value in values) {
                sum += Math.Pow(value, 2);
            }
            return Math.Sqrt(sum);
        }

        /// <summary>
        /// 獲取該向量的長度
        /// </summary>
        public double Length => Vector.Hypot(_x,_y);

        /// <summary>
        /// 獲取該向量的未開平方的長度
        /// </summary>
        public double LengthSquared => _x * _x + _y * _y;

        /// <summary>
        /// 將該向量和另一個向量相加
        /// </summary>
        public void Add(Vector vec)
        {
            _x = _x + vec.X;
            _y = _y + vec.Y;
        }

        /// <summary>
        /// 提供兩個<c>Vector</c>，返回相加後的<c>Vector</c>
        /// </summary>
        public static Vector AddVectors(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X + vec2.X, vec1.Y + vec2.Y);
        }

        /// <summary>
        /// 將該向量減去另一個向量
        /// </summary>
        public void Subtract(Vector vec)
        {
            _x = _x - vec.X;
            _y = _y - vec.Y;
        }

        /// <summary>
        /// 提供兩個<c>Vector</c>，返回<c><paramref name="vec1"/>-<paramref name="vec2"/></c>後的<c>Vector</c>
        /// </summary>
        public static Vector SubtractVectors(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X - vec2.X, vec1.Y - vec2.Y);
        }

        /// <summary>
        /// 將該向量除以一個數值
        /// </summary>
        public void DivideBy(double n)
        {
            _x = _x / n;
            _y = _y / n;
        }

        /// <summary>
        /// 提供兩個<c>Vector</c>，返回<c><paramref name="vec"/>/<paramref name="n"/></c>後的<c>Vector</c>
        /// </summary>
        public static Vector DivideByVector(Vector vec, double n)
        {
            return new Vector(vec.X / n, vec.Y / n);
        }

        /// <summary>
        /// 將該向量乘以一個數值
        /// </summary>
        public void Multiply(double n)
        {
            _x = _x * n;
            _y = _y * n;
        }

        /// <summary>
        /// 提供兩個<c>Vector</c>，返回<c><paramref name="vec"/>*<paramref name="n"/></c>後的<c>Vector</c>
        /// </summary>
        public static Vector MultiplyVector(Vector vec, double n)
        {
            return new Vector(vec.X * n, vec.Y * n);
        }
    }
}
