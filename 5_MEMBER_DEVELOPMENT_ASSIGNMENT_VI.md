# 👥 PHÂN CÔNG CÔNG VIỆC 5 THÀNH VIÊN
## Dự Án SIMS - Phân Chia Trách Nhiệm Phát Triển Mã

---

## 🎯 **TỔNG QUAN**

Mỗi thành viên sẽ chịu trách nhiệm phát triển và bảo trì các module cụ thể. Điều này đảm bảo:
- ✅ Sở hữu rõ ràng các tính năng
- ✅ Phát triển song song (không xung đột)
- ✅ Tách biệt kiến thức (mỗi người là chuyên gia trong lĩnh vực)
- ✅ Phân bổ công việc đều đặn

---

---

## 👨‍💻 **THÀNH VIÊN 1: QUẢN LÝ XÁC THỰC VÀ PHÂN QUYỀN**
### **Sở hữu: Quản lý người dùng, Đăng nhập, Bảo mật, Roles**

#### **Trách nhiệm:**
1. Xác thực người dùng (đăng nhập/đăng xuất)
2. Bảo mật mật khẩu (BCrypt)
3. Quản lý vai trò
4. Phân quyền (ai có thể truy cập gì)
5. Quản lý session người dùng

#### **Các file mã cần phát triển/bảo trì:**

**Controllers (3 file):**
- `SIMS.Web/Controllers/AccountController.cs` - Endpoints đăng nhập/đăng xuất
- `SIMS.Web/Controllers/AdminController.cs` - Quản lý người dùng (tạo/xóa/chỉnh sửa)
- `SIMS.Web/Controllers/FacultyManagementController.cs` - Thao tác CRUD giảng viên

**Services (2 file):**
- `SIMS.Web/Services/AuthenticationService.cs` - Hashing BCrypt, logic đăng nhập
- `SIMS.Web/Services/RoleService.cs` - Gán vai trò, kiểm tra quyền hạn

**Helpers (1 file):**
- `SIMS.Web/Helpers/AuthorizeAttribute.cs` - Custom authorization decorator

**Models (3 file):**
- `SIMS.Web/Models/User.cs` - Entity người dùng cơ sở
- `SIMS.Web/Models/Role.cs` - Định nghĩa vai trò
- `SIMS.Web/Models/Administrator.cs` - Các thuộc tính riêng của admin

**Views (3 file):**
- `SIMS.Web/Views/Account/Login.cshtml` - Giao diện đăng nhập
- `SIMS.Web/Views/Account/AccessDenied.cshtml` - Trang từ chối quyền truy cập
- `SIMS.Web/Views/Admin/ManageRoles.cshtml` - Giao diện quản lý vai trò

**Database:**
- `SIMS.Web/Data/DbInitializer.cs` - Seeding vai trò mặc định & người dùng admin

#### **Các tính năng chính cần triển khai:**
- [ ] Biểu mẫu đăng nhập (tên người dùng/mật khẩu)
- [ ] Xác thực mật khẩu với BCrypt
- [ ] Quản lý session
- [ ] Kiểm tra phân quyền dựa trên vai trò
- [ ] Tạo người dùng admin
- [ ] Gán vai trò
- [ ] Chức năng đặt lại mật khẩu
- [ ] Khóa tài khoản sau khi đăng nhập thất bại
- [ ] Nhật ký kiểm toán đăng nhập

#### **Trách nhiệm kiểm thử:**
- Viết unit tests cho `AuthenticationService`
- Viết security tests cho phân quyền
- Test hashing mật khẩu
- Test kiểm soát truy cập dựa trên vai trò

#### **Phụ thuộc:**
- Phụ thuộc vào: Database (Thành viên 5)
- Cung cấp cho: Tất cả các thành viên khác (mọi người cần xác thực)

---

---

