using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoManager
{
    public static class CommonFunction
    {
        /// <summary>
        /// 计算算数平均数:（x1+x2+...+xn）/n
        /// </summary>
        /// <param name="arr">数组</param>
        /// <returns>算术平均数</returns>
        public static double ArithmeticMean(double[] arr)
        {
            double result = 0;
            foreach (double num in arr)
            {
                result += num;
            }
            return result / arr.Length;
        }

        /// <summary>
        /// 几何平均数：(x1*x2*...*xn)^(1/n)
        /// </summary>
        /// <param name="arr">数组</param>
        /// <returns>几何平均数</returns>
        public static double GeometricMean(double[] arr)
        {
            double result = 1;
            foreach (double num in arr)
            {
                result *= Math.Pow(num, 1.0 / arr.Length);
            }
            return result;
        }

        /// <summary>
        /// 调和平均数：n/((1/x1)+(1/x2)+...+(1/xn))
        /// </summary>
        /// <param name="arr">数组</param>
        /// <returns>调和平均数</returns>
        public static double HarmonicMean(double[] arr)
        {
            double temp = 0;
            foreach (double num in arr)
            {
                temp += (1.0 / num);
            }
            return arr.Length / temp;
        }

        /// <summary>
        /// 平方平均数：((x1*x1+x2*x2+...+xn*xn)/n)^(1/2)
        /// </summary>
        /// <param name="arr">数组</param>
        /// <returns>平方平均数</returns>
        public static double RootMeanSquare(double[] arr)
        {
            double temp = 0;
            foreach (double num in arr)
            {
                temp += (num * num);
            }
            return Math.Sqrt(temp / arr.Length);
        }

        /// <summary>
        /// 计算中位数
        /// </summary>
        /// <param name="arr">数组</param>
        /// <returns></returns>
        public static double Median(double[] arr)
        {
            //为了不修改arr值，对数组的计算和修改在tempArr数组中进行
            double[] tempArr = new double[arr.Length];
            arr.CopyTo(tempArr, 0);

            //对数组进行排序
            double temp;
            for (int i = 0; i < tempArr.Length; i++)
            {
                for (int j = i; j < tempArr.Length; j++)
                {
                    if (tempArr[i] > tempArr[j])
                    {
                        temp = tempArr[i];
                        tempArr[i] = tempArr[j];
                        tempArr[j] = temp;
                    }
                }
            }

            //针对数组元素的奇偶分类讨论
            if (tempArr.Length % 2 != 0)
            {
                return tempArr[arr.Length / 2 + 1];
            }
            else
            {
                return (tempArr[tempArr.Length / 2] +
                    tempArr[tempArr.Length / 2 + 1]) / 2.0;
            }
        }

        public static bool CheckURLValid(string source)
        {
            Uri uriResult;
            return Uri.TryCreate(source, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }

        public static bool IsImage(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            List<string> jpg = new List<string> { "FF", "D8" };
            List<string> bmp = new List<string> { "42", "4D" };
            List<string> gif = new List<string> { "47", "49", "46" };
            List<string> png = new List<string> { "89", "50", "4E", "47", "0D", "0A", "1A", "0A" };
            List<List<string>> imgTypes = new List<List<string>> { jpg, bmp, gif, png };

            List<string> bytesIterated = new List<string>();

            for (int i = 0; i < 8; i++)
            {
                string bit = stream.ReadByte().ToString("X2");
                bytesIterated.Add(bit);

                bool isImage = imgTypes.Any(img => !img.Except(bytesIterated).Any());
                if (isImage)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
