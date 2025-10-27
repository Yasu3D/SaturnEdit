using SaturnData.Notation.Core;

namespace SaturnEdit.UndoRedo.BookmarkOperations;

public class BookmarkEditOperation(Bookmark bookmark, uint oldColor, uint newColor, string oldMessage, string newMessage) : IOperation
{
    public void Revert()
    {
        bookmark.Color = oldColor;
        bookmark.Message = oldMessage;
    }

    public void Apply()
    {
        bookmark.Color = newColor;
        bookmark.Message = newMessage;
    }
}