## 👨‍💻 **THÀNH VIÊN 2: QUẢN LÝ HỌC SINH VÀ ĐĂNG KÝ**
### **Sở hữu: Tính năng học sinh, Khóa học, Đăng ký, Hồ sơ học tập**

#### **Trách nhiệm:**
1. CRUD học sinh (tạo, đọc, cập nhật, xóa)
2. Đăng ký học sinh vào khóa học
3. Hồ sơ học tập & theo dõi GPA
4. Bảng điều khiển & hồ sơ học sinh
5. Duyệt xem khóa học (góc nhìn học sinh)

#### **Các file mã cần phát triển/bảo trì:**

**Controllers (2 file):**
- `SIMS.Web/Controllers/StudentController.cs` - Hồ sơ học sinh, khóa học, điểm số
- `SIMS.Web/Controllers/CourseController.cs` - Danh sách khóa học, chi tiết, đăng ký

**Services (3 file):**
- `SIMS.Web/Services/StudentService.cs` - CRUD học sinh, cập nhật hồ sơ
- `SIMS.Web/Services/EnrollmentService.cs` - Logic đăng ký học sinh-khóa học
- `SIMS.Web/Services/CourseService.cs` - Danh sách khóa học, lọc, chi tiết

**Repositories (3 file):**
- `SIMS.Web/Repositories/StudentRepository.cs` - Truy cập dữ liệu học sinh
- `SIMS.Web/Repositories/CourseRepository.cs` - Truy cập dữ liệu khóa học
- `SIMS.Web/Repositories/IRepository.cs` - Giao diện repository chung

**Models (4 file):**
- `SIMS.Web/Models/Student.cs` - Entity học sinh + đăng ký
- `SIMS.Web/Models/Course.cs` - Entity khóa học
- `SIMS.Web/Models/Enrollment.cs` - Mối quan hệ Học sinh-Khóa học
- `SIMS.Web/Models/AcademicRecord.cs` - Điểm số & GPA

**ViewModels (4 file):**
- `SIMS.Web/ViewModels/StudentViewModel.cs` - Dữ liệu xem danh sách/chi tiết học sinh
- `SIMS.Web/ViewModels/CourseViewModel.cs` - Dữ liệu xem khóa học
- `SIMS.Web/ViewModels/EnrollmentViewModel.cs` - Dữ liệu thao tác đăng ký
- `SIMS.Web/ViewModels/AcademicRecordViewModel.cs` - Dữ liệu báo cáo điểm

**Views (5 file):**
- `SIMS.Web/Views/Student/Index.cshtml` - Danh sách học sinh
- `SIMS.Web/Views/Student/Detail.cshtml` - Hồ sơ học sinh
- `SIMS.Web/Views/Student/Edit.cshtml` - Chỉnh sửa thông tin học sinh
- `SIMS.Web/Views/Student/AcademicRecord.cshtml` - Xem điểm & GPA
- `SIMS.Web/Views/Course/Index.cshtml` - Duyệt khóa học
- `SIMS.Web/Views/Course/Detail.cshtml` - Chi tiết khóa học với đăng ký

#### **Các tính năng chính cần triển khai:**
- [ ] Tạo học sinh mới (admin)
- [ ] Xem danh sách học sinh
- [ ] Xem hồ sơ học sinh (thông tin cá nhân, liên hệ)
- [ ] Chỉnh sửa thông tin học sinh
- [ ] Xóa học sinh
- [ ] Đăng ký học sinh vào khóa học
- [ ] Xem khóa học đã đăng ký
- [ ] Xem hồ sơ học tập (điểm, GPA)
- [ ] Tính GPA tự động
- [ ] Duyệt xem khóa học
- [ ] Tìm kiếm/lọc khóa học
- [ ] Xem chi tiết khóa học & học sinh đã đăng ký
- [ ] Hủy khóa học (với kiểm tra hạn chót)

