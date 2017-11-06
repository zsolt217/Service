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

                                  serviceConfig.Service<MainLogic>(


                              LogicInstance =>
                                  {
                                      LogicInstance.ConstructUsing(() => new MainLogic());

                                      LogicInstance.WhenStarted(
                                          execute => execute.Start());

                                      LogicInstance.WhenStopped(
                                          execute => execute.Stop());
                                  });

                                  serviceConfig.SetServiceName("AwesomeFileConverter");
                                  serviceConfig.SetDisplayName("Awesome File Converter");
                                  serviceConfig.SetDescription("A Pluralsight demo service");

                                  serviceConfig.StartAutomatically();
                              });
        }
    }
}
