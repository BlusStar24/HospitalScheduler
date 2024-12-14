using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HospitalScheduler.Model
{
    internal class ScheduleManager
    {
        private List<Schedule> allSchedules;

        public ScheduleManager()
        {
            // Khởi tạo danh sách lịch trình với Day là kiểu int
            allSchedules = new List<Schedule>
        {
            new Schedule(0, 1, 12, 1), // Chủ nhật
            new Schedule(1, 1, 12, 2), // Thứ hai
            new Schedule(2, 1, 12, 1), // Thứ ba
            new Schedule(3, 1, 12, 2), // Thứ tư
            new Schedule(4, 1, 12, 3), // Thứ năm
            new Schedule(5, 1, 12, 1), // Thứ sáu
            new Schedule(6, 1, 12, 2)  // Thứ bảy
        };
        }

        public List<Schedule> GetSchedulesForRoomAndWeek(int roomId, DateTime startDate)
        {
            // Lấy ngày trong tuần của startDate (0 = Chủ nhật, 6 = Thứ bảy)
            int startDayOfWeek = (int)startDate.DayOfWeek;

            // Kiểm tra startDayOfWeek để đảm bảo đúng
            MessageBox.Show($"Start day of the week: {startDayOfWeek}");

            // Tính toán ngày cuối tuần (7 ngày sau startDate)
            DateTime endDate = startDate.AddDays(7);
            MessageBox.Show($"Start date: {startDate}, End date: {endDate}");

            // Lọc các lịch trình theo RoomId và ngày trong tuần
            var filteredSchedules = allSchedules
                .Where(s => s.RoomId == roomId &&
                            s.Day >= startDayOfWeek && s.Day < startDayOfWeek + 7) // So sánh ngày trong tuần với startDate
                .ToList();

            // Kiểm tra kết quả
            if (filteredSchedules.Count == 0)
            {
                Console.WriteLine("No schedules found for the specified room and week.");
            }

            return filteredSchedules;
        }

    }

}
