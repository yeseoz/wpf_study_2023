﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace wp05_bikeshop.Logics
{
    internal class MyConverter : IValueConverter
    {
        // 대상에다가 표현할 때 값을 변환, 표현(OneWay)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return int.Parse(value.ToString()) * 2; // 두배(숫자)
            return value.ToString() + "km/h"; // 값 뒤에 km/h 값 넣어줌
        }
        
        // 대상값이 바뀌어서 원본(소스)의 값을 변환, 표현(TwoWay)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.Parse(value.ToString()) * 3;
        }
    }
}