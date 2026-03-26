# 🍚 Cơm Tấm Huyền Thoại

> Game mô phỏng bán cơm tấm đường phố Việt Nam — góc nhìn thứ nhất (FPS).  
> Phát triển bởi **Group 5**.

---

## 1. Setup Instructions (Hướng dẫn cài đặt)

### Phiên bản Engine

| Thông tin          | Giá trị                                |
| ------------------ | -------------------------------------- |
| Engine             | **Unity 6.3 LTS**                      |
| Phiên bản chính xác | `6000.3.10f1`                          |
| Revision           | `e35f0c77bd8e`                         |
| Template gốc       | `com.unity.template.3d@9.1.0`          |

### Cách mở project

1. Mở **Unity Hub** (phiên bản mới nhất).
2. Bấm **Add** → **Add project from disk** → chọn thư mục gốc `com-tam-huyen-thoai/`.
3. Đảm bảo Unity Hub đã cài đặt phiên bản **6000.3.10f1**. Nếu chưa có, bấm **Install** tại mục phiên bản trong Unity Hub.
4. Chờ Unity import toàn bộ asset (lần đầu có thể mất 5–10 phút do project có nhiều file FBX và texture PBR dung lượng lớn).

### Cách chạy trong Editor

| Bước | Thao tác |
| ---- | -------- |
| 1    | Mở scene chính: `Assets/Scenes/GameScene.unity` |
| 2    | Bấm nút **▶ Play** trên Editor |
| 3    | Đọc bảng hướng dẫn Tutorial → bấm phím **T** để bắt đầu Ngày 1 |

> **Lưu ý:** Scene `MainMenu.unity` là menu chính (scene index 0 trong Build). Khi build thành game, người chơi sẽ vào MainMenu trước → bấm Play → xem Intro → vào GameScene. Nhưng khi **debug trong Editor**, mở thẳng `GameScene.unity` để tiết kiệm thời gian.

### Thứ tự Scene (Build Settings)

| Index | Scene                            | Mô tả                        |
| ----- | -------------------------------- | ----------------------------- |
| 0     | `Assets/Scenes/MainMenu.unity`   | Menu chính (Play / Options / Quit) |
| 1     | `Assets/Scenes/IntroScene.unity` | Cutscene giới thiệu câu chuyện |
| 2     | `Assets/Scenes/GameScene.unity`  | Scene gameplay chính          |

---

## 2. Dependencies (Thư viện & Package)

### Unity Packages (trong `Packages/manifest.json`)

| Package | Phiên bản | Mục đích |
| ------- | --------- | -------- |
| **Input System** | `1.18.0` | Xử lý input (WASD, Mouse, E tương tác) — dùng `PlayerInput` component |
| **AI Navigation** | `2.0.11` | NavMesh cho NPC đi bộ trên đường và tìm ghế ngồi |
| **glTFast** | `6.16.1` | Import model 3D định dạng glTF |
| **Timeline** | `1.8.10` | Hỗ trợ cắt cảnh / animation timeline |
| **Visual Scripting** | `1.9.9` | Tích hợp sẵn (không sử dụng nhiều trong code) |
| **uGUI** | `2.0.0` | UI hệ thống (Canvas, Button, Slider, Image) |
| **TextMesh Pro** | *(tích hợp sẵn Unity 6)* | Hiển thị text UI (dialogue, menu, HUD tiền) |
| **2D Sprite** | `1.0.0` | Hỗ trợ sprite 2D |
| **Particle System** | *(module sẵn)* | VFX khói nướng thịt, hiệu ứng sao đêm |

### Các Module Unity quan trọng đang bật

- `com.unity.modules.ai` — AI / NavMesh
- `com.unity.modules.physics` — Physics 3D (Raycast, Collider, CharacterController)
- `com.unity.modules.animation` — Animation
- `com.unity.modules.audio` — Âm thanh 3D (tiếng bước chân, nấu ăn)
- `com.unity.modules.particlesystem` — Particle (khói, sao trời)
- `com.unity.modules.terrain` — Terrain

### Plugin / SDK bên ngoài

> ⚠️ **Hiện tại project KHÔNG sử dụng SDK bên thứ ba** (không có Firebase, Ads, Steamworks, Photon, DOTween…).  
> Toàn bộ logic được viết bằng C# thuần trên Unity built-in packages.

---

## 3. Export / Build Settings (Cấu hình Build)

### Nền tảng Build

| Thông tin              | Giá trị                                 |
| ---------------------- | --------------------------------------- |
| **Platform**           | **Standalone (Windows x64)**            |
| Application Identifier | `com.DefaultCompany.3D-Project`         |
| Company Name           | `Group 5`                               |
| Product Name           | `Cơm Tấm Huyền Thoại`                  |
| Bundle Version         | `1.0.0`                                 |
| Default Resolution     | `1920 × 1080`                           |
| Fullscreen Mode        | Fullscreen Window (có thể chuyển Alt+Enter) |
| Active Color Space     | **Linear** (`m_ActiveColorSpace: 1`)    |
| Active Input Handler   | **New Input System only** (`activeInputHandler: 1`) |

