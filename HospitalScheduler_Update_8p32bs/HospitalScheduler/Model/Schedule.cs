using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalScheduler.Model
{
        internal class Schedule
        {

            public int Day { get; set; }
            public int RoomId { get; set; }
            public int DoctorId { get; set; }
            public int Shift { get; set; } // 1: Sáng, 2: Chiều, 3: Tối

            public Schedule(int day, int roomId, int doctorId, int shift)
            {
                Day = day;
                RoomId = roomId;
                DoctorId = doctorId;
                Shift = shift;
            }
        }
}
