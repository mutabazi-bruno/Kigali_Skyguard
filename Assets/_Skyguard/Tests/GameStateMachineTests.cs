using System.Collections.Generic;
using NUnit.Framework;
using Skyguard.Core;

namespace Skyguard.Tests
{
    public class GameStateMachineTests
    {
        class FakeState : IGameState
        {
            readonly List<string> log;
            readonly string name;

            public FakeState(string name, GameStateId id, List<string> log)
            {
                this.name = name;
                this.log = log;
                Id = id;
            }

            public GameStateId Id { get; }
            public void Enter() => log.Add(name + " enter");
            public void Tick(float deltaTime) => log.Add(name + " tick");
            public void Exit() => log.Add(name + " exit");
        }

        [Test]
        public void ChangingStateExitsTheOldOneFirst()
        {
            var log = new List<string>();
            var machine = new GameStateMachine();
            machine.ChangeState(new FakeState("menu", GameStateId.MainMenu, log));
            machine.ChangeState(new FakeState("wave", GameStateId.Wave, log));

            CollectionAssert.AreEqual(new[] { "menu enter", "menu exit", "wave enter" }, log);
        }

        [Test]
        public void PauseOverlayResumesWithoutReenteringTheWave()
        {
            var log = new List<string>();
            var machine = new GameStateMachine();
            machine.ChangeState(new FakeState("wave", GameStateId.Wave, log));
            machine.PushOverlay(new FakeState("pause", GameStateId.Paused, log));
            machine.Tick(0.1f);
            machine.PopOverlay();
            machine.Tick(0.1f);

            CollectionAssert.AreEqual(new[] { "wave enter", "pause enter", "pause tick", "pause exit", "wave tick" }, log);
            Assert.AreEqual(GameStateId.Wave, machine.Current.Id);
        }

        [Test]
        public void QuittingFromPauseClosesBothStates()
        {
            var log = new List<string>();
            var machine = new GameStateMachine();
            machine.ChangeState(new FakeState("wave", GameStateId.Wave, log));
            machine.PushOverlay(new FakeState("pause", GameStateId.Paused, log));
            machine.ChangeState(new FakeState("menu", GameStateId.MainMenu, log));

            CollectionAssert.AreEqual(new[] { "wave enter", "pause enter", "pause exit", "wave exit", "menu enter" }, log);
            Assert.IsFalse(machine.HasOverlay);
        }
    }
}
