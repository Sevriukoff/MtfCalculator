using System;
using System.Drawing;
using System.Linq;
using PracticalTask.Enums;
using PracticalTask.Interfaces;
using Image = _AVM.Library.Core.Image;
using System.Collections.Generic;

namespace PracticalTask.Services
{
    /// <summary>
    /// Сервис для работы с изображением, основанный на классе Image._8bit.
    /// </summary>
    public class ImageService
    {
        #region Properties

        public bool IsBinarized
        {
            get {return CheckImageIsBinarized();}
        }

        public int[] Histogram
        {
            get { return CreateHistogram(); }
        }
        
        public Size Size
        {
            get { return new Size(_image.Size.Width, _image.Size.Height); }
        }

        public byte this[int x, int y]
        {
            set { _image[x, y] = value; }
            get { return _image[x, y]; }
        }

        #endregion
        
        #region Fields

        private Image._8bit _image;
        private readonly IMathService _mathService;

        #endregion

        #region ctors

        public ImageService(Image._8bit image, IMathService mathService)
        {
            _mathService = mathService;
            _image = image;
        }
        
        public ImageService(Image._8bit.Pixel[,] pixel, IMathService mathService)
        {
            _mathService = mathService;
            _image = new Image._8bit(pixel);
        }

        public ImageService(string file, IMathService mathService)
        {
            _mathService = mathService;
            _image = new Image._8bit(file);
        }

        public ImageService(string file, Rectangle rectangle, IMathService mathService)
        {
            _mathService = mathService;
            _image = new Image._8bit(file, rectangle);
        }

        #endregion
        
        /// <summary>
        /// Определяет ориентацию ROI на изображении, не учитывая ориентацию острого угла ROI.
        /// </summary>
        /// <returns>одно из значений Rigt, Left, Bottom, Top.</returns>
        /// <exception cref="ArgumentException">ROI располагается на диагонали.</exception>
        public OrientationEnum GetEdgeOrientation()
        {
            float intensityS1 = _image.Averrage(0,0, _image.Size.Width / 2, _image.Size.Height / 2);
            float intensityS2 = _image.Averrage(_image.Size.Width / 2, 0, _image.Size.Width - 1, _image.Size.Height / 2);
            float intensityS3 = _image.Averrage(0, _image.Size.Height / 2, _image.Size.Width /2, _image.Size.Height - 1);
            float intensityS4 = _image.Averrage(_image.Size.Width / 2, _image.Size.Height / 2, _image.Size.Width - 1, _image.Size.Height - 1);

            float min = Math.Min(Math.Min(intensityS1, intensityS2), Math.Min(intensityS3, intensityS4));
            float max = Math.Max(Math.Max(intensityS1, intensityS2), Math.Max(intensityS3, intensityS4));
            
            bool sector1IsCloserToMin = _mathService.GetCloserNum(intensityS1, false, min, max) <= min;
            bool sector2IsCloserToMin = _mathService.GetCloserNum(intensityS2, false, min, max) <= min;
            bool sector3IsCloserToMin = _mathService.GetCloserNum(intensityS3, false, min, max) <= min;
            bool sector4IsCloserToMin = _mathService.GetCloserNum(intensityS4, false, min, max) <= min;
            
            if (sector1IsCloserToMin && sector3IsCloserToMin)
            {
                return intensityS1 + intensityS2 > intensityS3 + intensityS4 ? OrientationEnum.LeftDown : OrientationEnum.LeftUp;
            }
            if (sector2IsCloserToMin && sector4IsCloserToMin)
            {
                return intensityS1 + intensityS2 > intensityS3 + intensityS4 ? OrientationEnum.RightDown : OrientationEnum.RightUp;
            }
            if (sector1IsCloserToMin && sector2IsCloserToMin)
            {
                return intensityS1 + intensityS3 > intensityS2 + intensityS4 ? OrientationEnum.TopRight : OrientationEnum.TopLeft;
            }
            if (sector3IsCloserToMin && sector4IsCloserToMin)
            {
                return intensityS1 + intensityS3 > intensityS2 + intensityS4 ? OrientationEnum.BottomRight : OrientationEnum.BottomLeft;
            }
            
            throw new ArgumentException("Invalid input image");
        }

