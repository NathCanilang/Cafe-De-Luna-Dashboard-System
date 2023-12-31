﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CafeDeLunaSystem
{
    internal class CreateAndEditAcc
    {
        public Bitmap ResizeImage(Image sourceImage, int maxWidth, int maxHeight)
        {
            double aspectRatio = (double)sourceImage.Width / sourceImage.Height;
            int newWidth, newHeight;

            if (sourceImage.Width > sourceImage.Height)
            {
                newWidth = maxWidth;
                newHeight = (int)(maxWidth / aspectRatio);
            }
            else
            {
                newHeight = maxHeight;
                newWidth = (int)(maxHeight * aspectRatio);
            }

            using (Bitmap resizedImage = new Bitmap(newWidth, newHeight))
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                graphics.DrawImage(sourceImage, 0, 0, newWidth, newHeight);

                return new Bitmap(resizedImage);
            }
        }

        public int AgeCalculation(DateTime employeeBirth)
        {
            int years = DateTime.Now.Year - employeeBirth.Year;

            if (employeeBirth.AddYears(years) > DateTime.Now) years--;
            return years;
        }
    }
}
