# Changelog - Rent Is Due

Tất cả những thay đổi, cập nhật tính năng mới của dự án sẽ được ghi chú tại đây.

## [MVP Development]

### Phase 06 - Selling & Economy (Hoàn thành)
- **Thêm mới**: Lớp `EconomyManager.cs` lưu trữ số dư tiền tệ, hỗ trợ hàm bán đồ và tính toán giá trị (kết hợp `InventoryManager`).
- **Thêm mới**: `DealerInteractable.cs` cài đặt giao diện `IInteractable`, đóng vai trò là điểm thu mua (khi nhấn E sẽ bán sạch đồ trong túi).
- **Thêm mới**: `EconomyUI.cs` vẽ UI hiển thị số dư tài khoản ở góc trên bên phải màn hình.

### Phase 05 - Loot & Search (Hoàn thành)
- **Thêm mới**: ScriptableObject `LootTable.cs` để cấu hình danh sách vật phẩm rơi ra với tỉ lệ tương ứng (bao gồm tỉ lệ rơi rỗng/trượt).
- **Thêm mới**: `SearchableObject.cs` cài đặt giao diện `IInteractable`, mô phỏng hành động "lục lọi" có thời gian chờ (delay) và tự động spawn ra model vật phẩm sau khi lục xong.

### Phase 04 - Item, Pickup & Inventory (Hoàn thành)
- **Thêm mới**: ScriptableObject `ItemData.cs` định nghĩa dữ liệu cho vật phẩm (Tên, Độ hiếm, Giá trị, Cân nặng...).
- **Thêm mới**: Lớp quản lý `InventoryManager.cs` giới hạn túi đồ 8 slot và tải trọng 20kg.
- **Thêm mới**: `PickupInteractable.cs` cài đặt giao diện `IInteractable`, cho phép nhặt vật thể đưa vào kho và xóa model 3D trên Scene.
- **Thêm mới**: Lớp hiển thị nhanh `InventoryUI.cs` dùng GUI để hiện danh sách đồ trên màn hình và debug.

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
