using System;
using System.Threading;
using NetEti.Globals;
using System.Threading.Tasks;

namespace NetEti.ApplicationControl
{
    /// <summary>
    /// Aufzählungstyp für verschiedene Task-Zustände.
    /// </summary>
    public enum TaskWorkerStatus
    {
        /// <summary>Kann gestartet werden.</summary>
        Ready,
        /// <summary>Läuft gerade.</summary>
        Running,
        /// <summary>Ist angehalten, kann mit ContinueTask wieder aktiviert werden.</summary>
        Halted
    }

    /// <summary>
    /// Führt eine übergebene Action in einer eigenen Task aus.
    /// </summary>
    /// <remarks>
    /// File: TaskWorker.cs
    /// Autor: Erik Nagel, NetEti
    ///
    /// 25.04.2013 Erik Nagel: erstellt
    /// 10.09.2013 Erik Nagel: RunTask mit Parameter-Übergabe implementiert.
    /// </remarks>
    public class TaskWorker : IDisposable
    {
        #region IDisposable Member

        private bool _disposed; // = false wird vom System vorbelegt;

        /// <summary>
        /// Öffentliche Methode zum Aufräumen.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Hier wird aufgeräumt.
        /// </summary>
        /// <param name="disposing">False, wenn vom eigenen Destruktor auferufen.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this._disposed)
            {
                if (this.WorkerStatus != TaskWorkerStatus.Ready && this._cancellationTokenSource != null)
                {
                    this._cancellationTokenSource.Cancel();
                    Thread.Sleep(500);
                    this._disposed = true;
                }
            }
        }

        /// <summary>
        /// Finalizer: wird vom GarbageCollector aufgerufen.
        /// </summary>
        ~TaskWorker()
        {
            this.Dispose(false);
        }

        #endregion IDisposable Member

        #region public members

        /// <summary>
        /// Wird aufgerufen, wenn sich der Verarbeitungs-Fortschritt einer Task geändert hat.
        /// </summary>
        public event CommonProgressChangedEventHandler TaskProgressChanged;

        /// <summary>
        /// Wird aufgerufen, wenn die Verarbeitung einer Task abgeschlossen wurde.
        /// </summary>
        public event CommonProgressFinishedEventHandler TaskProgressFinished;

        /// <summary>
        /// Aktueller Zustand des TaskWorkers: Ready, Running oder Halted.
        /// </summary>
        public TaskWorkerStatus WorkerStatus { get; private set; }

        /// <summary>
        /// Versucht, den TaskWorker anzuhalten (Loop+Sleep).
        /// </summary>
        public void HaltTask()
        {
            if (this.WorkerStatus == TaskWorkerStatus.Running)
            {
                this._haltRequested = true;
            }
        }

        /// <summary>
        /// Lässt den angehaltenen TaskWorker weiterlaufen.
        /// </summary>
        public void ContinueTask()
        {
            if (this.WorkerStatus == TaskWorkerStatus.Halted)
            {
                this._haltRequested = false;
            }
        }

        /// <summary>
        /// Startet die Verarbeitung einer asynchronen Task (ist selbst noch synchron).
        /// </summary>
        /// <param name="worker">Callback-Action für die Task.</param>
        public void RunTask(Action<TaskWorker> worker)
        {
            this._worker = worker;
            this._cancellationTokenSource = new CancellationTokenSource();
            this._cancellationToken = this._cancellationTokenSource.Token;
            this._cancellationToken.Register(() => this.cancelNotification());

            this._asyncWorkerTask = new Task(() => this.runAsync(0));
            this._asyncWorkerTask.Start();
        }

        /// <summary>
        /// Startet die Verarbeitung einer asynchronen Task (ist selbst noch synchron).
        /// </summary>
        /// <param name="worker">Callback-Action für die Task.</param>
        /// <param name="parameters">Parameter für die Callback-Action für die Task.</param>
        public void RunTask(Action<TaskWorker, object> worker, object parameters)
        {
            this._worker2 = worker;
            this._cancellationTokenSource = new CancellationTokenSource();
            this._cancellationToken = this._cancellationTokenSource.Token;
            this._cancellationToken.Register(() => this.cancelNotification());

            this._asyncWorkerTask = new Task(() => this.runAsync(0, parameters));
            this._asyncWorkerTask.Start();
        }

