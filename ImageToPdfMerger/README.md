# ImageToPdfMerger

Fotograflari tek bir PDF dosyasina birlestiren Windows masaustu uygulamasi.

**Tek EXE dosyasi** -- `ImageToPdfMerger.exe` dosyasina cift tiklayin, hemen calisir. Kurulum yok, .NET yuklemenize gerek yok.

---

# Turkce Dokumantasyon

## Nedir?

ImageToPdfMerger, fotograflarinizi (JPEG, PNG, BMP, HEIC, WebP) surukle-birak ile yukleyip, istediginiz sirada duzenleyip, tek bir PDF dosyasina birlestirmenizi saglayan bir Windows masaustu uygulamasidir. Arama ve otomatik tamamlama ozelligi ile cok sayida fotograf arasinda kolayca istediginizi bulabilirsiniz.

## Nasil Kullanilir?

### Adim 1: Uygulamayi Ac
1. `ImageToPdfMerger.exe` dosyasini indirin
2. Dosyaya **cift tiklayin** -- uygulama aninda acilir
3. Hicbir kurulum veya ayar gerekmez, tek dosya ile calisir

### Adim 2: Fotograflari Yukle
Fotograflari uygulamaya eklemenin 3 yolu var:
- **Surukle & Birak**: Fotograflari dosya yoneticisinden secip uygulamanin ust kismindaki alana birakin
- **Tiklayarak Sec**: Surukle-birak alanina tiklayarak dosya secim penceresini acin
- **Klavye Kisayolu**: `Ctrl+O` tusuna basarak dosya secim penceresini acin

Desteklenen formatlar: **JPEG, PNG, BMP, GIF, TIFF, HEIC, WebP**

> HEIC ve WebP dosyalari otomatik olarak JPEG'e donusturulur, ekstra bir islem yapmaniza gerek yoktur.

### Adim 3: Fotograflari Duzenle

#### Arama ve Otomatik Tamamlama
- Sol paneldeki **arama kutusuna** yazmaya baslayin (`Ctrl+F` ile odaklanin)
- Yazdikca liste aninda filtrelenir ve otomatik tamamlama onerileri gosterilir
- Aramayı temizlemek icin `X` butonuna basin veya `Escape` tusuna basin

#### Siralama
- Listedeki fotograflari **surukleyerek** istediginiz siraya tasiyin
- Veya bir fotograf secip **Yukari/Asagi** butonlarina tiklayin
- `Ctrl+Up` ve `Ctrl+Down` kisayollari ile de siralama yapabilirsiniz

#### Silme
- Silinecek fotograflari secip **Delete** butonuna tiklayin veya `Delete` tusuna basin
- **Tumunu Temizle** butonu ile tum listeyi bosaltin

#### Secim
- Her fotografin yanindaki **onay kutusu** ile PDF'e dahil edilip edilmeyecegini belirleyin
- Isaretli olmayan fotograflar PDF'e eklenmez

### Adim 4: PDF Ayarlari
Sag panelin alt kismindaki ayarlar:

| Ayar | Secenekler | Aciklama |
|------|-----------|----------|
| **Sayfa Boyutu** | A4, Letter, A3, Orijinal | Orijinal seceneginde her resim kendi boyutunda bir sayfa olusturur |
| **Yonelim** | Dikey (Portrait), Yatay (Landscape) | Sayfanin dik mi yatay mi olacagini belirler |
| **Olcekleme** | Sayfaya Sigdir, Orijinal Boyut, Genislige Sigdir | Resmin sayfaya nasil yerlestirilecegin belirler |
| **Kenar Boslugu** | Acik/Kapali (10mm) | Sayfa kenarlarinda bosluk birakir |

### Adim 5: Onizleme ve Kaydetme
1. **"Merge & Preview"** butonuna tiklayin veya `F5` tusuna basin
2. Sag panelde PDF onizlemesi gorunur -- `<` ve `>` butonlari ile sayfalar arasinda gezinin
3. Sonuctan memnunsaniz **"Save PDF"** butonuna tiklayin veya `Ctrl+S` basin
4. Kaydetmek istediginiz konumu ve dosya adini secin, tamam deyin

PDF dosyaniz hazir!

## Klavye Kisayollari

