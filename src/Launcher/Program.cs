

// async count down 3 seconds
await Task.Run(async () => {
    for (int i = 3; i > 0; i--)
    {
        await Task.Delay(1000);
        Console.WriteLine(i);
    }
});
