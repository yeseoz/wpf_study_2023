using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using wp11_movieFinder.Models;

namespace wp11_movieFinder
{
    /// <summary>
    /// TrailerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TrailerWindow : MetroWindow
    {
        List<Youtubeitem> youtubeItems = null;
        public TrailerWindow()
        {
            InitializeComponent();
        }
        
        // 부모에서 데이터를 가져오려면
        public TrailerWindow(string movieName):this()
        {
            LblMovieName.Content = $"{movieName} 예고편";
        }

        // 화면 로드 완료후에 YoutubeAPI 실행
        private void MetroWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            youtubeItems = new List<Youtubeitem>(); //초기화
            SearchYoutubeApi();
        }

        private async void SearchYoutubeApi()
        {
            await LoadDataCollection();
            LsvResult.ItemsSource = youtubeItems; // direct binding
        }

        private async Task LoadDataCollection()
        {
            var youtubeService = new YouTubeService(
                new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyDftxHJJUTrR8L8ngwApSlWvy8KRm5lWLo",
                    ApplicationName = this.GetType().ToString()
                });

            var req = youtubeService.Search.List("snippet");
            req.Q = LblMovieName.Content.ToString();
            req.MaxResults = 10;

            var res = await req.ExecuteAsync();// 검색결과를 받아옴

            Debug.WriteLine("=====유튜브 검색결과====");
            // Debug.WriteLine(res.ToString());
            foreach(var item in res.Items) 
            {
                Debug.WriteLine(item.Snippet.Title);
                if (item.Id.Kind.Equals("youtube#video"))
                {
                    Youtubeitem youtube = new Youtubeitem()
                    {
                        Title = item.Snippet.Title,
                        ChannelTitle = item.Snippet.ChannelTitle,
                        URL = $"https://www.youtube.com/watch?v={item.Id.VideoId}",
                       // Author = item.Snippet.ChannelTitle
                    };

                    youtube.Thumbnail = new BitmapImage(new Uri(item.Snippet.Thumbnails.Default__.Url,
                                                        UriKind.RelativeOrAbsolute));
                    youtubeItems.Add(youtube);
                }
            }
        }

        private void LsvResult_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LsvResult.SelectedItem is Youtubeitem)
            {
                var video = LsvResult.SelectedItem as Youtubeitem;
                BrsYoutube.Address = video.URL;
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BrsYoutube.Address = string.Empty;//웹브라우저 주소 클리어
            BrsYoutube.Dispose(); // 리소스 해제!=> 메모리 릭 발생가능
        }
    }
}
