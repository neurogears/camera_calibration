using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using OpenCvSharp.Aruco;

[Combinator]
[Description("")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class FindCharucoBoard
{
    public FindCharucoBoard()
    {
        InterpolateCorners = false;
        SubPixelRegion = new Size(3, 3);
    }

    public Size BoardSize { get; set; }

    public float SquareLength { get; set; }

    public float MarkerLength { get; set; }

    public PredefinedDictionaryType Dictionary { get; set; }

    public Size SubPixelRegion { get; set; }

    public bool InterpolateCorners { get; set; }

    public IObservable<CharucoBoard> Process(IObservable<IplImage> source)
    {
        return Observable.Defer(() =>
        {
            OpenCvSharp.Mat grayTemp = null;
            return source.Select(value =>
            {
                CharucoBoard board;
                board.Image = value;
                var boardSize = BoardSize;
                var matType = new OpenCvSharp.MatType(value.ElementType);
                using (var inputArray = OpenCvSharp.Mat.FromPixelData(value.Height, value.Width, matType, value.ImageData))
                {
                    if (grayTemp == null || grayTemp.Size() != inputArray.Size())
                        grayTemp = new OpenCvSharp.Mat(inputArray.Rows, inputArray.Cols, inputArray.Type());
                    OpenCvSharp.Cv2.CvtColor(inputArray, grayTemp, OpenCvSharp.ColorConversionCodes.BGR2GRAY);

                    CvAruco.DetectCharucoBoard(
                        grayTemp,
                        boardSize.Width,
                        boardSize.Height,
                        SquareLength,
                        MarkerLength,
                        Dictionary,
                        out board.CharucoCorners,
                        out board.CharucoIds,
                        out board.MarkerCorners,
                        out board.MarkerIds);

                    if (board.CharucoCorners.Length > 0 && InterpolateCorners)
                        CvAruco.InterpolateCornersCharuco(
                            grayTemp,
                            boardSize.Width,
                            boardSize.Height,
                            SquareLength,
                            MarkerLength,
                            Dictionary,
                            board.MarkerCorners,
                            board.MarkerIds,
                            out board.CharucoCorners,
                            out board.CharucoIds);

                    var subPixelRegion = SubPixelRegion;
                    if (board.CharucoCorners.Length > 0 && subPixelRegion.Width > 0)
                        OpenCvSharp.Cv2.Find4QuadCornerSubpix(
                            grayTemp,
                            board.CharucoCorners,
                            new OpenCvSharp.Size(subPixelRegion.Width, subPixelRegion.Height));

                    var chessboardCorners = new OpenCvSharp.Point3f[boardSize.Width * boardSize.Height];
                    for (int i = 0; i < chessboardCorners.Length; i++)
                    {
                        chessboardCorners[i].X = i % boardSize.Width;
                        chessboardCorners[i].Y = i / boardSize.Width;
                        chessboardCorners[i].Z = 0;
                    }

                    board.ImageSize = inputArray.Size();
                    board.ObjectCorners = Array.ConvertAll(board.CharucoIds, i => chessboardCorners[i]);
                }

                return board;
            });
        });
    }
}

public struct CharucoBoard
{
    public IplImage Image;
    public OpenCvSharp.Size ImageSize;
    public OpenCvSharp.Point2f[] CharucoCorners;
    public int[] CharucoIds;
    public OpenCvSharp.Point2f[][] MarkerCorners;
    public int[] MarkerIds;
    public OpenCvSharp.Point3f[] ObjectCorners;
}
