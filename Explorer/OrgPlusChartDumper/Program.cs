using System.ComponentModel;
using System.Reflection;
using System.Text;
using OrgPlusChartReader;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: OrgPlusChartDumper <path-to-chart.opx>");
    return 1;
}

try
{
    using var stream = File.OpenRead(args[0]);
    var chart = new OrganizationChart(stream);

    foreach (var record in chart.Records)
    {
        Dump(record, 0);
    }

    if (chart.SlackBytes.Length != 0)
    {
        Console.WriteLine($"Slack bytes before footer: {chart.SlackBytes.Length}");
    }

    return 0;
}
catch (Exception e) when (e is InvalidDataException or IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Could not read '{args[0]}': {e.Message}");
    return 1;
}

static void Dump(OrganizationChartRecord record, int indent)
{
    string padding = new(' ', indent * 2);
    if (record is OrganizationChartSubstream substream)
    {
        Console.WriteLine($"{padding}{record.Id} {{");
        foreach (var child in substream.Records)
        {
            Dump(child, indent + 1);
        }

        Console.WriteLine($"{padding}}}");
        return;
    }

    Console.WriteLine($"{padding}{record.Id}: {Describe(record)}");
}

// Records are plain property bags, so describe them by reflection.
static string Describe(OrganizationChartRecord record)
{
    var builder = new StringBuilder();
    foreach (var property in record.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
        if (property.DeclaringType == typeof(OrganizationChartRecord))
        {
            continue;
        }

        object? value = property.GetValue(record);
        var converter = property.GetCustomAttribute<TypeConverterAttribute>();
        string text = converter != null && value != null
            ? TypeDescriptor.GetProperties(record)[property.Name]!.Converter.ConvertToString(value)!
            : value?.ToString() ?? "null";

        if (builder.Length != 0)
        {
            builder.Append(", ");
        }

        builder.Append($"{property.Name}={text}");
    }

    return builder.Length != 0 ? builder.ToString() : Convert.ToHexString(record.Bytes);
}
