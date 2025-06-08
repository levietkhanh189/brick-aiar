# Hệ thống Mobile Joystick cho LEGO Minifig

## Tổng quan

Hệ thống này cho phép tự động tạo và quản lý joystick trên mobile để điều khiển LEGO Minifig. Joystick sẽ được tạo tự động khi chạy trên mobile và tự động xoá khi Minifig bị destroy.

## Components

### 1. MinifigMobileJoystickManager
Component chính quản lý việc tạo, xoá và điều khiển joystick.

**Tính năng:**
- Tự động phát hiện mobile platform
- Tạo joystick từ prefab hoặc tạo joystick mặc định
- Quản lý vị trí và cài đặt joystick
- Tự động xoá khi Minifig bị destroy
- Xử lý application pause/resume

### 2. MinifigControllerExtension
Component mở rộng cho MinifigController để hỗ trợ input từ nguồn external.

**Tính năng:**
- Nhận input từ joystick
- Fallback về keyboard input
- Áp dụng movement relative to camera
- Debug visualization trong Scene view

### 3. MinifigMobileSetupEditor
Editor tool để dễ dàng setup mobile joystick cho Minifig.

## Cách sử dụng

### Setup tự động (Khuyến nghị)

1. Chọn GameObject có MinifigController trong scene
2. Vào menu `LEGO Tools > Setup Mobile Joystick for Minifig`
3. Trong cửa sổ Setup:
   - Chọn Minifig GameObject (hoặc sẽ tự động detect)
   - Tuỳ chỉnh cài đặt nếu cần
   - Click "Setup Mobile Joystick"

### Setup nhanh cho tất cả Minifig

Vào menu `LEGO Tools > Setup All Minifigs for Mobile` để tự động setup cho tất cả Minifig trong scene.

### Setup thủ công

1. Thêm `MinifigMobileJoystickManager` vào GameObject có MinifigController
2. Thêm `MinifigControllerExtension` vào cùng GameObject
3. Cấu hình các cài đặt trong Inspector

## Cài đặt

### MinifigMobileJoystickManager Settings

- **Joystick Prefab**: Prefab joystick tùy chỉnh (để trống để dùng joystick mặc định)
- **Auto Create On Mobile**: Tự động tạo joystick khi chạy trên mobile
- **Joystick Position**: Vị trí joystick trên màn hình (pixel từ góc dưới trái)
- **Joystick Sensitivity**: Độ nhạy của joystick (0.1 - 5.0)

### MinifigControllerExtension Settings

- **Use External Input**: Bật/tắt sử dụng external input
- **Override Keyboard Input**: Ghi đè keyboard input bằng joystick
- **External Input Dead Zone**: Vùng chết của external input

## Joystick Prefabs hỗ trợ

Hệ thống hỗ trợ tất cả joystick prefabs từ Joystick Pack:
- Dynamic Joystick (Khuyến nghị cho mobile)
- Fixed Joystick
- Floating Joystick
- Variable Joystick

## API Reference

### MinifigMobileJoystickManager

```csharp
// Tạo joystick
public void CreateJoystick()

// Xoá joystick
public void DestroyJoystick()

// Bật/tắt joystick
public void SetJoystickActive(bool active)

// Lấy input từ joystick
public Vector2 GetJoystickInput()

// Thiết lập vị trí joystick
public void SetJoystickPosition(Vector2 position)
```

### MinifigControllerExtension

```csharp
// Bật/tắt external input
public void SetUseExternalInput(bool use)

// Thiết lập movement input từ external
public void SetExternalMovementInput(Vector2 input)

// Kiểm tra trạng thái external input
public bool IsUsingExternalInput()

// Lấy external input hiện tại
public Vector2 GetExternalMovementInput()
```

## Platform Detection

Hệ thống tự động phát hiện mobile platform:
- `Application.isMobilePlatform`
- `RuntimePlatform.Android`
- `RuntimePlatform.IPhonePlayer`

## Troubleshooting

### Joystick không xuất hiện
1. Kiểm tra xem có đang chạy trên mobile platform không
2. Kiểm tra `autoCreateOnMobile` đã được bật
3. Kiểm tra console để xem thông báo lỗi

### Movement không hoạt động
1. Kiểm tra `MinifigControllerExtension` đã được thêm
2. Kiểm tra `Use External Input` đã được bật
3. Kiểm tra Camera.main có tồn tại không

### Joystick không bị xoá khi destroy
1. Kiểm tra `OnDestroy()` method có được gọi không
2. Kiểm tra không có reference nào khác giữ joystick

## Performance Notes

- Joystick chỉ được tạo khi cần thiết (mobile platform)
- Sử dụng object pooling cho joystick nếu cần create/destroy thường xuyên
- UI Canvas được tối ưu cho mobile với CanvasScaler

## Tương thích

- Unity 2020.3 LTS trở lên
- LEGO Microgame package
- Joystick Pack (đi kèm)
- Hỗ trợ Android và iOS

## Debug và Testing

Trong Editor:
- Sử dụng `Application.isEditor` để test mobile behavior
- Scene Gizmos hiển thị movement direction
- Inspector hiển thị real-time joystick input

## Extension và Customization

Có thể mở rộng hệ thống bằng cách:
1. Tạo custom joystick prefabs
2. Thêm touch buttons cho jump/special actions
3. Tích hợp với analytics để track user input
4. Thêm haptic feedback cho mobile

## Changelog

### Version 1.0
- Tạo hệ thống cơ bản
- Hỗ trợ auto create/destroy joystick
- Editor tools
- Documentation 