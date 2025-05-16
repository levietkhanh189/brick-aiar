# Brick AIAR - Ứng Dụng LEGO AR

Ứng dụng thực tế tăng cường (AR) cho phép người dùng tương tác với các mô hình LEGO trong không gian thực.

## Tổng Quan

Brick AIAR là một ứng dụng Unity kết hợp AR Foundation để tạo trải nghiệm thực tế tăng cường với các mô hình LEGO. Người dùng có thể đặt, tương tác và chơi với các khối LEGO ảo trong môi trường thực.

## Tính Năng

- **Nhận Diện Bề Mặt**: Phát hiện bề mặt trong thế giới thực để đặt mô hình LEGO
- **Đặt Mô Hình**: Đặt và di chuyển các mô hình LEGO trong không gian AR
- **Tương Tác**: Tương tác với các mô hình thông qua màn hình cảm ứng
- **Gói Nội Dung Mở Rộng**: Hỗ trợ nhiều gói nội dung LEGO khác nhau (Danger Zone, Moody Skies, Santa's Brickshop, v.v.)

## Yêu Cầu Hệ Thống

### Phát Triển
- Unity 2022.3 hoặc cao hơn
- AR Foundation
- ARCore (Android) / ARKit (iOS)
- .NET Framework tương thích với phiên bản Unity hiện tại

### Thiết Bị Hỗ Trợ
- iOS 13.0 trở lên (iPhone 6s hoặc mới hơn)
- Android 8.0 trở lên với hỗ trợ ARCore

## Cài Đặt

1. Clone repository:
   ```
   git clone https://github.com/yourusername/brick-aiar.git
   ```
2. Mở dự án trong Unity Hub
3. Mở scene chính trong Assets/_Main/Scenes/
4. Dựng ứng dụng cho nền tảng mục tiêu (iOS hoặc Android)

## Cấu Trúc Dự Án

- `Assets/_Main/`: Các assets chính của dự án
- `Assets/LEGO/`: Assets LEGO chính thức (mô hình, vật liệu, v.v.)
- `Assets/AddOns/`: Các gói nội dung mở rộng
- `Assets/MobileARTemplateAssets/`: Tài nguyên AR Foundation

## Phát Triển

Dự án này sử dụng mô hình kiến trúc dựa trên component của Unity với tập trung vào:
- Tính mô-đun và khả năng tái sử dụng
- Tối ưu hóa hiệu suất cho thiết bị di động
- Trải nghiệm AR mượt mà
