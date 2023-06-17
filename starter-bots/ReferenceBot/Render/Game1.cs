using Domain.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Render;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private int width;
    private int height;

    private BotStateDTO? _botState;
    private List<Tuple<int, int>> terrain;
    private List<Tuple<int, int>> platforms;
    private List<Tuple<int, int>> ladders;
    private List<Tuple<int, int>> hazards;
    private List<Tuple<int, int>> collectibles;

    private Texture2D _texture;

    private const int TILE_SIZE = 64;

    private Tuple<int, int> GetWindowDimensions()
    {
        return new(_botState.HeroWindow.Length * TILE_SIZE, _botState.HeroWindow[0].Length * TILE_SIZE);
    }

    private Point GetTilePosition(int x, int y)
    {
        return new(x * TILE_SIZE, y * TILE_SIZE);
    }

    public void SetBotState(BotStateDTO botState)
    {
        _botState = botState;
    }

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        terrain = new();
        platforms = new();
        ladders = new();
        hazards = new();
        collectibles = new();

        Window.AllowUserResizing = false;
        Window.AllowAltF4 = false;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        _texture = new Texture2D(GraphicsDevice, 1, 1);
        _texture.SetData<Color>(new Color[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {

        if (_botState != null)
        {
            var bounds = GetWindowDimensions();
            if (bounds.Item1 != width || bounds.Item2 != height)
            {
                width = bounds.Item1;
                height = bounds.Item2;

                _graphics.PreferredBackBufferWidth = bounds.Item1;
                _graphics.PreferredBackBufferHeight = bounds.Item2;
                _graphics.ApplyChanges();
            }
            ProcessBotState();

        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        if (_botState != null)
        {
            DrawTerrain();
            DrawPlatformsAndLadders();
        }

        base.Draw(gameTime);
    }

    private void ProcessBotState()
    {
        terrain = new();
        platforms = new();
        ladders = new();
        hazards = new();
        collectibles = new();

        if (_botState == null)
        {
            return;
        }

        for (int x = 0; x < _botState.HeroWindow.Length; x++)
        {
            var col = _botState.HeroWindow[x];
            for (int y = col.Length - 1; y >= 0; y--)
            {
                switch ((ObjectType)col[y])
                {
                    case ObjectType.Solid:
                        terrain.Add(new(x, y));
                        break;
                    case ObjectType.Platform:
                        platforms.Add(new(x, y));
                        break;
                    case ObjectType.Ladder:
                        ladders.Add(new(x, y));
                        break;
                    case ObjectType.Hazard:
                        hazards.Add(new(x, y));
                        break;
                    case ObjectType.Collectible:
                        collectibles.Add(new(x, y));
                        break;
                }
            }
        }
    }

    private Rectangle GetTile(int x, int y)
    {
        return new Rectangle(GetTilePosition(x, y), new Point(TILE_SIZE, TILE_SIZE));
    }

    private void DrawTerrain()
    {
        foreach (var tile in terrain)
        {
            var rectangle = GetTile(tile.Item1, tile.Item2);
            _spriteBatch.Draw(_texture, rectangle, Color.Brown);
        }
    }

    private void DrawPlatformsAndLadders()
    {
        foreach (var tile in platforms)
        {
            var rectangle = GetTile(tile.Item1, tile.Item2);
            _spriteBatch.Draw(_texture, rectangle, Color.Green);
        }
        foreach (var tile in ladders)
        {
            var rectangle = GetTile(tile.Item1, tile.Item2);
            _spriteBatch.Draw(_texture, rectangle, Color.Blue);
        }
    }
}
