using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jane.Common.Utility.Html
{
    public static class Captcha
    {
        private static double[] addVector(double[] a, double[] b)
        {
            return new double[] { a[0] + b[0], a[1] + b[1], a[2] + b[2] };
        }

        private static double[] scalarProduct(double[] vector, double scalar)
        {
            return new double[] { vector[0] * scalar, vector[1] * scalar, vector[2] * scalar };
        }

        private static double dotProduct(double[] a, double[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        private static double norm(double[] vector)
        {
            return Math.Sqrt(dotProduct(vector, vector));
        }

        private static double[] normalize(double[] vector)
        {
            return scalarProduct(vector, 1.0 / norm(vector));
        }

        private static double[] crossProduct(double[] a, double[] b)
        {
            return new double[]
                    {
                    (a[1] * b[2] - a[2] * b[1]),
                    (a[2] * b[0] - a[0] * b[2]),
                    (a[0] * b[1] - a[1] * b[0])
                    };
        }

        private static double[] vectorProductIndexed(double[] v, double[] m, int i)
        {
            return new double[]
                    {
                    v[i + 0] * m[0] + v[i + 1] * m[4] + v[i + 2] * m[8] + v[i + 3] * m[12],
                    v[i + 0] * m[1] + v[i + 1] * m[5] + v[i + 2] * m[9] + v[i + 3] * m[13],
                    v[i + 0] * m[2] + v[i + 1] * m[6] + v[i + 2] * m[10]+ v[i + 3] * m[14],
                    v[i + 0] * m[3] + v[i + 1] * m[7] + v[i + 2] * m[11]+ v[i + 3] * m[15]
                    };
        }

        private static double[] vectorProduct(double[] v, double[] m)
        {
            return vectorProductIndexed(v, m, 0);
        }

        private static double[] matrixProduct(double[] a, double[] b)
        {
            double[] o1 = vectorProductIndexed(a, b, 0);
            double[] o2 = vectorProductIndexed(a, b, 4);
            double[] o3 = vectorProductIndexed(a, b, 8);
            double[] o4 = vectorProductIndexed(a, b, 12);

            return new double[]
                    {
                    o1[0], o1[1], o1[2], o1[3],
                    o2[0], o2[1], o2[2], o2[3],
                    o3[0], o3[1], o3[2], o3[3],
                    o4[0], o4[1], o4[2], o4[3]
                    };
        }

        private static double[] cameraTransform(double[] C, double[] A)
        {
            double[] w = normalize(addVector(C, scalarProduct(A, -1)));
            double[] y = new double[] { 0, 1, 0 };
            double[] u = normalize(crossProduct(y, w));
            double[] v = crossProduct(w, u);
            double[] t = scalarProduct(C, -1);

            return new double[]
                    {
                    u[0], v[0], w[0], 0,
                    u[1], v[1], w[1], 0,
                    u[2], v[2], w[2], 0,
                    dotProduct(u, t), dotProduct(v, t), dotProduct(w, t), 1
                    };
        }

        private static double[] viewingTransform(double fov, double n, double f)
        {
            fov *= (Math.PI / 180);
            double cot = 1.0 / Math.Tan(fov / 2);
            return new double[] { cot, 0, 0, 0, 0, cot, 0, 0, 0, 0, (f + n) / (f - n), -1, 0, 0, 2 * f * n / (f - n), 0 };
        }

        public static Image Generate(string captchaText)
        {
            int fontsize = 24;
            Font font = new Font("Arial", fontsize);

            SizeF sizeF;
            using (Graphics g = Graphics.FromImage(new Bitmap(1, 1)))
            {
                sizeF = g.MeasureString(captchaText, font, 0, StringFormat.GenericDefault);
            }

            int image2d_x = (int)sizeF.Width;
            int image2d_y = (int)(fontsize * 1.3);

            Bitmap image2d = new Bitmap(image2d_x, image2d_y);
            Color black = Color.Black;
            Color white = Color.White;

            using (Graphics g = Graphics.FromImage(image2d))
            {
                g.Clear(black);
                g.DrawString(captchaText, font, Brushes.White, 0, 0);
            }

            Random rnd = new Random();
            double[] T = cameraTransform(new double[] { rnd.Next(-90, 90), -200, rnd.Next(150, 250) }, new double[] { 0, 0, 0 });
            T = matrixProduct(T, viewingTransform(60, 300, 3000));

            double[][] coord = new double[image2d_x * image2d_y][];

            int count = 0;
            for (int y = 0; y < image2d_y; y += 2)
            {
                for (int x = 0; x < image2d_x; x++)
                {
                    int xc = x - image2d_x / 2;
                    int zc = y - image2d_y / 2;
                    double yc = -(double)(image2d.GetPixel(x, y).ToArgb() & 0xff) / 256 * 4;
                    double[] xyz = new double[] { xc, yc, zc, 1 };
                    xyz = vectorProduct(xyz, T);
                    coord[count] = xyz;
                    count++;
                }
            }

            int image3d_x = 256;
            int image3d_y = image3d_x * 9 / 16;
            Bitmap image3d = new Bitmap(image3d_x, image3d_y);
            Color fgcolor = Color.White;
            Color bgcolor = Color.Black;
            using (Graphics g = Graphics.FromImage(image3d))
            {
                g.Clear(bgcolor);
                count = 0;
                double scale = 1.75 - (double)image2d_x / 400;
                for (int y = 0; y < image2d_y; y += 2)
                {
                    for (int x = 0; x < image2d_x; x++)
                    {
                        if (x > 0)
                        {
                            double x0 = coord[count - 1][0] * scale + image3d_x / 2;
                            double y0 = coord[count - 1][1] * scale + image3d_y / 2;
                            double x1 = coord[count][0] * scale + image3d_x / 2;
                            double y1 = coord[count][1] * scale + image3d_y / 2;
                            g.DrawLine(new Pen(fgcolor), (float)x0, (float)y0, (float)x1, (float)y1);
                        }
                        count++;
                    }
                }
            }
            return image3d;
        }
    }
    public static class StringCAPTCHA { 
        /// <summary>
        /// create a random key
        /// </summary>
        static readonly Random Random = new Random(~unchecked((int)DateTime.Now.Ticks));
        static readonly char[] NumberList = { '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        static readonly char[] CharList = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        static readonly char[] MixedList = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' }; //remove I & O

        #region 生成随机数字
        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        public static string Number(int Length)
        {
            return Create(Length, false, NumberList);
        }

        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        public static string Number(int Length, bool Sleep)
        {
            return Create(Length, Sleep, NumberList);
        }
        #endregion

        #region 生成随机字母与数字
        /// <summary>
        /// 生成随机字母与数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        public static string Mixed(int Length)
        {
            return Create(Length, false, MixedList);
        }

        /// <summary>
        /// 生成随机字母与数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        public static string Mixed(int Length, bool Sleep)
        {
            return Create(Length, Sleep, MixedList);
        }
        #endregion

        #region 生成随机纯字母随机数
        /// <summary>
        /// 生成随机纯字母随机数
        /// </summary>
        /// <param name="Length">生成长度</param>
        public static string Char(int Length)
        {
            return Create(Length, false, CharList);
        }

        /// <summary>
        /// 生成随机纯字母随机数
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        public static string Char(int Length, bool Sleep)
        {
            return Create(Length, Sleep, CharList);
        }
        #endregion

        /// <summary>
        /// Create the CAPTCHA specified Length, Sleep and List.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="Length">Length.</param>
        /// <param name="Sleep">If set to <c>true</c> sleep.</param>
        /// <param name="List">List create CAPTCHA based on</param>
        private static string Create(int Length, bool Sleep, char[] List)
        {
            if (Sleep) Thread.Sleep(3);
            char[] Pattern = List;
            string result = string.Empty;
            int n = Pattern.Length;

            for (int i = 0; i < Length; i++)
            {
                int rnd = Random.Next(0, n);
                result += Pattern[rnd];
            }
            return result;
        }



    }

    /// <summary>
    /// 验证图片类
    /// </summary>
    public class PictureCAPTCHA
    {
        #region 私有字段
        private string Text { get; set; }
        private Bitmap Image { get; set; }
        private int LetterCount { set; get; }  //验证码位数
        private int Type { set; get; }
        private int letterWidth = 16;  //单个字体的宽度范围
        private int letterHeight = 20; //单个字体的高度范围
        private static Random Random = new Random(~unchecked((int)DateTime.Now.Ticks));
        private Font[] fonts =
        {
           new Font(new FontFamily("Times New Roman"),10 +Random.Next(1),FontStyle.Regular),
           new Font(new FontFamily("Georgia"), 10 + Random.Next(1),FontStyle.Regular),
           new Font(new FontFamily("Arial"), 10 + Random.Next(1),FontStyle.Regular),
           new Font(new FontFamily("Comic Sans MS"), 10 + Random.Next(1),FontStyle.Regular)
        };
        #endregion



        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Common.Utility.PictureCAPTCHA"/> class. default length is 4 with number list 
        /// </summary>
        public PictureCAPTCHA()
        {
            LetterCount = 4;
            Type = 0;
            InitText();
            CreateImage();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Common.Utility.PictureCAPTCHA"/> class with number list;
        /// </summary>
        /// <param name="Length">Length.</param>
		public PictureCAPTCHA(int Length)
        {
             LetterCount = Length;
            Text = StringCAPTCHA.Number(LetterCount);
            CreateImage();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Common.Utility.PictureCAPTCHA"/> class.
        /// </summary>
        /// <param name="Length">Length.</param>
        /// <param name="type">Type 0 number , 1 char , 2 mixed.</param>
		public PictureCAPTCHA(int Length, int type)
        {
            LetterCount = Length;
            Type = type;
            InitText();
            CreateImage();
        }

        #endregion

        #region Public function

        public void Redraw(bool NewCAPTCHA)
        {
            if (NewCAPTCHA)
            {
                InitText();
            }
            CreateImage();
        }

        #endregion


        #region private funcation

        private void InitText()
        {
            switch (Type)
            {
                case 0: Text = StringCAPTCHA.Number(LetterCount); break;
                case 1: Text = StringCAPTCHA.Char(LetterCount); break;
                case 2: Text = StringCAPTCHA.Mixed(LetterCount); break;
                default:

                    break;
            }
        }

        /// <summary>
        /// 绘制验证码
        /// </summary>
        private void CreateImage()
        {
            int ImageWidth = this.Text.Length * letterWidth;
            Bitmap Img = new Bitmap(ImageWidth, letterHeight);
            Graphics g = Graphics.FromImage(Img);
            g.Clear(Color.White);
            for (int i = 0; i < 2; i++)
            {
                int x1 = Random.Next(Img.Width - 1);
                int x2 = Random.Next(Img.Width - 1);
                int y1 = Random.Next(Img.Height - 1);
                int y2 = Random.Next(Img.Height - 1);
                g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
            }
            int _x = -12, _y = 0;
            for (int int_index = 0; int_index < this.Text.Length; int_index++)
            {
                _x += Random.Next(12, 16);
                _y = Random.Next(-2, 2);
                string str_char = this.Text.Substring(int_index, 1);
                str_char = Random.Next(1) == 1 ? str_char.ToLower() : str_char.ToUpper();
                Brush newBrush = new SolidBrush(GetRandomColor());
                Point thePos = new Point(_x, _y);
                g.DrawString(str_char, fonts[Random.Next(fonts.Length - 1)], newBrush, thePos);
            }
            for (int i = 0; i < 10; i++)
            {
                int x = Random.Next(Img.Width - 1);
                int y = Random.Next(Img.Height - 1);
                Img.SetPixel(x, y, Color.FromArgb(Random.Next(0, 255), Random.Next(0, 255), Random.Next(0, 255)));
            }
            Img = TwistImage(Img, true, Random.Next(1, 3), Random.Next(4, 6));
            g.DrawRectangle(new Pen(Color.LightGray, 1), 0, 0, ImageWidth - 1, (letterHeight - 1));
            Image = Img;
        }


        /// <summary>
        /// 字体随机颜色
        /// </summary>
        private Color GetRandomColor()
        {
            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);
            Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);
            int int_Red = RandomNum_First.Next(180);
            int int_Green = RandomNum_Sencond.Next(180);
            int int_Blue = (int_Red + int_Green > 300) ? 0 : 400 - int_Red - int_Green;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;
            return Color.FromArgb(int_Red, int_Green, int_Blue);
        }

        /// <summary>
        /// 正弦曲线Wave扭曲图片
        /// </summary>
        /// <param name="srcBmp">图片路径</param>
        /// <param name="bXDir">如果扭曲则选择为True</param>
        /// <param name="dMultValue">波形的幅度倍数，越大扭曲的程度越高,一般为3</param>
        /// <param name="dPhase">波形的起始相位,取值区间[0-2*PI)</param>
        private Bitmap TwistImage(Bitmap srcBmp, bool bXDir, double dMultValue, double dPhase)
        {
            double PI = 6.283185307179586476925286766559;
            Bitmap destBmp = new Bitmap(srcBmp.Width, srcBmp.Height);
            Graphics graph = Graphics.FromImage(destBmp);
            graph.FillRectangle(new SolidBrush(Color.White), 0, 0, destBmp.Width, destBmp.Height);
            graph.Dispose();
            double dBaseAxisLen = bXDir ? (double)destBmp.Height : (double)destBmp.Width;
            for (int i = 0; i < destBmp.Width; i++)
            {
                for (int j = 0; j < destBmp.Height; j++)
                {
                    double dx = 0;
                    dx = bXDir ? (PI * (double)j) / dBaseAxisLen : (PI * (double)i) / dBaseAxisLen;
                    dx += dPhase;
                    double dy = Math.Sin(dx);
                    int nOldX = 0, nOldY = 0;
                    nOldX = bXDir ? i + (int)(dy * dMultValue) : i;
                    nOldY = bXDir ? j : j + (int)(dy * dMultValue);

                    Color color = srcBmp.GetPixel(i, j);
                    if (nOldX >= 0 && nOldX < destBmp.Width
                     && nOldY >= 0 && nOldY < destBmp.Height)
                    {
                        destBmp.SetPixel(nOldX, nOldY, color);
                    }
                }
            }
            srcBmp.Dispose();
            return destBmp;
        }
        #endregion
    }

}
