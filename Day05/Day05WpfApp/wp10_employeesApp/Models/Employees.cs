﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wp10_employeesApp.Models
{
    public class Employees
    {
        private int salary;
        public int Idx { get; set; }
        public string FullName { get; set; }
        public int Salary 
        {
            get=>salary;
            set
            {
                if(value<=0 ||  value>=50000000)
                {
                    throw new Exception("급여를 잘못 입력했습니다.");
                }
                else
                {
                    salary = value;
                }
            }
        }
        public string DeptName { get; set; }
        public string Address { get; set; }
    }
}
