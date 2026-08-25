using _Game.CodeBase.Features.BuildingModule.Scripts.Data;

namespace _Game.CodeBase.Features.BuildingModule.Scripts.Supports
{
    public class SupportFactory : ISupportFactory
    {
        private readonly SupportPool _pool;
        private readonly SupportsConfig _config;

        public SupportFactory(SupportPool pool, SupportsConfig config)
        {
            _pool = pool;
            _config = config;
        }

        public Support Create(SupportData data, SupportMaterialLevel material)
        {
            var support = _pool.Spawn();
            support.Setup(data, material);
            return support;
        }
    }
}