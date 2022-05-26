using Hardware.Info;

namespace Terminal;

public static class HardwareInitializer
{
    
    private static readonly HardwareInfo Info = new();

    private static HardwareInfoDisplay InfoDisplay;

    public static void Setup()
    {
        Info.RefreshCPUList();
        InfoDisplay = new HardwareInfoDisplay(Info.CpuList[0].Name);
    }

    public static HardwareInfoDisplay Display() => InfoDisplay;

}

public struct HardwareInfoDisplay
{

    public readonly string CpuDash;
    public readonly string CpuName;

    public HardwareInfoDisplay(string cpuName)
    {
        CpuName = cpuName;
        CpuDash = "";
        for (int i = 0; i < cpuName.Length + 4; i++) CpuDash += "─";
    } 

}