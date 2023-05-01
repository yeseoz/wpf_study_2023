using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CefSharp.DevTools.Network;
using ControlzEx.Standard;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using project_dic.Logics;
using project_dic.Models;

namespace project_dic
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        bool isSaved = false;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxbSearch.Text))
            {
                await Commons.ShowMessageAsync("검색", "아무것도 입력되지 않았습니다.");
                return;
            }

            try
            {
                SearchWord(TxbSearch.Text);
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"오류발생\n{ex.Message}");
            }
        }

        private async void SearchWord(string searchWord)
        {
            string query = searchWord; // 검색할 문자열
            string result = string.Empty;

            string apiId = "네이버 api 아이디";
            string apiKey = "네이버 api 비밀번호";
            string url = "https://openapi.naver.com/v1/search/encyc.json?query=" + query; // JSON 결과
            url += "&display=100";

            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            try
            {
                req = WebRequest.Create(url);
                req.Headers.Add("X-Naver-Client-Id", apiId); // 네이버 api id
                req.Headers.Add("X-Naver-Client-Secret", apiKey); // 네이버 api pass
                res = req.GetResponse();
                reader = new StreamReader(res.GetResponseStream());

                result = reader.ReadToEnd();

                Debug.WriteLine(result);
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"오류발생!\n{ex.Message}");
            }
            finally
            {
                reader.Close();
                res.Close();
            }

            //result를 json으로 변환
            var jsonResult = JObject.Parse(result); // string->json
            int total = Convert.ToInt32(jsonResult["total"]);

            try
            {
                var items = jsonResult["items"];
                var json_array = items as JArray;
            
                var searchItems = new List<SearchItem>();

                foreach (var val in json_array)
                {
                    if (Convert.ToString(val["description"]) != string.Empty)
                    {
                        searchItems.Add(new SearchItem
                        {
                            Title = Convert.ToString(val["title"]).Replace("<b>", "").Replace("</b>", ""),
                            Link = Convert.ToString(val["link"]),
                            Description = Convert.ToString(val["description"]).Replace("<b>","").Replace("</b>",""),
                            Thumbnail = Convert.ToString(val["thumbnail"])
                        });
                    }
                }
                                
                this.DataContext = searchItems;
                isSaved = false;
                StsSearch.Content = $"{searchItems.Count}건 조회";
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"json처리 오류\n{ex.Message}");
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TxbSearch.Focus();
        }

        private async void GrdSearch_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            try
            {
                string imgPath = string.Empty;

                if (GrdSearch.SelectedItem is SearchItem)
                {
                    var search = GrdSearch.SelectedItem as SearchItem;
                    imgPath = search.Thumbnail;
                    // Debug.WriteLine(search.Thumbnail);
                }
                else if(GrdSearch.SelectedItem is NaverSearch)
                {
                    var search = GrdSearch.SelectedItem as NaverSearch;
                    imgPath = search.Thumbnaul;
                }

                if (string.IsNullOrEmpty(imgPath))
                {
                    ImgSearch.Source = new BitmapImage(new Uri("noimg.jpg", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    var imgUri = imgPath;
                    ImgSearch.Source = new BitmapImage(new Uri(imgUri, UriKind.RelativeOrAbsolute));
                }
            }
            catch
            {
                await Commons.ShowMessageAsync("오류", $"이미지로드 오류 발생");
            }

        }

        private async void GrdSearch_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (GrdSearch.SelectedItem == null)
            {
                await Commons.ShowMessageAsync("오류", "선택된 값이 없습니다");
            }
            else if(GrdSearch.SelectedItem is SearchItem)
            {
                string searchuri = string.Empty;

                if (GrdSearch.SelectedItem is SearchItem)
                {
                    var search = GrdSearch.SelectedItem as SearchItem;
                    searchuri = search.Link;

                    Debug.WriteLine(searchuri);
                }

                var searchWindow = new SearchNaver(searchuri);
                searchWindow.Owner = this;
                searchWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                searchWindow.ShowDialog();
            }
            else if(GrdSearch.SelectedItem is NaverSearch)
            {
                string searchuri = string.Empty;

                if (GrdSearch.SelectedItem is NaverSearch)
                {
                    var search = GrdSearch.SelectedItem as NaverSearch;
                    searchuri = search.Link;

                    Debug.WriteLine(searchuri);
                }

                var searchWindow = new SearchNaver(searchuri);
                searchWindow.Owner = this;
                searchWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                searchWindow.ShowDialog();
            }
        }

        private void TxbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSearch_Click(sender, e);
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (GrdSearch.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "저장할 내용을 선택하세요(복수 선택 가능)");
                return;
            }

            if (isSaved)
            {
                await Commons.ShowMessageAsync("오류", "이미 저장되어 있습니다.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(Commons.connString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    var query = @"INSERT INTO [dbo].[NaverSearch]
                                            ( [Title]
                                            , [Link]
                                            , [Description]
                                            , [Thumbnail] )
                                       VALUES
                                            ( @Title
                                            , @Link
                                            , @Description
                                            , @Thumbnail )";

                    var insRes = 0;
                    foreach (SearchItem item in GrdSearch.SelectedItems)
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);

                        cmd.Parameters.AddWithValue("@Title", item.Title);
                        cmd.Parameters.AddWithValue("@Link", item.Link);
                        cmd.Parameters.AddWithValue("@Description", item.Description);
                        cmd.Parameters.AddWithValue("@Thumbnail", item.Thumbnail);

                        insRes += cmd.ExecuteNonQuery();
                    }
                    await Commons.ShowMessageAsync("저장", "저장성공!");
                    StsSearch.Content = $"DB저장{insRes}건 성공!";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB저장 오류! {ex.Message}");
            }
        }

        private async void BtnView_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            TxbSearch.Text = string.Empty;

            List<NaverSearch> list = new List<NaverSearch>();
            try
            {
                using (SqlConnection conn = new SqlConnection(Commons.connString))
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    var query = @"SELECT Id
                                       , Title
                                       , Link
                                       , Description
                                       , Thumbnail
                                    FROM NaverSearch";
                    var cmd = new SqlCommand(query, conn);
                    var adapter = new SqlDataAdapter(cmd);
                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, "NaverSearch");

                    foreach (DataRow dr in dataSet.Tables["NaverSearch"].Rows)
                    {
                        list.Add(new NaverSearch
                        {
                            Id = Convert.ToInt32(dr["id"]),
                            Title = Convert.ToString(dr["title"]),
                            Link = Convert.ToString(dr["link"]),
                            Description = Convert.ToString(dr["description"]),
                            Thumbnaul = Convert.ToString(dr["thumbnail"])
                        });
                    }
                    this.DataContext = list;
                    isSaved = true;
                    StsSearch.Content = $"{list.Count} 건 조회완료";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB조회 오류{ex.Message}");
            }
        }

        private async void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            if(!isSaved)
            {
                await Commons.ShowMessageAsync("오류", "저장된 데이터만 삭제할 수 있습니다.");
                return;
            }

            if(GrdSearch.SelectedItems.Count==0)
            {
                await Commons.ShowMessageAsync("오류", "삭제할 데이터를 선택하세요");
                return;
            }

            try
            {
                using(SqlConnection conn = new SqlConnection(Commons.connString))
                {
                    if(conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    var query = "DELETE FROM NaverSearch WHERE Id = @Id";
                    var delRes = 0;

                    foreach(NaverSearch item in GrdSearch.SelectedItems)
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", item.Id);

                        delRes += cmd.ExecuteNonQuery();
                    }

                    if(delRes == GrdSearch.SelectedItems.Count)
                    {
                        await Commons.ShowMessageAsync("삭제", "삭제성공!");
                        StsSearch.Content = $"{delRes}건 삭제완료";
                    }
                    else
                    {
                        await Commons.ShowMessageAsync("삭제", "DB삭제 일부성공!");
                    }
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB 삭제 오류{ex.Message}");
            }

            BtnView_Click(sender, e);
        }
    }
}