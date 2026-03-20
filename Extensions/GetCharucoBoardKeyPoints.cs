using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using Bonsai.Vision;

[Combinator]
[Description("")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class GetCharucoBoardKeyPoints
{
    public IObservable<KeyPointCollection> Process(IObservable<CharucoBoard> source)
    {
        return source.Select(value =>
        {
            var keyPoints = new KeyPointCollection(value.Image);
            foreach (var corner in value.CharucoCorners)
                keyPoints.Add(new Point2f(corner.X, corner.Y));
            return keyPoints;
        });
    }
}
