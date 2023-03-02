namespace SimpleClock;

using SkiaSharp;

using TuringSmartScreenLib;
using TuringSmartScreenLib.Helpers.SkiaSharp;

public class Worker : BackgroundService
{
    // TODO
    private const int Margin = 2;
    private const int Digits = 4;

    private readonly ILogger<Worker> logger;

    private readonly Settings settings;

    public Worker(ILogger<Worker> logger, Settings settings)
    {
        this.logger = logger;
        this.settings = settings;
    }

    // ReSharper disable FunctionNeverReturns
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Simple clock start");

        // Create screen
        using var screen = ScreenFactory.Create(Enum.Parse<ScreenType>(settings.Type), settings.Port);
        screen.SetBrightness(100);
        // TODO
        screen.Orientation = ScreenOrientation.ReverseLandscape;

        // Clear
        var clearBuffer = screen.CreateBuffer();
        clearBuffer.Clear();
        screen.DisplayBuffer(clearBuffer);

        // Paint
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        // TODO
        paint.TextSize = 192;
        paint.Color = new SKColor(0, 200, 83);

        // Calc image size
        var imageWidth = 0;
        var imageHeight = 0;
        for (var i = 0; i < 10; i++)
        {
            var rect = default(SKRect);
            paint.MeasureText($"{i}", ref rect);

            imageWidth = Math.Max(imageWidth, (int)Math.Floor(rect.Width));
            imageHeight = Math.Max(imageHeight, (int)Math.Floor(rect.Height));
        }

        imageWidth += Margin * 2;
        imageHeight += Margin * 2;

        // Create digit image
        var digitImages = new IScreenBuffer[10];
        for (var i = 0; i < 10; i++)
        {
            using var bitmap = new SKBitmap(imageWidth, imageHeight);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Black);
            canvas.DrawText($"{i}", Margin, imageHeight - Margin, paint);
            canvas.Flush();

            var buffer = screen.CreateBuffer(imageWidth, imageHeight);
            buffer.ReadFrom(bitmap, 0, 0, imageWidth, imageHeight);
            digitImages[i] = buffer;
        }

        // Prepare display setting
        var baseX = (screen.Width - (imageWidth * Digits)) / 2;
        var baseY = (screen.Height / 2) - (imageHeight / 2);

        var previousValues = new int[Digits];
        for (var i = 0; i < previousValues.Length; i++)
        {
            previousValues[i] = Int32.MinValue;
        }

        // Display loop
        while (true)
        {
            var now = DateTime.Now;
            var value = (now.Hour * 100) + now.Minute;
            for (var i = Digits - 1; i >= 0; i--)
            {
                var number = value % 10;
                if (previousValues[i] != number)
                {
                    screen.DisplayBuffer(baseX + (imageWidth * i), baseY, digitImages[number]);
                    previousValues[i] = number;
                }

                value /= 10;
            }

            await Task.Delay(100, stoppingToken).ConfigureAwait(false);
        }
    }
    // ReSharper restore FunctionNeverReturns
}
