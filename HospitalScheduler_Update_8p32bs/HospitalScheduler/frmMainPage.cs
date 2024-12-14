using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using HospitalScheduler.Model;

namespace HospitalScheduler
{
    public partial class frmMainPage : Form
    {
        List<Department> departments;
        List<Room> rooms;
        List<Doctor> doctors;
        List<Schedule> optimizedSchedule;
        private int currentWeek = 0; 
        private int totalWeeks = 4;  
        private int selectedRoomId = -1;
        private int currentWeekIndex = 0;
        private int selectedDoctorId = -1; 
        ScheduleManager scheduleManager =new ScheduleManager();
        public frmMainPage()
        {
            departments = new List<Department>();
            rooms = new List<Room>();
            doctors = new List<Doctor>();
            optimizedSchedule = new List<Schedule>();
            InitializeComponent();
        }

        #region Load dữ liệu
        void loadPhongBan()
        {
            var dep1 = new Department(1, "Khoa tim mạch");
            var dep2 = new Department(2, "Khoa thần kinh");
            var dep3 = new Department(3, "Khoa nhi"); 
            var dep4 = new Department(4, "Khoa cấp cứu");

            departments.Add(dep1);
            departments.Add(dep2);
            departments.Add(dep3);
            departments.Add(dep4);
            dataGridViewPhongBan.DataSource = departments;
            dataGridViewPhongBan.ColumnHeadersDefaultCellStyle.Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold);
        }

      
        void loadPhongKham()
        {
            rooms.Clear();
            for (int i = 1; i <= 8; i++)
            {
                int departmentId = i <= 2 ? 1 : i <= 4 ? 2 : i <= 6 ? 3 : 4;
                int departmentIndex = departments.FindIndex(t => t.Id == departmentId);
                var room = new Room(i, $"Phòng {i}", departmentId, departments[departmentIndex].Name);
                rooms.Add(room);
            }


            dataGridViewPhongKham.DataSource = null;
            dataGridViewPhongKham.DataSource = rooms;
            dataGridViewPhongKham.ColumnHeadersDefaultCellStyle.Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold);

        }

        void loadBacSi()
        {
            doctors.Clear();
            for (int i = 1; i <= 8; i++) // Thêm 8 bác sĩ cho Khoa tim mạch
            {
                var departmentId = 1; // Khoa tim mạch
                var departmentIndex = departments.FindIndex(t => t.Id == departmentId);
                var doctor = new Doctor(i, $"Bác sĩ {i}", departmentId, departments[departmentIndex].Name);
                doctors.Add(doctor);
            }
            for (int i = 9; i <= 16; i++) // Thêm 8 bác sĩ cho Khoa thần kinh
            {
                var departmentId = 2; // Khoa thần kinh
                var departmentIndex = departments.FindIndex(t => t.Id == departmentId);
                var doctor = new Doctor(i, $"Bác sĩ {i}", departmentId, departments[departmentIndex].Name);
                doctors.Add(doctor);
            }
            for (int i = 17; i <= 24; i++) // Thêm 8 bác sĩ cho Khoa nhi
            {
                var departmentId = 3; // Khoa nhi
                var departmentIndex = departments.FindIndex(t => t.Id == departmentId);
                var doctor = new Doctor(i, $"Bác sĩ {i}", departmentId, departments[departmentIndex].Name);
                doctors.Add(doctor);
            }
            for (int i = 25; i <= 32; i++) // Thêm 8 bác sĩ cho khoa cấp cứu
            {
                var departmentId = 4; // Khoa cấp cứu
                var departmentIndex = departments.FindIndex(t => t.Id == departmentId);
                var doctor = new Doctor(i, $"Bác sĩ {i}", departmentId, departments[departmentIndex].Name);
                doctors.Add(doctor);
            }

            dataGridViewBacSi.DataSource = doctors;
            dataGridViewBacSi.ColumnHeadersDefaultCellStyle.Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold);
        }

        private void frmMainPage_Load(object sender, EventArgs e)
        {
            loadPhongBan();
            loadPhongKham();
            loadBacSi();
            comboBoxLoc.SelectedIndex = 0;
        }
        #endregion

        #region Xử lý
        //Hàm chính
        List<Schedule> SimulatedAnnealing(List<Room> rooms, List<Doctor> doctors, int totalDays)
        {
            double temperature = 1000.0;
            double coolingRate = 0.95;
            int iterations = 1000;

            List<Schedule> currentSchedule = GenerateInitialSchedule(rooms, doctors, totalDays);
            double currentCost = CalculateCost(currentSchedule, doctors, rooms, totalDays);

            // Gán lịch và chi phí hiện tại là tốt nhất
            List<Schedule> bestSchedule = new List<Schedule>(currentSchedule);
            double bestCost = currentCost;

            Random random = new Random();

            for (int i = 0; i < iterations; i++)
            {
                // Lấy lịch lân cận và tính chi phí của lịch này
                List<Schedule> newSchedule = GenerateNeighbor(currentSchedule, doctors, rooms);
                double newCost = CalculateCost(newSchedule, doctors, rooms, totalDays);

                // Tính xác suất chấp nhận
                if (newCost < currentCost || random.NextDouble() < Math.Exp((currentCost - newCost) / temperature))
                {
                    currentSchedule = newSchedule;
                    currentCost = newCost;

                    if (newCost < bestCost)
                    {
                        bestSchedule = new List<Schedule>(newSchedule);
                        bestCost = newCost;
                    }
                }

                temperature *= coolingRate;
            }

            return bestSchedule;
        }
        #endregion

        #region tạo lịch

        List<Schedule> GenerateInitialSchedule(List<Room> rooms, List<Doctor> doctors, int totalDays)
        {
            Random random = new Random();
            List<Schedule> schedule = new List<Schedule>();
            Dictionary<int, int> doctorShiftCounts = doctors.ToDictionary(d => d.Id, d => 0);

            for (int day = 0; day < totalDays; day++)
            {
                foreach (var room in rooms)
                {
                    var availableDoctors = doctors.Where(d => d.DepartmentID == room.DepartmentID).ToList();

                    // Đảm bảo có đủ bác sĩ cho các ca của phòng cấp cứu
                    for (int shift = 1; shift <= 3; shift++)
                    {
                        if (room.DepartmentID == 4 || shift <= 2) // Phòng cấp cứu (DepartmentID = 4) cần đủ 3 ca
                        {
                            var shiftDoctors = availableDoctors
                                .OrderBy(d => doctorShiftCounts[d.Id]) 
                                .ToList();

                            if (shiftDoctors.Any())
                            {
                                var selectedDoctor = shiftDoctors.First();
                                schedule.Add(new Schedule(day, room.Id, selectedDoctor.Id, shift));
                                doctorShiftCounts[selectedDoctor.Id]++;
                                availableDoctors.Remove(selectedDoctor);
                            }
                            else
                            {
                                // Nếu không có bác sĩ cho ca này, sẽ chọn bác sĩ từ khoa khác (nếu cần thiết)
                                var selectedDoctor = doctors.FirstOrDefault(d => d.DepartmentID != room.DepartmentID && doctorShiftCounts[d.Id] < 3);
                                if (selectedDoctor != null)
                                {
                                    schedule.Add(new Schedule(day, room.Id, selectedDoctor.Id, shift));
                                    doctorShiftCounts[selectedDoctor.Id]++;
                                }
                                else
                                {
                                    schedule.Add(new Schedule(day, room.Id, -1, shift));
                                }
                            }
                        }
                    }
                }
            }

            return schedule;
        }

        #endregion

        #region chi phí
        double CalculateCost(List<Schedule> schedules, List<Doctor> doctors, List<Room> rooms, int days)
        {
            double cost = 0;

            //1.Bác sĩ làm nhiều phòng cùng ca
            var groupedByDoctorDay = schedules
             .GroupBy(s => new { s.DoctorId, s.Day, s.Shift })
             .Where(g => g.Key.DoctorId != -1);

            foreach (var group in groupedByDoctorDay)
            {
                if (group.Select(s => s.RoomId).Distinct().Count() > 1)
                {
                    cost += 50; // Phạt nếu bác sĩ làm nhiều ca hoặc ở nhiều phòng trong 1 ca
                }
            }

            //2.Bác sĩ trực không đúng chuyên khoa
            foreach (var schedule in schedules.Where(s => s.DoctorId != -1))
            {
                var doctor = doctors.FirstOrDefault(d => d.Id == schedule.DoctorId);
                var room = rooms.FirstOrDefault(r => r.Id == schedule.RoomId);

                if (doctor != null && room != null && doctor.DepartmentID != room.DepartmentID)
                {
                    cost += 150; // Phạt nếu bác sĩ trực không đúng chuyên khoa
                }
            }

            //3.Trực ca tối sẽ mà vẫn làm ca sáng hôm sau
            foreach (var schedule in schedules.Where(s => s.Shift == 3))
            {
                int nextDay = schedule.Day + 1;
                if (nextDay < days)
                {
                    bool hasWorkNextDayMorning = schedules.Any(s => s.DoctorId == schedule.DoctorId && s.Day == nextDay && s.Shift == 1);
                    if (hasWorkNextDayMorning)
                    {
                        cost += 1000; // Phạt nếu bác sĩ làm việc ca sáng ngày hôm sau khi đã trực ca tối
                    }
                }
            }

            //4.Phân bổ cân đổi số ca trực của bác sĩ
            var doctorShiftCounts = schedules
            .Where(s => s.DoctorId != -1)
            .GroupBy(s => s.DoctorId)
            .Select(g => g.Count());

            if (doctorShiftCounts.Any())
            {
                int maxShifts = doctorShiftCounts.Max();
                int minShifts = doctorShiftCounts.Min();

                if (maxShifts - minShifts > 2)
                {
                    cost += 200 * (maxShifts - minShifts - 2); // Phạt nếu sự chênh lệch vượt quá 2 ca
                }
            }

            return cost;
        }

        #endregion

        #region hàng xóm
        List<Schedule> GenerateNeighbor(List<Schedule> currentSchedule, List<Doctor> doctors, List<Room> rooms)
        {
            Random random = new Random();
            List<Schedule> newSchedule = new List<Schedule>(currentSchedule);

            int index = random.Next(newSchedule.Count);
            Schedule selectedSchedule = newSchedule[index];
            Room correspondingRoom = rooms.FirstOrDefault(r => r.Id == selectedSchedule.RoomId);

            if (correspondingRoom != null)
            {
                // Lọc ra những bác sĩ thuộc cùng khoa và không trùng ca trực trong ngày
                var availableDoctors = doctors
                    .Where(d => d.DepartmentID == correspondingRoom.DepartmentID &&
                                !newSchedule.Any(s => s.Day == selectedSchedule.Day &&
                                                    s.Shift == selectedSchedule.Shift &&
                                                    s.DoctorId == d.Id))
                    .ToList();


                if (availableDoctors.Any())
                {
                    // Kiểm tra bác sĩ đó có bị trực ca sáng ngày hôm sau không nếu đang ở ca tối
                    var validDoctors = availableDoctors.Where(doctor =>
                    {
                        if (selectedSchedule.Shift == 3)
                        {
                            //Nếu đang là ca tối thì không được trực ca sáng ngày hôm sau
                            return !newSchedule.Any(s => s.Day == selectedSchedule.Day + 1 && s.Shift == 1 && s.DoctorId == doctor.Id);
                        }
                        return true; 
                    }).ToList();
                    if (validDoctors.Any())
                    {
                        var selectedDoctor = validDoctors.OrderBy(d => random.Next()).First();
                        selectedSchedule.DoctorId = selectedDoctor.Id;
                        if (selectedSchedule.Shift == 3)
                        {
                            //Nếu chọn ca tối thì xóa những ca của bác sĩ đó ngày hôm sau
                            newSchedule.RemoveAll(s => s.Day == selectedSchedule.Day + 1 && s.DoctorId == selectedDoctor.Id);
                        }
                    }

                }
            }

            return newSchedule;
        }
        #endregion
        #region hiển thị Schedule
        private void DisplaySchedule(List<Schedule> schedules, DataGridView dataGridView, List<Room> rooms, int startDay, int days)
        {
            
            dataGridView.Columns.Clear();
            dataGridView.Rows.Clear();

          
            foreach (var room in rooms)
            {
                dataGridView.Columns.Add($"Room_{room.Id}", room.Name);
            }

            for (int day = startDay; day < startDay + days; day++)
            {
                string[] shifts = { "Sáng", "Chiều", "Tối" };
                for (int shift = 1; shift <= 3; shift++)
                {
                    int rowIndex = dataGridView.Rows.Add();
                    var row = dataGridView.Rows[rowIndex];
                    row.HeaderCell.Value = $"Ngày {day + 1} - {shifts[shift - 1]}";

                    foreach (var schedule in schedules.Where(s => s.Day == day && s.Shift == shift))
                    {
                        int colIndex = rooms.FindIndex(r => r.Id == schedule.RoomId);
                        if (colIndex != -1)
                        {
                            row.Cells[colIndex].Value = schedule.DoctorId == -1 ? "Trống" : $"Bác sĩ {schedule.DoctorId}";
                        }
                    }
                }
            }
        }
        #endregion

        #region hiển thị ScheduleForWeek
        private void DisplayScheduleForWeek(int startDay, int daysToDisplay)
        {
            // Reset DataGridView
            dataGridViewLich.Columns.Clear();
            dataGridViewLich.Rows.Clear();
            var schedulesForWeek = optimizedSchedule
                .Where(s => s.Day >= startDay && s.Day < startDay + daysToDisplay)
                .ToList();
            DisplaySchedule(schedulesForWeek, dataGridViewLich, rooms, startDay, daysToDisplay);
        }

        #endregion

        #region Button
        private void buttonTaoLich_Click(object sender, EventArgs e)
        {
            try
            {
                int month = int.Parse(txt_thang.Text);
                int year = int.Parse(txt_nam.Text);

               
                if (month < 1 || month > 12)
                {
                    MessageBox.Show("Tháng phải nằm trong khoảng từ 1 đến 12.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (year < 1900 || year > 2100)
                {
                    MessageBox.Show("Năm phải nằm trong khoảng từ 1900 đến 2100.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txt_thang.Text) || string.IsNullOrWhiteSpace(txt_nam.Text))
                {
                    MessageBox.Show("Vui lòng nhập cả tháng và năm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
               
                int daysInMonth = DateTime.DaysInMonth(year, month);

               
                optimizedSchedule = SimulatedAnnealing(rooms, doctors, daysInMonth);
                DisplaySchedule(optimizedSchedule, dataGridViewLich, rooms, 0, daysInMonth);

                MessageBox.Show($"Lịch đã được tạo cho tháng {month}/{year} với {daysInMonth} ngày.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FormatException)
            {
                MessageBox.Show("Vui lòng nhập tháng và năm là số nguyên hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region hiển thị lọc ScheduleForWeek
        private void comboBoxLoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = comboBoxLoc.SelectedIndex;
          
            int startDay = 0;
            int daysToDisplay = 7;

            switch (index)
            {
                case 1:
                    startDay = 0; 
                    break;
                case 2:
                    startDay = 7; 
                    break;
                case 3: 
                    startDay = 14;
                    break;
                case 4: 
                    startDay = 21;
                    break;
                case 5: 
                    startDay = 28; 
                    daysToDisplay = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - 28; 
                    break;
                default:

                    startDay = 0;
                    daysToDisplay = 30; 
                    break;
            }

            DisplayScheduleForWeek(startDay, daysToDisplay);
            
        }
        #endregion

        #region hiển thị ScheduleForRoom
        private void DisplayScheduleForRoom(int roomId)
        {
            if (optimizedSchedule == null || !optimizedSchedule.Any())
            {
                MessageBox.Show("Chưa có lịch để hiển thị. Vui lòng tạo lịch trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lấy thông tin tháng và năm
            if (!int.TryParse(txt_nam.Text, out int year) || !int.TryParse(txt_thang.Text, out int month))
            {
                MessageBox.Show("Vui lòng nhập tháng và năm hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Tính số ngày trong tháng
            int daysInMonth = DateTime.DaysInMonth(year, month);

            // Lọc lịch của phòng đã chọn
            var schedulesForRoom = optimizedSchedule
                .Where(s => s.RoomId == roomId)
                .ToList();

            if (!schedulesForRoom.Any())
            {
                MessageBox.Show("Không có lịch cho phòng này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Hiển thị lịch theo tuần hiện tại
            int daysToDisplay = 7; // Mỗi tuần 7 ngày
            int startDay = currentWeekIndex * daysToDisplay;

            // Đảm bảo không vượt quá số ngày trong tháng
            if (startDay >= daysInMonth)
            {
                MessageBox.Show("Tuần này không có ngày hợp lệ trong tháng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            daysToDisplay = Math.Min(daysToDisplay, daysInMonth - startDay);

            var schedulesForCurrentWeek = schedulesForRoom
                .Where(s => s.Day >= startDay && s.Day < startDay + daysToDisplay)
                .ToList();

            DisplaySchedule(schedulesForCurrentWeek, dataGridViewLich, rooms, startDay, daysToDisplay);
        }

        #endregion

        #region hiển thị dataGridViewPhongKham
        private void dataGridViewPhongKham_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0 && dataGridViewPhongKham.Rows[e.RowIndex].Cells[0].Value != null)
            {
                if (int.TryParse(dataGridViewPhongKham.Rows[e.RowIndex].Cells[0].Value.ToString(), out int roomId))
                {
                    // Reset trạng thái bác sĩ
                    selectedDoctorId = -1;
                    if (dataGridViewBacSi.SelectedRows.Count > 0)
                    {
                        dataGridViewBacSi.ClearSelection(); // Xóa chọn trong dataGridViewBacSi
                    }
                    selectedRoomId = roomId;
                    currentWeek = 0; // Reset tuần về tuần đầu tiên
                    DisplayScheduleForRoom(roomId);
                }
                else
                {
                    MessageBox.Show("Không thể lấy RoomId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region hiển thị btn_trolai
        private void btn_trolai_Click(object sender, EventArgs e)
        {
            if (currentWeekIndex <= 0)
            {
                MessageBox.Show("Đây là tuần đầu tiên, không thể quay lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            currentWeekIndex--; // Giảm tuần hiện tại

            int daysPerWeek = 7;
            int startDay = currentWeekIndex * daysPerWeek;

            if (selectedDoctorId != -1)
            {
                // Lọc lịch theo bác sĩ đã chọn
                var schedulesForDoctor = optimizedSchedule
                    .Where(s => s.DoctorId == selectedDoctorId && s.Day >= startDay && s.Day < startDay + daysPerWeek)
                    .ToList();

                DisplayScheduleForCurrentWeek(schedulesForDoctor, dataGridViewLich, rooms, startDay);
            }
            else if (selectedRoomId != 0)
            {
                if (!int.TryParse(txt_nam.Text, out int year) || !int.TryParse(txt_thang.Text, out int month))
                {
                    MessageBox.Show("Vui lòng nhập tháng và năm hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Reset chọn trong dataGridViewBacSi
                if (dataGridViewBacSi.SelectedRows.Count > 0)
                {
                    dataGridViewBacSi.ClearSelection(); // Xóa tất cả dòng đang được chọn
                }
                int daysInMonth = DateTime.DaysInMonth(year, month);
                int daysToDisplay = Math.Min(daysPerWeek, daysInMonth - startDay);

                // Lọc lịch cho phòng đã chọn
                var schedulesForRoom = optimizedSchedule
                    .Where(s => s.RoomId == selectedRoomId && s.Day >= startDay && s.Day < startDay + daysToDisplay)
                    .ToList();

                DisplaySchedule(schedulesForRoom, dataGridViewLich, rooms, startDay, daysToDisplay);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn bác sĩ hoặc phòng trước khi thao tác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
        #endregion

        #region hiển thị btn_tiep
        private void btn_tiep_Click(object sender, EventArgs e)
        {

            if (!int.TryParse(txt_nam.Text, out int year) || !int.TryParse(txt_thang.Text, out int month))
            {
                MessageBox.Show("Vui lòng nhập tháng và năm hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int daysInMonth = DateTime.DaysInMonth(year, month);
            int daysPerWeek = 7;

            currentWeekIndex++;
            int startDay = currentWeekIndex * daysPerWeek;

            if (startDay >= daysInMonth)
            {
                MessageBox.Show("Không còn tuần tiếp theo để hiển thị.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                currentWeekIndex--;
                return;
            }

            if (selectedDoctorId != -1)
            {
                // Lọc lịch theo bác sĩ đã chọn
                var schedulesForDoctor = optimizedSchedule
                    .Where(s => s.DoctorId == selectedDoctorId && s.Day >= startDay && s.Day < startDay + daysPerWeek)
                    .ToList();

                DisplayScheduleForCurrentWeek(schedulesForDoctor, dataGridViewLich, rooms, startDay);
            }
            else if (selectedRoomId != 0)
            {
                // Reset chọn trong dataGridViewBacSi
                if (dataGridViewBacSi.SelectedRows.Count > 0)
                {
                    dataGridViewBacSi.ClearSelection(); // Xóa tất cả dòng đang được chọn
                }
                int daysToDisplay = Math.Min(daysPerWeek, daysInMonth - startDay);

                // Lọc lịch cho phòng đã chọn
                var schedulesForRoom = optimizedSchedule
                    .Where(s => s.RoomId == selectedRoomId && s.Day >= startDay && s.Day < startDay + daysToDisplay)
                    .ToList();

                DisplaySchedule(schedulesForRoom, dataGridViewLich, rooms, startDay, daysToDisplay);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn bác sĩ hoặc phòng trước khi thao tác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion
        #region Hiển thị dataGridViewLich
        private void dataGridViewLich_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        #endregion
        #region Hiển thị theo bs
        private void DisplayWeeklySchedule(List<Schedule> schedules, DataGridView dataGridView, List<Room> rooms, int startDay)
        {
            // Reset DataGridView
            dataGridView.Columns.Clear();
            dataGridView.Rows.Clear();

          
            string[] daysOfWeek = { "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật" };
            for (int i = 0; i < 7; i++)
            {
                int currentDay = startDay + i;
                dataGridView.Columns.Add($"Day_{currentDay}", daysOfWeek[i] + $" ({currentDay + 1})");
            }
            string[] shifts = { "Sáng", "Chiều", "Tối" };
            for (int shiftIndex = 0; shiftIndex < 3; shiftIndex++)
            {
                int rowIndex = dataGridView.Rows.Add();
                var row = dataGridView.Rows[rowIndex];
                row.HeaderCell.Value = shifts[shiftIndex];

                for (int day = 0; day < 7; day++)
                {
                    int currentDay = startDay + day;
                    var schedule = schedules.FirstOrDefault(s => s.Day == currentDay && s.Shift == shiftIndex + 1);

                    if (schedule != null)
                    {
                        string roomName = rooms.FirstOrDefault(r => r.Id == schedule.RoomId)?.Name ?? "Không rõ";
                        string doctorName = schedule.DoctorId == -1 ? "Trống" : $" (Bác sĩ {schedule.DoctorId})";
                        row.Cells[day].Value = $"{roomName}\n{doctorName}";
                    }
                    else
                    {
                        row.Cells[day].Value = "Trống";
                    }
                }
            }

            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.RowHeadersWidth = 150;
        }
        private void DisplayScheduleForCurrentWeek(List<Schedule> schedules, DataGridView gridView, List<Room> rooms, int startDay)
        {
            int daysPerWeek = 7;
            startDay = currentWeekIndex * daysPerWeek;

            if (selectedDoctorId != -1)
            {
                // Lọc và hiển thị lịch theo bác sĩ đã chọn
                DisplayScheduleForDoctor(selectedDoctorId, startDay, daysPerWeek);
            }
            else
            {
                // Hiển thị lịch của tất cả bác sĩ nếu không có bác sĩ nào được chọn
                var schedulesForCurrentWeek = optimizedSchedule
                    .Where(s => s.Day >= startDay && s.Day < startDay + daysPerWeek)
                    .ToList();

                DisplayWeeklySchedule(schedulesForCurrentWeek, dataGridViewLich, rooms, startDay);
            }
        }


        private void DisplayScheduleForDoctor(int doctorId, int startDay, int daysPerWeek)
        {
            var filteredSchedules = optimizedSchedule
                .Where(s => s.Day >= startDay && s.Day < startDay + daysPerWeek && s.DoctorId == doctorId)
                .ToList();

            DisplayWeeklySchedule(filteredSchedules, dataGridViewLich, rooms, startDay);
        }


        private void dataGridViewBacSi_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0 && dataGridViewBacSi.Rows[e.RowIndex].Cells[0].Value != null)
            {
                if (int.TryParse(dataGridViewBacSi.Rows[e.RowIndex].Cells[0].Value.ToString(), out int doctorId))
                {
                    selectedDoctorId = doctorId; // Lưu DoctorId để sử dụng lại
                    int daysPerWeek = 7;
                    int startDay = currentWeekIndex * daysPerWeek;

                    // Lọc lịch theo bác sĩ và tuần hiện tại
                    var schedulesForDoctor = optimizedSchedule
                        .Where(s => s.DoctorId == doctorId && s.Day >= startDay && s.Day < startDay + daysPerWeek)
                        .ToList();

                    if (!schedulesForDoctor.Any())
                    {
                        MessageBox.Show($"Không có lịch cho Bác sĩ {doctorId} trong tuần này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DisplayWeeklySchedule(schedulesForDoctor, dataGridViewLich, rooms, startDay);
                }
                else
                {
                    MessageBox.Show("Không thể lấy DoctorId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}