#### **Trách nhiệm kiểm thử:**
- Viết unit tests cho `StudentService`, `EnrollmentService`, `CourseService`
- Viết integration tests cho quy trình đăng ký
- Test độ chính xác tính toán GPA
- Test ràng buộc khóa học (tối đa đăng ký, v.v.)

#### **Phụ thuộc:**
- Phụ thuộc vào: Xác thực (Thành viên 1), Database (Thành viên 5)
- Cung cấp cho: Giảng viên (Thành viên 3), Báo cáo (Thành viên 4), Thông báo (Thành viên 5)

---

---

## 👨‍💻 **THÀNH VIÊN 3: QUẢN LÝ GIẢNG VIÊN VÀ NHẬP ĐIỂM**
### **Sở hữu: Tính năng giảng viên, Điểm số, Bài tập, Điểm danh**

#### **Trách nhiệm:**
1. Hồ sơ & thông tin giảng viên
2. Nhập/quản lý điểm số
3. Tạo bài tập
4. Điểm danh (tùy chọn)
5. Đánh giá khóa học
6. Bảng điều khiển giảng viên

#### **Các file mã cần phát triển/bảo trì:**

**Controllers (4 file):**
- `SIMS.Web/Controllers/FacultyController.cs` - Bảng điều khiển giảng viên, nhập điểm
- `SIMS.Web/Controllers/AssignmentController.cs` - Tạo/quản lý bài tập
- `SIMS.Web/Controllers/AttendanceController.cs` - Điểm danh/xem điểm danh
- `SIMS.Web/Controllers/CourseReviewController.cs` - Đánh giá khóa học

**Services (4 file):**
- `SIMS.Web/Services/FacultyService.cs` - Thao tác giảng viên
- `SIMS.Web/Services/AssignmentService.cs` - CRUD bài tập
- `SIMS.Web/Services/AttendanceService.cs` - Theo dõi điểm danh
- `SIMS.Web/Services/CourseReviewService.cs` - Đánh giá khóa học

**Models (5 file):**
- `SIMS.Web/Models/Faculty.cs` - Entity giảng viên
- `SIMS.Web/Models/Assignment.cs` - Entity bài tập
- `SIMS.Web/Models/Submission.cs` - Nộp bài tập
- `SIMS.Web/Models/Attendance.cs` - Bản ghi điểm danh
- `SIMS.Web/Models/CourseReview.cs` - Đánh giá/đánh giá khóa học

**ViewModels (2 file):**
- `SIMS.Web/ViewModels/DashboardViewModel.cs` - Dữ liệu bảng điều khiển giảng viên
- `SIMS.Web/ViewModels/ReportViewModels.cs` - Dữ liệu báo cáo

**Views (8 file):**
- `SIMS.Web/Views/Faculty/InputGrade.cshtml` - Biểu mẫu nhập điểm
- `SIMS.Web/Views/Faculty/SelectStudent.cshtml` - Chọn học sinh để nhập điểm
- `SIMS.Web/Views/Faculty/Detail.cshtml` - Hồ sơ giảng viên
- `SIMS.Web/Views/Assignment/Create.cshtml` - Tạo bài tập
- `SIMS.Web/Views/Assignment/Detail.cshtml` - Xem bài tập
- `SIMS.Web/Views/Assignment/MyAssignment.cshtml` - Bài tập của tôi (xem học sinh)
- `SIMS.Web/Views/Attendance/TakeAttendance.cshtml` - Điểm danh
- `SIMS.Web/Views/CourseReview/Create.cshtml` - Đánh giá khóa học

**Repository:**
- `SIMS.Web/Repositories/CourseReviewRepository.cs` - Truy cập dữ liệu đánh giá khóa học

