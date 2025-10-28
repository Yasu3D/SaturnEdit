using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.BookmarkOperations;

public class BookmarkRemoveOperation(Bookmark bookmark, int index) : IOperation
{
    public void Revert()
    {
        if (index < 0 || index >= ChartSystem.Chart.Bookmarks.Count)
        {
            ChartSystem.Chart.Bookmarks.Add(bookmark);
        }
        else
        {
            ChartSystem.Chart.Bookmarks.Insert(index, bookmark);
        }
    }

    public void Apply()
    {
        ChartSystem.Chart.Bookmarks.Remove(bookmark);
    }
}