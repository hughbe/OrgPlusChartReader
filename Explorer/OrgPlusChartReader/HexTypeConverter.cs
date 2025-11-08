using System;
using System.ComponentModel;
using System.Globalization;

namespace OrgPlusChartReader;

public class HexTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }
        
        return base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            return true;
        }
        
        return base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is uint l)
        {
            return $"0x{l:X8}";
        }
        else if (destinationType == typeof(string) && value is uint u)
        {
            return $"0x{u:X8}";
        }
        else if (destinationType == typeof(string) && value is ushort s)
        {
            return $"0x{s:X4}";
        }
        else if (destinationType == typeof(string) && value is byte b)
        {
            return $"0x{b:X2}";
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
            string input = (string)value;

            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(2);
            }

            return uint.Parse(input, NumberStyles.HexNumber, culture);
        }
        
        return base.ConvertFrom(context, culture, value);
    }
}
