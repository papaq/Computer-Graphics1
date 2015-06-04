using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public WriteableBitmap bm;
        bool firstDot;
        int xA = 0, yA = 0, xB = 0, yB = 0;
        int imgHeight, imgWidth;
        int radius1 = 100, radius2 = 100;

        private Dictionary<int, Action<int, int, int, int>> _algorithms = new Dictionary<int, Action<int, int, int, int>>();

        private void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

        public MainWindow()
        {
            InitializeComponent();
            initImage();
            initDictionary();
        }

        private void initImage()
        {
            imgHeight = (int)image.Height;
            imgWidth = (int)image.Width;
            //image = new Image();
            bm = new WriteableBitmap(imgWidth, imgHeight, 96, 96, PixelFormats.Bgra32, null);
            image.Source = bm;
        }

        private void initDictionary()
        {
            _algorithms.Add(0, algorithmDDA);
            _algorithms.Add(1, algorithmBresenham);
            _algorithms.Add(2, algorithmWu);
        }

        private void drawPixel(int x1, int y1)
        {
            if (x1 < 1 || x1 > imgWidth || y1 < 1 || y1 > imgHeight)
            {
                return;
            }
            byte blue = 0;
            byte green = 0;
            byte red = 0;
            byte alpha = 255;
            byte[] colorData = { blue, green, red, alpha };
            Int32Rect rect = new Int32Rect(x1, y1, 1, 1);
             bm.WritePixels(rect, colorData, 4, 0);
        }

        private void drawPixel(int x1, int y1, double transparent)
        {
            if (x1 < 1 || x1 > imgWidth - 1 || y1 < 1 || y1 > imgHeight - 1)
            {
                return;
            }
            byte blue = 0;
            byte green = 0;
            byte red = 0;
            byte alpha = (byte)(255 * transparent);
            byte[] colorData = { blue, green, red, alpha };
            Int32Rect rect = new Int32Rect(x1, y1, 1, 1);
            bm.WritePixels(rect, colorData, 4, 0);
        }

        private void buttonDraw_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(textBoxR1.Text, out radius1))
                textBoxR1.Text = "100";
            if (!int.TryParse(textBoxR2.Text, out radius2))
                textBoxR2.Text = "100";
            switch (comboLine.SelectedIndex)
            {
                case 0: _algorithms[comboAlgorithm.SelectedIndex](xA, yA, xB, yB);
                    break;
                case 1: circle((firstDot ? xA : xB), (firstDot ? yA : yB));
                    break;
                case 2: ellipse((firstDot ? xA : xB), (firstDot ? yA : yB));
                    break;
                case 3: initials();
                    break;
                default: return;
            }
        }

        private void algorithmDDA(int x1, int y1, int x2, int y2)
        {
            double l = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1)),
                xStart = x1,
                yStart = y1,
                deltaX = (x2 - x1) / l,
                deltaY = (y2 - y1) / l;

            for (int i = 0; i < (int)l + 1; i++)
            {
                drawPixel((int)xStart, (int)yStart);
                xStart += deltaX;
                yStart += deltaY;
            }

        }

        private void algorithmBresenham(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Swap<int>(ref x0, ref y0);
                Swap<int>(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Swap<int>(ref x0, ref x1);
                Swap<int>(ref y0, ref y1);
            }
            int dX = x1 - x0,
                ystep = (y0 < y1 ? 1 : -1),
                dY = ystep * (y1 - y0),
                err = dX / 2,
                y = y0;

            for (int x = x0; x <= x1; ++x)
            {
                drawPixel((steep ? y : x), (steep ? x : y));
                err = err - dY;
                if (err < 0)
                {
                    y += ystep;
                    err += dX;
                }
            }
        }

        private void algorithmWu(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Swap<int>(ref x0, ref y0);
                Swap<int>(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Swap<int>(ref x0, ref x1);
                Swap<int>(ref y0, ref y1);
            }

            double dX = x1 - x0,
                dY = y1 - y0,
                gradient = dY / dX,
                y = y0 + gradient;

            drawPixel((steep ? (int)y0 : (int)x0), (steep ? (int)x0 : (int)y0));
            drawPixel((steep ? (int)y1 : (int)x1), (steep ? (int)x1 : (int)y1));

            for (int x = x0 + 1; x < x1; ++x)
            {
                drawPixel((steep ? (int)y : (int)x), (steep ? (int)x : (int)y), 1 - (y - (int)y));
                drawPixel((steep ? (int)y + 1 : (int)x), (steep ? (int)x : (int)y + 1), y - (int)y);
                y += gradient;
            }
        }

        private void circle(int x1, int y1)
        {
            int x = 0,
                y = radius1,
                delta = 1 - 2 * radius1,
                error = 0;
            while (y >= 0)
            {

                drawPixel(x1 + x, y1 + y);
                drawPixel(x1 + x, y1 - y);
                drawPixel(x1 - x, y1 + y);
                drawPixel(x1 - x, y1 - y);
                error = 2 * (delta + y) - 1;
                if ((delta < 0) && (error <= 0))
                {
                    delta += 2 * ++x + 1;
                    continue;
                }
                error = 2 * (delta - x) - 1;
                if ((delta > 0) && (error > 0))
                {
                    delta += 1 - 2 * --y;
                    continue;
                }
                x++;
                delta += 2 * (x - y);
                y--;
            }
        }

        private void ellipse(int x1, int y1)
        {
            int a2 = radius1 * radius1;
            int b2 = radius2 * radius2;
            int fa2 = 4 * a2, fb2 = 4 * b2;
            int x, y, sigma;

            /* first half */
            for (x = 0, y = radius2, sigma = 2 * b2 + a2 * (1 - 2 * radius2); b2 * x <= a2 * y; x++)
            {
                drawPixel(x1 + x, y1 + y);
                drawPixel(x1 - x, y1 + y);
                drawPixel(x1 + x, y1 - y);
                drawPixel(x1 - x, y1 - y);
                if (sigma >= 0)
                {
                    sigma += fa2 * (1 - y);
                    y--;
                }
                sigma += b2 * ((4 * x) + 6);
            }

            /* second half */
            for (x = radius1, y = 0, sigma = 2 * a2 + b2 * (1 - 2 * radius1); a2 * y <= b2 * x; y++)
            {
                drawPixel(x1 + x, y1 + y);
                drawPixel(x1 - x, y1 + y);
                drawPixel(x1 + x, y1 - y);
                drawPixel(x1 - x, y1 - y);
                if (sigma >= 0)
                {
                    sigma += fb2 * (1 - x);
                    x--;
                }
                sigma += a2 * ((4 * y) + 6);
            }
        }

        private void initials()
        {
            radius1 = 70; 
            radius2 = 200; 
            ellipse(220, 250);
            algorithmBresenham(150, 250, 80, 250);
            algorithmBresenham(80, 50, 80, 450);

            ellipse(520, 250);
            algorithmBresenham(450, 250, 380, 250);
            algorithmBresenham(380, 50, 380, 450);
                //algorithmBresenham();
                //algorithmBresenham();
        }

        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(image);
            drawPixel((int)p.X, (int)p.Y);
            if (firstDot)
            {
                xB = (int)p.X;
                yB = (int)p.Y;
            }
            else
            {
                xA = (int)p.X;
                yA = (int)p.Y;
            }

            firstDot = !firstDot;
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            image.Source = null;
            bm = new WriteableBitmap(imgWidth, imgHeight, 96, 96, PixelFormats.Bgra32, null);
            image.Source = bm;
        }

        private void comboLine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboAlgorithm == null)
            {
                return;
            }
            if (comboLine.SelectedIndex != 0 && comboLine.SelectedIndex != 3)
            {
                comboAlgorithm.IsEnabled = false;
                gridR.Visibility = Visibility.Visible;
                comboAlgorithm.SelectedIndex = 1;
            }
            else
            {
                gridR.Visibility = Visibility.Hidden;
                if (comboLine.SelectedIndex == 3)
                    comboAlgorithm.IsEnabled = false;
                else
                    comboAlgorithm.IsEnabled = true;
            }
            if (comboLine.SelectedIndex == 2)
            {
                textBoxR2.IsEnabled = true;
            }
            if (comboLine.SelectedIndex == 3)
            {
                textBoxR2.IsEnabled = false;
            }
        }

    }
}
