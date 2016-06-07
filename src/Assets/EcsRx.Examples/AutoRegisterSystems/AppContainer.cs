﻿using EcsRx.Unity;
using EcsRx.Unity.Components;

namespace Assets.Examples.AutoRegisterSystems
{
    public class AppContainer : EcsRxContainer
    {
        protected override void SetupSystems() { }

        protected override void SetupEntities()
        {
            var defaultPool = PoolManager.GetPool();

            var entity = defaultPool.CreateEntity();
            entity.AddComponent(new ViewComponent());
        }
    }
}