| Kisayol | Islem |
|---------|-------|
| `Ctrl+O` | Dosya secim penceresini ac |
| `Ctrl+S` | PDF'i kaydet |
| `Ctrl+F` | Arama kutusuna odaklan |
| `Ctrl+A` | Tum fotograflari sec |
| `Delete` | Secili fotograflari sil |
| `Ctrl+Yukari` | Secili fotograflari yukari tasi |
| `Ctrl+Asagi` | Secili fotograflari asagi tasi |
| `F5` | Birlestir ve onizle |
| `Escape` | Aramayi temizle, listeye don |

## Ozellikler

- **Surukle & Birak**: Fotograflari uygulamaya surukleyin veya tiklayarak secin
- **Arama & Otomatik Tamamlama**: Dosya adina gore aninda filtreleme ve oneri
- **Siralama**: Surukleyerek veya butonlarla sira degistirme
- **PDF Ayarlari**: Sayfa boyutu, yonelim, olcekleme modu, kenar boslugu
- **Onizleme**: Kaydetmeden once sayfa sayfa onizleme
- **HEIC/WebP Destegi**: Bu formatlari otomatik olarak donusturur
- **Tekrar Engelleme**: Ayni dosya iki kez eklenmez
- **Tek EXE**: Bagimsiz calisir, hicbir sey kurmaya gerek yok

## Ekran Goruntusu

```
+------------------------+--------------------------------------+
|  [Surukle-Birak Alani] |                                      |
|  Fotograflari buraya   |      PDF ONIZLEME PANELI             |
|  surukleyin veya       |                                      |
|  tiklayarak secin      |   [Sayfa gorseli burada gorunur]     |
+------------------------+                                      |
|  [Ara... (Ctrl+F)]  [X]|                                      |
+------------------------+--------------------------------------+
|  Fotograf Listesi:     |  Sayfa: < 1/5 >                      |
|  [x] foto1.jpg         +--------------------------------------+
|  [x] foto2.png         |  Boyut: A4   Yon: Dikey              |
|  [x] foto3.heic        |  Olcek: Sayfaya Sigdir               |
|  [x] foto4.webp        |  [x] Kenar Boslugu (10mm)            |
+------------------------+                                      |
| [Sil][Yukari][Asagi]   |  [Birlestir & Onizle] [PDF Kaydet]  |
| [Tumunu Temizle]       |                                      |
+------------------------+--------------------------------------+
|  Durum: 4 fotograf yuklendi | Toplam: 8.2 MB | Hazir         |
+---------------------------------------------------------------+
```

---

# English Documentation

## What is it?

ImageToPdfMerger is a Windows desktop application that lets you drag-and-drop images (JPEG, PNG, BMP, HEIC, WebP), reorder them, search/filter by filename with autocomplete, and merge them into a single PDF file.

**Single EXE** -- Double-click `ImageToPdfMerger.exe` and it just works. No installation, no .NET runtime needed.

## How to Use

