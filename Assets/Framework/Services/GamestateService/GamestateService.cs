using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Services.GamestateService {
    public class GamestateService : ServiceBase, IGamestateService {
        Dictionary<Type, IGamestate> gamestates;

        IGamestate currentGamestate;
        IGamestate nextGamestate;

        /// <summary>
        /// A worker thread for ticking currentGamestate.TickThreaded()
        /// </summary>
        Thread workerThread;
        /// <summary>
        /// Thread barrier to rendevouz mainThread and workerthread
        /// </summary>
        Barrier threadBarrier;

        /// <summary>
        /// Current delta time
        /// </summary>
        float deltaTime;
        
        class WorkerThreadData {
            public float deltaTime;
        }
        WorkerThreadData workerThreadData;

        public override async Task InitializeAsync() {
            await base.InitializeAsync();

            gamestates = new Dictionary<Type, IGamestate>();
            currentGamestate = null;
            nextGamestate = null;

            //Create worker thread data object
            workerThreadData = new WorkerThreadData();

            //barrier to wait for two threads: mainThread and workerThread. On rendezvouz: Keep both threads "locked" and call ThreadRendezvouz(). Once ThreadRendezvouz() is finished, all threads will continue
            threadBarrier = new Barrier(2, ThreadRendezvouz);
            //Create worker thread
            workerThread = new Thread(WorkerThread);
            workerThread.Start();
        }

        public void Register(IGamestate gamestate) {
            gamestates.Add(gamestate.GetType(), gamestate);
        }

        public void SwitchTo<TGamestate>(object context = null) where TGamestate : IGamestate {
            nextGamestate = gamestates[typeof(TGamestate)];
            nextGamestate.SetContext(context);
        }

        public async Task TickAsync(float deltaTime) {
            if (nextGamestate != null) {
                //Exit from current state
                if(currentGamestate != null) {
                    currentGamestate.PreExit();

                    await currentGamestate.OnExitAsync();

                    currentGamestate.PostExit();
                }
                //Switch to next gamestate
                nextGamestate.PreEnter();
                currentGamestate = nextGamestate;
                nextGamestate = null;
                await currentGamestate.OnEnterAsync();
                currentGamestate.PostEnter();
            }

            //Tick current gamestate
            if(currentGamestate != null) {
                this.deltaTime = deltaTime;
                //Fire barrier and wait for worker thread to also fire the barrier. Worker thread will call currentGamestate.TickThreaded()
                threadBarrier.SignalAndWait();
                //Tick current gamestate
                currentGamestate.Tick(this.deltaTime);
            }
        }

        /// <summary>
        /// Worker thread
        /// </summary>
        void WorkerThread() {
            while (Core.IsRunning) {
                //Fire barrier and wait for main thread to also fire the barrier
                threadBarrier.SignalAndWait();

                currentGamestate.TickThreaded(workerThreadData.deltaTime);
            }
            threadBarrier.Dispose();
            Logger.Log("GamestateService: Disposed worker thread and rendezvouz-barrier.");
        }

        /// <summary>
        /// Called when worker thread and main thread rendevouz
        /// </summary>
        /// <param name="barrier"></param>
        void ThreadRendezvouz(Barrier barrier) {
            Logger.Log("-> Thread Rendezvouz");
            //Delta time
            workerThreadData.deltaTime = deltaTime;
            //TODO: Copy user-input from mainThread to workerThread

            //TODO: Grab output from workerThread and copy to main thread for processing (i.e. rendering-data).
            //NOTE: Maybe don't do that here, but do it in parallel while the workerThread is working. Let workerthread wait at specific position until main thread grabbed data before workerThread will overwrite it.
            // workerThread: do stuff
            // mainThread: process or copy "someData" and signal workerThread that data was copied. NOTE: Check which is faster (direct process or copy). Directly processing MIGHT be faster. Depends on how long it takes and if the workerThread still has work to do while mainThread processes data
            // workerThread: Wait for mainThread to grab someData
            // workerThread: change someData

        }
    }
}