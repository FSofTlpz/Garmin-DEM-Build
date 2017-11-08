using System.Threading;

namespace BuildDEMFile {
   class ThreadPoolExt {

      /// <summary>
      /// Anzahl der Threads "in Arbeit"
      /// </summary>
      int runningthreads;
      /// <summary>
      /// für die Funktion <see cref="Wait4NotWorking"/>
      /// </summary>
      ManualResetEvent working;

      WaitCallback workerfunc;

      protected WaitCallback msgfunc;

      int _ExceptionCount;

      /// <summary>
      /// kann bei der Ausgabe verwendet werden
      /// </summary>
      protected object msglocker = new object();


      /// <summary>
      /// Threadpool verwenden
      /// <para>Die 1. Funktion kann verwendet werden, um Infos zurück zu senden.</para>
      /// <para>Die 2. Funktion kann als direkt externe Funktion für die Arbeit verwendet werden. I.A. sollte aber eine abgeleitete Klasse verwendet werden.</para>
      /// </summary>
      /// <param name="msgfunc"></param>
      /// <param name="workfunc"></param>
      public ThreadPoolExt(WaitCallback msgfunc = null, WaitCallback workfunc = null) {
         working = new ManualResetEvent(false);
         workerfunc = workfunc != null ? workfunc : DoWork;
         this.msgfunc = msgfunc;
         _ExceptionCount = 0;
      }

      /// <summary>
      /// startet einen Thread mit den Parametern
      /// </summary>
      /// <param name="para"></param>
      public void Start(object para) {
         Interlocked.Increment(ref runningthreads);
         working.Reset();
         ThreadPool.QueueUserWorkItem(DoWorkFrame, para);
      }

      /// <summary>
      /// wartet, bis kein Thread mehr arbeitet
      /// </summary>
      public void Wait4NotWorking() {
         working.WaitOne();
      }

      protected void DoWorkFrame(object para) {
         workerfunc(para);
         Interlocked.Decrement(ref runningthreads);
         if (runningthreads == 0)
            working.Set();
      }

      protected virtual void DoWork(object para) { }

      /// <summary>
      /// akt. Wert des Zählers
      /// </summary>
      /// <returns></returns>
      public int ExceptionCount {
         get {
            return Interlocked.Add(ref _ExceptionCount, 0);
         }
      }

      /// <summary>
      /// erhöht den Zähler um 1
      /// </summary>
      protected void IncrementExceptionCount() {
         Interlocked.Increment(ref _ExceptionCount);
      }

      /// <summary>
      /// setzt den Zähler auf 0 zurück
      /// </summary>
      public void ClearExceptionCount() {
         Interlocked.Add(ref _ExceptionCount, -_ExceptionCount);
      }

   }
}
