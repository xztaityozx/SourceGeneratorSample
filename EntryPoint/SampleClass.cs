namespace EntryPoint;

[PerlPackageSourceGenerator.PackageGenerator]
public partial class SampleClass
{
    private int c => 100;

    public int Add(int a, int b)
    {
        return a + b + c;
    }
}
