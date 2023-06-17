using Domain.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Domain.Enums;
using System.Collections.Generic;
using System;

namespace ReferenceBot.Render;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SpriteFont font;

    private int width;
    private int height;

    private BotStateDTO? _botState;
    private List<Point> terrain;
    private List<Point> platforms;
    private List<Point> ladders;
    private List<Point> hazards;
    private List<Point> collectibles;

    private Texture2D _texture;

    private const int TILE_SIZE = 32;

    private Tuple<int, int> GetWindowDimensions()
    {
        return new(_botState.HeroWindow.Length * TILE_SIZE, _botState.HeroWindow[0].Length * TILE_SIZE);
    }

    private Point GetTilePosition(int x, int y)
    {
        return new(x * TILE_SIZE, (_botState.HeroWindow[0].Length - y) * TILE_SIZE);
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

        _texture = new Texture2D(GraphicsDevice, 1, 1);
        _texture.SetData<Color>(new Color[] { Color.White });

        font = Content.Load<SpriteFont>("Arial");
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
            DrawHazardsAndCollectibles();
            DrawPlayer();
            DrawHUD();
            DrawDebugHelpers();
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

    private Rectangle GetTile(int x, int y, int width = 1, int height = 1)
    {
        var position = GetTilePosition(x, y);
        return new Rectangle(position.X, position.Y, TILE_SIZE * width, TILE_SIZE * height);
    }

    private void DrawTerrain()
    {
        _spriteBatch.Begin();
        foreach (var tile in terrain)
        {
            DrawTile(new Rectangle(tile.X, tile.Y, 1, 1), Color.Brown);
        }
        _spriteBatch.End();
    }

    private void DrawPlatformsAndLadders()
    {
        _spriteBatch.Begin();
        foreach (var tile in platforms)
        {
            DrawTile(new Rectangle(tile.X, tile.Y, 1, 1), Color.Green);
        }
        foreach (var tile in ladders)
        {
            DrawTile(new Rectangle(tile.X, tile.Y, 1, 1), Color.Blue);
        }
        _spriteBatch.End();
    }

    private void DrawHazardsAndCollectibles()
    {
        _spriteBatch.Begin();
        foreach (var tile in  hazards)
        {
            DrawTile(new Rectangle(tile.X, tile.Y, 1, 1), Color.Red);
        }
        foreach (var tile in collectibles)
        {
            DrawTile(new Rectangle(tile.X, tile.Y, 1, 1), Color.Gold);
        }
        _spriteBatch.End();
    }

    private void DrawTile(Rectangle tile, Color colour)
    {
        _spriteBatch.Draw(_texture, GetTile(tile.X, tile.Y, tile.Width, tile.Height), colour);
    }

    private Rectangle GetPlayerBounds()
    {
        var x = (_botState.HeroWindow.Length / 2) - 1;
        var y = _botState.HeroWindow[0].Length / 2;
        return new Rectangle(x, y, 2, 2);
    }

    private void DrawPlayer()
    {
        _spriteBatch.Begin();
        var playerBounds = GetPlayerBounds();
        DrawTile(playerBounds, Color.Pink);
        _spriteBatch.End();
    }

    private void DrawDebugHelpers()
    {
        // var playerBounds = GetPlayerBounds();
        // _spriteBatch.Begin();
        // Console.WriteLine($"AAAA: {playerBounds}");
        // DrawTile(new Rectangle(playerBounds.Left, playerBounds.Top - 1, 2, 1), Color.White);
        // _spriteBatch.End();
    }

    private void DrawHUD()
    {
        _spriteBatch.Begin();
        Vector2 position = new(20, 20);

        String hudText =
            $"Connection ID: {_botState.ConnectionId}\n" +
            $"Bot Position: ({_botState.X},{_botState.Y})\n" +
            $"Current Level: {_botState.CurrentLevel}\n" +
            $"Collected: {_botState.Collected}";

        _spriteBatch.DrawString(font, hudText, position, Color.White);
        _spriteBatch.End();
    }
}
