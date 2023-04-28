using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using wp12_finedustCheck.Models;

namespace wp12_finedustCheck
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // DB(MySql)에서 조회 리스트 뿌리기
        private void CboReqDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboReqDate.SelectedValue != null)
            {
                //MessageBox.Show(CboReqDate.SelectedValue.ToString());
                using (MySqlConnection conn = new MySqlConnection(Commons.myconnString))
                {
                    conn.Open();

                    var query = @"SELECT Id
                                       , Dev_id
                                       , Name
                                       , Loc
                                       , Coordx
                                       , Coordy
                                       , Ison
                                       , Pm10_after
                                       , Pm25_after
                                       , State
                                       , Timestamp
                                       , Company_id
                                       , Company_name
                                       , DustSensorcol
                                    FROM dustsensor
                                  WHERE date_format(Timestamp,'%Y-%m-%d') = @Timestamp";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("Timestamp", CboReqDate.SelectedValue.ToString());
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, "dustsensor");
                    List<DustSensor> dustSensors = new List<DustSensor>();

                    foreach (DataRow row in ds.Tables["dustsensor"].Rows)
                    {
                        dustSensors.Add(new DustSensor
                        {
                            Id = Convert.ToInt32(row["Id"]),
                            Dev_id = Convert.ToString(row["Dev_id"]),
                            Name = Convert.ToString(row["Name"]),
                            Loc = Convert.ToString(row["Loc"]),
                            Coordx = Convert.ToDouble(row["Coordx"]),
                            Coordy = Convert.ToDouble(row["Coordy"]),
                            Ison = Convert.ToBoolean(row["Ison"]),
                            Pm10_after = Convert.ToInt32(row["Pm10_after"]),
                            Pm25_after = Convert.ToInt32(row["Pm25_after"]),
                            State = Convert.ToInt32(row["State"]),
                            Timestamp = Convert.ToDateTime(row["Timestamp"]),
                            Company_id = Convert.ToString(row["Company_id"]),
                            Company_name = Convert.ToString(row["Company_name"])

                        });
                    }
                    this.DataContext = dustSensors;
                    StsResult.Content = $"DB {dustSensors.Count}건 조회완료";
                }
            }
            else
            {
                this.DataContext = null;
                StsResult.Content = $"DB조회 클리어";
            }
        }

        // 검색한 결과 DB(MySql)에 저장
        private async void BtnSaveData_Click(object sender, RoutedEventArgs e)
        {
            if(GrdResult.Items.Count==0)
            {
                await Commons.ShowMessageAsync("오류", "조회후 저장하세요.");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myconnString))
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    var query = @"INSERT INTO dustsensor 
                                        ( Dev_id ,
                                         Name ,
                                         Loc ,
                                         Coordx ,
                                         Coordy ,
                                         Ison ,
                                         Pm10_after ,
                                         Pm25_after ,
                                         State ,
                                         Timestamp ,
                                         Company_id ,
                                         Company_name)
                                        VALUES
                                        ( @Dev_id ,
                                         @Name ,
                                         @Loc ,
                                         @Coordx ,
                                         @Coordy ,
                                         @Ison ,
                                         @Pm10_after ,
                                         @Pm25_after ,
                                         @State ,
                                         @Timestamp ,
                                         @Company_id ,
                                         @Company_name)";

                    var insRes = 0;
                    foreach(var temp in GrdResult.Items)
                    {
                        if(temp is DustSensor)
                        {
                            var item = temp as DustSensor;

                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@Dev_id", item.Dev_id);
                            cmd.Parameters.AddWithValue("@Name", item.Name);
                            cmd.Parameters.AddWithValue("@Loc", item.Loc);
                            cmd.Parameters.AddWithValue("@Coordx", item.Coordx);
                            cmd.Parameters.AddWithValue("@Coordy", item.Coordy);
                            cmd.Parameters.AddWithValue("@Ison", item.Ison);
                            cmd.Parameters.AddWithValue("@Pm10_after", item.Pm10_after);
                            cmd.Parameters.AddWithValue("@Pm25_after", item.Pm25_after);
                            cmd.Parameters.AddWithValue("@State", item.State);
                            cmd.Parameters.AddWithValue("@Timestamp", item.Timestamp);
                            cmd.Parameters.AddWithValue("@Company_id", item.Company_id);
                            cmd.Parameters.AddWithValue("@Company_name", item.Company_name);

                            insRes += cmd.ExecuteNonQuery();
                        }
                    }
                    await Commons.ShowMessageAsync("저장","저장 성공!");
                    StsResult.Content = $"DB저장{insRes}건 성공";                   
                }
            }
            catch(Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB저장 오류! {ex.Message}");
            }

        }

        // 김해시 openAPI조회
        private async void BtnReqRealTime_Click(object sender, RoutedEventArgs e)
        {
            string openapiUri = "https://smartcity.gimhae.go.kr/smart_tour/dashboard/api/publicData/dustSensor";
            string result = string.Empty;

            // 사용할 WebRequest, WebResponse 객체
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            try
            {
                req = WebRequest.Create(openapiUri);
                res = await req.GetResponseAsync();
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();

                Debug.WriteLine(result);
            }
            catch(Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"OpenAPI 조회 오류{ex.Message}");
            }

            var jsonResult = JObject.Parse(result);
            var status = Convert.ToInt32(jsonResult["status"]);

            try // 발생할때마다 속도가 느려지긴 함
            {
                if (status == 200) // 정상이면 데이터 받아서 처리
                {
                    var data = jsonResult["data"];
                    var json_array = data as JArray;

                    var dustSensors = new List<DustSensor>();
                    foreach(var sensor in json_array)
                    {
                        dustSensors.Add(new DustSensor
                        {
                            Id = 0,
                            Dev_id = Convert.ToString(sensor["dev_id"]),
                            Name = Convert.ToString(sensor["name"]),
                            Loc = Convert.ToString(sensor["loc"]),
                            Coordx = Convert.ToDouble(sensor["coordx"]),
                            Coordy = Convert.ToDouble(sensor["coordy"]),
                            Ison = Convert.ToBoolean(sensor["ison"]),
                            Pm10_after = Convert.ToInt32(sensor["pm10_after"]),
                            Pm25_after = Convert.ToInt32(sensor["pm25_after"]),
                            State = Convert.ToInt32(sensor["state"]),
                            Timestamp = Convert.ToDateTime(sensor["timestamp"]),
                            Company_id = Convert.ToString(sensor["company_id"]),
                            Company_name = Convert.ToString(sensor["company_name"])
                        }); ;
                    }

                    this.DataContext = dustSensors;
                    StsResult.Content = $"OpenAPI {dustSensors.Count}건 조회 완료";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"Json 처리오류{ex.Message}");
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 콤보박스에 들어갈 날짜를 DB에서 불러오기
            // 저장한 뒤에도 콤보박스를 재조회해야 날짜 전부 출력
            using(MySqlConnection conn = new MySqlConnection(Commons.myconnString))
            {
                conn.Open();
                var query = @"SELECT DATE_FORMAT(timestamp,'%Y-%m-%d')AS Save_Date
                              FROM dustsensor
                              GROUP BY 1
                              ORDER BY 1;";
                                        

                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                List<string> saveDateList = new List<string>();
                foreach(DataRow row in ds.Tables[0].Rows)
                {
                    saveDateList.Add(Convert.ToString(row["Save_Date"]));
                }
                CboReqDate.ItemsSource = saveDateList;
            }
        }

        private void GrdResult_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selItem = GrdResult.SelectedItem as DustSensor;

            var mapWindow = new MapWindow(selItem.Coordy, selItem.Coordx);
            mapWindow.Owner = this; // MainWindow가 부모
            mapWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner; // 부모창 중간에 출력
            mapWindow.ShowDialog();
        }
    }
}