        /// <summary>
        /// Abbrechen der Task.
        /// </summary>
        public void BreakTask()
        {
            if (this._cancellationTokenSource != null)
            {
                this._cancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Wartet auf das Beenden der Task.
        /// </summary>
        public void WaitForTask()
        {
            if (this._asyncWorkerTask != null)
            {
                this._asyncWorkerTask.Wait();
            }
        }

        /// <summary>
        /// Meldet den Task-Fortschritt an Routinen, die sich in
        /// TaskProgressChanged eingehängt haben.
        /// </summary>
        /// <param name="args">Informationen über den Verarbeitungsfortschritt.</param>
        public void OnTaskProgressChanged(CommonProgressChangedEventArgs args)
        {
            if (this._haltRequested)
            {
                this.WorkerStatus = TaskWorkerStatus.Halted;
                //Console.WriteLine("TaskWorker: halted");
            }
            if (this._cancellationToken.IsCancellationRequested)
            {
                this._cancellationToken.ThrowIfCancellationRequested();
            }
            if (TaskProgressChanged != null)
            {
                TaskProgressChanged(null, args);
            }
            if (this._haltRequested)
            {
                while (this._haltRequested)
                {
                    Thread.Sleep(100);
                }
                //Console.WriteLine("TaskWorker: continued");
            }
            this.WorkerStatus = TaskWorkerStatus.Running;
        }

        /// <summary>
        /// Meldet das Task-Ende an Routinen, die sich in
        /// TaskProgressFinished eingehängt haben.
        /// </summary>
        /// <param name="threadException">Eventuell Exception aus der Task oder null.</param>
        public void OnTaskProgressFinished(Exception threadException)
        {
            this.WorkerStatus = TaskWorkerStatus.Ready;
            if (TaskProgressFinished != null)
            {
                TaskProgressFinished(null, threadException);
            }
        }

        /// <summary>
        /// Standard-Konstruktor.
        /// </summary>
        public TaskWorker() { }

        #endregion public members

        #region private members

        private Task _asyncWorkerTask;
        private CancellationTokenSource _cancellationTokenSource { get; set; }
        private CancellationToken _cancellationToken;
        private Action<TaskWorker> _worker;
        private Action<TaskWorker, object> _worker2;
        private bool _haltRequested;

        /// <summary>
        /// Informiert über den Abbruch der Verarbeitung.
        /// </summary>
        private void cancelNotification()
        {
            //Console.WriteLine("TaskTest.cancelNotification - Cancellation request");
            this.OnTaskProgressChanged(new CommonProgressChangedEventArgs(0, null));
        }

        /// <summary>
        /// Eigene Task Action für den Run einer Task.
        /// </summary>
        /// <param name="dummy">Aus Kompatibilitätsgründen, wird hier nicht genutzt.</param>
        /// <param name="parameters">Parameter für die asynchron auszuführende Routine.</param>
        private void runAsync(long dummy, object parameters = null)
        {
            this.runAsync(parameters);
        }

        /// <summary>
        /// Eigene Task Action für den Run einer Task.
        /// </summary>
        /// <param name="parameters">Parameter für die asynchron auszuführende Routine.</param>
        private void runAsync(object parameters)
        {
            if (!this._cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (parameters != null)
                    {
                        this._worker2(this, parameters);
                    }
                    else
                    {
                        this._worker(this);
                    }
                }
                catch (System.OperationCanceledException ex)
                {
                    this.OnTaskProgressFinished(ex);
                }
                catch (Exception ex)
                {
                    this.OnTaskProgressFinished(ex);
                }
                finally
                {
                    this._cancellationTokenSource = null;
                }
            }
            else
            {
                this._cancellationToken.ThrowIfCancellationRequested();
            }
        }

        #endregion private members

    }
}
