using System;
using System.Collections.Generic;
using Configs;
using Economy;
using Laser;
using Prison;
using Save;
using Simulation;

namespace Core
{
    public sealed class GameModel : IDisposable
    {
        public GameConfigs Setup { get; private set; }
        public GameEconomy Economy { get; private set; }
        public LaserEnergy Laser { get; private set; }
        public PrisonBlock Prison { get; private set; }
        public GameSimulation Simulation { get; private set; }
        public RoomCatalog Catalog { get; private set; }
        public RateTracker Rates { get; private set; }

        private GameModel()
        {
        }

        public static GameModel CreateNew(GameConfigs setup)
        {
            var model = CreateEmpty(setup);
            model.Economy.Add(setup.StartingBalances);
            return model;
        }

        public static GameModel FromSave(GameConfigs setup, GameState state, out List<string> warnings)
        {
            var model = CreateEmpty(setup);
            warnings = SaveMapper.Restore(state, model);
            return model;
        }

        public void Dispose()
        {
            if (Rates != null)
                Rates.Dispose();
        }

        private static GameModel CreateEmpty(GameConfigs setup)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var model = new GameModel();
            model.Setup = setup;
            model.Catalog = new RoomCatalog(setup.Rooms);
            model.Economy = new GameEconomy();
            model.Laser = new LaserEnergy(setup.LaserSettings);
            model.Prison = new PrisonBlock(setup.SlotCount, model.Economy, model.Laser, setup.LaserSettings);
            model.Simulation = new GameSimulation(model.Economy, model.Laser, model.Prison);
            model.Rates = new RateTracker(model.Prison, model.Simulation);
            return model;
        }
    }
}
