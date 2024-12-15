# G4T - Dự án bán game kỹ thuật số
Đây là dự án bán game kỹ thuật số tương tự như steam, epic, itch.io, gog.com,... Với mong muốn đưa game của nhà phát triển đến gần hơn với người dùng. 
  # Với các chức năng nổi bật như: 
    - Đăng nhập Google
    - Thanh toán MoMo
    - Thanh toán ngân hàng (payos)
    - Kiểm duyệt file game (bằng VirusTotal)
    - Kiểm duyệt hình ảnh (Azure Content Safety)
  Sử dụng ngôn ngữ, design pattern, công cụ, và 3rd party liên quan:
  Design pattern
      - Microservice Architecture
      - MVC Design Pattern
      - API Gateway Pattern 
      - Repository và Dependency injection Design Pattern
  Database
      - MongoDB
  # Bankend
      - C#
      - ASP.NET Core MVC, RESTful API
      - Entity Framework ( kết hợp với Extension cho MongoDB)
      - Ocelot (Cho Gateway)
  # Frontend
      - HTML, CSS, JS, Bootstrap
      - jQuery, AJAX
  # Third party
      - Google Console
        + Đăng nhập, đăng ký bằng Google
        + Upload game bằng api Google Drive
      - Cloudiary
        + Upload avatar
        + Upload hình ảnh của game
      - Momo - tích hợp thanh toán (UAT - Dev test)
      - PayOS (VietQR thanh toán ngân hàng)
      - VirusTotal (kiểm duyệt file game)
      - Azure Content Safety (kiểm duyệt hình ảnh)
      - Azure Key Vault (lưu enviroment variable)
  # Công cụ sử dụng
      - Docker
      - MongoDB Atlas, Mongo Shell
      - Git, Github

# Các thành viên thực hiện dự án gồm:
  - Trần Ngọc Hưng
  - Huỳnh Trần Tuấn Kiệt
  - Võ Bá Tần
  - Phạm Minh Tuấn
  - Nguyễn Trung Nhân
