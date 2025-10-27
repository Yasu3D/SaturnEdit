using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.BookmarkOperations;

public class BookmarkAddOperation(Bookmark bookmark, int index) : IOperation
{
    public void Revert()
    {
        ChartSystem.Chart.Bookmarks.Remove(bookmark);
    }

    public void Apply()
    {
        ChartSystem.Chart.Bookmarks.Insert(index, bookmark);
    }
}