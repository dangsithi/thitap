# ThiTap — Bộ gõ tiếng Việt cho Windows

Bộ gõ tiếng Việt cá nhân, chạy dưới khay hệ thống, viết bằng C# / WinForms cho .NET Framework 4.x.
Hỗ trợ **Telex** và **VNI**, xuất **Unicode dựng sẵn**, có gõ tắt, tự khôi phục tiếng Anh và bật/tắt riêng theo từng ứng dụng.

- **Tác giả:** Ông Thi
- **Giấy phép:** MIT (xem [LICENSE](LICENSE))

## Tải về

Bản chạy sẵn (không cần cài đặt): [ThiTap 1.0 — ThiTap-1.0-win64.zip](https://github.com/dangsithi/thitap/releases/latest)

## Tính năng

- **Kiểu gõ Telex và VNI**, chuyển qua lại trong hộp thoại cấu hình.
- **Bảng mã Unicode dựng sẵn** — dán được vào mọi ứng dụng hiện đại.
- **Gõ tắt** theo bảng khoá → nội dung, mở rộng khi gõ dấu cách hoặc dấu câu.
- **Tự động khôi phục phím tiếng Anh**: gõ `test` không bị biến thành `tést`.
- **Xoá lui rồi gõ tiếp vẫn đúng**: bấm Backspace giữa chữ (vd. `hội` → `hộ`) thì phím dấu gõ tiếp theo đặt lại trên chữ còn lại, không bị dính `j`, `s`, `f`... ra ngoài.
- **Đặt dấu muộn vẫn đúng**: `w` gõ sau phụ âm cuối vẫn đổi được nguyên âm của âm tiết (`ungws` → `ứng`, `thuongw` → `thương`).
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
| `[` `]` | gõ nhanh ư và ơ (`th[` → thư, `c]` → cơ) |
| `uo` + `w` | ươ (`nguowif` → người) |

gõ `w` muộn, sau cả phụ âm cuối, vẫn đổi đúng nguyên âm của âm tiết: `ungw` → ưng, `ungws` → ứng, `thuongw` → thương.

Ví dụ: `tieengs Vieetj` → tiếng Việt.

### VNI

| Phím | Dấu |
| --- | --- |
| `1` `2` `3` `4` `5` | sắc, huyền, hỏi, ngã, nặng |
| `6` | â / ê / ô |
| `7` | ư / ơ (`uo` + `7` → `ươ`) |
| `8` | ă |
| `9` | đ (gõ sau cả từ vẫn đổi được chữ `d`) |
| `0` | bỏ toàn bộ dấu của từ |

Ví dụ: `tie6ng1 Vie6t5` → tiếng Việt, `d9u7o7ng2` → đường, `duong9772` → đường (dấu móc và chữ đ gõ ở cuối từ).

Từ không phải âm tiết tiếng Việt thì **không bị bỏ dấu**, đúng như UniKey: gõ `WWindowws` ra `Windows` (không ra `Windów`), `hello` ra `hello`, `guitarr` ra `guitar`, `www` ra `ww`. Muốn dấu được đặt cả khi từ không hợp lệ thì tắt **Bật kiểm tra chính tả** trong hộp thoại cấu hình.

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
| `spellCheck` | 1 = kiểm tra chính tả (từ không phải âm tiết tiếng Việt thì không bỏ dấu) |
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

## Phát hành bản mới

`bin/` bị `.gitignore` bỏ qua, nên file chạy sẵn không nằm trong repo mà được đưa lên **Releases**.

1. Build và kiểm thử: `.\build.ps1 -Test` phải in `TẤT CẢ ĐỀU ĐÚNG`.
2. Đóng gói (chỉ ba file này, không kèm `settings.ini` để người dùng mới tự sinh). Làm bước này **sau** khi commit mọi thay đổi README để bản zip chứa README mới nhất:

   ```powershell
   Compress-Archive -Path bin\ThiTap.exe, README.md, LICENSE -DestinationPath bin\ThiTap-<version>-win64.zip -Force
   ```

3. Tạo release với tag `v<version>` (GitHub → Releases → *Draft a new release*, hoặc gọi REST API `POST /repos/dangsithi/thitap/releases` rồi upload asset lên `uploads.github.com`).
4. Kiểm tra lại: tải asset từ đường dẫn `.../releases/download/<tag>/<file>` phải trả về `HTTP 200` với đúng số byte.

Mục **Tải về** ở đầu README trỏ tới `/releases/latest` nên không cần sửa lại mỗi lần phát hành.

### Lịch sử phát hành

- **v1.2** — `ThiTap-1.2-win64.zip`: `w` gõ sau phụ âm cuối vẫn đổi đúng nguyên âm (`ungws` → `ứng`, `thuongw` → `thương`); xoá lui bỏ đúng phím của chữ vừa mất nên gõ tiếp vẫn ra đúng từ.
- **v1.1** — `ThiTap-1.1-win64.zip`: sửa lỗi đặt dấu sai sau khi bấm Backspace rồi gõ tiếp (`hội` → xoá → `ij` ra `hộị` như bản cũ, nay ra `hội`).
- **v1.0** — `ThiTap-1.0-win64.zip`: bản đầu tiên, Telex + VNI, biểu tượng khay cờ Việt Nam / cờ Liên hiệp Anh, bỏ thông báo khi chuyển ngôn ngữ, luật gõ `ww` = `w`.

## Kiểm thử

`bin\TelexTest.exe` (do `build.ps1 -Test` sinh ra) chạy hoàn toàn ở mức lõi gõ, không cần bàn phím thật:

- 114 từ và 5 câu Telex;
- 36 từ và 2 câu VNI;
- 16 chuỗi Telex và 3 chuỗi VNI có bấm Backspace giữa chừng (mô phỏng cả màn hình lẫn lõi gõ);
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