### Step 1: Open the App
1. Download `ImageToPdfMerger.exe` from the [Releases](https://github.com/murataslan1/fileuploadexe/releases) page
2. **Double-click** the EXE file -- the app opens instantly
3. No installation, no setup, no dependencies -- it's a single portable file

### Step 2: Add Images
Three ways to add images:
- **Drag & Drop**: Drag image files from File Explorer onto the drop zone area at the top
- **Click to Browse**: Click the drop zone to open a file browser dialog
- **Keyboard**: Press `Ctrl+O` to open the file dialog

Supported formats: **JPEG, PNG, BMP, GIF, TIFF, HEIC, WebP**

> HEIC and WebP files are automatically converted to JPEG -- no extra steps needed.

### Step 3: Organize Images

#### Search & Autocomplete
- Start typing in the **search box** on the left panel (`Ctrl+F` to focus)
- The list filters in real-time as you type, with autocomplete suggestions
- Click `X` or press `Escape` to clear the search

#### Reorder
- **Drag items** in the list to rearrange them
- Or select an image and click the **Up/Down** buttons
- Keyboard: `Ctrl+Up` and `Ctrl+Down`

#### Remove
- Select images and click **Delete** button or press `Delete` key
- **Clear All** button removes everything

#### Selection
- Use the **checkbox** next to each image to include/exclude it from the PDF
- Unchecked images are skipped during PDF generation

### Step 4: Configure PDF Settings
Settings are at the bottom of the right panel:

| Setting | Options | Description |
|---------|---------|-------------|
| **Page Size** | A4, Letter, A3, Original | Original keeps each image's native dimensions |
| **Orientation** | Portrait, Landscape | Page direction |
| **Scale Mode** | Fit to Page, Original Size, Fit to Width | How images are placed on pages |
| **Margin** | On/Off (10mm) | Adds whitespace around page edges |

### Step 5: Preview & Save
1. Click **"Merge & Preview"** or press `F5` to generate the PDF
2. Preview appears in the right panel -- navigate pages with `<` and `>` buttons
3. Click **"Save PDF"** or press `Ctrl+S` to save the final file
4. Choose your save location and filename

Your PDF is ready!

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+O` | Open file dialog |
| `Ctrl+S` | Save PDF |
| `Ctrl+F` | Focus search box |
| `Ctrl+A` | Select all images |
| `Delete` | Delete selected images |
| `Ctrl+Up` | Move selected up |
| `Ctrl+Down` | Move selected down |
| `F5` | Merge & Preview |
| `Escape` | Clear search, return to list |

## Features

- **Drag & Drop**: Drop images onto the app or click to browse
- **Search & Autocomplete**: Real-time filtering with dropdown suggestions as you type
- **Reorder**: Drag-reorder items in the list or use Up/Down buttons
- **PDF Settings**: Page size, orientation, scaling mode, margin toggle
- **Preview**: Page-by-page preview before saving
- **HEIC/WebP Support**: Automatically converts HEIC and WebP images
- **Duplicate Detection**: Same file won't be added twice
- **Single EXE**: Self-contained, no dependencies, just double-click and run

## Screenshot

```
+------------------------+--------------------------------------+
|  [Drop Zone]           |                                      |
|  Drag & drop images    |      PDF PREVIEW PANEL               |
|  here or click to      |                                      |
|  browse                |   [Page image displayed here]        |
+------------------------+                                      |
|  [Search... (Ctrl+F)][X]|                                     |
+------------------------+--------------------------------------+
|  Image List:           |  Page: < 1/5 >                       |
|  [x] photo1.jpg        +--------------------------------------+
|  [x] photo2.png        |  Size: A4   Orient: Portrait         |
|  [x] photo3.heic       |  Scale: Fit to Page                  |
|  [x] photo4.webp       |  [x] Margin (10mm)                   |
+------------------------+                                      |
| [Del][Up][Down]        |  [Merge & Preview]  [Save PDF]       |
| [Clear All]            |                                      |
+------------------------+--------------------------------------+
|  Status: 4 images loaded | Total: 8.2 MB | Ready             |
+---------------------------------------------------------------+
```

---

# Build from Source

## Prerequisites
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build & Run
```bash
cd ImageToPdfMerger
dotnet build
dotnet run
```

## Publish as Single Self-Contained EXE
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output: `bin/Release/net8.0-windows/win-x64/publish/ImageToPdfMerger.exe`

This produces a **single EXE file** (~60-80 MB) that includes the .NET runtime and all dependencies. Copy it anywhere, double-click, and it runs.

---

# Project Structure

```
ImageToPdfMerger/
+-- Program.cs                          # Uygulama giris noktasi / Entry point
+-- Forms/
|   +-- MainForm.cs                     # Ana form mantigi, arama, event'ler / Main form logic
|   +-- MainForm.Designer.cs            # Arayuz tasarimi / UI layout
+-- Services/
|   +-- ImageService.cs                 # Resim yukleme, HEIC donusturme / Image loading
|   +-- PdfService.cs                   # PDF olusturma / PDF generation
|   +-- PreviewService.cs               # Onizleme render / Preview rendering
+-- Models/
|   +-- ImageItem.cs                    # Resim veri modeli / Image data model
|   +-- PdfSettings.cs                  # PDF ayar modeli / PDF settings model
+-- Controls/
|   +-- DropZonePanel.cs                # Surukle-birak paneli / Drag-and-drop panel
|   +-- ThumbnailListView.cs            # Siralanabilir liste / Reorderable list
+-- Utils/
    +-- FileHelper.cs                   # Dosya yardimcilari / File utilities
```

# Tech Stack

- **.NET 8** Windows Forms (C#)
- **PdfSharpCore** -- PDF olusturma / PDF generation (MIT license)
- **SixLabors.ImageSharp** -- Goruntu isleme / Image processing, HEIC/WebP support

# License

MIT
