# Changelog - Rent Is Due

Tất cả những thay đổi, cập nhật tính năng mới của dự án sẽ được ghi chú tại đây.

## [MVP Development]

### Phase 03 - Interaction System (Hoàn thành)
- **Thêm mới**: Interface `IInteractable.cs` làm nền tảng cho mọi vật thể có thể tương tác (nhặt, mở, bán...).
- **Thêm mới**: Script `PlayerInteractor.cs` tích hợp Input System mới, cho phép dò tìm các vật thể xung quanh và tương tác bằng phím `E`.
- **Thêm mới**: Script mẫu `TestInteractable.cs` để test tương tác in log ra Console.

### Phase 02 - Player Movement & Camera (Hoàn thành)
- **Thêm mới**: Script `PlayerMovement.cs` sử dụng CharacterController, kết hợp Input System mới hỗ trợ di chuyển mượt mà (WASD/Arrow keys), tự động xoay người và không bị xuyên tường.
- **Thêm mới**: Script `CameraController.cs` hỗ trợ camera góc nhìn từ trên xuống (Top-down / Isometric), góc nghiêng cố định và bám theo nhân vật mượt mà bằng nội suy (Lerp).

### Phase 01 - Project Setup (Hoàn thành)
- **Khởi tạo**: Khởi tạo Unity 6 Project với Universal Render Pipeline (URP).
- **Khởi tạo**: Tự động sinh cấu trúc thư mục chuẩn (Art, Audio, Scripts, Prefabs, ScriptableObjects,...) để quản lý tài nguyên.
- **Thêm mới**: File `.gitignore` chuẩn của Unity.
- **Thêm mới**: Script nền tảng `GameManager.cs` (Singleton) dùng quản lý trạng thái và chuyển Scene.
- **Lưu trữ**: Khởi tạo Git và liên kết với remote GitHub repository.
