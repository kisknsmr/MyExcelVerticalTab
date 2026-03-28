using System;

namespace ExcelVerticalTab;

public class EventArgs<T>(T value) : EventArgs
{
    public T Value { get; } = value;
}
