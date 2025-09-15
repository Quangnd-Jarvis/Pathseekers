# TilePathGame - Tài liệu Thiết kế Game

## 1. Tổng quan Hệ thống

### 1.1. Mục tiêu Game
- **Mục tiêu chính**: Đặt tiles để phủ kín hoàn toàn map được sinh ngẫu nhiên
- **Thể loại**: Puzzle game với cơ chế tetris-like trên map 2D
- **Đối tượng người chơi**: Casual gamers thích thử thách logic và không gian

### 1.2. Core Gameplay Loop
1. Sinh map ngẫu nhiên có kết nối đầy đủ
2. Hiển thị 3 tiles ngẫu nhiên cho người chơi chọn
3. Người chơi đặt tile lên map (drag & drop hoặc click)
4. Kiểm tra placement hợp lệ và cập nhật map state
5. Lặp lại cho đến khi map được phủ kín hoàn toàn
6. Tăng level và sinh map mới với độ khó cao hơn

## 2. Các Thành phần Chính

### 2.1. Map System
**Chức năng**: Quản lý việc sinh và hiển thị map game

**Components**:
- `MapGenerator`: Sinh map ngẫu nhiên theo level
- `MapRenderer`: Hiển thị map và tiles đã đặt
- `MapValidator`: Kiểm tra tính hợp lệ và kết nối của map

**Thuật toán sinh map**:
- Số ô map = 4 + Level
- Đảm bảo tất cả ô đều kết nối với nhau
- Tránh tạo map không thể giải được

### 2.2. Tile System
**Chức năng**: Quản lý 15 loại tiles và tiles nhiễu loạn

**Components**:
- `TileData` (ScriptableObject): Lưu trữ shape và properties của từng tile
- `TilePool`: Object pooling cho tiles UI
- `TileSelector`: Random 3 tiles cho người chơi
- `TileRenderer`: Hiển thị tile trên UI và map

**15 loại tiles cơ bản**:
- Straight tiles (4 loại): Ngang, dọc, góc 4 hướng
- L-shaped tiles (4 loại): L trong 4 hướng xoay
- T-shaped tiles (4 loại): T trong 4 hướng xoay 
- Cross tile (1 loại): Giao nhau 4 hướng
- Complex tiles (2 loại): Hình dạng đặc biệt

### 2.3. Placement System
**Chức năng**: Xử lý logic đặt tiles lên map

**Components**:
- `PlacementValidator`: Kiểm tra vị trí đặt hợp lệ
- `PlacementHandler`: Xử lý input và đặt tile
- `UndoSystem`: Hỗ trợ undo placement (tùy chọn)

**Validation Rules**:
- Tile phải nằm trong boundaries của map
- Không được overlap với tiles đã đặt
- Phải maintain connectivity của map

### 2.4. Level System
**Chức năng**: Quản lý progression và difficulty scaling

**Components**:
- `LevelManager`: Theo dõi level hiện tại và điều kiện thắng
- `DifficultyScaler`: Tăng độ khó theo level
- `ScoreCalculator`: Tính điểm dựa trên hiệu quả

**Scaling Factors**:
- Map size tăng theo công thức: 4 + Level
- Thêm tiles nhiễu loạn ở level cao
- Giảm thời gian suy nghĩ (nếu có time limit)

### 2.5. Health System
**Chức năng**: Quản lý 5 máu và game over logic

**Components**:
- `HealthManager`: Theo dõi máu hiện tại
- `GameOverHandler`: Xử lý logic khi hết máu
- `HealthUI`: Hiển thị health bar

**Health Loss Conditions**:
- Đặt tile không hợp lệ (-1 máu)
- Không thể hoàn thành map với tiles hiện tại (-1 máu)
- Time out (nếu có time limit) (-1 máu)

## 3. Tương tác giữa các Thành phần

### 3.1. Game Flow Architecture
```
GameManager
├── MapSystem
│   ├── MapGenerator
│   ├── MapRenderer
│   └── MapValidator
├── TileSystem
│   ├── TileSelector
│   ├── TileRenderer
│   └── TilePool
├── PlacementSystem
│   ├── PlacementValidator
│   └── PlacementHandler
├── LevelSystem
│   └── LevelManager
└── HealthSystem
    └── HealthManager
```

