using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Main()
    {
        Game game = new Game();
        game.Start();
    }
}

class Game
{
    const int Width = 30; // Increased width to include borders
    const int Height = 23;
    const string HorizontalPlayerChar = "sSs";
    const string VerticalPlayerChar = "s\nS\ns";
    const char ObstacleChar = 'X';
    const char BigObstacleChar = 'O';
    const char BonusChar = 'B';
    const char ShotChar = '\'';
    const int InitialSpeed = 100;
    const int Duration = 90;
    const int MaxLives = 3;

    int playerPosition = Width / 2;
    bool isVertical = false;
    List<int> obstacles = new List<int>();
    List<int> bigObstacles = new List<int>();
    List<int> bonuses = new List<int>();
    List<int> shots = new List<int>();
    int score = 0;
    int speed = InitialSpeed;
    bool isPlaying = true;
    DateTime startTime;
    int lives = MaxLives;
    string[] screenBuffer;

    public void Start()
    {
        Console.CursorVisible = false;
        Console.Clear();
        screenBuffer = new string[Height];

        for (int i = 0; i < Height; i++)
        {
            screenBuffer[i] = new string(' ', Width);
        }

        startTime = DateTime.Now;

        while (isPlaying && (DateTime.Now - startTime).TotalSeconds < Duration)
        {
            Update();
            Draw();
            HandleInput();
            Thread.Sleep(speed);
        }

        Console.Clear();
        Console.WriteLine($"Game Over! Final Score: {score}");
    }

    void Update()
    {
        // Move obstacles, big obstacles, bonuses, and shots
        for (int i = 0; i < obstacles.Count; i++)
            obstacles[i] += Width;
        for (int i = 0; i < bigObstacles.Count; i++)
            bigObstacles[i] += Width;
        for (int i = 0; i < bonuses.Count; i++)
            bonuses[i] += Width;
        for (int i = 0; i < shots.Count; i++)
            shots[i] -= Width;

        // Add new obstacles and bonuses
        if (new Random().Next(5) == 0)
            obstacles.Add(new Random().Next(1, Width - 1)); // Prevents obstacles from spawning on the edge
        if (new Random().Next(20) == 0) // Less frequent for big obstacles
        {
            int xPos = new Random().Next(1, Width - 3); // Ensure space for 3x2 block
            bigObstacles.Add(xPos);
        }
        if (new Random().Next(10) == 0)
            bonuses.Add(new Random().Next(1, Width - 1)); // Prevents bonuses from spawning on the edge

        // Check for collisions
        if (isVertical)
        {
            int playerStartIndex = (Height - 3) * Width + playerPosition;
            for (int i = 0; i < 3; i++)
            {
                int currentIndex = playerStartIndex + i * Width;
                if (obstacles.Contains(currentIndex) || BigObstacleCollision(playerPosition, (Height - 3) + i))
                {
                    lives--;
                    obstacles.Remove(currentIndex);
                    bigObstacles.Remove(playerPosition + (Height - 3) + i);
                    if (lives <= 0)
                        isPlaying = false;
                }
                else if (bonuses.Contains(currentIndex))
                {
                    score += 10;
                    bonuses.Remove(currentIndex);
                    speed = Math.Max(50, speed - 10);
                }
            }
        }
        else
        {
            int playerStartIndex = (Height - 1) * Width + playerPosition;
            for (int i = playerStartIndex; i < playerStartIndex + HorizontalPlayerChar.Length; i++)
            {
                if (obstacles.Contains(i) || BigObstacleCollision(i % Width, (Height - 1)))
                {
                    lives--;
                    obstacles.Remove(i);
                    bigObstacles.Remove(i % Width);
                    if (lives <= 0)
                        isPlaying = false;
                }
                else if (bonuses.Contains(i))
                {
                    score += 10;
                    bonuses.Remove(i);
                    speed = Math.Max(50, speed - 10);
                }
            }
        }

        // Check for shots hitting bonuses
        List<int> shotsToRemove = new List<int>();
        List<int> bonusesToRemove = new List<int>();

        foreach (int shot in shots)
        {
            if (bonuses.Contains(shot))
            {
                score += 20; // Double the bonus score
                bonusesToRemove.Add(shot);
                shotsToRemove.Add(shot);
            }
        }

        foreach (int shot in shotsToRemove)
            shots.Remove(shot);

        foreach (int bonus in bonusesToRemove)
            bonuses.Remove(bonus);

        // Remove obstacles, big obstacles, bonuses, and shots that are out of screen
        obstacles.RemoveAll(o => o >= Width * Height);
        bigObstacles.RemoveAll(o => o >= Width * Height);
        bonuses.RemoveAll(b => b >= Width * Height);
        shots.RemoveAll(s => s < 0);
    }