#### **Các tính năng chính cần triển khai:**
- [ ] Xem khóa học được giao
- [ ] Nhập/chỉnh sửa điểm học sinh
- [ ] Xác thực điểm (0-100, tính GPA)
- [ ] Tạo bài tập có hạn chót
- [ ] Xem bài nộp của học sinh
- [ ] Chấm điểm bài tập
- [ ] Điểm danh cho khóa học
- [ ] Xem báo cáo điểm danh
- [ ] Biểu mẫu đánh giá khóa học
- [ ] Phản hồi cho học sinh
- [ ] Bảng điều khiển giảng viên (tóm tắt nhiệm vụ)
- [ ] Xuất điểm dưới dạng CSV/Excel

#### **Trách nhiệm kiểm thử:**
- Viết unit tests cho logic nhập điểm
- Test xác thực bài tập
- Test theo dõi điểm danh
- Test tái tính GPA khi thay đổi điểm

#### **Phụ thuộc:**
- Phụ thuộc vào: Dữ liệu học sinh (Thành viên 2), Xác thực (Thành viên 1), Database (Thành viên 5)
- Cung cấp cho: Báo cáo (Thành viên 4), Thông báo (Thành viên 5)

---

---

## 👨‍💻 **THÀNH VIÊN 4: BÁNG CÁO VÀ PHÂN TÍCH**
### **Sở hữu: Báo cáo, Bảng điều khiển, Phân tích dữ liệu, Thông báo**

#### **Trách nhiệm:**
1. Tạo báo cáo (học sinh, khóa học, giảng viên)
2. Phân tích dữ liệu & thống kê
3. Hiển thị bảng điều khiển
4. Hệ thống thông báo
5. Chức năng xuất (PDF, Excel)

#### **Các file mã cần phát triển/bảo trì:**

**Controllers (2 file):**
- `SIMS.Web/Controllers/ReportController.cs` - Tạo báo cáo
- `SIMS.Web/Controllers/NotificationController.cs` - Hệ thống thông báo

**Services (3 file):**
- `SIMS.Web/Services/ReportService.cs` - Logic tạo báo cáo
- `SIMS.Web/Services/DashboardService.cs` - Tổng hợp dữ liệu bảng điều khiển
- `SIMS.Web/Services/NotificationService.cs` - Quản lý thông báo

**Models (1 file):**
- `SIMS.Web/Models/Notification.cs` - Entity thông báo

**ViewModels (1 file):**
- `SIMS.Web/ViewModels/ReportViewModels.cs` - Dữ liệu xem báo cáo

**Views (2 file):**
- `SIMS.Web/Views/Report/Index.cshtml` - Giao diện tạo báo cáo
- `SIMS.Web/Views/Notification/Index.cshtml` - Trung tâm thông báo
- `SIMS.Web/Views/Shared/_Layout.cshtml` - Bao gồm widget bảng điều khiển

#### **Các tính năng chính cần triển khai:**

**Báo cáo học sinh:**
- [ ] Điểm theo khóa học
- [ ] Phân bố điểm
- [ ] Tỷ lệ điểm danh
- [ ] Hoàn thành bài tập

**Báo cáo giảng viên:**
- [ ] Thống kê khóa học
- [ ] Xu hướng đăng ký học sinh
- [ ] Phân bố điểm
- [ ] Tỷ lệ nộp bài tập

**Báo cáo khóa học:**
- [ ] Số lượng đăng ký
- [ ] Điểm trung bình
- [ ] Thống kê điểm danh
- [ ] Tỷ lệ hoàn thành

**Widget bảng điều khiển:**
- [ ] Thống kê nhanh (tổng học sinh, khóa học, v.v.)
- [ ] Hoạt động gần đây
- [ ] Sự kiện sắp tới

**Thông báo:**
- [ ] Thông báo đã đăng điểm
- [ ] Nhắc nhở hạn chót bài tập
- [ ] Xác nhận đăng ký khóa học
- [ ] Thông báo gán giảng viên

**Chức năng xuất:**
- [ ] Xuất sang PDF
- [ ] Xuất sang Excel
- [ ] Xuất sang CSV