### Scripting Backend

| Thông tin | Giá trị |
| --------- | ------- |
| Scripting Backend (Standalone) | **Mono** (mặc định — không set IL2CPP cho Standalone) |
| Scripting Backend (Android) | IL2CPP (`scriptingBackend.Android: 1`) |
| API Compatibility | .NET Standard 2.1 (`apiCompatibilityLevel: 6`) |

### Cấu hình Build khuyến nghị

1. **File → Build Settings** → chọn **Windows, Mac, Linux** → **Architecture: x86_64**.
2. **Scene build list** (đúng thứ tự):
   - `[0]` MainMenu
   - `[1]` IntroScene  
   - `[2]` GameScene
3. **Development Build**: 
   - ✅ **Bật** khi test (để xem Debug.Log trong Console và Player.log).
   - ❌ **Tắt** khi build bản phát hành (release) để tối ưu hiệu năng.
4. Bấm **Build** → chọn thư mục output (khuyến nghị: `Build/` trong thư mục project).
5. Chạy file `.exe` được sinh ra.

### Thư mục output build khuyến nghị

```
com-tam-huyen-thoai/
├── Build/                    ← Thư mục output build (tạo mới)
│   ├── ComTamHuyenThoai.exe  ← File chạy game
│   ├── ComTamHuyenThoai_Data/
│   ├── UnityCrashHandler64.exe
│   └── UnityPlayer.dll
```

---

## 4. Usage Notes (Hướng dẫn sử dụng)

### Điều khiển (Controls)

| Phím / Input     | Hành động                             |
| ---------------- | ------------------------------------- |
| `W A S D`        | Di chuyển (đi bộ)                     |
| `Shift` (giữ)   | Chạy nhanh (sprint)                   |
| `Space`          | Nhảy                                  |
| `Mouse`          | Xoay camera (nhìn xung quanh)         |
| `E`              | Tương tác (nhặt nguyên liệu, đặt lên bếp, lấy thành phẩm, đặt đĩa lên bàn) |
| `T`              | Đóng bảng Tutorial và bắt đầu game    |
| `Click chuột / Space / Enter` | Chuyển trang trong Intro Cutscene |

### Cách chơi cơ bản

1. **Bắt đầu**: Đọc Tutorial → bấm `T`. Game bắt đầu Ngày 1/10 với sự kiện ngẫu nhiên.
2. **Nấu ăn**:
   - Lấy **thịt sống** từ hộp thịt → đặt lên **Lò Nướng** → chờ timer → lấy **thịt chín**.
   - Lấy **trứng sống** từ vĩ trứng → đặt lên **Chảo** → chờ timer → lấy **trứng chiên**.
   - Lấy **cơm** từ nồi cơm, **đĩa** từ rổ đĩa.
3. **Ghép đĩa**: Cầm đĩa trống → bấm `E` vào bếp có thành phẩm để thêm nguyên liệu lên đĩa.
4. **Phục vụ**: Mang đĩa đến **bàn phục vụ** (Serving Table) cho khách đang chờ.
5. **Thu tiền**: Khách ăn xong sẽ tự trả tiền. Giá cơ bản: **35.000đ/đĩa**.
6. **Kết thúc ngày**: Chu kỳ ngày đêm 20 phút thực → hết ngày → bảng tổng kết thu chi → tiếp tục.
7. **Kết thúc game**: Sau 10 ngày, hiện bảng kết quả cuối cùng.

### Hệ thống kinh tế

| Thông số            | Giá trị mặc định |
| -------------------- | ----------------- |
| Giá bán / đĩa       | 35.000đ           |
| Chi phí vận hành / ngày | 500.000đ       |
| Vốn ban đầu         | 0đ                |

### Hệ thống sự kiện hàng ngày

| Sự kiện       | Loại | Mô tả |
| ------------- | ---- | ----- |
| Công an phạt  | ❌ Xấu | Mất 50.000đ (ngày 5, 9) |
| Bảo kê        | ❌ Xấu | Mất 30.000đ (ngày chẵn: 2, 4, 6, 8, 10) |
| x2 Tiền       | ✅ Tốt | Nhân đôi tiền nhận mỗi đĩa (ngày 1 cố định, random từ ngày 5) |
| Giảm giá      | ✅ Tốt | Giá bán giảm xuống 25.000đ (random 15% từ ngày 5) |

### Hệ thống NPC khách hàng

