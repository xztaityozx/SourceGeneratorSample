using EntryPoint;

Console.WriteLine("Hello World");

var s = new SampleClass();

await s.PerlPackageSourceGenerator_GeneratePerlAsync("./");
