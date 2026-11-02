# ThiTap — Bộ gõ tiếng Việt cho Windows

Bộ gõ tiếng Việt cá nhân, chạy dưới khay hệ thống, viết bằng C# / WinForms cho .NET Framework 4.x.
Hỗ trợ **Telex** và **VNI**, xuất **Unicode dựng sẵn**, có gõ tắt, tự khôi phục tiếng Anh và bật/tắt riêng theo từng ứng dụng.

- **Tác giả:** Ông Thi
- **Giấy phép:** MIT (xem [LICENSE](LICENSE))

## Tính năng

- **Kiểu gõ Telex và VNI**, chuyển qua lại trong hộp thoại cấu hình.
- **Bảng mã Unicode dựng sẵn** — dán được vào mọi ứng dụng hiện đại.
- **Gõ tắt** theo bảng khoá → nội dung, mở rộng khi gõ dấu cách hoặc dấu câu.
- **Tự động khôi phục phím tiếng Anh**: gõ `test` không bị biến thành `tést`.
- **Luôn dùng clipboard cho Unicode** cho ứng dụng không nhận `SendInput` Unicode.
- **Bật/tắt theo từng ứng dụng**: khai báo danh sách tiến trình không muốn gõ tiếng Việt.
- **Chuyển tiếng Việt / English** bằng `Ctrl + Shift`, `Alt + Z` hoặc bấm chuột trái vào biểu tượng ở khay.
- **Biểu tượng khay đổi theo chế độ**: cờ Việt Nam (đỏ, sao vàng) khi gõ tiếng Việt, cờ Liên hiệp Anh (Union Jack) khi dùng tiếng Anh.
- **Khởi động cùng Windows** bằng một mục trong menu khay.
- **Cấu hình portable**: `settings.ini` nằm cạnh `ThiTap.exe` (nếu không ghi được thì dùng `%APPDATA%\ThiTap`).

## Bảng phím

### Telex

| Phím | Dấu |
| --- | --- |
| `s` | sắc |
| `f` | huyền |
| `r` | hỏi |
| `x` | ngã |
| `j` | nặng |
| `z` | bỏ dấu vừa đặt |
| `aa` `ee` `oo` | â ê ô |
| `w` | ư / ơ / ă (bấm `ww` để ra chữ `w`) |
| `dd` | đ |
| `uo` + `w` | ươ (`nguowif` → người) |

Ví dụ: `tieengs Vieetj` → tiếng Việt.

### VNI

| Phím | Dấu |
| --- | --- |
| `1` `2` `3` `4` `5` | sắc, huyền, hỏi, ngã, nặng |
| `6` | â / ê / ô |
| `7` | ư / ơ (`uo` + `7` → `ươ`) |
| `8` | ă |
| `9` | đ |
| `0` | bỏ toàn bộ dấu của từ |

Ví dụ: `tie6ng1 Vie6t5` → tiếng Việt, `d9u7o7ng2` → đường.

## Sử dụng

1. Tải `ThiTap.exe` (hoặc tự build, xem bên dưới) và đặt vào một thư mục bất kỳ.
2. Chạy `ThiTap.exe` — biểu tượng ở khay hệ thống là cờ Việt Nam (đỏ, sao vàng); khi chuyển sang English, biểu tượng đổi thành cờ Liên hiệp Anh (Union Jack).
3. Bấm chuột trái vào biểu tượng để chuyển nhanh giữa tiếng Việt và English.
4. Bấm chuột phải để mở **Cấu hình...**, bật **Khởi động cùng Windows** hoặc **Thoát**.

ThiTap không cài đặt vào hệ thống: muốn dùng ở máy khác chỉ cần copy cả thư mục (file exe và `settings.ini`).

## Cấu hình

### `settings.ini`

File được tạo tự động bên cạnh `ThiTap.exe` khi chạy lần đầu, có thể sửa tay khi ứng dụng đã tắt:

