using CefSharp;
using CefSharp.DevTools.Page;
using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using wp11_movieFinder.Logics;
using wp11_movieFinder.Models;

namespace wp11_movieFinder
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        bool isFavorite = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnNaverMovie_Click(object sender, RoutedEventArgs e)
        {
            await Commons.ShowMessageAsync("네이버영화", "네이버 영화 사이트로 이동합니다 ");
        }

        // 검색버튼, 네이버 API 영화 검색
        private async void BtnSearchMovie_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtMovieName.Text))
            {
                await Commons.ShowMessageAsync("검색", "검색할 영화명을 입력하세요.");
                return;
            }

            //if(TxtMovieName.Text.Length < 2) // 검색어를 제대로 입력하지 않았을 때
            //{
            //    await Commons.ShowMessageAsync("검색", "검색어를 2자이상 입력하세요");
            //}

            try
            {
                SearchMovie(TxtMovieName.Text);
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"오류 발생{ex.Message}");
            }
        }

        // TxtMovieName에서 인터를 누르면 검색시작
        private void TxtMovieName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSearchMovie_Click(sender, e);
            }
        }

        private async void SearchMovie(string movieName)
        {
            string tmdb_apiKey = "TMDB KEY 입력";
            string encoding_movieName = HttpUtility.UrlEncode(movieName, Encoding.UTF8);
            string openApiUri = $@"https://api.themoviedb.org/3/search/movie?api_key={tmdb_apiKey}"
                              + $"&language=ko-KR&page=1&include_adult=false&query={encoding_movieName}"; //  검색 URL
            string result = string.Empty; // 결과값

            // API 실행할 객체
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            // TMDB API 요청
            try
            {
                req = WebRequest.Create(openApiUri); // URL을 넣어서 객체를 생성
                res = await req.GetResponseAsync(); // 요청한 결과를 비동기 응답에 할당
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd(); // json 결과 텍스트로 저장

                Debug.WriteLine(result);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
                res.Close();
            }

            // result를 json으로 
            var jsonResult = JObject.Parse(result); // string -> json

            var total = Convert.ToInt32(jsonResult["total_results"]); // 전체 검색 결과 수

            //await Commons.ShowMessageAsync("검색결과", total.ToString());
            var items = jsonResult["results"];
            // items를 데이터 그리드에 넣어 표시
            var json_array = items as JArray;

            var movieItems = new List<MovieItem>();
            foreach (var val in json_array)
            {
                var MovieItem = new MovieItem()
                {
                    Adult = Convert.ToBoolean(val["audlt"]),
                    Id = Convert.ToInt32(val["id"]),
                    Original_Language = Convert.ToString(val["original_language"]),
                    Original_Title = Convert.ToString(val["original_title"]),
                    Overview = Convert.ToString(val["overview"]),
                    Popularity = Convert.ToDouble(val["popularity"]),
                    Poster_Path = Convert.ToString(val["poster_path"]),
                    Release_Date = Convert.ToString(val["release_date"]),
                    Title = Convert.ToString(val["title"]),
                    Vote_Average = Convert.ToDouble(val["vote_average"]),
                };
                movieItems.Add(MovieItem);
            }
            this.DataContext = movieItems;
            isFavorite = false; // 얘는 즐겨찾기가 아니다!
            StsResult.Content = $"OpenAPI {movieItems.Count} 건 조회 완료";
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TxtMovieName.Focus(); // 텍스트 박스에 포커스를 줌
        }

        private async void GrdResult_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                string posterPath = string.Empty;

                if(GrdResult.SelectedItem is MovieItem) // openAPI로 검색된 영화의 포스트
                {
                    var movie = GrdResult.SelectedItem as MovieItem;
                    posterPath = movie.Poster_Path;
                }
                else if(GrdResult.SelectedItem is FavoriteMovieItem) // 즐겨찾기 DB에서 가져온 영화 포스터
                {
                    var movie = GrdResult.SelectedItem as FavoriteMovieItem;
                    posterPath = movie.Poster_Path;
                }

                //Debug.WriteLine(movie.Poster_Path); // 포스터 패스 경로
                if (string.IsNullOrEmpty(posterPath)) // 포스터 이미지가 없으면 No_Picture
                {
                    ImgPoster.Source = new BitmapImage(new Uri("No_Picture.png", UriKind.RelativeOrAbsolute));
                }
                else // 아니면 포스터 패스 경로를 뿌림
                {
                    var imageUri = $"https://image.tmdb.org/t/p/w300_and_h450_bestv2/";
                    imageUri += posterPath;

                    ImgPoster.Source = new BitmapImage(new Uri(imageUri, UriKind.RelativeOrAbsolute));
                }
            }
            catch
            {
                await Commons.ShowMessageAsync("오류", $"이미지로드 오류 발생");
            }
        }

        // 영화 예고편 유투브 보기
        private async void BtnWatchTrailer_Click(object sender, RoutedEventArgs e)
        {
            if (GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("유투브", "영화를 선택하세요");
                return;
            }

            if (GrdResult.SelectedItems.Count > 1)
            {
                await Commons.ShowMessageAsync("유투브", "영화를 하나만 선택하세요");
                return;
            }

            string movieName = string.Empty;
            if(GrdResult.SelectedItem is MovieItem)
            {
                var movie = GrdResult.SelectedItem as MovieItem;
                movieName = movie.Title;
            }
            else if(GrdResult.SelectedItem is FavoriteMovieItem)
            {
                var movie = GrdResult.SelectedItem as FavoriteMovieItem;
                movieName = movie.Title;
            }

            

            //await Commons.ShowMessageAsync("유튜브", $"예고편 볼 영화{movieName}");
            var trailerWindow = new TrailerWindow(movieName);
            trailerWindow.Owner = this; // TrailerWindow의 부모는 MainWindow
            trailerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;// 부모창의 정중앙에 위치
            //trailerWindow.Show();
            trailerWindow.ShowDialog();
        }

        // 검색 결과 중에서 자신이 좋아하는 영화 저장
        private async void BtnAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "즐겨찾기에 추가할 영화를 선택하세요 (복수 선택 가능)");
                return;
            }

            
            if (isFavorite)
            {
                await Commons.ShowMessageAsync("오류", "이미 즐겨찾기한 영화입니다.");
                return;
            }

            // 리스트 만드는 거 필요 없어!
            //List<FavoriteMovieItem> list = new List<FavoriteMovieItem>();
            //foreach (MovieItem item in GrdResult.SelectedItems)
            //{
            //    var favoriteMovie = new FavoriteMovieItem
            //    {
            //        Id = item.Id,
            //        Title = item.Title,
            //        Original_Title = item.Original_Title,
            //        Original_Language = item.Original_Language,
            //        Adult = item.Adult,
            //        Overview = item.Overview,
            //        Release_Date = item.Release_Date,
            //        Vote_Average = item.Vote_Average,
            //        Popularity = item.Popularity,
            //        Poster_Path = item.Poster_Path,
            //        Reg_Date = DateTime.Now
            //    };

            //    list.Add(favoriteMovie);
            //}

            //await Commons.ShowMessageAsync("저장할 데이터 수", list.Count.ToString());

            try
            {
                // DB 데이터 입력
                using (SqlConnection conn = new SqlConnection(Commons.connString))
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    var query = @"INSERT INTO [dbo].[FavoriteMovieItem]
                                             ( [Id]
                                             , [Title]
                                             , [Original_Title]
                                             , [Release_Date]
                                             , [Original_Language]
                                             , [Adult]
                                             , [Popularity]
                                             , [Vote_Average]
                                             , [Poster_Path]
                                             , [Overview]
                                             , [Reg_Date])
                                       VALUES
                                             ( @Id
                                             , @Title
                                             , @Original_Title
                                             , @Release_Date
                                             , @Original_Language
                                             , @Adult
                                             , @Popularity
                                             , @Vote_Average
                                             , @Poster_Path
                                             , @Overview
                                             , @Reg_Date )";



                    var insRes = 0;
                    foreach (MovieItem item in GrdResult.SelectedItems)
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);

                        cmd.Parameters.AddWithValue("Id", item.Id);
                        cmd.Parameters.AddWithValue("Title", item.Title);
                        cmd.Parameters.AddWithValue("Original_Title", item.Original_Title);
                        cmd.Parameters.AddWithValue("Release_Date", item.Release_Date);
                        cmd.Parameters.AddWithValue("Original_Language", item.Original_Language);
                        cmd.Parameters.AddWithValue("Adult", item.Adult);
                        cmd.Parameters.AddWithValue("Popularity", item.Popularity);
                        cmd.Parameters.AddWithValue("Vote_Average", item.Vote_Average);
                        cmd.Parameters.AddWithValue("Poster_Path", item.Poster_Path);
                        cmd.Parameters.AddWithValue("Overview", item.Overview);
                        cmd.Parameters.AddWithValue("Reg_Date", DateTime.Now);

                        insRes += cmd.ExecuteNonQuery();
                    }
                    if (GrdResult.SelectedItems.Count == insRes)
                    {
                        await Commons.ShowMessageAsync("저장", "DB저장 성공");
                        StsResult.Content = $"즐겨찾기 {insRes} 건 저장완료";
                    }
                    else
                    {
                        await Commons.ShowMessageAsync("저장", "DB저장 오류 관리자에게 문의하세요");
                    }
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB저장 오류{ex.Message}");
            }
        }

        private async void BtnViewFavorite_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            TxtMovieName.Text = string.Empty;

            List<FavoriteMovieItem> list = new List<FavoriteMovieItem>();
            try
            {
                using (SqlConnection conn = new SqlConnection(Commons.connString))
                {
                    if(conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    var query = @"SELECT Id
                                     , Title
                                     , Original_Title
                                     , Release_Date
                                     , Original_Language
                                     , Adult
                                     , Popularity
                                     , Vote_Average
                                     , Poster_Path
                                     , Overview
                                     , Reg_Date
                                  FROM FavoriteMovieItem
                                 ORDER BY Id ASC";
                    var cmd = new SqlCommand(query, conn);
                    var adapter = new SqlDataAdapter(cmd);
                    var dataSet = new DataSet();
                    adapter.Fill(dataSet,"FavoriteMovieItem");

                    foreach(DataRow dr in dataSet.Tables["FavoriteMovieItem"].Rows)
                    {
                        list.Add(new FavoriteMovieItem
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Title = Convert.ToString(dr["Title"]),
                            Original_Title = Convert.ToString(dr["Original_Title"]),
                            Release_Date = Convert.ToString(dr["Release_Date"]),
                            Original_Language = Convert.ToString(dr["Original_Language"]),
                            Adult = Convert.ToBoolean(dr["Adult"]),
                            Popularity = Convert.ToDouble(dr["Popularity"]),
                            Vote_Average = Convert.ToDouble(dr["Vote_Average"]),
                            Poster_Path = Convert.ToString(dr["Poster_Path"]),
                            Overview = Convert.ToString(dr["Overview"]),
                            Reg_Date = Convert.ToDateTime(dr["Reg_Date"])
                        });
                    }
                    this.DataContext = list;
                    isFavorite = true;
                    StsResult.Content = $"즐겨찾기{list.Count} 건 조회완료";

                }
            }
            catch(Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB조회 오류{ex.Message}");
            }
        }

        private async void BtnDelFavorite_Click(object sender, RoutedEventArgs e)
        {
            if(isFavorite == false)
            {
                await Commons.ShowMessageAsync("오류", "즐겨찾기만 삭제할 수 있습니다.");
                return;
            }

            if(GrdResult.SelectedItems.Count ==0)
            {
                await Commons.ShowMessageAsync("오류", "삭제할 영화를 선택하세요.");
                return;
            }

            try // 삭제
            {
                using (SqlConnection conn = new SqlConnection(Commons.connString))
                {
                    if(conn.State == ConnectionState.Closed) 
                    { 
                        conn.Open(); 
                    }

                    var query = "DELETE FROM  FavoriteMovieItem WHERE Id = @Id";
                    var delRes = 0;

                    foreach(FavoriteMovieItem item in GrdResult.SelectedItems)
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", item.Id);

                        delRes += cmd.ExecuteNonQuery();
                    }

                    if (delRes == GrdResult.SelectedItems.Count)
                    {
                        await Commons.ShowMessageAsync("삭제", "DB삭제 성공!!");
                        StsResult.Content = $"즐겨찾기 {delRes} 건 삭제완료"; // 화면에 나오지 않음
                    }
                    else
                    {
                        await Commons.ShowMessageAsync("삭제","DB삭제 일부 성공!!"); // 발생할일이 거의 전무
                    }
                }
            }
            catch(Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB삭제 오류{ex.Message}");
            }

            BtnViewFavorite_Click(sender, e); // 즐겨찾기 보기 이벤트 핸들러를 한번 실행

        }
    }
}

