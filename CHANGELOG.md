# Changelog - Rent Is Due

Tất cả những thay đổi, cập nhật tính năng mới của dự án sẽ được ghi chú tại đây.

## [Post-MVP Development]

### Phase 12 - Economy & Gameplay Balance (Hoàn thành)
- **Thêm mới**: Hệ thống Thể lực `StaminaSystem.cs` hỗ trợ Chạy nhanh (Sprint bằng `LeftShift` nhanh hơn 55%), tiêu hao thể lực khi nhảy và vẽ thanh Thể lực `⚡ STAMINA` mượt mà trên HUD.
- **Thêm mới**: Hệ thống Quản lý Vay nợ khẩn cấp `DebtManager.cs` cho phép người chơi vay tiền chủ nhà khi thiếu tiền nhà ở hạn chót 22:00 với lãi suất 20% thay vì bị Game Over ngay lập tức.
- **Cập nhật**: Tích hợp toàn diện `DaySummaryUI`, `PlayerMovement`, `DayManager`, `SaveManager` với hệ thống Thể lực và Nợ nần.

### Phase 11 - Vertical Slice (Hoàn thành)
- **Thêm mới**: Hệ thống quản lý âm thanh `AudioManager.cs` với âm thanh tổng hợp đa âm (Procedural Audio Synthesizer) cho nhặt đồ, bán đồ (Ka-ching), bới rác, qua ngày và Game Over mà không cần file audio ngoài.
- **Thêm mới**: Giao diện thông báo nổi `FloatingFeedbackUI.cs` hiển thị chữ tiền bay `+$XX`, cảnh báo đầy túi/quá tải và thông báo ngày mới mượt mà.
- **Thêm mới**: Bảng tổng kết cuối ngày `DaySummaryUI.cs` hiển thị tiền nhà đã trả, tiền tiết kiệm còn lại, cảnh báo tiền nhà ngày mai và màn hình Game Over khi không đủ tiền.
- **Cập nhật**: Tích hợp toàn diện `EconomyManager`, `PickupInteractable`, `DayManager` và `PlayerInteractor` với hệ thống phản hồi âm thanh & hình ảnh.

### Feature - Categorized Loot Tables & Searchable Containers (Hoàn thành)
- **Thêm mới**: Công cụ `LootTableGenerator.cs` tự động phân loại 30 item và sinh ra 5 bảng Loot Table chuyên biệt: `TrashLootTable`, `DeskLootTable`, `KitchenLootTable`, `WardrobeLootTable`, `SecretSafeLootTable`.
- **Cập nhật**: Nâng cấp `SearchableObject.cs` hỗ trợ đặt tên hòm/tủ riêng biệt (`containerName`), theo dõi tiến độ bới rác theo thời gian thực (0% - 100%), và hiển thị thông tin rớt đồ sinh động.

## [MVP Development]

### Feature - First-Person Controller & Jump (Hoàn thành)
- **Cập nhật**: Chuyển đổi `PlayerMovement.cs` sang góc nhìn thứ nhất (di chuyển theo hướng nhìn của nhân vật) và thêm cơ chế Nhảy (Jump) bằng phím `Space`.
- **Cập nhật**: Nâng cấp `CameraController.cs` thành First-Person Mouse Look Controller: xoay thân (Yaw), gật đầu (Pitch giới hạn -85° đến 85°), tự động khóa/mở khóa con trỏ chuột khi chơi/tạm dừng.
- **Cập nhật**: Nâng cấp `PlayerInteractor.cs` hỗ trợ Raycast từ tâm mắt và hiển thị tâm ngắm (Crosshair) + gợi ý tương tác `[E]` ngay giữa màn hình.

### Phase 10 - MVP Polish & Playtest (Hoàn thành)
- **Thêm mới**: `PauseMenu.cs` quản lý trạng thái đóng băng thời gian của game, cho phép bấm phím ESC để mở Menu tạm dừng với các nút Resume và Quit.
- **Tổng kết**: Hoàn thành vòng lặp cốt lõi (Core Loop) của dự án. Trò chơi đã sẵn sàng để Build và gửi cho Tester chơi thử.

### Phase 09 - Save, Load & Game States (Hoàn thành)
- **Thêm mới**: Lớp dữ liệu `SaveData.cs` để đóng gói thông tin (Ngày, Tiền, Các cấp độ nâng cấp).
- **Thêm mới**: `SaveManager.cs` xử lý việc lưu và tải dữ liệu dưới dạng file JSON vào thư mục hệ thống (persistentDataPath). Đồng thời tự động áp dụng lại các thông số nâng cấp khi Load.
- **Thêm mới**: `SaveUI.cs` vẽ các nút Save/Load ở góc dưới màn hình để test.
- **Cập nhật**: Sửa `DayManager.cs` để tự động kích hoạt tính năng Auto Save mỗi khi người chơi sống sót qua một ngày mới.

### Phase 08 - Upgrade & Progression (Hoàn thành)
- **Thêm mới**: `UpgradeManager.cs` lưu trữ các chỉ số cấp độ nâng cấp (Balo, Tốc độ chạy, Tốc độ lục lọi).
- **Thêm mới**: `UpgradeInteractable.cs` cài đặt giao diện tương tác để người chơi mở cửa hàng nâng cấp.
- **Thêm mới**: `UpgradeUI.cs` vẽ bảng giao diện Cửa hàng bằng OnGUI, cho phép mua và áp dụng ngay lập tức các chỉ số nâng cấp vào nhân vật.
- **Cập nhật**: Sửa code `SearchableObject.cs` để tự động giảm thời gian chờ dựa vào chỉ số Tốc độ lục lọi hiện tại của người chơi.

### Phase 07 - Time, Rent & Day Cycle (Hoàn thành)
- **Thêm mới**: `TimeManager.cs` điều khiển thời gian trong game chạy từ 08:00 sáng đến 22:00 đêm.
- **Thêm mới**: `DayManager.cs` theo dõi ngày hiện tại, tự động tăng tiền thuê nhà theo cấp số nhân (hệ số 1.25) và kiểm tra ví tiền (EconomyManager) vào lúc 22:00 mỗi ngày để quyết định qua ngày hoặc Game Over.
- **Thêm mới**: `TimeUI.cs` vẽ UI hiển thị thông tin thời gian, ngày hiện tại và số tiền nhà phải nộp lên góc màn hình.

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