#### **Trách nhiệm kiểm thử:**
- Viết unit tests cho tính toán
- Test độ chính xác báo cáo
- Test chức năng xuất
- Test kích hoạt thông báo

#### **Phụ thuộc:**
- Phụ thuộc vào: Dữ liệu học sinh (Thành viên 2), Dữ liệu giảng viên (Thành viên 3), Database (Thành viên 5)
- Tiêu thụ: Tất cả các module khác cung cấp dữ liệu

---

---

## 👨‍💻 **THÀNH VIÊN 5: CƠ SỞ DỮ LIỆU VÀ HẠTẦNG TẦN**
### **Sở hữu: Lược đồ cơ sở dữ liệu, Migrations, Khởi tạo dữ liệu, Cấu hình**

#### **Trách nhiệm:**
1. Thiết kế & lược đồ cơ sở dữ liệu
2. Cấu hình Entity Framework
3. Migrations cơ sở dữ liệu
4. Seeding dữ liệu
5. Tối ưu hóa hiệu suất
6. Sao lưu & khôi phục

#### **Các file mã cần phát triển/bảo trì:**

**Database (3 file):**
- `SIMS.Web/Data/AppDbContext.cs` - Cấu hình entity, mối quan hệ
- `SIMS.Web/Data/DbInitializer.cs` - Seeding dữ liệu mẫu
- `SIMS.Web/Data/Migrations/*` - Các file migration

**Configuration:**
- `SIMS.Web/Program.cs` - Đăng ký DbContext, middleware
- `SIMS.Web/appsettings.json` - Chuỗi kết nối, ghi nhật ký
- `SIMS.Web/Properties/launchSettings.json` - Profil phát triển

**Infrastructure:**
- `Dockerfile` - Docker containerization
- `docker-compose.yml` - Orchestration đa container
- `.gitignore` - Loại trừ kiểm soát phiên bản

#### **Các tính năng chính cần triển khai:**
- [ ] Thiết kế lược đồ cơ sở dữ liệu
  - [ ] Bảng Users (TPH inheritance)
  - [ ] Students, Faculty, Administrators
  - [ ] Bảng Courses
  - [ ] Bảng Enrollments
  - [ ] Bảng AcademicRecords
  - [ ] Assignments, Submissions
  - [ ] Bảng Attendance
  - [ ] Bảng Notifications
  - [ ] Bảng CourseReviews

- [ ] Mối quan hệ entity
  - [ ] Một-nhiều (Course → Students)
  - [ ] Nhiều-nhiều (Students ↔ Courses)
  - [ ] Quy tắc xóa tầng (cascade delete)

- [ ] Migrations cơ sở dữ liệu
  - [ ] Tạo lược đồ ban đầu
  - [ ] Thêm cột/bảng theo từng bước
  - [ ] Khả năng rollback

- [ ] Seeding dữ liệu
  - [ ] Tạo vai trò mặc định
  - [ ] Tạo người dùng kiểm thử (admin, giảng viên, học sinh)
  - [ ] Tạo khóa học mẫu
  - [ ] Tạo đăng ký mẫu

- [ ] Tối ưu hóa hiệu suất
  - [ ] Tạo chỉ mục (cột thường dùng trong truy vấn)
  - [ ] Tối ưu hóa truy vấn (Include, Select)
  - [ ] Kết nối pooling

- [ ] Thiết lập Docker
  - [ ] Dockerize ứng dụng
  - [ ] Container SQL Server
  - [ ] Quản lý volume

#### **Trách nhiệm kiểm thử:**
- Xác minh tính toàn vẹn lược đồ
- Test migrations lên/xuống
- Test script seeding
- Load testing (1000+ học sinh)

#### **Phụ thuộc:**
- Cung cấp cho: Tất cả các thành viên khác (mọi người sử dụng cơ sở dữ liệu)
- Phụ thuộc vào: Không (nền tảng)

---

---

