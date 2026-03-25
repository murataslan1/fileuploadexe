# ImageToPdfMerger

A Windows Forms desktop application (.NET 8) that lets you drag-and-drop images, reorder them, search/filter by filename with autocomplete, and merge them into a single PDF.

## Features

- **Drag & Drop**: Drop images (JPEG, PNG, BMP, HEIC, WebP) onto the app or click to browse
- **Search & Autocomplete**: Real-time filtering of loaded images by filename with autocomplete suggestions
- **Reorder**: Drag-reorder items in the list or use Up/Down buttons
- **PDF Settings**: Choose page size (A4/Letter/A3/Original), orientation (Portrait/Landscape), and scaling mode (Fit to Page/Original Size/Fit to Width)
- **Preview**: See a page-by-page preview before saving
- **Self-Contained EXE**: Runs directly without .NET runtime installation

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
| `Escape` | Clear search |

## Build & Run

### Prerequisites
- Windows 10/11
- .NET 8 SDK (for building)

### Build
```bash
cd ImageToPdfMerger
dotnet build
dotnet run
```

### Publish as Self-Contained EXE
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The output EXE will be in `bin/Release/net8.0-windows/win-x64/publish/ImageToPdfMerger.exe`.

Double-click the EXE to run -- no .NET runtime installation required.

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
