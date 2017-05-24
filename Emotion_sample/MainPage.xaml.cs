using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using Microsoft.ProjectOxford.Emotion;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;


// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace Emotion_sample
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture capture;
        private StorageFile file;
        private EmotionServiceClient client;

        /// <summary>
        /// 
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //クライアント
            client = new EmotionServiceClient("{your subscription key}");

            //キャプチャーの設定
            MediaCaptureInitializationSettings captureInitSettings = new MediaCaptureInitializationSettings();
            captureInitSettings.StreamingCaptureMode = StreamingCaptureMode.Video;
            captureInitSettings.PhotoCaptureSource = PhotoCaptureSource.VideoPreview;

            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            captureInitSettings.VideoDeviceId = devices[0].Id;

            capture = new MediaCapture();
            await capture.InitializeAsync(captureInitSettings);

            //キャプチャーのサイズなど
            VideoEncodingProperties vp = new VideoEncodingProperties();
            vp.Width = 320;
            vp.Height = 240;
            vp.Subtype = "YUY2";

            await capture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, vp);

            preview.Source = capture;
            await capture.StartPreviewAsync();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            resultview.Source = null;

            //ファイルにキャプチャーを保存
            file = await KnownFolders.PicturesLibrary.CreateFileAsync("emotion.jpg", CreationCollisionOption.ReplaceExisting);
            ImageEncodingProperties imageproperties = ImageEncodingProperties.CreateJpeg();
            await capture.CapturePhotoToStorageFileAsync(imageproperties, file);

            //保存したファイルの呼び出し
            IRandomAccessStream stream = await file.OpenReadAsync();
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(stream);
            resultview.Source = bitmap;
            
            //VisionAPI呼び出し
            Getdata();
        }

        /// <summary>
        /// 
        /// </summary>
        private async void Getdata()
        {
            //ファイル呼び出し
            var datafile = await KnownFolders.PicturesLibrary.GetFileAsync("emotion.jpg");
            var fileStream = await datafile.OpenAsync(FileAccessMode.Read);

            //APIの呼び出し
            var emotions = await client.RecognizeAsync(fileStream.AsStream());

            //結果を表示
            var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                canvas.Children.Clear();

                if (emotions != null)
                {
                    foreach (var emotion in emotions)
                    {
                        
                        var ranked = emotion.Scores.ToRankedList();

                        resultTbox.Text = "";
                        Color color = Colors.Transparent;

                        foreach(var rank in ranked)
                        {
                            switch (rank.Key)
                            {
                                case "Anger":
                                    resultTbox.Text = resultTbox.Text + "怒り : " + rank.Value.ToString("F4") + "\n";
                                    if (color == Colors.Transparent)
                                    {
                                        color = Colors.Red;
                                    }
                                    break;
                                case "Contempt":
                                    resultTbox.Text = resultTbox.Text + "軽蔑 : " + rank.Value.ToString("F4") + "\n";
                                    if (color == Colors.Transparent)
                                    {
                                        color = Colors.White;
                                    }
                                    break;
                                case "Disgust":
                                    resultTbox.Text = resultTbox.Text + "嫌悪 : " + rank.Value.ToString("F4") + "\n";
                                    if (color == Colors.Transparent)
                                    {
                                        color = Colors.Purple;
                                    }
                                    break;
                                case "Fear":
                                    resultTbox.Text = resultTbox.Text + "恐怖 : " + rank.Value.ToString("F4") + "\n";
                                    if (color == Colors.Transparent)
                                    {
                                        color = Colors.Black;
                                    }
                                    break;
                                case "Happiness":
                                    resultTbox.Text = resultTbox.Text + "喜び : " + rank.Value.ToString("F4") + "\n";
                                    if (color == Colors.Transparent)
                                    {
                                        color = Colors.Pink;
                                    }
                                    break;
                                case "Neutral":
                                    resultTbox.Text = resultTbox.Text + "中立 : " + rank.Value.ToString("F4") + "\n";
                                    if (color == Colors.Transparent)
                                    {
                                        color = Colors.Green;
                                    }
                                    break;
                                case "Sadness":
                                    resultTbox.Text = resultTbox.Text + "悲しみ : " + rank.Value.ToString("F4") + "\n";
                                    if (color == Colors.Transparent)
                                    {
                                        color = Colors.Blue;
                                    }
                                    break;
                                case "Surprise":
                                    resultTbox.Text = resultTbox.Text + "驚き : " + rank.Value.ToString("F4") + "\n";
                                    if (color == Colors.Transparent)
                                    {
                                        color = Colors.Yellow;
                                    }
                                    break;
                            }
                        }

                        var faceRect = emotion.FaceRectangle;

                        Windows.UI.Xaml.Shapes.Rectangle rect = new Windows.UI.Xaml.Shapes.Rectangle
                        {
                            Height = faceRect.Height,
                            Width = faceRect.Width,
                            Stroke = new SolidColorBrush(color),
                            StrokeThickness = 2
                        };

                        canvas.Children.Add(rect);
                        Canvas.SetLeft(rect, faceRect.Left);
                        Canvas.SetTop(rect, faceRect.Top);

                    }
                }
            });
        }
    }
}
