using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenCvSharp;
using Bonsai.Vision;

[Combinator]
[Description("")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class CalibrateCameraCharuco
{
    /// <summary>
    /// Gets or sets a value specifying the operation flags used for calibrating
    /// camera intrinsics.
    /// </summary>
    [Description("Specifies the operation flags used for calibrating camera intrinsics.")]
    public CalibrationFlags CalibrationFlags { get; set; }

    public IObservable<CameraCalibration> Process(IObservable<IEnumerable<CharucoBoard>> source)
    {
        return Observable.Defer(() =>
        {
            return source.Select(value =>
            {
                var cameraMatrix = new double[,]
                {
                    { 1169,    0, 720 },
                    {    0, 1169, 540 },
                    {    0,    0,   1} 
                };
                var distCoeffs = new double[] { 0, 0, 0, 0, 0 };

                var image = value.First();
                var objectPoints = value.Select(board => board.ObjectCorners);
                var imagePoints = value.Select(board => board.CharucoCorners);
                // var reprojectionError = 0;
                Vec3d[] rvecs, tvecs;
                var reprojectionError = Cv2.CalibrateCamera(
                    objectPoints,
                    imagePoints,
                    image.ImageSize,
                    cameraMatrix,
                    distCoeffs,
                    out rvecs,
                    out tvecs,
                    CalibrationFlags | CalibrationFlags.UseIntrinsicGuess | CalibrationFlags.FixPrincipalPoint,
                    TermCriteria.Both(10000, 1e-9));

                return new CameraCalibration
                {
                    Intrinsics = new Intrinsics
                    {
                        PrincipalPoint = new OpenCV.Net.Point2d(cameraMatrix[0, 2], cameraMatrix[1, 2]),
                        FocalLength = new OpenCV.Net.Point2d(cameraMatrix[0, 0], cameraMatrix[1, 1]),
                        RadialDistortion = new OpenCV.Net.Point3d(distCoeffs[0], distCoeffs[1], distCoeffs[2]),
                        TangentialDistortion = new OpenCV.Net.Point2d(distCoeffs[3], distCoeffs[4]),
                        ImageSize = image.Image.Size
                    },
                    ReprojectionError = reprojectionError
                };
            });
        });
    }
}
