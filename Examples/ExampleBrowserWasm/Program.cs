using ExampleBrowserWasm;

try
{
    ImGuiSmoke.Run(Console.WriteLine);
    Console.WriteLine("SMOKE PASS");
    return 0;
}
catch (Exception e)
{
    Console.WriteLine($"SMOKE FAIL {e}");
    return 1;
}
