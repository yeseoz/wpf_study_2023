using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wp08_personalInfoApp.ViewModels
{
    internal class MainViewModel:ViewModelBase
    {
        // 뷰모델을 만드는 작업
        // 뷰에서 사용할 멤버변수 선언 
        // 입력쪽 변수
        private string inFirstName;
        private string inLastName;
        private string inEmail;
        private DateTime inDate;

        // 결과 출력쪽 변수
        private string outFirstName;
        private string outLastName;
        private string outEmail;
        private string outDate; // 생일 날짜 출력할때는 문자열 대체
        private string outAult;
        private string outBirthday;
        private string outZodiac;

        // 일이 많아짐. 실제로 사용할 속성
        // 입력을 위한 속성들
        public string InFirstName 
        {
            get => inFirstName;
            set
            {
                inFirstName = value;
                RaisePropertyChanged(nameof(InFirstName)); // "InFirstName바꼈음!"
            }
        }
        
        public string InLastName 
        {
            get => inLastName;
            set
            {
                inLastName = value; 
                RaisePropertyChanged(nameof(InLastName));
            }
        }

        public string InEmail 
        {
            get => inEmail;
            set
            {
                inEmail = value;
                RaisePropertyChanged(nameof(InEmail));
            }
        }
        public DateTime InDate 
        {
            get => inDate;
            set
            {
                inDate = value;
                RaisePropertyChanged(nameof(InDate));
            }
        }

        // 출력을 위한 속성들
        public string OutFirstName 
        { 
            get => outFirstName;
            set
            {
                OutFirstName = value;
                RaisePropertyChanged(nameof(OutFirstName));
            }
        }
        
        public string OutLastName 
        { 
            get => outLastName;
            set
            {
                OutLastName = value;
                RaisePropertyChanged(nameof(OutLastName));
            }
        }

        public string OutEmail 
        {
            get => outEmail; 
            set
            {
                OutEmail = value;
                RaisePropertyChanged(nameof(OutEmail));
            }
        }

        public string OutDate
        {
            get => outDate;
            set
            {
                OutDate = value;
                RaisePropertyChanged(nameof(OutDate));
            }
        }
        
        public string OutAult 
        {
            get => outAult;
            set
            {
                OutAult = value;
                RaisePropertyChanged(nameof(OutAult));
            }
        }
        
        public string OutBirthday 
        {
            get => outBirthday;
            set
            { 
                OutBirthday = value;
                RaisePropertyChanged(nameof(OutBirthday));
            }
        }
        
        public string OutZodiac 
        {
            get => outZodiac;
            set
            {
                OutZodiac = value;
                RaisePropertyChanged(nameof(OutZodiac));
            }
        }
    }
}
