using System.Data;
using System.Numerics;
using System.Text.Json.Serialization;

namespace shared.Models;

public class PlayerShip
{
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Position { get; private set; }
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Velocity { get; private set; }
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Direction { get; private set; }
  public InputState? InputState { get; set; }
  public int Health { get; private set; }
  public int MaxHealth { get; } = 100;
  public string Color { get; private set; }
  private const float acceleration = 0.0005f;
  private const float rotationAmount = 0.05f;
  public float VelocityCap { get; init; } = 0.3f;
  private const int maxX = 400 * 3;
  private const int maxY = 300 * 3;
  private static readonly Random random = new();
  private static readonly string[] colors = ["blue", "red", "green", "yellow", "purple", "orange"];

  public PlayerShip()
  {
    Position = new Vector2(
        random.Next(-maxX, maxX),
        random.Next(-maxY, maxY)
    );
    Velocity = new Vector2(0.0f, 0);
    Direction = new Vector2(0, -1);
    Health = MaxHealth;
    Color = colors[random.Next(colors.Length)];
  }

  [JsonConstructor]
  public PlayerShip(Vector2 position, Vector2 velocity, Vector2 direction, int health, string color)
  {
    Position = position;
    Velocity = velocity;
    Direction = direction;
    Health = health;
    Color = color;
  }

  public void Update(float timeStep)
  {
    if (InputState != null && InputState.RotationDirection == RotationDirection.Left)
    {
      Direction = Vector2.Transform(Direction, Matrix3x2.CreateRotation(rotationAmount));
      Direction = Vector2.Normalize(Direction);
    }
    else if (InputState != null && InputState.RotationDirection == RotationDirection.Right)
    {
      Direction = Vector2.Transform(Direction, Matrix3x2.CreateRotation(-rotationAmount));
      Direction = Vector2.Normalize(Direction);
    }

    if (InputState != null && InputState.Thrusting)
    {
      Vector2 oldPosition = Position;
      Position += Velocity * timeStep + 0.5f * Direction * acceleration * timeStep * timeStep;
      Velocity = (Position - oldPosition) / timeStep;

      if (Velocity.Length() > VelocityCap)
      {
        Velocity = Vector2.Normalize(Velocity) * VelocityCap;
      }
    }
    else
    {
      Position += Velocity * timeStep;
    }
    Position = new Vector2(
        WrapValue(Position.X, -maxX, maxX),
        WrapValue(Position.Y, -maxY, maxY)
    );
  }

  private float WrapValue(float value, float min, float max)
  {
    float range = max - min;
    while (value < min) value += range;
    while (value > max) value -= range;
    return value;
  }
}

public enum RotationDirection
{
  None,
  Left,
  Right
}

public class InputState
{
  public bool Thrusting { get; set; }
  public RotationDirection RotationDirection { get; set; }
  public int ShootPressed { get; set; }
}