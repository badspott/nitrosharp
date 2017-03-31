﻿using HoppyFramework;
using System.Numerics;

namespace ProjectHoppy.Graphics
{
    public class AnimationSystem : EntityProcessingSystem
    {
        public AnimationSystem()
            : base(typeof(FloatAnimation), typeof(ColorAnimation))
        {

        }

        public override void Process(Entity entity, float deltaMilliseconds)
        {
            foreach (var animation in entity.GetComponents<FloatAnimation>())
            {
                if (animation.IsEnabled)
                {
                    bool incr = animation.FinalValue > animation.InitialValue;
                    var delta = (animation.FinalValue - animation.InitialValue) * (deltaMilliseconds / (float)animation.Duration.TotalMilliseconds);
                    animation.CurrentValue += delta;

                    if (incr && animation.CurrentValue >= animation.FinalValue || !incr && animation.CurrentValue <= animation.FinalValue)
                    {
                        animation.CurrentValue = animation.FinalValue;
                        animation.IsEnabled = false;
                    }

                    animation.PropertySetter(animation.TargetComponent, animation.CurrentValue);
                }
            }

            //foreach (var colorAnimation in entity.GetComponents<ColorAnimation>())
            //{
            //    if (colorAnimation.IsEnabled)
            //    {
            //        ProcessColorAnimation(entity, colorAnimation, deltaMilliseconds);
            //    }
            //}
        }

        private void ProcessColorAnimation(Entity e, ColorAnimation animation, float deltaMilliseconds)
        {
            Vector4 delta = (Vector4)animation.FinalValue * (deltaMilliseconds / (float)animation.Duration.TotalMilliseconds);
            animation.CurrentValue += delta;

            if (animation.CurrentValue.R >= animation.FinalValue.R)
            {
                animation.CurrentValue = animation.InitialValue;
                //animation.CurrentValue = animation.FinalValue;
                //animation.IsEnabled = false;
            }

            animation.PropertySetter(e, animation.CurrentValue);
        }
    }
}
