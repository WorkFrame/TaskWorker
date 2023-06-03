using NetEti.ApplicationControl;
using System.ComponentModel;

namespace NetEti.DemoApplications
{
    class Program
    {
        static void Main(string[] args)
        {
            using (TaskWorker taskTest = new TaskWorker())
            {
                taskTest.TaskProgressChanged -= SubTaskProgressChanged;
                taskTest.TaskProgressChanged += SubTaskProgressChanged;
                taskTest.TaskProgressFinished -= SubTaskProgressFinished;
                taskTest.TaskProgressFinished += SubTaskProgressFinished;

                Console.WriteLine("Main: Ende mit E, Halten mit H, Weiter mit W oder C, Abbrechen mit B");
                taskTest.RunTask(new Action<TaskWorker>(Program.work));
                Console.WriteLine("Async gestartet...");
                taskTest.RunTask(new Action<TaskWorker, object>(Program.work2), "bla");
                Console.WriteLine("Async gestartet...");
                //taskTest.WaitForTask();

                string inKey = "";
                do
                {
                    Thread.Sleep(100);
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo ki = Console.ReadKey();
                        inKey = ki.KeyChar.ToString().ToUpper();
                        switch (inKey)
                        {
                            case "H": taskTest.HaltTask(); break;
                            case "C":
                            case "W":
                                taskTest.ContinueTask(); break;
                            case "B": taskTest.BreakTask(); break;
                            default:
                                break;
                        }
                    }
                    //Console.WriteLine(taskTest.WorkerStatus.ToString());
                } while (taskTest.WorkerStatus != TaskWorkerStatus.Ready && inKey.ToString().ToUpper() != "E");
                taskTest.BreakTask();
            }
            Thread.Sleep(2000);
            Console.ReadLine();
        }

        static void work(TaskWorker taskTest)
        {
            try
            {
                taskTest.OnTaskProgressChanged(new ProgressChangedEventArgs(0, null));
                Thread.Sleep(2000);
                taskTest.OnTaskProgressChanged(new ProgressChangedEventArgs(20, null));
                Thread.Sleep(2000);
                taskTest.OnTaskProgressChanged(new ProgressChangedEventArgs(40, null));
                Thread.Sleep(2000);
                taskTest.OnTaskProgressChanged(new ProgressChangedEventArgs(60, null));
                Thread.Sleep(2000);
                taskTest.OnTaskProgressChanged(new ProgressChangedEventArgs(80, null));
                Thread.Sleep(2000);
                taskTest.OnTaskProgressFinished(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Work " + ex.GetType().ToString() + ": " + ex.Message);
                throw;
            }
        }

        static void work2(TaskWorker taskTest, object parameters)
        {
            try
            {
                taskTest.OnTaskProgressChanged(new ProgressChangedEventArgs(0, parameters));
                Thread.Sleep(3000);
                taskTest.OnTaskProgressChanged(new ProgressChangedEventArgs(20, parameters));
                Thread.Sleep(3000);
                taskTest.OnTaskProgressChanged(new ProgressChangedEventArgs(40, parameters));
                Thread.Sleep(3000);
                taskTest.OnTaskProgressChanged(new ProgressChangedEventArgs(60, parameters));
                Thread.Sleep(3000);
                taskTest.OnTaskProgressChanged(new ProgressChangedEventArgs(80, parameters));
                Thread.Sleep(3000);
                taskTest.OnTaskProgressFinished(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Work2 " + ex.GetType().ToString() + ": " + ex.Message);
                throw;
            }
        }

        static void SubTaskProgressChanged(object? sender, ProgressChangedEventArgs args)
        {
            string userState = args.UserState == null ? "" : args.UserState.ToString()?? "";
            Console.WriteLine($"ProgressPercentage: {args.ProgressPercentage}, UserState: {userState}");
        }

        static void SubTaskProgressFinished(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                Console.WriteLine("Finished with success");
            }
            else
            {
                Console.WriteLine("Aborted with '{0}'", e.Error?.Message);
            }
        }
    }
}