        /// <summary>
        /// Поворот изображения на угл кратный 90 по часовой стрелки.
        /// </summary>
        /// <param name="degrees">угл поворота.</param>
        /// <exception cref="ArgumentException">угл повора не кратен 90.</exception>
        public void Rotate(int degrees = 90) //TODO: Rotate antiClockWise.
        {
            if (degrees % 90 != 0)
                throw new ArgumentException("Degrees should be 90, 180, 270 or 360");
            
            for (int i = 0; i < degrees / 90; i++)
            {
                int width = _image.Size.Width, height = _image.Size.Height;
                byte[,] imgContainer = new byte[width, height];

                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        imgContainer[x, y] = _image[x, y];

                var pixelArray = new Image._8bit.Pixel[height, width];

                for (int x = 0; x < height; x++)
                    for (int y = 0; y < width; y++)
                        pixelArray[x, y].Gray = imgContainer[y, height - 1 - x];
                
                _image = new Image._8bit(pixelArray);
            }
        }

        /// <summary>
        /// Зеркальное отражение изображения по горизонтали.
        /// </summary>
        public void FlipHorizontal()
        {
            var pixelArrayHeight = _image.Size.Height;
            var pixelArrayWidth = _image.Size.Width;

            var pixelArray = new Image._8bit.Pixel[pixelArrayWidth, pixelArrayHeight];
            _image.CopyTo(pixelArray);

            for (int y = 0; y < _image.Size.Height; y++)
            {
                for (int x = 0; x < _image.Size.Width / 2; x++)
                {
                    pixelArray[x, y].Gray = _image[_image.Size.Width - 1 - x , y];
                    pixelArray[pixelArrayWidth - 1 - x, y].Gray = _image[x, y]; ;
                }
            }

            _image = new Image._8bit(pixelArray);
        }
        
        /// <summary>
        /// Зеркальное отражение изображения по вертикали.
        /// </summary>
        public void FlipVertical()
        {
            var pixelArrayHeight = _image.Size.Height;
            var pixelArrayWidth = _image.Size.Width;

            var pixelArray = new Image._8bit.Pixel[pixelArrayWidth, pixelArrayHeight];
            _image.CopyTo(pixelArray);

            for (int x = 0; x < _image.Size.Width; x++)
            {
                for (int y = 0; y < _image.Size.Height / 2; y++)
                {
                    pixelArray[x, y].Gray = _image[x, _image.Size.Height - 1 - y];
                    pixelArray[x, pixelArrayHeight - 1 - y].Gray = _image[x, y];
                }
            }
            
            _image = new Image._8bit(pixelArray);
        }

        /// <summary>
        /// Бинаризация изображения на основе данных гистограммы.
        /// </summary>
        /// <param name="bottomThreshold">нижний порог бинаризации.</param>
        /// <param name="topThreshold">верхний порог бинаризации.</param>
        /// <param name="percentThreshold"></param>
        public void Binarize(int bottomThreshold, int topThreshold, int percentThreshold = 60)
        {
            if (IsBinarized)
                return;

            float onePercent = (float)(topThreshold - bottomThreshold) / 100;
            byte numThreshold = Convert.ToByte(onePercent * percentThreshold);
            Image._8bit.Pixel[,] array = new Image._8bit.Pixel[_image.Size.Width, _image.Size.Height];

            for (int x = 0; x < _image.Size.Width; x++)
            {
                for (int y = 0; y < _image.Size.Height; y++)
                {
                    if (_image[x,y] >= numThreshold)
                    {
                        array[x, y].Gray = 255;
                    }
                    else if (_image[x,y] <= numThreshold)
                    {
                        array[x, y].Gray = 0;
                    }
                    else
                    {
                        array[x, y].Gray = _image[x, y];
                    }
                }
            }

            _image = new Image._8bit(array);
        }

        public Image._8bit GetImageCopy()
        {
            Image._8bit.Pixel[,] array = new Image._8bit.Pixel[_image.Size.Width, _image.Size.Height];

            for (int x = 0; x < _image.Size.Width; x++)
            {
                for (int y = 0; y < _image.Size.Height; y++)
                {
                    array[x, y].Gray = _image[x, y];
                }
            }
            
            return new Image._8bit(array);
        }

        public void DrawFunction(Func<int, int> func, byte brightness = 255)
        {
            for (int x = 0; x < _image.Size.Width; x++)
            {
                int y = func(x);
                _image[x, y] = brightness;
            }
        }

        public float AverrageIntensity(int x1, int y1, int x2, int y2)
        {
            var result = _image.Averrage(x1, y1, x2, y2);
            return result;
        }
        
        /// <summary>
        /// Сохранение изображения.
        /// </summary>
        /// <param name="file">путь сохранения. Можно указать как относительный, так и абсолютный путь.</param>
        public void Save(string file)
        {
            _image.Save(file);
        }
        
