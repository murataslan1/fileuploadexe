# ImageToPdfMerger

A Windows Forms desktop application (.NET 8) that lets you drag-and-drop images, reorder them, search/filter by filename with autocomplete, and merge them into a single PDF.

**Single EXE** -- Double-click `ImageToPdfMerger.exe` and it just works. No installation, no .NET runtime needed.

## Quick Start (Usage)

### 1. Download & Run
1. Download `ImageToPdfMerger.exe` from the [Releases](https://github.com/murataslan1/fileuploadexe/releases) page
2. Double-click the EXE -- the app opens instantly, no setup required

### 2. Add Images
- **Drag & drop** image files (JPEG, PNG, BMP, HEIC, WebP) onto the drop zone area
- Or **click** the drop zone to open a file browser
- Or press `Ctrl+O` to open the file dialog

### 3. Organize Your Images
- **Search**: Type in the search box (`Ctrl+F`) to filter images by filename -- autocomplete suggests matches as you type
- **Reorder**: Drag items in the list to change order, or select and use the `Up`/`Down` buttons
- **Remove**: Select images and click `Delete` or press the `Delete` key
- **Check/Uncheck**: Use checkboxes to include or exclude individual images from the PDF

### 4. Configure PDF Settings
- **Page Size**: A4, Letter, A3, or Original (keeps each image's native size)
- **Orientation**: Portrait or Landscape
- **Scale Mode**: Fit to Page, Original Size, or Fit to Width
- **Margin**: Toggle 10mm margin on/off

### 5. Preview & Save
1. Click **"Merge & Preview"** (or press `F5`) to generate the PDF and see a page-by-page preview
2. Navigate pages with the `<` and `>` buttons
3. Click **"Save PDF"** (or press `Ctrl+S`) to save the final PDF file

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
+----------------------+--------------------------------------+
|  [Drop Zone]         |                                      |
|  Drag & drop images  |     PDF PREVIEW PANEL                |
|  or click to browse  |                                      |
+----------------------+     [Page image displayed here]      |
|  [Search... Ctrl+F]  |                                      |
+----------------------+--------------------------------------+
|  ListView:           |  Page: < 1/5 >                       |
|  [x] photo1.jpg      +--------------------------------------+
|  [x] photo2.png      |  Page: A4  Orient: Portrait          |
|  [x] photo3.heic     |  Scale: Fit to Page  [x] Margin      |
+----------------------+  [Merge & Preview]  [Save PDF]       |
|  [Del][Up][Down][Clr] |                                      |
+----------------------+--------------------------------------+
```

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

## Build from Source

### Prerequisites
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build & Run
```bash
cd ImageToPdfMerger
dotnet build
dotnet run
```

### Publish as Single Self-Contained EXE
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output: `bin/Release/net8.0-windows/win-x64/publish/ImageToPdfMerger.exe`

This produces a **single EXE file** that includes the .NET runtime and all dependencies. Copy it anywhere, double-click, and it runs -- no installation needed.

## Project Structure

```
ImageToPdfMerger/
+-- Program.cs                          # Entry point
+-- Forms/
|   +-- MainForm.cs                     # Main form logic, search, events
|   +-- MainForm.Designer.cs            # UI layout
+-- Services/
|   +-- ImageService.cs                 # Image loading, HEIC conversion, thumbnails
|   +-- PdfService.cs                   # PDF generation with PdfSharpCore
|   +-- PreviewService.cs               # Page preview rendering
+-- Models/
|   +-- ImageItem.cs                    # Image data model
|   +-- PdfSettings.cs                  # PDF configuration model
+-- Controls/
|   +-- DropZonePanel.cs                # Custom drag-and-drop panel
|   +-- ThumbnailListView.cs            # ListView with drag-reorder support
+-- Utils/
    +-- FileHelper.cs                   # File utilities
```

## Tech Stack

- **.NET 8** Windows Forms (C#)
- **PdfSharpCore** -- PDF generation (MIT license)
- **SixLabors.ImageSharp** -- Image processing, HEIC/WebP support

## License

MIT
