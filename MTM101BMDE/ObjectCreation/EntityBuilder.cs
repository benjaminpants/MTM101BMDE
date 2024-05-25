using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.ObjectCreation
{
    public class EntityBuilder
    {

        static readonly FieldInfo _rendererBase = AccessTools.Field(typeof(Entity), "rendererBase");
        static readonly FieldInfo _collider = AccessTools.Field(typeof(Entity), "collider");
        static readonly FieldInfo _trigger = AccessTools.Field(typeof(Entity), "trigger");
        static readonly FieldInfo _externalActivity = AccessTools.Field(typeof(Entity), "externalActivity");
        static readonly FieldInfo _collisionLayerMask = AccessTools.Field(typeof(Entity), "collisionLayerMask");

        string entityName = "Unnamed";
        float baseRadius = 1f;
        float triggerRadius = 0f;
        string layer;
        LayerMask collisionLayerMask;
        Func<Entity, Transform> addRenderBaseFunc;

        public Entity Build()
        {
            GameObject entityObject = new GameObject();
            entityObject.ConvertToPrefab(false);
            entityObject.name = entityName;
            entityObject.layer = LayerMask.NameToLayer(layer);
            Entity entity = entityObject.AddComponent<Entity>();

            CapsuleCollider mainCollider = entityObject.AddComponent<CapsuleCollider>();
            mainCollider.radius = baseRadius;

            CapsuleCollider triggerCollider = entityObject.AddComponent<CapsuleCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = (triggerRadius > 0f) ? triggerRadius : baseRadius;
            triggerCollider.enabled = (triggerRadius > 0f);

            _trigger.SetValue(entity, triggerCollider);
            _collider.SetValue(entity, mainCollider);
            _collisionLayerMask.SetValue(entity, collisionLayerMask);
            _externalActivity.SetValue(entity, entityObject.AddComponent<ActivityModifier>());

            if (addRenderBaseFunc != null)
            {
                _rendererBase.SetValue(entity, addRenderBaseFunc.Invoke(entity));
            }

            return entity;
        }

        /// <summary>
        /// Set's the base collision radius of the entity.
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public EntityBuilder SetBaseRadius(float radius)
        {
            baseRadius = radius;
            return this;
        }

        /// <summary>
        /// Adds a function that will be called to set/create the rendererBase for the entity.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public EntityBuilder AddRenderbaseFunction(Func<Entity, Transform> function)
        {
            addRenderBaseFunc = function;
            return this;
        }

        /// <summary>
        /// Adds a trigger with the specified radius to the entity.
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public EntityBuilder AddTrigger(float radius)
        {
            triggerRadius = radius;
            return this;
        }

        /// <summary>
        /// Sets the name of the entity's GameObject.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EntityBuilder SetName(string name)
        {
            entityName = name;
            return this;
        }

        /// <summary>
        /// Sets the layer of the entity.
        /// </summary>
        /// <param name="layer">The name of the layer.</param>
        /// <returns></returns>
        public EntityBuilder SetLayer(string layer)
        {
            this.layer = layer;
            return this;
        }

        /// <summary>
        /// Sets the layer collision mask of the entity.
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public EntityBuilder SetLayerCollisionMask(LayerMask mask)
        {
            collisionLayerMask = mask;
            return this;
        }
    }
}