        /// <summary>
        /// Двухэтапная очистка изображения от шумов. Первый этап - фильтрация от чёрных пикселей на фоне белых.
        /// Второй этап - фильтрация от белых пикселей на фоне чёрных.
        /// </summary>
        /// <param name="innerN">Внутренний радиус матрицы</param>
        /// <param name="outerN">Внешний радиус матрицы</param>
        /// <param name="percentSensitivity">Чувствительность фильтрации. Например при 51% равно 5 / 8 или 17 / 32</param>
        public void CleaningNoise(int innerN, int outerN = -1, int percentSensitivity = 51)
        {
            outerN = outerN == -1 ? innerN : outerN;

            int dimension = (int) Math.Pow(innerN + outerN * 2, 2);
            int countFreeCells = dimension - innerN * innerN;

            float blackSensitivity =
                (float) (255 * (countFreeCells - Math.Ceiling(countFreeCells * ((float)percentSensitivity / 100))) / dimension);
            float whiteSensitivity =
                (float) (255 * (dimension - (countFreeCells - Math.Ceiling(countFreeCells * ((float)percentSensitivity / 100)))) / dimension);

            CleaningNoiseInternal(innerN, outerN, blackSensitivity, true); //Фильтрация от чёрных пикселей на фоне белых
            //CleaningNoiseInternal(innerN, outerN, whiteSensitivity, false); //Фильтрация от белых пикселей на фоне чёрных
        }

        private void CleaningNoiseInternal(int innerN, int outerN, float sensitivity, bool blackFilter)
        {
            for (int x = innerN; x < _image.Size.Width - (innerN - 1) - outerN - 1; x += innerN)
            {
                for (int y = innerN; y < _image.Size.Height - (innerN - 1) - outerN - 1; y += innerN)
                {
                    float averrage = _image.Averrage(x - outerN, y - outerN, x + (innerN - 1) + outerN,
                        y + (innerN - 1) + outerN);

                    if (averrage >= sensitivity && !blackFilter) // TODO: Refactor condition.
                        continue;
                    
                    if (averrage <= sensitivity && blackFilter) 
                        continue;
                    
                    for (int i = 0; i < innerN; i++)
                    {
                        for (int j = 0; j < innerN; j++)
                        {
                            _image[x + i, y + j] = (byte) (blackFilter ? 255 : 0);
                        }
                    }
                }
            }
        }
        
        private void CleaningNoiseInternalBySectors(int innerN, int outerN, float sensitivity, bool blackFilter)
        {
            for (int x = innerN; x < _image.Size.Width - (innerN - 1) - outerN - 1; x += innerN)
            {
                for (int y = innerN; y < _image.Size.Height - (innerN - 1) - outerN - 1; y += innerN)
                {
                    float sector1 = _image.Averrage(x - outerN, y - outerN, x, y);
                    float sector2 = _image.Averrage(x, y - outerN, x + outerN, y);
                    float sector3 = _image.Averrage(x + outerN, y - outerN, x + innerN + outerN, y);
                    float sector4 = _image.Averrage(x - outerN, y, x, y + innerN);
                    float sector5 = _image.Averrage(x + innerN, y, x + innerN + outerN, y + innerN);
                    float sector6 = _image.Averrage(x - outerN, y + innerN, x, y + innerN + outerN);
                    float sector7 = _image.Averrage(x, y + innerN, x + innerN, y + innerN + outerN);
                    float sector8 = _image.Averrage(x + innerN, y + innerN, x + innerN + outerN, y + innerN + outerN);

                    float centerSector = _image.Averrage(x, y, x + innerN, y + innerN);

                    for (int i = 0; i < innerN; i++)
                    {
                        for (int j = 0; j < innerN; j++)
                        {
                            _image[x + i, y + j] = (byte) (blackFilter ? 255 : 0);
                        }
                    }
                }
            }
        }

        private bool CheckImageIsBinarized()
        {
            for (int i = 0; i < _image.Size.Width; i++)
            {
                for (int j = 0; j < _image.Size.Height; j++)
                {
                    if (_image[i,j] % 255 != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private int[] CreateHistogram()
        {
            var histogram = new int[256];
            
            for (int i = 0; i < _image.Size.Width; i++)
            {
                for (int j = 0; j < _image.Size.Height; j++)
                {
                    histogram[_image[i, j]]++;
                }
            }

            return histogram;
        }
    }
}
        