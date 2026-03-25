using System.Drawing;
using ImageToPdfMerger.Models;

namespace ImageToPdfMerger.Controls;

public class ThumbnailListView : ListView
{
    private readonly ImageList _imageList;
    private int _imageKeyCounter;

    public ThumbnailListView()
    {
        View = View.Details;
        FullRowSelect = true;
        CheckBoxes = true;
        AllowDrop = true;
        MultiSelect = true;
        HideSelection = false;
        DoubleBuffered = true;
        GridLines = false;
        BorderStyle = BorderStyle.None;

        _imageList = new ImageList
        {
            ImageSize = new Size(40, 40),
            ColorDepth = ColorDepth.Depth32Bit
        };
        SmallImageList = _imageList;

        Columns.Add("", 30);
        Columns.Add("Filename", 170);
        Columns.Add("Size", 65);
        Columns.Add("Resolution", 85);

        ItemDrag += OnItemDrag;
        DragEnter += OnDragEnter;
        DragOver += OnDragOver;
        DragDrop += OnDragDrop;
        ItemChecked += OnItemChecked;
    }

    public void AddImageItem(ImageItem item)
    {
        var imageKey = $"img_{_imageKeyCounter++}";
        if (item.Thumbnail != null)
            _imageList.Images.Add(imageKey, item.Thumbnail);

        var lvi = new ListViewItem(string.Empty)
        {
            ImageKey = imageKey,
            Checked = item.IsSelected,
            Tag = item
        };
        lvi.SubItems.Add(item.FileName);
        lvi.SubItems.Add(item.FormattedSize);
        lvi.SubItems.Add(item.Resolution);

        Items.Add(lvi);
    }

    public void RemoveSelected()
    {
        var toRemove = SelectedItems.Cast<ListViewItem>().ToList();
        foreach (var lvi in toRemove)
        {
            var item = lvi.Tag as ImageItem;
            item?.Dispose();
            Items.Remove(lvi);
        }
    }

    public List<ImageItem> GetRemovedSelectedItems()
    {
        var removed = new List<ImageItem>();
        var toRemove = SelectedItems.Cast<ListViewItem>().ToList();
        foreach (var lvi in toRemove)
        {
            if (lvi.Tag is ImageItem item)
                removed.Add(item);
        }
        return removed;
    }

    public void MoveSelectedUp()
    {
        if (SelectedItems.Count == 0) return;
        var indices = SelectedIndices.Cast<int>().OrderBy(i => i).ToList();
        if (indices[0] == 0) return;

        BeginUpdate();
        foreach (var index in indices)
        {
            var lvi = Items[index];
            Items.RemoveAt(index);
            Items.Insert(index - 1, lvi);
        }
        EndUpdate();
    }

    public void MoveSelectedDown()
    {
        if (SelectedItems.Count == 0) return;
        var indices = SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
        if (indices[0] == Items.Count - 1) return;

        BeginUpdate();
        foreach (var index in indices)
        {
            var lvi = Items[index];
            Items.RemoveAt(index);
            Items.Insert(index + 1, lvi);
        }
        EndUpdate();
    }

    public void ClearAll()
    {
        foreach (ListViewItem lvi in Items)
        {
            (lvi.Tag as ImageItem)?.Dispose();
        }
        Items.Clear();
        _imageList.Images.Clear();
        _imageKeyCounter = 0;
    }

    public List<ImageItem> GetOrderedItems()
    {
        var items = new List<ImageItem>();
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].Tag is ImageItem item)
            {
                item.Order = i;
                item.IsSelected = Items[i].Checked;
                items.Add(item);
            }
        }
        return items;
    }

    public void ScrollToAndHighlight(int index)
    {
        if (index < 0 || index >= Items.Count) return;
        SelectedItems.Clear();
        Items[index].Selected = true;
        Items[index].Focused = true;
        EnsureVisible(index);
    }

    public void RefreshFromList(List<ImageItem> items)
    {
        BeginUpdate();
        Items.Clear();
        _imageList.Images.Clear();
        _imageKeyCounter = 0;

        foreach (var item in items)
        {
            AddImageItem(item);
        }
        EndUpdate();
    }

    private void OnItemDrag(object? sender, ItemDragEventArgs e)
    {
        DoDragDrop(e.Item!, DragDropEffects.Move);
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(typeof(ListViewItem)) == true)
            e.Effect = DragDropEffects.Move;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(typeof(ListViewItem)) == true)
        {
            e.Effect = DragDropEffects.Move;
            var targetPoint = PointToClient(new Point(e.X, e.Y));
            var targetIndex = InsertionMark.NearestIndex(targetPoint);
            if (targetIndex >= 0)
            {
                var itemBounds = GetItemRect(targetIndex);
                InsertionMark.AppearsAfterItem = targetPoint.Y > itemBounds.Top + itemBounds.Height / 2;
                InsertionMark.Index = targetIndex;
            }
        }
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        InsertionMark.Index = -1;

        if (e.Data?.GetData(typeof(ListViewItem)) is not ListViewItem draggedItem) return;

        var targetPoint = PointToClient(new Point(e.X, e.Y));
        var targetIndex = InsertionMark.NearestIndex(targetPoint);
        if (targetIndex < 0) targetIndex = Items.Count - 1;

        var itemBounds = GetItemRect(targetIndex);
        if (targetPoint.Y > itemBounds.Top + itemBounds.Height / 2)
            targetIndex++;

        var oldIndex = draggedItem.Index;
        if (oldIndex == targetIndex || oldIndex == targetIndex - 1) return;

        BeginUpdate();
        Items.RemoveAt(oldIndex);
        if (targetIndex > oldIndex) targetIndex--;
        Items.Insert(targetIndex, draggedItem);
        EndUpdate();
    }

    private void OnItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        if (e.Item.Tag is ImageItem item)
            item.IsSelected = e.Item.Checked;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _imageList.Dispose();
        }
        base.Dispose(disposing);
    }
}
