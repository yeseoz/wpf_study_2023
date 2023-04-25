using System;
using wp08_personalInfoApp.Logics;

namespace wp08_personalInfoApp.Models
{
    internal class Person
    {
        // 외부에서 접근 불가
        //private string firstName;
        //private string lastName;
        private string email;
        private DateTime date;

        public string FirstName { get; set; } // AutoProperty =>> 이름에는 제약사항이 없어서 사용가능
        public string LastName { get; set; }
        public string Email
        {
            get => email;
            set
            {
                if (Commons.IsValidEmail(value)!=true) // 이메일은 형식에 일치하지 않음
                {
                    throw new Exception("유효하지 않은 이메일 형식");
                }
                else
                {
                    email = value;
                }
            }
        }
        public DateTime Date 
        { 
            get => date;
            set 
            {
                var result = Commons.GetAge(value);
                if(result > 120 || result<=0)
                {
                    throw new Exception("유효하지 않은 생일!");
                }
                else
                {
                    date = value;
                }
            }
        }

        public bool IsAult
        {
            get=> Commons.GetAge(date) > 18; //19살 이상이면 true
        }

        public bool IsBirthDay
        {
            get
            {
                return DateTime.Now.Month== date.Month &&
                    DateTime.Now.Day== date.Day; // 오늘하고 월일이 같으면 생일!
            }
        }

        public string Zodiac
        {
            get => Commons.GetZodiac(date); // 띠를 받아옴
        }

        public Person(string firstName, string lastName, string email, DateTime date)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Date = date;
        }
    }
}
