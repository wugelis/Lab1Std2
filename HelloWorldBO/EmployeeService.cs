using EmployeeViewObjects;
using System;
using System.Linq;
using System.Collections.Generic;

namespace HelloWorldBO
{
    public class EmployeeService
    {
        public string GetHelloWorld()
        {
            return "Hello World for Web API Framework 第三梯！";
        }

        public IEnumerable<EmployeeVO> GetEmployees(MyHelloWorldVO input)
        {
            return new EmployeeVO[] {
                new EmployeeVO() { EmpId = 1, EmpChtName = $"Gelis_{input.Test}", Title = "工程師"},
                new EmployeeVO() { EmpId = 2, EmpChtName = "Allan", Title = "工程師"}
            };
        }
    }
}
