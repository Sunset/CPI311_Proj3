using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SkinnedModel;

namespace Common
{
    public class AnimatedObject : ModelObject
    {
        public AnimationPlayer AnimationPlayer { get; set; }

        public AnimatedObject()
        {
            AnimationPlayer = null;
        }

        private Model _model;
        public Model Model
        {
            set
            {
                _model = value;
                SkinningData skinningData = _model.Tag as SkinningData;
                if (skinningData == null)
                    return;
                AnimationPlayer = new AnimationPlayer(skinningData);
            }

            get
            {
                return _model;
            }
        }

        public void StartAnimation()
        {

        }

        public void StopAnimation()
        {

        }

        public virtual void Update(GameTime gameTime)
        {
            // Call update for AnimationPlayer
        }

        public override void Draw(Effect effect)
        {
            base.Draw(effect);
        }

        public override void Draw(Matrix view, Matrix projection)
        {
            // Fix this!
        }
    }
}
