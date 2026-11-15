using Engine.Core;
using Engine.Runtime;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.MonoGame
{
    public class EngineGame : Game
    {
        protected EngineHost Engine;
        protected GameRuntime Runtime;
        protected GraphicsDeviceManager Graphics;
        private SpriteBatch _spriteBatch;

        public EngineGame(EngineHost engine, GameRuntime runtime)
        {
            Engine = engine;
            Runtime = runtime;
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Runtime.Update(gameTime.ElapsedGameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }

        public new void Run()
        {
            base.Run();
        }

        public new void Run(GameRunBehavior behavior)
        {
            base.Run(behavior);
        }
    }
}
