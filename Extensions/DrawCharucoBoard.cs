using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenCvSharp.Aruco;
using OpenCV.Net;

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
                CvAruco.DrawDetectedCornersCharuco(outputArray, value.CharucoCorners, value.CharucoIds);
                CvAruco.DrawDetectedMarkers(outputArray, value.MarkerCorners, value.MarkerIds);
            }

            return result;
        });
    }
}
