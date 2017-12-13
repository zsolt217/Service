using Topshelf;

namespace WindowsService
{
    public class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(serviceConfig =>
                              {
                                  serviceConfig.UseNLog();
                                  serviceConfig.Service<Converter>(
                              LogicInstance =>
                                  {
                                      LogicInstance.ConstructUsing(() => new Converter());

                                      LogicInstance.WhenStarted(
                                          execute => execute.Start());

                                      LogicInstance.WhenStopped(
                                          execute => execute.Stop());
                                  });

                                  serviceConfig.SetServiceName("Converter");
                                  serviceConfig.SetDisplayName("Reporting data converter");
                                  serviceConfig.SetDescription("Runs stored SQLs in Reporting Web Service.");
                                  serviceConfig.StartAutomatically();
                                  serviceConfig.EnableServiceRecovery(x =>
                                  {
                                      x.RestartService(0);
                                      x.RestartComputer(5, "Converter service stopped second time!");
                                      x.OnCrashOnly();
                                      x.SetResetPeriod(1);
                                  });
                              });
        }
    }
}
