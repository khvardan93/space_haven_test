using System;
using System.Collections.Generic;
using Economy;
using Laser;
using Prison;
using Save;
using Simulation;

namespace Core
{
    public sealed class GameModel : IDisposable
    {
        public GameSetup Setup { get; private set; }
        public GameEconomy Economy { get; private set; }
        public LaserEnergy Laser { get; private set; }
        public PrisonBlock Prison { get; private set; }
        public GameSimulation Simulation { get; private set; }
        public RoomCatalog Catalog { get; private set; }
        public RateTracker Rates { get; private set; }

        public GameModel(GameSetup setup)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            Setup = setup;
            Catalog = new RoomCatalog(setup.Rooms);
            Economy = new GameEconomy();
            Laser = new LaserEnergy(setup.Laser);
            Prison = new PrisonBlock(setup.SlotCount, Economy, Laser, setup.Laser);
            Simulation = new GameSimulation(Economy, Laser, Prison);
            Rates = new RateTracker(Prison, Simulation);

            Economy.Add(setup.StartingBalances);
        }

        public GameModel(GameSetup setup, GameState state, out List<string> warnings)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            Setup = setup;
            Catalog = new RoomCatalog(setup.Rooms);
            Economy = new GameEconomy();
            Laser = new LaserEnergy(setup.Laser);
            Prison = new PrisonBlock(setup.SlotCount, Economy, Laser, setup.Laser);
            Simulation = new GameSimulation(Economy, Laser, Prison);
            Rates = new RateTracker(Prison, Simulation);

            warnings = SaveMapper.Restore(state, this);
        }

        public void Dispose()
        {
            Rates?.Dispose();
        }
    }
}
