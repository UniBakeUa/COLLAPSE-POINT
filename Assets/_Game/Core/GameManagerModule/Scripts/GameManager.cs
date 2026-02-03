using System;
using _Game.Core.GameManagerModule.Scripts.States;
using _Game.Core.StateMashineModule.Scripts;
using Zenject;

namespace _Game.Core.GameManagerModule.Scripts
{
    public class GameManager : IInitializable, IDisposable
    {
        public StateMachineBehaviour<StateBase> GameStateMachine { get; private set; }

        public void Initialize()
        {
            GameStateMachine = new StateMachineBehaviour<StateBase>();

            GameStateMachine.SetStates(new System.Collections.Generic.List<StateBase>
            {
                new LoadingState(),
                new MenuState(),
                new PausedState(),
                new PlayingState(),
                new GameoverState(),
                new VictoryState()
            });

            GameStateMachine.Enter<LoadingState>();
        }

        public bool IsCurrentState<T>() where T : StateBase
        {
            return GameStateMachine.GetCurrentState() == typeof(T);
        }

        public void LoadGame()
        {
            GameStateMachine.Enter<LoadingState>();
        }

        public void GoToMenu()
        {
            GameStateMachine.Enter<MenuState>();
        }

        public void PauseGame()
        {
            GameStateMachine.Enter<PausedState>();
        }

        public void ResumeGame()
        {
            GameStateMachine.Enter<PlayingState>();
        }
        public void LoseGame()
        {
            GameStateMachine.Enter<GameoverState>();
        }
        public void WinGame()
        {
            GameStateMachine.Enter<VictoryState>();
        }
        public void Dispose()
        {
            GameStateMachine.Dispose();
        }
    }
}