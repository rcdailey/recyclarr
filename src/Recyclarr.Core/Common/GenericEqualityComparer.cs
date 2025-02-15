using System.Diagnostics.CodeAnalysis;

namespace Recyclarr.Common;

[SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
public sealed class GenericEqualityComparer<T>(
    Func<T, T, bool> equalsPredicate,
    Func<T, int> hashPredicate
) : IEqualityComparer<T>
{
    public bool Equals(T? x, T? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return equalsPredicate(x, y);
    }

    public int GetHashCode(T obj)
    {
        return hashPredicate(obj);
    }
}