    bool BigObstacleCollision(int x, int y)
    {
        return bigObstacles.Contains(y * Width + x) || bigObstacles.Contains((y - 1) * Width + x) || bigObstacles.Contains((y - 1) * Width + (x - 1)) || bigObstacles.Contains(y * Width + (x - 1)) || bigObstacles.Contains(y * Width + (x - 2)) || bigObstacles.Contains((y - 1) * Width + (x - 2));
    }

    void Draw()
    {
        Console.SetCursorPosition(0, 0);

        for (int y = 0; y < Height; y++)
        {
            string currentLine = screenBuffer[y];

            // Add borders on the sides of the track
            string border = "||";
            string track = new string(' ', Width - 4); // Width minus the two border columns

            string lineToDraw = border + track + border;

            for (int x = 0; x < Width; x++)
            {
                char currentChar = currentLine[x];
                char newChar = ' ';

                if (x == 0 || x == Width - 1)
                {
                    newChar = '|'; // Display the track borders
                }
                else if (isVertical && x == playerPosition && y >= Height - 3 && y < Height)
                {
                    newChar = VerticalPlayerChar.Split('\n')[y - (Height - 3)][0];
                }
                else if (!isVertical && x >= playerPosition && x < playerPosition + HorizontalPlayerChar.Length && y == Height - 1)
                {
                    newChar = HorizontalPlayerChar[x - playerPosition];
                }
                else if (obstacles.Contains(y * Width + x))
                {
                    newChar = ObstacleChar;
                }
                else if (BigObstacleCollision(x, y))
                {
                    newChar = BigObstacleChar;
                }
                else if (bonuses.Contains(y * Width + x))
                {
                    newChar = BonusChar;
                }
                else if (shots.Contains(y * Width + x))
                {
                    newChar = ShotChar;
                }

                if (currentChar != newChar)
                {
                    Console.SetCursorPosition(x, y);
                    Console.ForegroundColor = GetColor(newChar);
                    Console.Write(newChar);
                    screenBuffer[y] = screenBuffer[y].Substring(0, x) + newChar + screenBuffer[y].Substring(x + 1);
                }
            }

            Console.WriteLine(); // New line after each track line
        }

        Console.ResetColor();
        Console.SetCursorPosition(0, Height);
        Console.Write($"Score: {score}    Lives: [");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write($"{new string('♥', lives)}");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"]    Time Left: {Duration - (int)(DateTime.Now - startTime).TotalSeconds} seconds");

        Console.WriteLine("");
        Console.WriteLine("123456789*123456789*123456789*123456789*123456789*");
        Console.WriteLine("Keys : <- -> to move | space to shoot | Tab to rotate");
    }

    ConsoleColor GetColor(char character)
    {
        switch (character)
        {
            case 's':
            case 'S':
                return ConsoleColor.DarkYellow;
            case ObstacleChar:
                return ConsoleColor.Red;
            case BigObstacleChar:
                return ConsoleColor.DarkRed;
            case BonusChar:
                return ConsoleColor.Green;
            case ShotChar:
                return ConsoleColor.Cyan;
            default:
                return ConsoleColor.White;
        }
    }

    void HandleInput()
    {
        if (!Console.KeyAvailable)
            return;

        var key = Console.ReadKey(true).Key;
        switch (key)
        {
            case ConsoleKey.LeftArrow:
                if (playerPosition > 1) // Prevent player from going off the left edge
                    playerPosition--;
                break;
            case ConsoleKey.RightArrow:
                if (playerPosition < Width - (isVertical ? 1 : HorizontalPlayerChar.Length) - 1) // Prevent player from going off the right edge
                    playerPosition++;
                break;
            case ConsoleKey.Spacebar:
                shots.Add((Height - (isVertical ? 3 : 1)) * Width + playerPosition + (isVertical ? 1 : HorizontalPlayerChar.Length / 2));
                break;
            case ConsoleKey.Tab:
                isVertical = !isVertical; // Toggle rotation
                break;
        }
    }
}
