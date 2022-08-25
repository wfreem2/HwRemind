namespace HwRemind.Authentication
{
    public class Program
    {
        public static void Main(string[] args) => 
            CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(lb =>
            {
                lb.ClearProviders();
                lb.AddConsole();
                lb.AddTraceSource("Information, ActivityTracing");
            })
            .ConfigureWebHostDefaults(wb => wb.UseStartup<Startup>());
    }
}