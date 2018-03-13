using EcsRx.Collections;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Unity.Handlers;
using EcsRx.Unity.MonoBehaviours;
using EcsRx.Views.Pooling;
using EcsRx.Views.Systems;
using UnityEngine;
using Zenject;

namespace EcsRx.Unity.Systems
{
    public abstract class UnityPooledViewResolverSystem : PooledViewResolverSystem
    {
        public IInstantiator Instantiator { get; }
        public IEntityCollectionManager CollectionManager { get; }

        public override IViewPool ViewPool { get; }
        
        protected abstract GameObject PrefabTemplate { get; }
        protected abstract int PoolIncrementSize { get; }

        protected IViewPool CreateViewPool()
        { return new ViewPool(PoolIncrementSize, new GameObjectViewHandler(Instantiator, PrefabTemplate)); }

        protected UnityPooledViewResolverSystem(IInstantiator instantiator, IEntityCollectionManager collectionManager)
        {
            Instantiator = instantiator;
            CollectionManager = collectionManager;
            ViewPool = CreateViewPool();
        }
        
        protected abstract void OnPoolStarting();
        protected abstract void OnViewAllocated(GameObject view, IEntity entity);
        protected abstract void OnViewRecycled(GameObject view);

        protected override void OnViewRecycled(object view)
        {
            var gameObject = view as GameObject;
            gameObject.transform.parent = null;

            var entityView = gameObject.GetComponent<EntityView>();
            entityView.Entity = null;
            entityView.EntityCollection = null;

            OnViewRecycled(gameObject);
        }

        protected override void OnViewAllocated(object view, IEntity entity)
        {
            var gameObject = view as GameObject;
            var entityView = gameObject.GetComponent<EntityView>();
            var containingPool = CollectionManager.GetCollectionFor(entity);
            entityView.Entity = entity;
            entityView.EntityCollection = containingPool;

            OnViewAllocated(gameObject, entity);
        }
    }
}