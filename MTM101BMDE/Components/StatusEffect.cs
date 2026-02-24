using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Components
{

    public abstract class StatusEffectFactory
    {
        public abstract StatusEffect CreateStatusEffect(string id);

        public abstract StatusEffect CreateTimedStatusEffect(string id, float time);
    }

    public abstract class TimedStatusEffectFactory : StatusEffectFactory
    {
        public override StatusEffect CreateTimedStatusEffect(string id, float time)
        {
            TimedStatusEffect statEffect = (TimedStatusEffect)CreateStatusEffect(id);
            throw new NotImplementedException();
        }
    }

    public abstract class StatusEffect
    {
        public string id { get; protected set; }
        protected Entity entity;
        protected NPC npc;
        protected PlayerManager pm;
        public virtual void OnAppliedEntity(Entity entity)
        {
            this.entity = entity;
        }
        public virtual void OnAppliedNPC(NPC npc)
        {
            OnAppliedEntity(npc.Entity);
            this.npc = npc;
        }
        public virtual void OnAppliedPlayer(PlayerManager pm)
        {
            OnAppliedEntity(pm.plm.Entity);
            this.pm = pm;
        }

        public virtual void OnRemoved()
        {

        }

        public virtual void Update()
        {

        }

        public abstract StatusEffect MakeCopy();

        public virtual void AddTo(StatusEffectManager man)
        {
            man.AddStatusEffect(MakeCopy());
        }
    }

    public abstract class TimedStatusEffect : StatusEffect
    {
        public abstract Sprite gaugeIcon { get; }
        public abstract bool allowMultiple { get; }
        public abstract bool resets { get; }
        protected HudGauge currentGauge;
        public float timeRemaining = 0f;
        public float totalTime = 0f;

        public override void OnAppliedPlayer(PlayerManager pm)
        {
            base.OnAppliedPlayer(pm);
            if (gaugeIcon != null)
            {
                currentGauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeIcon, totalTime);
            }
        }

        public virtual void ResetTime(float newTime)
        {
            newTime = Mathf.Max(newTime, timeRemaining);
            totalTime = newTime;
            timeRemaining = newTime;
            if (currentGauge == null) return;
            currentGauge.SetValue(totalTime, timeRemaining);
        }

        public override void Update()
        {
            timeRemaining -= Time.deltaTime * entity.Ec.EnvironmentTimeScale;
            if (currentGauge == null) return;
            currentGauge.SetValue(totalTime, timeRemaining);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            if (currentGauge != null)
            {
                currentGauge.Deactivate();
            }
        }

        public override void AddTo(StatusEffectManager man)
        {
            if (allowMultiple)
            {
                base.AddTo(man);
            }
        }
    }
}
