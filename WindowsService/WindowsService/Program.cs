using Topshelf;

namespace WindowsService
{
    public class Program
    {
        private static void Main(string[] args)
        {          
            HostFactory.Run(serviceConfig =>
                              {
                                  //https://stackoverflow.com/questions/19151363/windows-service-to-run-a-function-at-specified-time
                                  //itt írják hh kell

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
                              });
        }
    }
}
