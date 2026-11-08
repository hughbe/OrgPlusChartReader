using System.ComponentModel;
using System.Globalization;

namespace OrgPlusChartReader;

public class HexTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        if (destinationType == typeof(string))
        {
            return true;
        }

        return base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            switch (value)
            {
                case uint u:
                    return $"0x{u:X8}";
                case ushort s:
                    return $"0x{s:X4}";
                case byte b:
                    return $"0x{b:X2}";
            }
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string input)
        {
            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(2);
            }

            uint parsed = uint.Parse(input, NumberStyles.HexNumber, culture);

            // Return the type of the property being edited, so that it can be assigned back.
            Type? propertyType = context?.PropertyDescriptor?.PropertyType;
            if (propertyType == typeof(ushort))
            {
                return checked((ushort)parsed);
            }
            else if (propertyType == typeof(byte))
            {
                return checked((byte)parsed);
            }

            return parsed;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
