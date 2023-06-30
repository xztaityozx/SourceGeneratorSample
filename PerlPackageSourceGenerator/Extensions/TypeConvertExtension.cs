using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PerlPackageSourceGenerator.Extensions;

internal static class TypeConvertExtension
{
    /// <summary>
    /// C#の型をPerlの型に変換する。Perlには型がないので、Type::Tinyで表せるやつだけ
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedTypeConvertException"></exception>
    public static string ToPerlType(this TypeSyntax type)
    {
        switch (type)
        {
            case PredefinedTypeSyntax predefinedType:
                return predefinedType.Keyword.ValueText switch
                {
                    "bool" => "Bool",
                    "int"
                    or "long"
                    or "short"
                    or "byte"
                    or "sbyte"
                    or "uint"
                    or "ulong"
                    or "ushort"
                    or "char"
                        => "Int",
                    "float" or "double" or "decimal" => "Num",
                    "string" => "Str",
                    "object" => "HashRef",
                    "void" => "Undef",
                    _ => throw new NotSupportedTypeConvertException(type)
                };
            case ArrayTypeSyntax arrayType:
                return $"ArrayRef[{arrayType.ElementType.ToPerlType()}]";
            case NullableTypeSyntax nullableType:
                return $"Maybe[{nullableType.ElementType.ToPerlType()}]";
        }

        var iEnumerableRegex = new System.Text.RegularExpressions.Regex(
            @"^(IEnumerable|I?List)<(.*)>$"
        );
        if (
            iEnumerableRegex.IsMatch(type.ToString()) && type is GenericNameSyntax genericNameSyntax
        )
        {
            return $"ArrayRef[{string.Join(",", genericNameSyntax.TypeArgumentList.Arguments.Select(a => a.ToPerlType()))}]";
        }

        return "Any";
    }

    public class NotSupportedTypeConvertException : Exception
    {
        public NotSupportedTypeConvertException(TypeSyntax type)
            : base($"Not supported type convert: {type}") { }
    }
}
