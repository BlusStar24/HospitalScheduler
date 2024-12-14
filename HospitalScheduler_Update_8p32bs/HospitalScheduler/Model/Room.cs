using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalScheduler.Model
{
    internal class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Browsable(false)]
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }

        public Room(int id, string name, int departmentID, string deparmentName)
        {
            Id = id;
            Name = name;
            DepartmentID = departmentID;
            DepartmentName = deparmentName;
        }
    }
}