- NPC tự động spawn trên đường đi bộ (NavMesh Agent).
- Xác suất rẽ vào ăn: **30%** (mặc định).
- NPC ngồi xuống ghế → hiện bubble gọi món:
  - **Cơm Sườn** (không trứng)
  - **Cơm Sườn Trứng**
- Thanh kiên nhẫn (patience bar): tối đa **60 giây**.
  - Phục vụ sau khi thanh qua nửa → bị trừ **50% tiền**.
  - Hết thanh → khách bỏ đi, không trả tiền.
- Gọi trứng mà thiếu trứng → khách trừ **50% tiền đĩa**.

### Loại nguyên liệu trong game

| Enum | Tên | Nguồn |
| ---- | --- | ----- |
| `Com` | Cơm | Nồi cơm |
| `ThitSong` | Thịt sống | Hộp thịt ướp sống |
| `ThitChin` | Thịt chín | Nướng trên Lò Nướng |
| `TrungSong` | Trứng sống | Vĩ trứng |
| `TrungChien` | Trứng chiên | Chiên trên Chảo |
| `Dia` | Đĩa trống | Rổ đĩa |

### Known Issues (Lỗi đã biết)

1. **MissingReferenceException khi hot-reload**: Nếu sửa script khi đang Play, Unity hot-reload có thể gây lỗi `MissingReferenceException` do interface `IInteractable` không đi qua Unity null check. Workaround: dừng Play trước khi sửa code.

2. **NPC floating/lún ghế**: Offset ngồi ghế (`sitYOffset = -0.15f`) trong `CustomerNPC.cs` có thể cần chỉnh tùy model ghế. Thay đổi giá trị tại dòng `float sitYOffset = -0.15f;`.

3. **Khói VFX không hiện**: Nếu chưa kéo thả Particle System vào trường `Smoke VFX` trong Inspector của CookingStation, sẽ có warning trong Console. Đảm bảo đã gán đúng component.

4. **MainMenu scene tham chiếu "DemoScene"**: Script `MainMenu.cs` mặc định load scene tên `"DemoScene"` qua trường `gameplaySceneName`. Cần đổi thành `"IntroScene"` trong Inspector nếu muốn đi qua Intro, hoặc `"GameScene"` để nhảy thẳng vào gameplay.

5. **Model 3D scale lớn**: Các model FBX (Lò Nướng, Chảo, Bếp Ga…) có thể có scale rất lớn (~100). Script `TransformUtils.cs` đã xử lý bù trừ lossy scale, nhưng nếu thêm model mới cần chú ý vấn đề này.

---

## Cấu trúc thư mục chính

```
com-tam-huyen-thoai/
├── Assets/
│   ├── Animations/              # Animation clips & controllers
│   ├── Characters/              # Model nhân vật
│   ├── Materials/               # Materials chung
│   ├── Meshes,textures/         # Mesh & texture bổ sung
│   ├── Prefabs/                 # Prefab game objects
│   ├── Scenes/                  # 3 scene chính
│   │   ├── MainMenu.unity
│   │   ├── IntroScene.unity
│   │   └── GameScene.unity
│   ├── TextMesh Pro/            # TMP assets
│   ├── UI/                      # UI sprites & assets
│   └── XeComTam/                # ★ THƯ MỤC CHÍNH CỦA GAME
│       ├── Scripts/             # Toàn bộ C# scripts (~30 files)
│       │   ├── GameLoopManager.cs      # Vòng lặp 10 ngày
│       │   ├── DayNightCycle.cs        # Chu kỳ ngày đêm (20 phút)
│       │   ├── FPSLayerController.cs   # Điều khiển FPS (WASD/Mouse)
│       │   ├── PlayerInteraction.cs    # Raycast tương tác (E)
│       │   ├── PlayerInventory.cs      # Hệ thống cầm đồ
│       │   ├── CookingStation.cs       # Trạm nấu ăn (timer + VFX)
│       │   ├── CustomerNPC.cs          # NPC: Spawner + Walker + Customer
│       │   ├── EconomyManager.cs       # Quản lý tiền
│       │   ├── DailyEventManager.cs    # Sự kiện ngẫu nhiên hàng ngày
│       │   ├── ServingTable.cs         # Bàn phục vụ
│       │   ├── PlateItem.cs            # Logic đĩa cơm
│       │   └── ...
│       ├── Sound/               # Audio clips
│       ├── Building/            # Model kiến trúc
│       ├── NguyenLieu/          # Model nguyên liệu
│       └── *.fbx / *.png        # Model 3D (FBX) & Texture PBR
├── Packages/                    # Package manifest
├── ProjectSettings/             # Cấu hình project Unity
└── README.md                    # ← File này
```

---

## Thông tin liên hệ

- **Nhóm phát triển**: Group 5
- **Engine**: Unity 6.3 LTS (`6000.3.10f1`)
- **Ngôn ngữ lập trình**: C#
- **Thể loại**: Simulation / Cooking / FPS
