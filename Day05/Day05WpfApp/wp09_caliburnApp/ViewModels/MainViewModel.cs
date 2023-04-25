using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wp09_caliburnApp.Models;

namespace wp09_caliburnApp.ViewModels
{
    public class MainViewModel : Screen
    {
        private string firstName = "예서";
        public string FirstName
        {
            get => firstName;
            set
            {
                firstName = value;
                NotifyOfPropertyChange(() => FirstName);// 속성값이 변경된걸 시스템이 알려주는 역할
                NotifyOfPropertyChange(nameof(CanClearName));
                NotifyOfPropertyChange(() => FullName); //변화 통보     /*-77654321`1\][oiuytrewq   q[]\S'
                 
            }
        }

        private string lastName = "김";
        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                NotifyOfPropertyChange(() => LastName);
                NotifyOfPropertyChange(nameof(CanClearName));
                NotifyOfPropertyChange(() => FullName); //변화 통보
            }
        }

        public string FullName
        {
            get => $"{LastName} {FirstName}";
        }

        // 콤보박스에 바인딩할 속성
        //이런곳에서는 var을 쓸 수 없음
        private BindableCollection<Person> managers = new BindableCollection<Person>();

        public BindableCollection<Person> Managers
        {
            get => managers;
            set => managers = value;
        }

        // 콤보박스에 선택된 값을 지정할 속성
        private Person selectedManager;
        public Person SelectedManager
        {
            get => selectedManager;
            set
            {
                selectedManager = value;
                LastName = selectedManager.LastName;
                FirstName = selectedManager.FirstName;
                NotifyOfPropertyChange(nameof(SelectedManager));
            }
        }

        public MainViewModel()
        {
            // DB를 사용하면 여기서 DB접속 > 데이터 Select까지...
            Managers.Add(new Person { FirstName = "Jhon", LastName = "Carmmack" });
            Managers.Add(new Person { FirstName = "Steve", LastName = "Jobs" });
            Managers.Add(new Person { FirstName = "Jhon", LastName = "Carmmack" });
            Managers.Add(new Person { FirstName = "Elon", LastName = "Musk" });
        }

        public void ClearName()
        {
            FirstName = String.Empty;
            LastName = String.Empty;
        }

        //메서드와 이름을 동일하게하고 앞에 Can을 붙임
        public bool CanClearName
        {
            get => !string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName);
        }
    }
}
