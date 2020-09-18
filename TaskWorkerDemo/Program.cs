using System;
using NetEti.Globals;
using System.Threading;
using NetEti.ApplicationControl;

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
                taskTest.OnTaskProgressChanged(new CommonProgressChangedEventArgs("TaskTest", 100, 0, ItemsTypes.items, null));
                Thread.Sleep(2000);
                taskTest.OnTaskProgressChanged(new CommonProgressChangedEventArgs("TaskTest", 100, 20, ItemsTypes.items, null));
                Thread.Sleep(2000);
                taskTest.OnTaskProgressChanged(new CommonProgressChangedEventArgs("TaskTest", 100, 40, ItemsTypes.items, null));
                Thread.Sleep(2000);
                taskTest.OnTaskProgressChanged(new CommonProgressChangedEventArgs("TaskTest", 100, 60, ItemsTypes.items, null));
                Thread.Sleep(2000);
                taskTest.OnTaskProgressChanged(new CommonProgressChangedEventArgs("TaskTest", 100, 80, ItemsTypes.items, null));
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
                taskTest.OnTaskProgressChanged(new CommonProgressChangedEventArgs("TaskTest", 100, 0, ItemsTypes.items, parameters));
                Thread.Sleep(3000);
                taskTest.OnTaskProgressChanged(new CommonProgressChangedEventArgs("TaskTest", 100, 20, ItemsTypes.items, parameters));
                Thread.Sleep(3000);
                taskTest.OnTaskProgressChanged(new CommonProgressChangedEventArgs("TaskTest", 100, 40, ItemsTypes.items, parameters));
                Thread.Sleep(3000);
                taskTest.OnTaskProgressChanged(new CommonProgressChangedEventArgs("TaskTest", 100, 60, ItemsTypes.items, parameters));
                Thread.Sleep(3000);
                taskTest.OnTaskProgressChanged(new CommonProgressChangedEventArgs("TaskTest", 100, 80, ItemsTypes.items, parameters));
                Thread.Sleep(3000);
                taskTest.OnTaskProgressFinished(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Work2 " + ex.GetType().ToString() + ": " + ex.Message);
                throw;
            }
        }

        static void SubTaskProgressChanged(object sender, CommonProgressChangedEventArgs args)
        {
            Console.WriteLine("Changed - {0}: {1} von {2} {3}", args.ItemName, args.CountSucceeded, args.CountAll, args.UserState == null ? "" : args.UserState.ToString());
        }

        static void SubTaskProgressFinished(object sender, Exception threadException)
        {
            if (threadException == null)
            {
                Console.WriteLine("Finished with success");
            }
            else
            {
                Console.WriteLine("Aborted with '{0}'", threadException.Message);
            }
        }
    }
}