| Khoá | Ý nghĩa |
| --- | --- |
| `enabled` | 1 = đang gõ tiếng Việt, 0 = English |
| `inputMethod` | `telex` hoặc `vni` |
| `hotkey` | `ctrl_shift` hoặc `alt_z` |
| `freeTyping` | 1 = bỏ dấu cả khi âm tiết không hợp lệ |
| `spellCheck` | 1 = kiểm tra chính tả |
| `oldStyleTone` | 1 = đặt dấu oà, uý thay vì oá, úy |
| `macroEnabled` | 1 = bật gõ tắt |
| `macroWhenOff` | 1 = gõ tắt cả khi đang ở chế độ English |
| `autoRestore` | 1 = tự khôi phục phím tiếng Anh |
| `clipboardMode` | 1 = luôn dán qua clipboard |
| `appFilter` | 1 = bật/tắt theo danh sách ứng dụng |
| `beepOnSwitch` | 1 = bíp khi chuyển ngôn ngữ |
| `showDialogOnStart` | 1 = mở hộp thoại cấu hình mỗi lần khởi động |

### `macros.txt` (gõ tắt)

Mỗi dòng một cặp `khoá=nội dung`, dòng bắt đầu bằng `#` là ghi chú:

```
# ví dụ
vn=Việt Nam
ko=không
```

Sửa trực tiếp trong hộp thoại **Cấu hình → Bảng gõ tắt...**.

### `apps.txt` (bật/tắt theo ứng dụng)

Mỗi dòng một tên tiến trình cần tắt tiếng Việt, ví dụ `game.exe`. Quản lý bằng **Cấu hình → Chọn...**.

## Build từ mã nguồn

Yêu cầu: Windows 10/11 với .NET Framework 4.x (đã có sẵn), không cần cài .NET SDK.

```powershell
.\build.ps1        # build -> bin\ThiTap.exe
.\build.ps1 -Test  # build và chạy bộ kiểm thử lõi gõ
.\build.ps1 -Run   # build rồi chạy
```

Script dùng `csc.exe` của .NET Framework và tự thêm BOM UTF-8 cho mọi file `.cs` (nếu thiếu BOM, `csc.exe` sẽ đọc theo bảng mã ANSI và làm hỏng chữ tiếng Việt).

## Kiểm thử

`bin\TelexTest.exe` (do `build.ps1 -Test` sinh ra) chạy hoàn toàn ở mức lõi gõ, không cần bàn phím thật:

- 83 từ và 4 câu Telex;
- 30 từ và 2 câu VNI;
- gõ tắt (mở rộng đúng, không mở rộng khi tắt);
- khôi phục phím tiếng Anh (có và không được khôi phục).

Kết quả in `TẤT CẢ ĐỀU ĐÚNG` khi mọi ca đều qua.

## Cấu trúc thư mục

```
build.ps1            build bằng csc.exe, sinh icon và chạy kiểm thử
assets\ThiTap.ico    icon nhúng trong file exe
src\Engine\          lõi gõ: TelexEngine, bảng chữ Vietnamese, danh sách từ tiếng Anh
src\Input\           hook bàn phím (WH_KEYBOARD_LL) và bộ gửi phím
src\                 host WinForms: khay hệ thống, hộp thoại cấu hình, cài đặt, tự khởi động
tools\               TelexTest (kiểm thử), MakeIcon (sinh icon)
```

## Hạn chế đã biết

- Chỉ xuất bảng mã **Unicode dựng sẵn**; chưa có TCVN3 hay VNI-Windows.
- Chỉ có hai kiểu gõ Telex và VNI.
- Chỉ chạy trên Windows (hook bàn phím và WinForms).
- Cửa sổ chạy với quyền quản trị chỉ nhận gõ tiếng Việt khi ThiTap cũng chạy với quyền quản trị.

## Giấy phép

Phát hành theo giấy phép MIT — xem [LICENSE](LICENSE).
