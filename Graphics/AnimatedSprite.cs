using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rockfan.Framework.Graphics
{
    public class AnimatedSprite : Sprite
    {
        public int FrameCount { get { return Frames.Count; } }
        public float FrameRate { get; set; }
        public float AnimationLegth { get; set; }

        public List<TextureInfo> Frames { get; private set; }

        public AnimatedSprite()
            : base(null) {

            Frames = new List<TextureInfo>();
        }

        /// <summary>
        /// Swaps the position of the note over an amout of time in seconds
        /// </summary>
        /// <param name="delta">a float value indicating the amount of time in seconds it should take to swap the position of this note</param>
        public void Swap(float duration)
        {
            if (this.X == 0 || SwapDurationRemaining > 0)
                return;

            SwapDuration = duration;
            SwapDurationRemaining = duration;

            StartPosition = this.X;
            EndingPosition = this.X * -1;
        }

        public void UpdateState(float delta)
        {
            Update(delta);
        }

        protected override void Update(float delta)
        {
            var mod = (1.0f / FrameRate) * FrameCount;
            CurrentAnimationTime = (CurrentAnimationTime + delta) % mod;

            for (int i = 0; i < FrameCount; i++)
            {
                var frameTime = (1.0f / FrameRate) * i;

                if (CurrentAnimationTime <= frameTime)
                    break;

                CurrentFrameTexture = Frames[i];
            }

            if (CurrentFrameTexture != null)
            {
                this.TopTextureCoordinate = CurrentFrameTexture.TopTextureCoordinate;
                this.BottomTextureCoordinate = CurrentFrameTexture.BottomTextureCoordinate;
                this.LeftTextureCoordinate = CurrentFrameTexture.LeftTextureCoordinate;
                this.RightTextureCoordinate = CurrentFrameTexture.RightTextureCoordinate;
                this.Texture = CurrentFrameTexture.Texture;
            }

            if (SwapDurationRemaining > 0.0f)
            {
                this.X -= ((StartPosition - EndingPosition) / SwapDuration) * delta;

                SwapDurationRemaining -= delta;

                if (SwapDurationRemaining <= 0)
                    this.X = EndingPosition;
            }
            
            base.Update(delta);
        }

        private TextureInfo CurrentFrameTexture;
        private float CurrentAnimationTime;

        private float SwapDuration;
        private float StartPosition;
        private float EndingPosition;
        private float SwapDurationRemaining;
    }
}
