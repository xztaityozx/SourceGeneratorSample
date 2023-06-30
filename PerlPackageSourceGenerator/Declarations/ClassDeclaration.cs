﻿using PerlPackageSourceGenerator.Extensions;

namespace PerlPackageSourceGenerator.Declarations;

internal class ClassDeclaration
{
    public List<string> Lines { get; }

    public ClassDeclaration(string originalName, IEnumerable<SubRoutineDeclaration> subRoutines)
    {
        Lines = new List<string>
        {
            "# This file generated by PerlPackageSourceGenerator.",
            "# DO NOT EDIT MANUALLY.",
            $"# Original C# Class: {originalName}",
            $"package {originalName.ToClassName()};",
            "",
            "use warnings;",
            "use strict;",
            "",
        };
        Lines.AddRange(subRoutines.SelectMany(subRoutine => subRoutine.Lines));

        Lines.Add("1;");
    }
}