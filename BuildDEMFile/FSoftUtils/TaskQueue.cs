using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSoftUtils {

   /// <summary>
   /// Es kann max. eine festgelegte Anzahl von Tasks parallel abgearbeitet werden.
   /// Für jeden Task kann 
   /// <para>
   /// eine eigene Worker-Func mit unterschiedlicher Parameteranzahl,
   /// </para>
   /// <para>
   /// eine eigene Action für die Fortschrittsmeldung und 
   /// </para>
   /// <para>
   /// eine eigene "Abschluss-Action" angegeben werden.
   /// </para>
   /// <para>
   /// Die Action für die Fortschrittsmeldung wird einfach nur an die Worker-Funktion "durchgereicht". Die "Abschluss-Action" wird mit dem Ergebnis der Worker-Func aufgerufen.
   /// In der Worker-Func sollte periodisch "progress?.Report(dat)" und "cancellationToken.ThrowIfCancellationRequested()" aufgerufen werden. Der 2. Aufruf löst bei 
   /// cancellationToken.IsCancellationRequested eine OperationCanceledException aus.
   /// </para>
   /// <para>
   /// ACHTUNG: StartTask() blockiert solange, bis wieder ein Thread frei ist.
   /// </para>
   /// </summary>
   public class TaskQueue : IDisposable {

      /// <summary>
      /// Anzahl der max. erlaubten Tasks
      /// </summary>
      public int MaxTasks { get; }

      /// <summary>
      /// z.Z. noch mögliche neue Tasks (nur bei <see cref="MaxTasks"/> größer 0)
      /// </summary>
      public int PossibleTasks {
         get {
            return semaphore.CurrentCount;
         }
      }

      /// <summary>
      /// Zähler für die z.Z. noch möglichen neuen Tasks (nur bei <see cref="MaxTasks"/> größer 0)
      /// </summary>
      SemaphoreSlim semaphore;

      /// <summary>
      /// zum abwarten auf das Leeren der Taskliste
      /// </summary>
      ManualResetEventSlim mre;

      /// <summary>
      /// gemeinsames Token
      /// </summary>
      CancellationTokenSource CancellationtokenSource;

      /// <summary>
      /// Anzahl der Threads "in Arbeit" (nur bei <see cref="MaxTasks"/>=0)
      /// </summary>
      int runningthreads;


      /// <summary>
      /// <para>
      /// I.A. ist 0 eine gute Wahl, weil dann max. genausoviel Threads parallel laufen wie logischen Prozessoren vorhanden sind. Damit wird schon in etwa die größte
      /// Beschleunigung erreicht.
      /// </para>
      /// <para>
      /// Bei einem negativen Wert werden (fast) beliebig viele Tasks angenommen. Insgesamt wird die Arbeit aber kaum schneller ausgeführt als bei 0. Nachteilig ist,
      /// dass u.U. alle Tasks mehr oder weniger gleichzeitig fertig werden. Die CPU-Last ist in beiden Fällen etwa gleich (und hoch!).
      /// </para>
      /// </summary>
      /// <param name="max">bei 0 automatisch gleich Anzahl der logischen Prozessoren; bei Werten kleiner 0 ohne Einschränkung</param>
      public TaskQueue(int max = 0) {
         if (max < 0)
            MaxTasks = 0;
         else if (max == 0)
            MaxTasks = LogicalProcessorCores();
         else
            MaxTasks = max;
         semaphore = MaxTasks > 0 ? new SemaphoreSlim(MaxTasks) : null;
         mre = new ManualResetEventSlim();
         CancellationtokenSource = new CancellationTokenSource();
         runningthreads = 0;
      }

      /// <summary>
      /// einige Dinge vor der eigentlichen Task-Funktion
      /// </summary>
      void Taskpre() {
         if (semaphore != null)
            semaphore.Wait(); // wartet, solange semaphore.CurrentCount=0 ist; sonst wird die Anzahl der möglichen Tasks um 1 verringert
         else
            Interlocked.Increment(ref runningthreads);
         mre.Reset();
      }

      /// <summary>
      /// etwas aufräumen nach der Task-Funktion
      /// </summary>
      void Taskpost() {
         if (semaphore != null) {
            semaphore.Release();       // Anzahl der möglichen Tasks wird wieder um 1 erhöht
            if (semaphore.CurrentCount == MaxTasks)
               mre.Set();
         } else {
            Interlocked.Decrement(ref runningthreads);
            if (runningthreads == 0)
               mre.Set();
         }
      }

      /* verschiedene Templates für das Starten, damit einfach eine unterschiedliche Anzahl von Parametern an den Taskworker übergeben
       * werden kann
       * (bei noch mehr Parametern besser eine eigene Datenklasse bilden)
       */

      /// <summary>
      /// startet einen neuen Task wenn die max. anzahl von Tasks noch nicht erreicht ist; andernfalls wird gewartet, bis einer der laufenden Tasks fertig ist
      /// </summary>
      /// <typeparam name="TPROGRESS"></typeparam>
      /// <typeparam name="TEND"></typeparam>
      /// <param name="taskdata1"></param>
      /// <param name="taskworker"></param>
      /// <param name="taskprogress"></param>
      /// <param name="taskend"></param>
      public void StartTask<TPROGRESS, TEND>(Func<CancellationToken, IProgress<TPROGRESS>, TEND> taskworker,
                                             IProgress<TPROGRESS> taskprogress = null,
                                             Action<TEND> taskend = null) {
         Taskpre();
         Task t = Task.Run(() => {
            TEND result = taskworker(CancellationtokenSource.Token, taskprogress);
            taskend?.Invoke(result);   // über das Ende und das Ergebnis informieren
            Taskpost();
         });
      }

      /// <summary>
      /// startet einen neuen Task wenn die max. anzahl von Tasks noch nicht erreicht ist; andernfalls wird gewartet, bis einer der laufenden Tasks fertig ist
      /// </summary>
      /// <typeparam name="TDAT1"></typeparam>
      /// <typeparam name="TPROGRESS"></typeparam>
      /// <typeparam name="TEND"></typeparam>
      /// <param name="taskdata1"></param>
      /// <param name="taskworker"></param>
      /// <param name="taskprogress"></param>
      /// <param name="taskend"></param>
      public void StartTask<TDAT1, TPROGRESS, TEND>(TDAT1 taskdata1,
                                                    Func<TDAT1, CancellationToken, IProgress<TPROGRESS>, TEND> taskworker,
                                                    IProgress<TPROGRESS> taskprogress = null,
                                                    Action<TEND> taskend = null) {
         Taskpre();
         Task t = Task.Run(() => {
            TEND result = taskworker(taskdata1, CancellationtokenSource.Token, taskprogress);
            taskend?.Invoke(result);   // über das Ende und das Ergebnis informieren
            Taskpost();
         });
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TDAT1"></typeparam>
      /// <typeparam name="TDAT2"></typeparam>
      /// <typeparam name="TPROGRESS"></typeparam>
      /// <typeparam name="TEND"></typeparam>
      /// <param name="taskdata1"></param>
      /// <param name="taskdata2"></param>
      /// <param name="taskworker"></param>
      /// <param name="taskprogress"></param>
      /// <param name="taskend"></param>
      public void StartTask<TDAT1, TDAT2, TPROGRESS, TEND>(TDAT1 taskdata1,
                                                           TDAT2 taskdata2,
                                                           Func<TDAT1, TDAT2, CancellationToken, IProgress<TPROGRESS>, TEND> taskworker,
                                                           IProgress<TPROGRESS> taskprogress = null,
                                                           Action<TEND> taskend = null) {
         Taskpre();
         Task t = Task.Run(() => {
            TEND result = taskworker(taskdata1, taskdata2, CancellationtokenSource.Token, taskprogress);
            taskend?.Invoke(result);   // über das Ende und das Ergebnis informieren
            Taskpost();
         });
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TDAT1"></typeparam>
      /// <typeparam name="TDAT2"></typeparam>
      /// <typeparam name="TDAT3"></typeparam>
      /// <typeparam name="TPROGRESS"></typeparam>
      /// <typeparam name="TEND"></typeparam>
      /// <param name="taskdata1"></param>
      /// <param name="taskdata2"></param>
      /// <param name="taskdata3"></param>
      /// <param name="taskworker"></param>
      /// <param name="taskprogress"></param>
      /// <param name="taskend"></param>
      public void StartTask<TDAT1, TDAT2, TDAT3, TPROGRESS, TEND>(TDAT1 taskdata1,
                                                                  TDAT2 taskdata2,
                                                                  TDAT3 taskdata3,
                                                                  Func<TDAT1, TDAT2, TDAT3, CancellationToken, IProgress<TPROGRESS>, TEND> taskworker,
                                                                  IProgress<TPROGRESS> taskprogress = null,
                                                                  Action<TEND> taskend = null) {
         Taskpre();
         Task t = Task.Run(() => {
            TEND result = taskworker(taskdata1, taskdata2, taskdata3, CancellationtokenSource.Token, taskprogress);
            taskend?.Invoke(result);   // über das Ende und das Ergebnis informieren
            Taskpost();
         });
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TDAT1"></typeparam>
      /// <typeparam name="TDAT2"></typeparam>
      /// <typeparam name="TDAT3"></typeparam>
      /// <typeparam name="TDAT4"></typeparam>
      /// <typeparam name="TPROGRESS"></typeparam>
      /// <typeparam name="TEND"></typeparam>
      /// <param name="taskdata1"></param>
      /// <param name="taskdata2"></param>
      /// <param name="taskdata3"></param>
      /// <param name="taskdata4"></param>
      /// <param name="taskworker"></param>
      /// <param name="taskprogress"></param>
      /// <param name="taskend"></param>
      public void StartTask<TDAT1, TDAT2, TDAT3, TDAT4, TPROGRESS, TEND>(TDAT1 taskdata1,
                                                                         TDAT2 taskdata2,
                                                                         TDAT3 taskdata3,
                                                                         TDAT4 taskdata4,
                                                                         Func<TDAT1, TDAT2, TDAT3, TDAT4, CancellationToken, IProgress<TPROGRESS>, TEND> taskworker,
                                                                         IProgress<TPROGRESS> taskprogress = null,
                                                                         Action<TEND> taskend = null) {
         Taskpre();
         Task t = Task.Run(() => {
            TEND result = taskworker(taskdata1, taskdata2, taskdata3, taskdata4, CancellationtokenSource.Token, taskprogress);
            taskend?.Invoke(result);   // über das Ende und das Ergebnis informieren
            Taskpost();
         });
      }

      /// <summary>
      /// wartet, bis die Warteschlange völlig leer ist
      /// </summary>
      /// <returns></returns>
      public void Wait4EmptyQueue() {
         mre.Wait();
      }

      /// <summary>
      /// Communicates a request for cancellation, and specifies whether remaining callbacks and cancelable operations should be processed if an exception occurs.
      /// </summary>
      /// <param name="throwOnFirstException">true if exceptions should immediately propagate; otherwise, false</param>
      public void Cancel(bool throwOnFirstException = true) {
         CancellationtokenSource.Cancel(throwOnFirstException);
      }

      // ACHTUNG: System.Management.dll einbinden

      ///// <summary>
      ///// Anzahl der physischen Prozessoren (i.A. 1)
      ///// </summary>
      ///// <returns></returns>
      //public static int PhysicalProcessors() {
      //   foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get()) {
      //      return int.Parse(item["NumberOfProcessors"].ToString());
      //   }
      //   return 0;
      //}
      ///// <summary>
      ///// Anzahl der physischen Prozessorkerne (i.A. ausschlaggebend für sinnvolles Multithreading)
      ///// </summary>
      ///// <returns></returns>
      //public static int ProcessorCores() {
      //   int coreCount = 0;
      //   foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get()) {
      //      coreCount += int.Parse(item["NumberOfCores"].ToString());
      //   }
      //   return coreCount;
      //}
      /// <summary>
      /// Anzahl der logischen Prozessorkerne
      /// </summary>
      /// <returns></returns>
      public static int LogicalProcessorCores() {
         //foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get()) {
         //   return int.Parse(item["NumberOfLogicalProcessors"].ToString());
         //}
         return System.Environment.ProcessorCount;
      }

      #region Implementierung der IDisposable-Schnittstelle

      ~TaskQueue() {
         Dispose(false);
      }

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
               CancellationtokenSource.Cancel();
               CancellationtokenSource.Dispose();
               Wait4EmptyQueue();
               mre.Dispose();
               semaphore?.Dispose();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }

}
