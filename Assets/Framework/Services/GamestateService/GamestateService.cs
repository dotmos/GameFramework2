using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace Framework.Services.GamestateService {
	public class GamestateService : ServiceBase, IGamestateService {
		Dictionary<Type, IGamestate> gamestates;

		IGamestate currentGamestate;
		IGamestate nextGamestate;

		/// <summary>
		/// flag to indicate that the next gamestate should be pushed ontop of the current gamestate that gets suspended
		/// </summary>
		private bool pushNextGameState = false;
		/// <summary>
		/// flag to indicate to exit the current gamestate and to resume the gamestate ontop of the stack
		/// </summary>
		private bool popGameState = false;

		/// <summary>
		/// flag to indicate that the all gamestates of the stack should be run top to bottom through the gamestate exit lifecycle
		/// </summary>
		private bool throwAwayStackGamestates;

		public IGamestate Current => currentGamestate;
		public IGamestate Next => nextGamestate;

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

		/// <summary>
		/// flag to indicate if the current gamestate finished the whole startup-lifecycle
		/// </summary>
		bool currentGamestateStarted = false;
		/// <summary>
		/// TaskCompletion to wait for 
		/// </summary>
		TaskCompletionSource<IGamestate> waitForGSStarted;
		object lockWaitForGSStarted = new object();

		/// <summary>
		/// currently (someone) is waiting for this specific task to be finished
		/// </summary>
		public Type waitForSpecificGamestateType = null;

		/// <summary>
		/// Is the current gamestate started (finished the whole startup-lifecycle)
		/// </summary>
		public bool IsCurrentGamestateStarted => currentGamestateStarted;

		/// <summary>
		/// Storing pushed gamestates
		/// </summary>
		Stack<IGamestate> gamestateStack = new Stack<IGamestate>();

		public override async Task InitializeAsync() {
			await base.InitializeAsync();

			await Task.Delay(1000);

			gamestates = new Dictionary<Type, IGamestate>();
			currentGamestate = null;
			nextGamestate = null;

			//Create worker thread data object
			workerThreadData = new WorkerThreadData();

			//barrier to wait for two threads: mainThread and workerThread. On rendezvouz: Keep both threads "locked" and call ThreadRendezvouz(). Once ThreadRendezvouz() is finished, all threads will continue
			threadBarrier = new Barrier(2, ThreadRendezvouz);
			//Create worker thread
			workerThread = new Thread(WorkerThread);
			workerThread.Name = "GamestateService-WorkerThread";
			workerThread.Start();
		}

		public void Register(IGamestate gamestate) {
			gamestates.Add(gamestate.GetType(), gamestate);
		}

		public void SwitchTo<TGamestate>(object context = null, bool removeStackedGS=false) where TGamestate : IGamestate {
			SwitchTo(typeof(TGamestate), context, removeStackedGS);
		}

		public void SwitchTo(Type gameStateType,object context = null, bool killGamestatesOnStack=false) {
			if (killGamestatesOnStack) {
				throwAwayStackGamestates = true;
			} else {
				Assert.IsTrue(gamestateStack.Count == 0, "SwitchTo not allowed(yet) with gamestates living on the stack! If you want to force the switch, plz use SwitchTo(gs,ctx,force)");
			}
			nextGamestate = gamestates[gameStateType];
			nextGamestate.SetContext(context);
		}



		/// <summary>
		/// Push new gamestate on the stack, suspending the current and creating the new one
		/// </summary>
		/// <typeparam name="TGamestate"></typeparam>
		/// <param name="context"></param>
		public void PushGamestate<TGamestate>(object context = null) where TGamestate : IGamestate {
			nextGamestate = gamestates[typeof(TGamestate)];
			nextGamestate.SetContext(context);
			pushNextGameState = true;
		}

		/// <summary>
		/// Removing the current gamestate, set the next Gamestate from stack as active and call its resume-callback
		/// </summary>
		public void PopGamestate() {
			Assert.IsTrue(gamestateStack.Count > 0, "No gamestate on the stack! Cannot pop!");
			popGameState = true;
		}

		public async Task TickAsync(float deltaTime) {
			if (throwAwayStackGamestates) {
				throwAwayStackGamestates = false;
				await RunExitGamestateSequence(currentGamestate);
				while (gamestateStack.TryPop(out IGamestate gs)) {
					await RunExitGamestateSequence(gs);
				}
			}


			if (nextGamestate != null) {
				if (pushNextGameState) {
					// push to stack
					await currentGamestate.OnSuspend(); // tell the gamestate it got suspended
					gamestateStack.Push(currentGamestate); // store current gamestate in the stack
					pushNextGameState = false;
				} else {
					//Exit from current state
					if (currentGamestate != null) {
						await RunExitGamestateSequence(currentGamestate);
					}
				}
				
				currentGamestate = nextGamestate;
				currentGamestateStarted = false;
				//Switch to next gamestate
				nextGamestate.PreEnter();
				currentGamestate = nextGamestate; 
				nextGamestate = null;
				await currentGamestate.OnEnterAsync();
				currentGamestate.PostEnter();
				currentGamestateStarted = true;
				// someone did wait for a gamestate to be started
				lock (lockWaitForGSStarted) {
					if (waitForGSStarted != null) {
						if (waitForSpecificGamestateType == null || currentGamestate.GetType() == waitForSpecificGamestateType) {
							// mark the underlying task completed
							waitForSpecificGamestateType = null;
							waitForGSStarted.SetResult(currentGamestate);
							waitForGSStarted = null;
						}
					}
				}
			}

			if (popGameState) {
				popGameState = false;
				// exit the current gamestate
				if (currentGamestate != null) {
					await RunExitGamestateSequence(currentGamestate);
				}

				currentGamestate = gamestateStack.Pop();
				// tell the gamestate it got resumed
				await currentGamestate.OnResume();
			}

			//Tick current gamestate
			if (currentGamestate != null) {
				this.deltaTime = deltaTime;
				//Fire barrier and wait for worker thread to also fire the barrier. Worker thread will call currentGamestate.TickThreaded()
				threadBarrier.SignalAndWait();
				//Tick current gamestate
				currentGamestate.Tick(this.deltaTime);
			}
		}


		/// <summary>
		/// Exit lifecycle for gamestates
		/// </summary>
		/// <param name="gamestate"></param>
		/// <returns></returns>
		private async Task RunExitGamestateSequence(IGamestate gamestate) {
			currentGamestate.PreExit();

			await currentGamestate.OnExitAsync();

			currentGamestate.PostExit();
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
			//Logger.Log("-> Thread Rendezvouz");
			//Delta time
			workerThreadData.deltaTime = deltaTime;

			currentGamestate.OnThreadRendezvouz();

			//TODO: Copy user-input from mainThread to workerThread

			//TODO: Grab output from workerThread and copy to main thread for processing (i.e. rendering-data).
			//NOTE: Maybe don't do that here, but do it in parallel while the workerThread is working. Let workerthread wait at specific position until main thread grabbed data before workerThread will overwrite it.
			// workerThread: do stuff
			// mainThread: process or copy "someData" and signal workerThread that data was copied. NOTE: Check which is faster (direct process or copy). Directly processing MIGHT be faster. Depends on how long it takes and if the workerThread still has work to do while mainThread processes data
			// workerThread: Wait for mainThread to grab someData
			// workerThread: change someData

		}

		/// <summary>
		/// Wait until a new(!!) gamestate is started!
		/// If the current gamestate is still in startup this will wait for the current to startup
		/// CAUTION: this will not actively wait for you! You need to await on the returned task
		/// 
		/// This is mainly supposed to be used for automatic testing
		/// </summary>
		/// <returns></returns>
		public Task<IGamestate> WaitForGamestateStarted() {
			lock (lockWaitForGSStarted) {
				if (waitForGSStarted == null) {
					waitForGSStarted = new TaskCompletionSource<IGamestate>();
				}
			}
			return waitForGSStarted.Task;
		}

		/// <summary>
		/// Wait for a specific gamestate being started!
		/// CAUTION: this will not actively wait for you! You need to await on the returned task
		/// 
		/// This is mainly supposed to be used for automatic testing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public Task<IGamestate> WaitForGamestateStarted<T>() {
			Type newType = typeof(T);
			lock (lockWaitForGSStarted) {
				Assert.IsTrue(waitForSpecificGamestateType == null || waitForSpecificGamestateType == typeof(T));
				waitForSpecificGamestateType = typeof(T);
			}
			return WaitForGamestateStarted();
		}
	}
}