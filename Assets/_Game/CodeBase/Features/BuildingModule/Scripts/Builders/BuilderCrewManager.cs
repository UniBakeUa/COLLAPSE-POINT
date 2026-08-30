using System;
using System.Collections.Generic;
using _Game.CodeBase.Core.TimeControllerModule.Scripts;
using _Game.CodeBase.Features.BuildingModule.Scripts.Rooms;
using _Game.CodeBase.Features.BuildingModule.Scripts.Supports;

namespace _Game.CodeBase.Features.BuildingModule.Scripts
{
    public class BuilderCrewManager : IDisposable
    {
        private readonly SupportsGenerator _supportGenerator;
        private readonly SpeedController _speedController;
        private readonly List<BuilderCrew> _crews = new();
        private readonly BuilderCrewConfig _buildersConfig;

        public IReadOnlyList<BuilderCrew> Crews => _crews;

        public BuilderCrewManager(SupportsGenerator supportGenerator,SpeedController speedController,BuilderCrewConfig buildersConfig)
        {
            _supportGenerator = supportGenerator;
            _speedController = speedController;
            _buildersConfig = buildersConfig;
        }

        public BuilderCrew HireCrew()
        {
            var crew = new BuilderCrew(_supportGenerator, _speedController,_buildersConfig);
            _crews.Add(crew);
            return crew;
        }

        public void AssignCrewToRoom(BuilderCrew crew, Room room)
        {
            crew.AssignToRoom(room);
        }

        public void DismissCrew(BuilderCrew crew)
        {
            crew.Dispose();
            _crews.Remove(crew);
        }

        public void Dispose()
        {
            foreach (var crew in _crews)
                crew.Dispose();

            _crews.Clear();
        }
    }
}