## 📊 **BẢNG PHÂN BỐ KHỐI LƯỢNG CÔNG VIỆC**

| Thành viên | Module | Controllers | Services | Models | Views | Repos | % |
|-----------|--------|-------------|----------|--------|-------|-------|-------|
| **1** | Xác thực & Vai trò | 3 | 2 | 3 | 3 | 0 | 20% |
| **2** | Học sinh & Khóa học | 2 | 3 | 4 | 5 | 3 | 22% |
| **3** | Giảng viên & Nhập điểm | 4 | 4 | 5 | 8 | 1 | 24% |
| **4** | Báo cáo & Phân tích | 2 | 3 | 1 | 2 | 0 | 18% |
| **5** | Database & Config | 0 | 0 | 12* | 0 | 0 | 16% |

**\* Tất cả 12 models + migrations + infrastructure*

---

## 🔗 **MA TRẬN TƯƠNG TÁC**

```
Ai phụ thuộc vào ai:

Thành viên 2 (Học sinh) ← Thành viên 1 (Xác thực) ✓
Thành viên 3 (Giảng viên) ← Thành viên 1 (Xác thực) ✓
Thành viên 3 (Giảng viên) ← Thành viên 2 (Học sinh) ✓
Thành viên 4 (Báo cáo) ← Tất cả thành viên (2, 3) ✓
Tất cả ← Thành viên 5 (Database) ✓

Thứ tự phát triển:
1. Thành viên 5: Cơ sở dữ liệu (nền tảng)
2. Thành viên 1: Xác thực (mọi người cần)
3. Thành viên 2: Học sinh
4. Thành viên 3: Giảng viên
5. Thành viên 4: Báo cáo (cuối cùng - tiêu thụ tất cả)
```

---

## 📋 **LỊCH PHÁT TRIỂN**

### **Tuần 1: Nền tảng**
- Ngày 1-2: Thành viên 5 - Lược đồ cơ sở dữ liệu, migrations
- Ngày 3-5: Thành viên 1 - Hệ thống xác thực, Thành viên 5 - Seeding

### **Tuần 2: Tính năng cốt lõi**
- Ngày 1-5: Thành viên 2 - CRUD Học sinh/Khóa học
- Ngày 1-5: Thành viên 3 - Giảng viên/Nhập điểm (song song)

### **Tuần 3: Tích hợp**
- Ngày 1-3: Thành viên 4 - Báo cáo/Bảng điều khiển
- Ngày 4-5: Tất cả - Integration tests, sửa lỗi

### **Tuần 4: Triển khai**
- Ngày 1-3: Tất cả - Kiểm thử cuối cùng, tài liệu
- Ngày 4-5: Thành viên 5 - Triển khai production (Duck DNS/Azure)

---

## ✅ **DANH SÁCH KIỂM TRA ĐẢM BẢO CHẤT LƯỢNG**

Mỗi thành viên phải đảm bảo:

- ✅ Code tuân theo nguyên lý SOLID (SRP, DIP, v.v.)
- ✅ Quy ước đặt tên nhất quán (PascalCase cho class, camelCase cho methods)
- ✅ Unit tests được viết (>90% coverage cho module của bạn)
- ✅ Integration tests cho tương tác xuyên module
- ✅ Xử lý lỗi (try-catch, thông báo có ý nghĩa)
- ✅ Logging được triển khai (für debugging)
- ✅ Code được comment (logic phức tạp được giải thích)
- ✅ Không có giá trị hardcoded (dùng appsettings.json)
- ✅ HTTPS bảo mật được thực thi
- ✅ Xác thực đầu vào cho tất cả input người dùng

---

## 🚀 **QUY TRÌNH GIT**

