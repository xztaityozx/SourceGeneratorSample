using System.Text;

namespace PerlPackageSourceGenerator.Extensions;

internal static class CaseExtension
{
    public static string ToSnakeCase(this string str)
    {
        var sb = new StringBuilder();
        foreach (var c in str)
        {
            if (char.IsUpper(c))
            {
                sb.Append("_");
                sb.Append(char.ToLower(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString().TrimStart('_');
    }

    public static string ToClassName(this string original) =>
        string.Join("::", original.Split('.'));
}