#region < MySQL 테스트 >
//try
//            {
//                // DB 데이터 입력
//                // using (SqlConnection conn = new SqlConnection(Commons.connString))
//                //MySQL DB데이터입력(테스트용)
//                using (MySqlConnection conn = new MySqlConnection(Commons.myconnString))
//                {
//                    if (conn.State == ConnectionState.Closed)
//                        conn.Open();

//                    var query = @"INSERT INTO FavoriteMovieItem
//                                             ( Id
//                                             , Title
//                                             , Original_Title
//                                             , Release_Date
//                                             , Original_Language
//                                             , Adult
//                                             , Popularity
//                                             , Vote_Average
//                                             , Poster_Path
//                                             , Overview
//                                             , Reg_Date)
//                                       VALUES
//                                             ( @Id
//                                             , @Title
//                                             , @Original_Title
//                                             , @Release_Date
//                                             , @Original_Language
//                                             , @Adult
//                                             , @Popularity
//                                             , @Vote_Average
//                                             , @Poster_Path
//                                             , @Overview
//                                             , @Reg_Date )";
                                   
                    

//                    var insRes = 0;
//                    foreach(FavoriteMovieItem item in list)
//                    {
//                        MySqlCommand cmd = new MySqlCommand(query, conn);

//                        cmd.Parameters.AddWithValue("Id", item.Id);
//                        cmd.Parameters.AddWithValue("Title", item.Title);
//                        cmd.Parameters.AddWithValue("Original_Title", item.Original_Title);
//                        cmd.Parameters.AddWithValue("Release_Date", item.Release_Date);
//                        cmd.Parameters.AddWithValue("Original_Language", item.Original_Language);
//                        cmd.Parameters.AddWithValue("Adult", item.Adult);
//                        cmd.Parameters.AddWithValue("Popularity", item.Popularity);
//                        cmd.Parameters.AddWithValue("Vote_Average", item.Vote_Average);
//                        cmd.Parameters.AddWithValue("Poster_Path", item.Poster_Path);
//                        cmd.Parameters.AddWithValue("Overview", item.Overview);
//                        cmd.Parameters.AddWithValue("Reg_Date", item.Reg_Date);

//                        insRes += cmd.ExecuteNonQuery();
//                    }
//                    if (list.Count == insRes)
//                    {
//                        await Commons.ShowMessageAsync("저장", "DB저장 성공");
//                    }
//                    else
//                    {
//                        await Commons.ShowMessageAsync("저장", "DB저장 오류 관리자에게 문의하세요");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                await Commons.ShowMessageAsync("오류", $"DB저장 오류{ex.Message}");
//            }
//        }
//    }
//}

#endregion