```bash
# Mỗi thành viên tạo feature branch
git checkout -b member1/auth-system
git checkout -b member2/student-management
git checkout -b member3/faculty-grading
git checkout -b member4/reporting
git checkout -b member5/database-setup

# Commit thường xuyên
git add .
git commit -m "feat: Thêm logic đăng ký học sinh"

# Push lên GitHub
git push origin member1/auth-system

# Tạo Pull Request để review
# (Các thành viên khác review trước khi merge)

# Merge vào master khi được phê duyệt
git checkout master
git merge member1/auth-system
```

---

## 📝 **DANH SÁCH KIỂM TRA REVIEW MÃ**

Trước khi merge, các thành viên khác kiểm tra:
- ✅ Code tuân theo tiêu chuẩn team
- ✅ Không xung đột với code của thành viên khác
- ✅ Tests pass (tất cả xanh)
- ✅ Tài liệu được cập nhật
- ✅ Không có secrets/mật khẩu hardcoded
- ✅ Hiệu suất chấp nhận được

---

## 💬 **GIAO THỨC LIÊN LẠC**

**Hàng ngày (15 min standup):**
- Tôi đã hoàn thành cái gì?
- Tôi đang làm cái gì?
- Có vấn đề nào không?

**Khi bị chặn:**
- Tag thành viên phụ thuộc (ví dụ: @Thành viên 5 "Cần kết nối DB")
- Cung cấp context (bạn đang cố làm gì)
- Chờ phản hồi (tối đa 2 giờ)

**Xung đột code:**
- Thảo luận trong team chat
- Giải quyết cùng nhau
- Cập nhật shared decisions doc

---

## 📚 **CHIA SẺ KIẾN THỨC**

Mỗi thành viên tài liệu hóa module của mình:

**Weekly documentation (30 min):**
- Hướng dẫn cách sử dụng các tính năng
- Tài liệu API (nếu áp dụng)
- Lược đồ cơ sở dữ liệu cho models của bạn
- Xác thực/yêu cầu

**Ví dụ mã:**
- Hiển thị 2-3 ví dụ tốt
- Tài liệu hóa thuật toán phức tạp
- Liệt kê những cạm bẫy phổ biến

---

## 🎯 **TIÊU CHÍ THÀNH CÔNG**

Khi kết thúc phát triển:

✅ Thành viên 1: Người dùng có thể đăng nhập/đăng xuất với vai trò
✅ Thành viên 2: Học sinh có thể xem hồ sơ, đăng ký khóa học, xem điểm
✅ Thành viên 3: Giảng viên có thể nhập điểm, tạo bài tập, điểm danh
✅ Thành viên 4: Bất kỳ ai cũng có thể xem báng cáo và bảng điều khiển
✅ Thành viên 5: Mọi thứ hoạt động với cơ sở dữ liệu sạch, có thể triển khai production

---

## 📞 **ĐƯỜNG ESCALATION**

Nếu bạn bị chặn:
1. Cố gắng giải quyết độc lập (30 phút)
2. Hỏi đồng đội trong Slack/chat
3. Hỏi team lead (nếu không có team lead → hỏi toàn bộ team)
4. Tạo issue trên GitHub để theo dõi

---

## 🏆 **SẢN PHẨM GIAO**

**Mỗi thành viên nộp:**
- ✅ Mã nguồn (GitHub)
- ✅ Unit tests (>90% coverage)
- ✅ Tài liệu (README cho module của bạn)
- ✅ Kết quả integration tests
- ✅ Phản hồi review mã (review 2 module khác)

---

## 📊 **ĐÁNH GIÁ**

Mỗi thành viên được đánh giá dựa trên:
- **Chất lượng code** (30%) - Nguyên lý SOLID, khả năng đọc
- **Tính hoàn chỉnh** (30%) - Tất cả các tính năng được gán được triển khai
- **Kiểm thử** (20%) - Unit + integration tests
- **Cộng tác** (20%) - Liên lạc, review code, giúp đỡ người khác

---

**Đội của bạn sẵn sàng xây dựng SIMS cùng nhau!** 🚀

**Hãy bắt đầu coding!** 💻✨
