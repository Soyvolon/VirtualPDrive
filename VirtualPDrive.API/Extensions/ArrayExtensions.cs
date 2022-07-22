namespace VirtualPDrive.API.Extensions;

public static class ArrayExtensions
{
    public static bool IsChildOfOrEqualTo<T>(this T[] item, T[] check) where T : IComparable
    {
        // If they are equally empty, then the first
        // is a child of the second.
        if (check.Length == 0
            && item.Length == 0)
            return true;

        // If the first item has nothing,
        // its always a child.
        if (item.Length == 0)
            return true;
        
        // If the second item has nothing,
        // its never a child.
        if (check.Length == 0)
            return false;

        // If the item is longer than the check,
        // its not a child or the same.
        if (item.Length > check.Length)
            return false;

        // Otherwise, lets compare values...
        var enumerator = check.GetEnumerator();
        bool hasNext = true;
        foreach(var i in item)
        {
            // ,.. if both items have a value ...
            if (hasNext)
            {
                // ... get the value for item 2 ...
                var checkVal = enumerator.MoveNext();
                // ... and if they are not equal, its not a child.
                if (!i.Equals(enumerator.Current))
                    return false;
            }
        }

        // If we reach here, there was no difference for the compared
        // elements and this is a child or equal.
        return true;
    }
}
