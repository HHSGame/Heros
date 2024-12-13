
var task = async () => {
    await Task.Delay(1000);
    Console.WriteLine("Hello, world!");
};

Console.WriteLine("{0}", task.ToString());

