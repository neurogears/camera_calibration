using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenCvSharp.Aruco;
using OpenCV.Net;
using OpenCvSharp;

[Combinator]
[Description("")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class DrawCharucoBoard
{
    public IObservable<IplImage> Process(IObservable<CharucoBoard> source)
    {
        return source.Select(value =>
        {
            var image = value.Image;
            var result = image.Clone();
            var matType = new OpenCvSharp.MatType(image.ElementType);
            using (var inputArray = OpenCvSharp.Mat.FromPixelData(image.Height, image.Width, matType, image.ImageData))
            using (var outputArray = OpenCvSharp.Mat.FromPixelData(result.Height, result.Width, matType, result.ImageData))
            {
                var cornerCountTotal = value.BoardSize.Width * value.BoardSize.Height;
                for (int i = 0; i < value.CharucoCorners.Length; i++)
                {
                    var id = value.CharucoIds[i];
                    var position = value.CharucoCorners[i].ToPoint();
                    var hue = id / (double)cornerCountTotal;
                    var color = HsvToRgb(hue, 1, 1);
                    color.Val0 *= 255;
                    color.Val1 *= 255;
                    color.Val2 *= 255;
                    Cv2.Circle(outputArray, position, 15, color, -1);
                }
            }

            return result;
        });
    }

    public static OpenCvSharp.Scalar HsvToRgb(double hue, double saturation, double value)
    {
        if (saturation == 0.0)
            return new OpenCvSharp.Scalar(value, value, value);

        var i = (int)(hue*6.0);
        var f = (hue*6.0) - i;
        var p = value * (1.0 - saturation);
        var q = value * (1.0 - saturation * f);
        var t = value * (1.0 - saturation * (1.0 - f));
        i %= 6;

        switch(i)
        {
            case 0: return new OpenCvSharp.Scalar(value, t, p);
            case 1: return new OpenCvSharp.Scalar(q, value, p);
            case 2: return new OpenCvSharp.Scalar(p, value, t);
            case 3: return new OpenCvSharp.Scalar(p, q, value);
            case 4: return new OpenCvSharp.Scalar(t, p, value);
            case 5:
            default: return new OpenCvSharp.Scalar(value, p, q);
        }
    }
}