### 3.2. Data Flow
1. `LevelManager` → `MapGenerator`: Cung cấp level info để sinh map
2. `MapGenerator` → `MapRenderer`: Truyền map data để render
3. `TileSelector` → `TileRenderer`: Hiển thị 3 tiles cho người chơi
4. `PlacementHandler` → `PlacementValidator`: Kiểm tra placement hợp lệ
5. `PlacementValidator` → `MapRenderer`: Cập nhật map state
6. `LevelManager` → `HealthManager`: Kiểm tra điều kiện game over

## 4. Yêu cầu Chức năng

### 4.1. Core Features
- [x] Random map generation với kết nối đầy đủ
- [x] 15 loại tiles cơ bản + tiles nhiễu loạn
- [x] Drag & drop hoặc click placement system
- [x] Real-time validation khi đặt tiles
- [x] Health system với 5 máu
- [x] Level progression với difficulty scaling
- [x] Game over và restart functionality

### 4.2. Enhanced Features (Optional)
- [ ] Undo/Redo system cho placement
- [ ] Hint system gợi ý placement tốt
- [ ] Save/Load game state
- [ ] Multiple color themes
- [ ] Sound effects và background music
- [ ] Achievement system
- [ ] Leaderboard và high scores

## 5. Yêu cầu Phi chức năng

### 5.1. Performance
- **Target FPS**: 60 FPS trên mobile devices
- **Memory Usage**: < 100MB cho game session
- **Load Time**: < 3 giây để sinh map mới
- **Object Pooling**: Cho tất cả tiles UI để tránh GC

### 5.2. Platform Support
- **Primary**: Android/iOS (Unity Mobile)
- **Secondary**: PC/WebGL build
- **Resolution**: Responsive UI cho multiple screen sizes

### 5.3. Code Quality
- Tuân thủ C# Coding Standards đã định nghĩa
- SOLID principles cho architecture
- Data-driven design với ScriptableObjects
- Async operations với UniTask
- Performance optimization với object pooling

## 6. Ràng buộc Kỹ thuật

### 6.1. Unity Version
- **Unity Version**: 2022.3 LTS hoặc mới hơn
- **Rendering Pipeline**: URP cho performance tốt hơn
- **Input System**: New Input System cho cross-platform

### 6.2. External Dependencies
- **UniTask**: Cho async operations
- **DOTween**: Cho animations (optional)
- **Newtonsoft JSON**: Cho save/load system

### 6.3. Architecture Patterns
- **MVC Pattern**: Cho UI management
- **Observer Pattern**: Cho event-driven communication
- **Object Pool Pattern**: Cho memory management
- **Factory Pattern**: Cho tile creation

## 7. Môi trường Triển khai

### 7.1. Development Environment
- **IDE**: Visual Studio 2022 hoặc JetBrains Rider
- **Version Control**: Git với GitLFS cho assets
- **Build Automation**: Unity Cloud Build hoặc GitHub Actions

### 7.2. Testing Strategy
- **Unit Tests**: Cho core game logic
- **Integration Tests**: Cho component interactions
- **Playtesting**: Manual testing trên target devices
- **Performance Testing**: Memory và frame rate profiling

## 8. Timeline và Milestones

### Phase 1: Core Systems (2 tuần)
- Map generation và rendering
- Basic tile system với 15 loại tiles
- Placement system cơ bản

### Phase 2: Game Logic (1 tuần)
- Level progression system
- Health management
- Win/Lose conditions

### Phase 3: Polish và Optimization (1 tuần)
- UI/UX improvements
- Performance optimization
- Bug fixes và testing

### Phase 4: Enhanced Features (Optional)
- Advanced features dựa trên feedback
- Platform-specific optimizations
- Store submission preparation

---

*Tài liệu này sẽ được cập nhật thường xuyên trong quá trình development dựa trên feedback và requirements thay đổi.*
