using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalScheduler.Model
{
    internal class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Browsable(false)]
        public int DepartmentID;
        public string DepartmentName { get; set; }
       
        public Doctor(int id, string name, int departmentID, string departmentName)
        {
            Id = id;
            Name = name;
            DepartmentID = departmentID;
            DepartmentName = departmentName;
        }
    }
}
